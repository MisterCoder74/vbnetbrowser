'SftpFileService.vb - SFTP/SSH file service using SSH.NET library
Imports Renci.SshNet
Imports Renci.SshNet.Sftp
Imports System.IO
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks

Namespace vbnetbrowser.Services

    Public Class SftpFileService
        Inherits FileTransferService

        Private _sftpClient As SftpClient
        Private ReadOnly _bufferSize As Integer = 65536

        Public Sub New()
        End Sub

        Public Overrides Function ToString() As String
            Return $"SFTP: {_profile?.Host}"
        End Function

        Public Overrides Async Function ConnectAsync(profile As Models.ConnectionProfile, cancellationToken As CancellationToken) As Task(Of Boolean)
            Try
                _profile = profile

                ReportProgress(Nothing, "Connecting...", 0)

                Await Task.Run(Sub()
                    Dim connectionInfo As ConnectionInfo = CreateConnectionInfo(profile)

                    _sftpClient = New SftpClient(connectionInfo) With {
                        .KeepAliveInterval = TimeSpan.FromSeconds(30),
                        .ConnectionInfo = connectionInfo
                    }

                    _sftpClient.Connect()

                    _isConnected = _sftpClient.IsConnected
                    _connectionTime = DateTime.UtcNow

                    If _isConnected Then
                        OnConnectionStateChanged(profile)
                    End If
                End Sub, cancellationToken)

                Return _isConnected
            Catch ex As Exception
                _isConnected = False
                Return False
            End Try
        End Function

        Private Function CreateConnectionInfo(profile As Models.ConnectionProfile) As ConnectionInfo
            Dim methods As New List(Of AuthenticationMethod)()

            ' Password authentication
            If Not String.IsNullOrEmpty(profile.Username) AndAlso Not String.IsNullOrEmpty(profile.Password) Then
                methods.Add(New PasswordAuthenticationMethod(profile.Username, profile.Password))
            End If

            ' Private key file authentication
            If Not String.IsNullOrEmpty(profile.PrivateKeyPath) AndAlso File.Exists(profile.PrivateKeyPath) Then
                Dim privateKeyFile As New PrivateKeyFile(profile.PrivateKeyPath)
                methods.Add(New PrivateKeyAuthenticationMethod(profile.Username, privateKeyFile))
            End If

            ' Keyboard interactive authentication
            If methods.Count = 0 Then
                methods.Add(New KeyboardInteractiveAuthenticationMethod(profile.Username))
            End If

            Return New ConnectionInfo(
                profile.Host,
                profile.Port,
                profile.Username,
                methods.ToArray()
            )
        End Function

        Public Overrides Sub Disconnect()
            If _sftpClient IsNot Nothing Then
                If _sftpClient.IsConnected Then
                    _sftpClient.Disconnect()
                End If
                _sftpClient.Dispose()
                _sftpClient = Nothing
            End If
            _isConnected = False
        End Sub

        Public Overrides Async Function ListDirectoryAsync(path As String, progress As IProgress(Of Models.OperationProgress), cancellationToken As CancellationToken) As Task(Of List(Of Models.FileItem))
            If Not _sftpClient.IsConnected Then
                Throw New InvalidOperationException("Not connected to SFTP server. Call ConnectAsync first.")
            End If

            Dim result As New List(Of Models.FileItem)()

            ReportProgress(progress, "Listing directory...", 0)

            Dim normalizedPath As String = NormalizePath(path)

            Await Task.Run(Sub()
                Dim entries As IEnumerable(Of SftpFile) = _sftpClient.ListDirectory(normalizedPath)

                For Each entry As SftpFile In entries
                    cancellationToken.ThrowIfCancellationRequested()

                    ' Skip . and ..
                    If entry.Name = "." OrElse entry.Name = ".." Then Continue For

                    Dim fileItem As New Models.FileItem() With {
                        .Name = entry.Name,
                        .FullPath = CombinePath(normalizedPath, entry.Name),
                        .IsDirectory = entry.IsDirectory,
                        .Size = entry.Length,
                        .LastModified = entry.LastAccessTime,
                        .Permissions = entry.Attributes?.Permissions?.ToString(),
                        .Owner = entry.Attributes?.Owner,
                        .Group = entry.Attributes?.Group,
                        .Protocol = Models.ProtocolType.Sftp
                    }

                    result.Add(fileItem)
                Next
            End Sub, cancellationToken)

            ReportProgress(progress, $"Found {result.Count} items", 100)
            Return result
        End Function

        Public Overrides Async Function DownloadFileAsync(path As String, progress As IProgress(Of Models.OperationProgress), cancellationToken As CancellationToken) As Task(Of String)
            Dim bytes As Byte() = Await DownloadFileBytesAsync(path, progress, cancellationToken)
            Return Encoding.UTF8.GetString(bytes)
        End Function

        Public Overrides Async Function DownloadFileBytesAsync(path As String, progress As IProgress(Of Models.OperationProgress), cancellationToken As CancellationToken) As Task(Of Byte())
            If Not _sftpClient.IsConnected Then
                Throw New InvalidOperationException("Not connected to SFTP server. Call ConnectAsync first.")
            End If

            Return Await Task.Run(Of Byte())(Function()
                Using memoryStream As New MemoryStream()
                    ReportProgress(progress, $"Downloading {Path.GetFileName(path)}...", 0)

                    Dim fileInfo As Renci.SshNet.Sftp.SftpFile = Nothing
                    Dim totalBytes As Long = 0

                    Try
                        fileInfo = _sftpClient.Stat(path)
                        totalBytes = fileInfo.Length
                    Catch
                        ' File size unknown
                    End Try

                    Dim bytesWritten As Long = 0
                    Dim lastReportTime As DateTime = DateTime.MinValue

                    _sftpClient.DownloadFile(path, memoryStream, Sub(current)
                        cancellationToken.ThrowIfCancellationRequested()
                        bytesWritten = current

                        If DateTime.UtcNow - lastReportTime > TimeSpan.FromMilliseconds(100) Then
                            UpdateProgress(progress, bytesWritten, totalBytes, Path.GetFileName(path))
                            lastReportTime = DateTime.UtcNow
                        End If
                    End Sub)

                    UpdateProgress(progress, totalBytes, totalBytes, Path.GetFileName(path))
                    Return memoryStream.ToArray()
                End Function
            End Sub, cancellationToken)
        End Function

        Public Overrides Function UploadFileAsync(path As String, content As String, progress As IProgress(Of Models.OperationProgress), cancellationToken As CancellationToken) As Task
            Dim bytes As Byte() = Encoding.UTF8.GetBytes(content)
            Await UploadFileBytesAsync(path, bytes, progress, cancellationToken)
        End Function

        Public Overrides Async Function UploadFileBytesAsync(path As String, data As Byte(), progress As IProgress(Of Models.OperationProgress), cancellationToken As CancellationToken) As Task
            If Not _sftpClient.IsConnected Then
                Throw New InvalidOperationException("Not connected to SFTP server. Call ConnectAsync first.")
            End Function

            Await Task.Run(Sub()
                ReportProgress(progress, $"Uploading {Path.GetFileName(path)}...", 0)

                Using memoryStream As New MemoryStream(data)
                    Dim totalBytes As Long = data.Length
                    Dim bytesUploaded As Long = 0
                    Dim lastReportTime As DateTime = DateTime.MinValue

                    _sftpClient.UploadFile(memoryStream, path, True, Sub(current)
                        cancellationToken.ThrowIfCancellationRequested()
                        bytesUploaded = current

                        If DateTime.UtcNow - lastReportTime > TimeSpan.FromMilliseconds(100) Then
                            UpdateProgress(progress, bytesUploaded, totalBytes, Path.GetFileName(path))
                            lastReportTime = DateTime.UtcNow
                        End If
                    End Sub)
                End Using

                UpdateProgress(progress, totalBytes, totalBytes, Path.GetFileName(path))
            End Sub, cancellationToken)
        End Function

        Public Overrides Async Function DeleteAsync(path As String, cancellationToken As CancellationToken) As Task(Of Boolean)
            If Not _sftpClient.IsConnected Then
                Throw New InvalidOperationException("Not connected to SFTP server. Call ConnectAsync first.")
            End If

            Return Await Task.Run(Function()
                Try
                    Dim fileInfo As New FileInfo(path)
                    If fileInfo.IsDirectory() Then
                        _sftpClient.DeleteDirectory(path)
                    Else
                        _sftpClient.DeleteFile(path)
                    End If
                    Return True
                Catch ex As Exception
                    Return False
                End Function
            End Sub, cancellationToken)
        End Function

        Public Overrides Async Function CreateDirectoryAsync(path As String, name As String, cancellationToken As CancellationToken) As Task(Of Boolean)
            If Not _sftpClient.IsConnected Then
                Throw New InvalidOperationException("Not connected to SFTP server. Call ConnectAsync first.")
            End If

            Return Await Task.Run(Function()
                Dim fullPath As String = CombinePath(path, name)
                _sftpClient.CreateDirectory(fullPath)
                Return True
            End Sub, cancellationToken)
        End Function

        Public Overrides Function CreateFileAsync(path As String, content As String, cancellationToken As CancellationToken) As Task(Of Boolean)
            If Not _sftpClient.IsConnected Then
                Throw New InvalidOperationException("Not connected to SFTP server. Call ConnectAsync first.")
            End If

            Return Task.Run(Sub()
                Using stream As New MemoryStream(Encoding.UTF8.GetBytes(content))
                    _sftpClient.UploadFile(stream, path, True)
                End Using
            End Sub, cancellationToken).ContinueWith(Function(t) Not t.IsFaulted)
        End Function

        Public Overrides Async Function RenameAsync(path As String, newName As String, cancellationToken As CancellationToken) As Task(Of Boolean)
            If Not _sftpClient.IsConnected Then
                Throw New InvalidOperationException("Not connected to SFTP server. Call ConnectAsync first.")
            End If

            Return Await Task.Run(Function()
                _sftpClient.RenameFile(path, newName)
                Return True
            End Sub, cancellationToken)
        End Function

        Public Overrides Async Function GetFileInfoAsync(path As String, cancellationToken As CancellationToken) As Task(Of Models.FileItem)
            If Not _sftpClient.IsConnected Then
                Throw New InvalidOperationException("Not connected to SFTP server. Call ConnectAsync first.")
            End If

            Return Await Task.Run(Function()
                Dim attributes As SftpFileAttributes = _sftpClient.GetAttributes(path)
                Dim name As String = Path.GetFileName(path)

                Return New Models.FileItem() With {
                    .Name = name,
                    .FullPath = path,
                    .IsDirectory = attributes.IsDirectory,
                    .Size = attributes.Size,
                    .LastModified = attributes.LastAccessTime,
                    .Permissions = attributes.Permissions?.ToString(),
                    .Owner = attributes.Owner,
                    .Group = attributes.Group,
                    .Protocol = Models.ProtocolType.Sftp
                }
            End Sub, cancellationToken)
        End Function

        Public Overrides Function PathExists(path As String) As Boolean
            If Not _sftpClient.IsConnected Then Return False

            Try
                Return _sftpClient.Exists(path)
            Catch
                Return False
            End Try
        End Function

        Public Overrides Function GetHomeDirectory() As String
            If _sftpClient IsNot Nothing AndAlso _sftpClient.IsConnected Then
                Return _sftpClient.WorkingDirectory
            End If
            Return "/"
        End Function

        Protected Overrides Function CheckConnection() As Boolean
            Return _sftpClient IsNot Nothing AndAlso _sftpClient.IsConnected
        End Function
    End Class
End Namespace
