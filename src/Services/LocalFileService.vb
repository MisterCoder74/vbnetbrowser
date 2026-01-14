'LocalFileService.vb - Local file system service
Imports System.IO
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks

Namespace vbnetbrowser.Services

    Public Class LocalFileService
        Inherits FileTransferService

        Private _rootPath As String

        Public Sub New()
        End Sub

        Public Overrides Function ToString() As String
            Return $"Local: {_rootPath}"
        End Function

        Public Overrides Async Function ConnectAsync(profile As Models.ConnectionProfile, cancellationToken As CancellationToken) As Task(Of Boolean)
            Try
                _profile = profile
                _rootPath = If(String.IsNullOrEmpty(profile.BasePath), Directory.GetCurrentDirectory(), profile.BasePath)

                ' Ensure directory exists
                If Not Directory.Exists(_rootPath) Then
                    Directory.CreateDirectory(_rootPath)
                End If

                _isConnected = True
                _connectionTime = DateTime.UtcNow
                OnConnectionStateChanged(profile)

                Return True
            Catch ex As Exception
                _isConnected = False
                Return False
            End Try
        End Function

        Public Overrides Sub Disconnect()
            _isConnected = False
        End Sub

        Public Overrides Async Function ListDirectoryAsync(path As String, progress As IProgress(Of Models.OperationProgress), cancellationToken As CancellationToken) As Task(Of List(Of Models.FileItem))
            Dim fullPath As String = GetFullPath(path)
            Dim result As New List(Of Models.FileItem)()

            ReportProgress(progress, "Listing directory...", 0)

            ' Get directories first
            For Each dirPath As String In Directory.GetDirectories(fullPath)
                cancellationToken.ThrowIfCancellationRequested()

                Dim dirInfo As New DirectoryInfo(dirPath)
                result.Add(New Models.FileItem() With {
                    .Name = dirInfo.Name,
                    .FullPath = GetRelativePath(dirPath),
                    .IsDirectory = True,
                    .LastModified = dirInfo.LastWriteTime,
                    .Protocol = Models.ProtocolType.Local
                })
            Next

            ' Get files
            For Each filePath As String In Directory.GetFiles(fullPath)
                cancellationToken.ThrowIfCancellationRequested()

                Dim fileInfo As New FileInfo(filePath)
                result.Add(New Models.FileItem() With {
                    .Name = fileInfo.Name,
                    .FullPath = GetRelativePath(filePath),
                    .IsDirectory = False,
                    .Size = fileInfo.Length,
                    .LastModified = fileInfo.LastWriteTime,
                    .Protocol = Models.ProtocolType.Local
                })
            Next

            ReportProgress(progress, $"Found {result.Count} items", 100)
            Return result
        End Function

        Public Overrides Async Function DownloadFileAsync(path As String, progress As IProgress(Of Models.OperationProgress), cancellationToken As CancellationToken) As Task(Of String)
            Dim fullPath As String = GetFullPath(path)

            ReportProgress(progress, $"Reading {Path.GetFileName(path)}...", 0)

            Dim content As String = Await Task.Run(Async Function()
                Using reader As New StreamReader(fullPath, Encoding.UTF8, True)
                    Return Await reader.ReadToEndAsync()
                End Using
            End Function, cancellationToken)

            ReportProgress(progress, "Read complete", 100)
            Return content
        End Function

        Public Overrides Async Function DownloadFileBytesAsync(path As String, progress As IProgress(Of Models.OperationProgress), cancellationToken As CancellationToken) As Task(Of Byte())
            Dim fullPath As String = GetFullPath(path)
            Dim fileInfo As New FileInfo(fullPath)

            ReportProgress(progress, $"Reading {Path.GetFileName(path)}...", 0)
            UpdateProgress(progress, 0, fileInfo.Length, Path.GetFileName(path))

            Dim bytes As Byte() = Await Task.Run(Function()
                Return File.ReadAllBytes(fullPath)
            End Function, cancellationToken)

            UpdateProgress(progress, fileInfo.Length, fileInfo.Length, Path.GetFileName(path))
            Return bytes
        End Function

        Public Overrides Function UploadFileAsync(path As String, content As String, progress As IProgress(Of Models.OperationProgress), cancellationToken As CancellationToken) As Task
            Dim bytes As Byte() = Encoding.UTF8.GetBytes(content)
            Return UploadFileBytesAsync(path, bytes, progress, cancellationToken)
        End Function

        Public Overrides Async Function UploadFileBytesAsync(path As String, data As Byte(), progress As IProgress(Of Models.OperationProgress), cancellationToken As CancellationToken) As Task
            Dim fullPath As String = GetFullPath(path)
            Dim directory As String = Path.GetDirectoryName(fullPath)

            ReportProgress(progress, $"Writing {Path.GetFileName(path)}...", 0)

            If Not String.IsNullOrEmpty(directory) AndAlso Not Directory.Exists(directory) Then
                Directory.CreateDirectory(directory)
            End If

            Await Task.Run(Sub()
                File.WriteAllBytes(fullPath, data)
            End Sub, cancellationToken)

            ReportProgress(progress, "Write complete", 100)
        End Function

        Public Overrides Async Function DeleteAsync(path As String, cancellationToken As CancellationToken) As Task(Of Boolean)
            Dim fullPath As String = GetFullPath(path)

            Return Await Task.Run(Function()
                If File.Exists(fullPath) Then
                    File.Delete(fullPath)
                    Return True
                ElseIf Directory.Exists(fullPath) Then
                    Directory.Delete(fullPath, True)
                    Return True
                End If
                Return False
            End Function, cancellationToken)
        End Function

        Public Overrides Async Function CreateDirectoryAsync(path As String, name As String, cancellationToken As CancellationToken) As Task(Of Boolean)
            Dim fullPath As String = GetFullPath(Path.Combine(path, name))

            Return Await Task.Run(Sub()
                Directory.CreateDirectory(fullPath)
            End Sub, cancellationToken).ContinueWith(Function(t) True)
        End Function

        Public Overrides Async Function CreateFileAsync(path As String, content As String, cancellationToken As CancellationToken) As Task(Of Boolean)
            Dim fullPath As String = GetFullPath(path)
            Dim directory As String = Path.GetDirectoryName(fullPath)

            If Not String.IsNullOrEmpty(directory) AndAlso Not Directory.Exists(directory) Then
                Directory.CreateDirectory(directory)
            End If

            Await Task.Run(Sub()
                File.WriteAllText(fullPath, content)
            End Sub, cancellationToken)

            Return True
        End Function

        Public Overrides Async Function RenameAsync(path As String, newName As String, cancellationToken As CancellationToken) As Task(Of Boolean)
            Dim fullPath As String = GetFullPath(path)
            Dim parentDir As String = Path.GetDirectoryName(fullPath)
            Dim newPath As String = Path.Combine(parentDir, newName)

            Return Await Task.Run(Function()
                If File.Exists(fullPath) Then
                    File.Move(fullPath, newPath)
                ElseIf Directory.Exists(fullPath) Then
                    Directory.Move(fullPath, newPath)
                Else
                    Return False
                End If
                Return True
            End Function, cancellationToken)
        End Function

        Public Overrides Async Function GetFileInfoAsync(path As String, cancellationToken As CancellationToken) As Task(Of Models.FileItem)
            Dim fullPath As String = GetFullPath(path)

            Return Await Task.Run(Function()
                If File.Exists(fullPath) Then
                    Dim fileInfo As New FileInfo(fullPath)
                    Return New Models.FileItem() With {
                        .Name = fileInfo.Name,
                        .FullPath = path,
                        .IsDirectory = False,
                        .Size = fileInfo.Length,
                        .LastModified = fileInfo.LastWriteTime,
                        .Protocol = Models.ProtocolType.Local
                    }
                ElseIf Directory.Exists(fullPath) Then
                    Dim dirInfo As New DirectoryInfo(fullPath)
                    Return New Models.FileItem() With {
                        .Name = dirInfo.Name,
                        .FullPath = path,
                        .IsDirectory = True,
                        .LastModified = dirInfo.LastWriteTime,
                        .Protocol = Models.ProtocolType.Local
                    }
                End If
                Return Nothing
            End Function, cancellationToken)
        End Function

        Public Overrides Function PathExists(path As String) As Boolean
            Dim fullPath As String = GetFullPath(path)
            Return File.Exists(fullPath) OrElse Directory.Exists(fullPath)
        End Function

        Public Overrides Function GetHomeDirectory() As String
            Return _rootPath
        End Function

        Private Function GetFullPath(path As String) As String
            If String.IsNullOrEmpty(path) OrElse path = "/" OrElse path = "\" Then
                Return _rootPath
            End If

            ' Convert URL-style paths to local paths
            Dim localPath As String = path.Replace("/"c, Path.DirectorySeparatorChar).Replace("\"c, Path.DirectorySeparatorChar)
            Return Path.Combine(_rootPath, localPath)
        End Function

        Private Function GetRelativePath(fullPath As String) As String
            If fullPath.StartsWith(_rootPath) Then
                Dim relative As String = fullPath.Substring(_rootPath.Length)
                Return relative.Replace(Path.DirectorySeparatorChar, "/"c).TrimStart("/"c)
            End If
            Return fullPath
        End Function

        Protected Overrides Function CheckConnection() As Boolean
            Return Directory.Exists(_rootPath)
        End Function
    End Class
End Namespace
