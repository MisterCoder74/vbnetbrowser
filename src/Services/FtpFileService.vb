'FtpFileService.vb - FTP/FTPS file service with modern async operations
Imports System.IO
Imports System.Net
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks

Namespace vbnetbrowser.Services

    Public Class FtpFileService
        Inherits FileTransferService

        Private _ftpRequest As FtpWebRequest
        Private _ftpUrl As String
        Private ReadOnly _bufferSize As Integer = 65536

        Public Sub New()
        End Sub

        Public Overrides Function ToString() As String
            Return $"FTP: {_ftpUrl}"
        End Function

        Private Function GetProtocolPrefix(isSecure As Boolean) As String
            Return If(isSecure, "ftps", "ftp")
        End Function

        Public Overrides Async Function ConnectAsync(profile As Models.ConnectionProfile, cancellationToken As CancellationToken) As Task(Of Boolean)
            Try
                _profile = profile
                Dim prefix As String = GetProtocolPrefix(profile.Protocol = Models.ProtocolType.Ftps)
                _ftpUrl = $"{prefix}://{profile.Host}:{profile.Port}"

                ReportProgress(Nothing, "Connecting...", 0)
                Await TestConnectionAsync(cancellationToken)

                _isConnected = True
                _connectionTime = DateTime.UtcNow
                OnConnectionStateChanged(profile)

                Return True
            Catch ex As Exception
                _isConnected = False
                Return False
            End Try
        End Function

        Private Async Function TestConnectionAsync(cancellationToken As CancellationToken) As Task
            Dim request As FtpWebRequest = CreateFtpRequest("/")
            request.Method = WebRequestMethods.Ftp.ListDirectory

            Using response As FtpWebResponse = CType(Await request.GetResponseAsync(cancellationToken), FtpWebResponse)
                Using responseStream As Stream = response.GetResponseStream()
                    Using reader As New StreamReader(responseStream)
                        Await reader.ReadToEndAsync()
                    End Using
                End Using
            End Using
        End Function

        Public Overrides Sub Disconnect()
            _isConnected = False
        End Sub

        Public Overrides Async Function ListDirectoryAsync(path As String, progress As IProgress(Of Models.OperationProgress), cancellationToken As CancellationToken) As Task(Of List(Of Models.FileItem))
            Dim result As New List(Of Models.FileItem)()

            ReportProgress(progress, "Listing directory...", 0)

            Dim request As FtpWebRequest = CreateFtpRequest(path)
            request.Method = WebRequestMethods.Ftp.ListDirectory

            Try
                Using response As FtpWebResponse = CType(Await request.GetResponseAsync(cancellationToken), FtpWebResponse)
                    Using responseStream As Stream = response.GetResponseStream()
                        Using reader As New StreamReader(responseStream)
                            Dim directoryListing As String = Await reader.ReadToEndAsync()
                            Dim lines As String() = directoryListing.Split(New String() {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)

                            For Each line As String In lines
                                cancellationToken.ThrowIfCancellationRequested()

                                Dim fileItem As Models.FileItem = ParseFtpLine(line, path)
                                If fileItem IsNot Nothing Then
                                    result.Add(fileItem)
                                End If
                            Next
                        End Using
                    End Using
                End Using

                ' Also get detailed listing for file info
                Await EnrichWithDetailsAsync(result, cancellationToken)

                ReportProgress(progress, $"Found {result.Count} items", 100)
                Return result
            Catch ex As Exception When TypeOf ex Is TaskCanceledException OrElse TypeOf ex Is OperationCanceledException
                Throw
            Catch webEx As WebException
                If webEx.Response IsNot Nothing AndAlso CType(webEx.Response, FtpWebResponse).StatusCode = FtpStatusCode.ActionNotTakenFileUnavailable Then
                    Return result
                End If
                Throw
            End Try
        End Function

        Private Async Function EnrichWithDetailsAsync(items As List(Of Models.FileItem), cancellationToken As CancellationToken) As Task
            For Each item As Models.FileItem In items.Where(Function(i) Not i.IsDirectory)
                Try
                    Dim fileInfo As Models.FileItem = Await GetFileInfoAsync(item.FullPath, cancellationToken)
                    If fileInfo IsNot Nothing Then
                        item.Size = fileInfo.Size
                        item.LastModified = fileInfo.LastModified
                    End If
                Catch
                    ' Ignore errors for individual files
                End Try
            Next
        End Function

        Private Function ParseFtpLine(line As String, currentPath As String) As Models.FileItem
            Try
                ' FTP directory listing format varies, try to parse common formats
                ' Format: "drwxr-xr-x    2 ftp      ftp          4096 Nov 15 10:30 dirname"
                ' Or: "d--------    2 ftp      ftp          4096 Nov 15 10:30 dirname"

                If String.IsNullOrWhiteSpace(line) Then Return Nothing

                Dim parts As String() = line.Split(New String() {" "}, StringSplitOptions.RemoveEmptyEntries)
                If parts.Length < 9 Then Return Nothing

                Dim permissions As String = parts(0)
                Dim isDirectory As Boolean = permissions.StartsWith("d")

                ' Get the filename (last part)
                Dim name As String = String.Join(" ", parts.Skip(8))

                ' Handle symlinks
                If name.EndsWith(" -> .") Then
                    name = name.Substring(0, name.Length - 5)
                End If

                Return New Models.FileItem() With {
                    .Name = name,
                    .FullPath = CombinePath(currentPath, name),
                    .IsDirectory = isDirectory,
                    .Permissions = permissions,
                    .Protocol = _profile.Protocol
                }
            Catch ex As Exception
                Return Nothing
            End Try
        End Function

        Public Overrides Async Function DownloadFileAsync(path As String, progress As IProgress(Of Models.OperationProgress), cancellationToken As CancellationToken) As Task(Of String)
            Dim bytes As Byte() = Await DownloadFileBytesAsync(path, progress, cancellationToken)
            Return Encoding.UTF8.GetString(bytes)
        End Function

        Public Overrides Async Function DownloadFileBytesAsync(path As String, progress As IProgress(Of Models.OperationProgress), cancellationToken As CancellationToken) As Task(Of Byte())
            ReportProgress(progress, $"Downloading {Path.GetFileName(path)}...", 0)

            Dim request As FtpWebRequest = CreateFtpRequest(path)
            request.Method = WebRequestMethods.Ftp.DownloadFile
            request.UseBinary = True

            Using response As FtpWebResponse = CType(Await request.GetResponseAsync(cancellationToken), FtpWebResponse)
                Dim totalBytes As Long = response.ContentLength
                Using responseStream As Stream = response.GetResponseStream()
                    Using memoryStream As New MemoryStream()
                        Dim buffer As Byte() = New Byte(_bufferSize - 1) {}
                        Dim bytesRead As Long = 0
                        Dim lastReportTime As DateTime = DateTime.MinValue

                        While True
                            cancellationToken.ThrowIfCancellationRequested()

                            Dim bytesReadThisTime As Integer = Await responseStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)

                            If bytesReadThisTime = 0 Then Exit While

                            Await memoryStream.WriteAsync(buffer, 0, bytesReadThisTime, cancellationToken)
                            bytesRead += bytesReadThisTime

                            If DateTime.UtcNow - lastReportTime > TimeSpan.FromMilliseconds(100) Then
                                UpdateProgress(progress, bytesRead, totalBytes, Path.GetFileName(path))
                                lastReportTime = DateTime.UtcNow
                            End If
                        End While

                        UpdateProgress(progress, totalBytes, totalBytes, Path.GetFileName(path))
                        Return memoryStream.ToArray()
                    End Using
                End Using
            End Using
        End Function

        Public Overrides Function UploadFileAsync(path As String, content As String, progress As IProgress(Of Models.OperationProgress), cancellationToken As CancellationToken) As Task
            Dim bytes As Byte() = Encoding.UTF8.GetBytes(content)
            Await UploadFileBytesAsync(path, bytes, progress, cancellationToken)
        End Function

        Public Overrides Async Function UploadFileBytesAsync(path As String, data As Byte(), progress As IProgress(Of Models.OperationProgress), cancellationToken As CancellationToken) As Task
            ReportProgress(progress, $"Uploading {Path.GetFileName(path)}...", 0)

            Dim request As FtpWebRequest = CreateFtpRequest(path)
            request.Method = WebRequestMethods.Ftp.UploadFile
            request.UseBinary = True
            request.ContentLength = data.Length

            Using requestStream As Stream = Await request.GetRequestStreamAsync(cancellationToken)
                Dim totalBytes As Long = data.Length
                Dim bytesWritten As Long = 0
                Dim lastReportTime As DateTime = DateTime.MinValue
                Dim bufferSize As Integer = 4096

                For i As Integer = 0 To data.Length - 1 Step bufferSize
                    cancellationToken.ThrowIfCancellationRequested()

                    Dim bytesToWrite As Integer = Math.Min(bufferSize, data.Length - i)
                    Await requestStream.WriteAsync(data, i, bytesToWrite, cancellationToken)
                    bytesWritten += bytesToWrite

                    If DateTime.UtcNow - lastReportTime > TimeSpan.FromMilliseconds(100) Then
                        UpdateProgress(progress, bytesWritten, totalBytes, Path.GetFileName(path))
                        lastReportTime = DateTime.UtcNow
                    End If
                Next

                UpdateProgress(progress, totalBytes, totalBytes, Path.GetFileName(path))
            End Using

            Using response As FtpWebResponse = CType(Await request.GetResponseAsync(cancellationToken), FtpWebResponse)
                ' Response handled automatically
            End Using
        End Function

        Public Overrides Async Function DeleteAsync(path As String, cancellationToken As CancellationToken) As Task(Of Boolean)
            Dim request As FtpWebRequest = CreateFtpRequest(path)
            request.Method = WebRequestMethods.Ftp.DeleteFile

            Using response As FtpWebResponse = CType(Await request.GetResponseAsync(cancellationToken), FtpWebResponse)
                Return response.StatusCode = FtpStatusCode.ActionCompleted
            End Using
        End Function

        Public Overrides Async Function CreateDirectoryAsync(path As String, name As String, cancellationToken As CancellationToken) As Task(Of Boolean)
            Dim fullPath As String = CombinePath(path, name)
            Dim request As FtpWebRequest = CreateFtpRequest(fullPath)
            request.Method = WebRequestMethods.Ftp.MakeDirectory

            Using response As FtpWebResponse = CType(Await request.GetResponseAsync(cancellationToken), FtpWebResponse)
                Return response.StatusCode = FtpStatusCode.ActionCompleted OrElse
                       response.StatusCode = FtpStatusCode.PathnameCreated
            End Using
        End Function

        Public Overrides Function CreateFileAsync(path As String, content As String, cancellationToken As CancellationToken) As Task(Of Boolean)
            ' Upload an empty file
            Return UploadFileAsync(path, content, Nothing, cancellationToken).ContinueWith(Function(t) True)
        End Function

        Public Overrides Async Function RenameAsync(path As String, newName As String, cancellationToken As CancellationToken) As Task(Of Boolean)
            Dim request As FtpWebRequest = CreateFtpRequest(path)
            request.Method = WebRequestMethods.Ftp.Rename
            request.RenameTo = newName

            Using response As FtpWebResponse = CType(Await request.GetResponseAsync(cancellationToken), FtpWebResponse)
                Return response.StatusCode = FtpStatusCode.ActionCompleted
            End Using
        End Function

        Public Overrides Async Function GetFileInfoAsync(path As String, cancellationToken As CancellationToken) As Task(Of Models.FileItem)
            Dim request As FtpWebRequest = CreateFtpRequest(path)
            request.Method = WebRequestMethods.Ftp.GetFileSize

            Try
                Using response As FtpWebResponse = CType(Await request.GetResponseAsync(cancellationToken), FtpWebResponse)
                    Dim size As Long = response.ContentLength
                    Dim name As String = Path.GetFileName(path)

                    Return New Models.FileItem() With {
                        .Name = name,
                        .FullPath = path,
                        .Size = size,
                        .Protocol = _profile.Protocol
                    }
                End Using
            Catch webEx As WebException
                If CType(webEx.Response, FtpWebResponse)?.StatusCode = FtpStatusCode.ActionNotTakenFileUnavailable Then
                    Return Nothing
                End If
                Throw
            End Try
        End Function

        Public Overrides Function PathExists(path As String) As Boolean
            ' Quick check by attempting to get file info
            Return False ' Would need async, handled differently
        End Function

        Private Function CreateFtpRequest(path As String) As FtpWebRequest
            Dim fullUrl As String = CombinePath(_ftpUrl, path.TrimStart("/"c))
            Dim request As FtpWebRequest = CType(WebRequest.Create(fullUrl), FtpWebRequest)

            ' Enable SSL/TLS for FTPS
            If _profile.Protocol = Models.ProtocolType.Ftps Then
                request.EnableSsl = True
                request.AuthenticationLevel = System.Net.Security.AuthenticationLevel.MutualAuthRequested
            End If

            If Not String.IsNullOrEmpty(_profile.Username) Then
                request.Credentials = New NetworkCredential(_profile.Username, _profile.Password)
            Else
                request.UseDefaultCredentials = True
            End If

            request.UsePassive = True
            request.UseBinary = True
            request.KeepAlive = False
            request.timeout = 30000

            Return request
        End Function

        Protected Overrides Function CheckConnection() As Boolean
            Return Not String.IsNullOrEmpty(_ftpUrl)
        End Function
    End Class
End Namespace
