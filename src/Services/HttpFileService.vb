'HttpFileService.vb - HTTP/HTTPS file service using modern HttpClient
Imports System.Net
Imports System.Net.Http
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Diagnostics

Namespace vbnetbrowser.Services

    Public Class HttpFileService
        Inherits FileTransferService

        Private _httpClient As HttpClient
        Private _baseUrl As String
        Private ReadOnly _maxRetries As Integer = 3
        Private ReadOnly _retryDelay As TimeSpan = TimeSpan.FromSeconds(2)

        Public Sub New()
        End Sub

        Public Overrides Function ToString() As String
            Return $"HTTP: {_baseUrl}"
        End Function

        Public Overrides Async Function ConnectAsync(profile As Models.ConnectionProfile, cancellationToken As CancellationToken) As Task(Of Boolean)
            Try
                _profile = profile
                _baseUrl = NormalizeUrl(profile.Host)

                Dim handler As New HttpClientHandler() With {
                    .AllowAutoRedirect = True,
                    .MaxAutomaticRedirections = 5
                }

                If Not String.IsNullOrEmpty(profile.Username) AndAlso Not String.IsNullOrEmpty(profile.Password) Then
                    handler.Credentials = New NetworkCredential(profile.Username, profile.Password)
                End If

                _httpClient = New HttpClient(handler) With {
                    .Timeout = TimeSpan.FromSeconds(30)
                }
                _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("VB.NET Browser File Manager/1.0")
                _httpClient.DefaultRequestHeaders.Accept.ParseAdd("*/*")

                ReportProgress(Nothing, "Connecting...", 0)
                Dim response As HttpResponseMessage = Await _httpClient.GetAsync(_baseUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                response.EnsureSuccessStatusCode()

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
            If _httpClient IsNot Nothing Then
                _httpClient.Dispose()
                _httpClient = Nothing
            End If
            _isConnected = False
        End Sub

        Public Overrides Function ListDirectoryAsync(path As String, progress As IProgress(Of Models.OperationProgress), cancellationToken As CancellationToken) As Task(Of List(Of Models.FileItem))
            Throw New NotSupportedException("Directory listing is not supported for HTTP protocol. HTTP is request-response based.")
        End Function

        Public Overrides Async Function DownloadFileAsync(path As String, progress As IProgress(Of Models.OperationProgress), cancellationToken As CancellationToken) As Task(Of String)
            Dim bytes As Byte() = Await DownloadFileBytesAsync(path, progress, cancellationToken)
            Return System.Text.Encoding.UTF8.GetString(bytes)
        End Function

        Public Overrides Async Function DownloadFileBytesAsync(path As String, progress As IProgress(Of Models.OperationProgress), cancellationToken As CancellationToken) As Task(Of Byte())
            If _httpClient Is Nothing Then
                Throw New InvalidOperationException("Not connected. Call ConnectAsync first.")
            End If

            Dim url As String = CombinePath(_baseUrl, path)
            Dim attempt As Integer = 0
            Dim lastException As Exception = Nothing

            While attempt < _maxRetries
                Try
                    ReportProgress(progress, $"Downloading {System.IO.Path.GetFileName(path)}...", 0)

                    Using response As HttpResponseMessage = Await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                        response.EnsureSuccessStatusCode()

                        Dim totalBytes As Long = response.Content.Headers.ContentLength.GetValueOrDefault(-1)
                        Using contentStream As IO.Stream = Await response.Content.ReadAsStreamAsync()
                            Using memoryStream As New IO.MemoryStream()
                                Dim buffer As Byte() = New Byte(81919) {}
                                Dim bytesRead As Long = 0
                                Dim lastReportTime As DateTime = DateTime.MinValue

                                While True
                                    cancellationToken.ThrowIfCancellationRequested()

                                    Dim bytesReadThisTime As Integer = Await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)

                                    If bytesReadThisTime = 0 Then Exit While

                                    Await memoryStream.WriteAsync(buffer, 0, bytesReadThisTime, cancellationToken)
                                    bytesRead += bytesReadThisTime

                                    ' Update progress periodically
                                    If DateTime.UtcNow - lastReportTime > TimeSpan.FromMilliseconds(100) Then
                                        UpdateProgress(progress, bytesRead, totalBytes, System.IO.Path.GetFileName(path))
                                        lastReportTime = DateTime.UtcNow
                                    End If
                                End While

                                UpdateProgress(progress, totalBytes, totalBytes, System.IO.Path.GetFileName(path))
                                Return memoryStream.ToArray()
                            End Using
                        End Using
                    End Using
                Catch ex As Exception When TypeOf ex Is TaskCanceledException OrElse TypeOf ex Is OperationCanceledException
                    Throw
                Catch ex As Exception
                    lastException = ex
                    attempt += 1
                    If attempt < _maxRetries Then
                        ReportProgress(progress, $"Retrying ({attempt}/{_maxRetries})...", 0)
                        Await Task.Delay(_retryDelay, cancellationToken)
                    End If
                End Try
            End While

            Throw New Exception($"Failed to download after {_maxRetries} attempts: {lastException?.Message}", lastException)
        End Function

        Public Overrides Function UploadFileAsync(path As String, content As String, progress As IProgress(Of Models.OperationProgress), cancellationToken As CancellationToken) As Task
            Dim bytes As Byte() = System.Text.Encoding.UTF8.GetBytes(content)
            Return UploadFileBytesAsync(path, bytes, progress, cancellationToken)
        End Function

        Public Overrides Async Function UploadFileBytesAsync(path As String, data As Byte(), progress As IProgress(Of Models.OperationProgress), cancellationToken As CancellationToken) As Task
            If _httpClient Is Nothing Then
                Throw New InvalidOperationException("Not connected. Call ConnectAsync first.")
            End If

            Dim url As String = CombinePath(_baseUrl, path)
            ReportProgress(progress, $"Uploading {System.IO.Path.GetFileName(path)}...", 0)

            Using content As New ByteArrayContent(data)
                Using response As HttpResponseMessage = Await _httpClient.PutAsync(url, content, cancellationToken)
                    response.EnsureSuccessStatusCode()
                End Using
            End Using

            ReportProgress(progress, "Upload complete", 100)
        End Function

        Public Overrides Function DeleteAsync(path As String, cancellationToken As CancellationToken) As Task(Of Boolean)
            Throw New NotSupportedException("Delete operation is not supported for HTTP protocol.")
        End Function

        Public Overrides Function CreateDirectoryAsync(path As String, name As String, cancellationToken As CancellationToken) As Task(Of Boolean)
            Throw New NotSupportedException("Create directory operation is not supported for HTTP protocol.")
        End Function

        Public Overrides Function CreateFileAsync(path As String, content As String, cancellationToken As CancellationToken) As Task(Of Boolean)
            Throw New NotSupportedException("Create file operation is not supported for HTTP protocol.")
        End Function

        Public Overrides Function RenameAsync(path As String, newName As String, cancellationToken As CancellationToken) As Task(Of Boolean)
            Throw New NotSupportedException("Rename operation is not supported for HTTP protocol.")
        End Function

        Public Overrides Function GetFileInfoAsync(path As String, cancellationToken As CancellationToken) As Task(Of Models.FileItem)
            Throw New NotSupportedException("File info is not available for HTTP protocol.")
        End Function

        Public Overrides Function PathExists(path As String) As Boolean
            Return False ' Cannot check path existence in HTTP
        End Function

        Public Overrides Function GetHomeDirectory() As String
            Return _baseUrl
        End Function

        Private Function NormalizeUrl(host As String) As String
            If String.IsNullOrEmpty(host) Then Return ""

            If Not host.StartsWith("http://") AndAlso Not host.StartsWith("https://") Then
                host = "https://" & host
            End If

            ' Remove trailing slash
            Return host.TrimEnd("/"c)
        End Function

        Protected Overrides Function CheckConnection() As Boolean
            Return _httpClient IsNot Nothing
        End Function
    End Class
End Namespace
