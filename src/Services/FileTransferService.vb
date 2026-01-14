'FileTransferService.vb - Abstract base class for all file transfer protocols
Imports System.Threading
Imports System.Threading.Tasks

Namespace vbnetbrowser.Services

    Public MustInherit Class FileTransferService
        Protected _profile As Models.ConnectionProfile
        Protected _isConnected As Boolean = False
        Protected _connectionTime As DateTime
        Protected ReadOnly _timeout As TimeSpan = TimeSpan.FromSeconds(60)

        Public Event ConnectionStateChanged As EventHandler(Of Models.ConnectionProfile)
        Public Event OperationProgressChanged As EventHandler(Of Models.OperationProgress)
        Public Event OperationCompleted As EventHandler(Of Models.OperationResult(Of String))

        Public Overridable ReadOnly Property DisplayName As String
            Get
                Return _profile?.Name ?? "Unknown"
            End Get
        End Property

        Public ReadOnly Property IsConnected As Boolean
            Get
                Return _isConnected AndAlso CheckConnection()
            End Get
        End Property

        Public ReadOnly Property ConnectionTime As DateTime
            Get
                Return _connectionTime
            End Get
        End Property

        Public ReadOnly Property ElapsedTime As TimeSpan
            Get
                Return DateTime.UtcNow - _connectionTime
            End Get
        End Property

        Protected Overridable Function CheckConnection() As Boolean
            Return True
        End Function

        Public MustOverride Function ConnectAsync(profile As Models.ConnectionProfile, cancellationToken As CancellationToken) As Task(Of Boolean)
        Public MustOverride Sub Disconnect()
        Public MustOverride Function ListDirectoryAsync(path As String, progress As IProgress(Of Models.OperationProgress), cancellationToken As CancellationToken) As Task(Of List(Of Models.FileItem))
        Public MustOverride Function DownloadFileAsync(path As String, progress As IProgress(Of Models.OperationProgress), cancellationToken As CancellationToken) As Task(Of String)
        Public MustOverride Function DownloadFileBytesAsync(path As String, progress As IProgress(Of Models.OperationProgress), cancellationToken As CancellationToken) As Task(Of Byte())
        Public MustOverride Function UploadFileAsync(path As String, content As String, progress As IProgress(Of Models.OperationProgress), cancellationToken As CancellationToken) As Task
        Public MustOverride Function UploadFileBytesAsync(path As String, data As Byte(), progress As IProgress(Of Models.OperationProgress), cancellationToken As CancellationToken) As Task
        Public MustOverride Function DeleteAsync(path As String, cancellationToken As CancellationToken) As Task(Of Boolean)
        Public MustOverride Function CreateDirectoryAsync(path As String, name As String, cancellationToken As CancellationToken) As Task(Of Boolean)
        Public MustOverride Function CreateFileAsync(path As String, content As String, cancellationToken As CancellationToken) As Task(Of Boolean)
        Public MustOverride Function RenameAsync(path As String, newName As String, cancellationToken As CancellationToken) As Task(Of Boolean)
        Public MustOverride Function GetFileInfoAsync(path As String, cancellationToken As CancellationToken) As Task(Of Models.FileItem)
        Public MustOverride Function PathExists(path As String) As Boolean

        Protected Sub OnConnectionStateChanged(profile As Models.ConnectionProfile)
            _isConnected = CheckConnection()
            RaiseEvent ConnectionStateChanged(Me, profile)
        End Sub

        Protected Sub OnProgressChanged(progress As Models.OperationProgress)
            RaiseEvent OperationProgressChanged(Me, progress)
        End Sub

        Protected Sub OnOperationCompleted(result As Models.OperationResult(Of String))
            RaiseEvent OperationCompleted(Me, result)
        End Sub

        Protected Function CreateProgressReporter(Of T)(operationType As Models.OperationType) As IProgress(Of Models.OperationProgress)
            Return New Progress(Of Models.OperationProgress)(Sub(p)
                p.OperationType = operationType
                OnProgressChanged(p)
            End Sub)
        End Function

        Protected Sub UpdateProgress(progress As IProgress(Of Models.OperationProgress), current As Long, total As Long, currentItem As String)
            If progress Is Nothing Then Return

            Dim report As New Models.OperationProgress(operationType:=Models.OperationType.Download)
            report.CurrentBytes = current
            report.TotalBytes = total
            report.CurrentItem = currentItem
            report.Percentage = Models.OperationProgress.CalculatePercentage(current, total)
            progress.Report(report)
        End Sub

        Protected Sub ReportProgress(progress As IProgress(Of Models.OperationProgress), status As String, Optional percentage As Integer = 0)
            If progress Is Nothing Then Return

            Dim report As New Models.OperationProgress() With {
                .StatusMessage = status,
                .Percentage = percentage
            }
            progress.Report(report)
        End Sub

        Public Overridable Function GetHomeDirectory() As String
            Return "/"
        End Function

        Public Overridable Function CombinePath(path1 As String, path2 As String) As String
            If String.IsNullOrEmpty(path1) Then Return path2
            If String.IsNullOrEmpty(path2) Then Return path1

            Dim separator As Char = If(_profile?.Protocol = Models.ProtocolType.Local, System.IO.Path.DirectorySeparatorChar, "/"c)

            If path1.EndsWith(separator.ToString()) Then
                Return path1 & path2
            Else
                Return path1 & separator & path2
            End If
        End Function

        Public Overridable Function NormalizePath(path As String) As String
            If String.IsNullOrEmpty(path) Then Return GetHomeDirectory()

            Dim separator As Char = If(_profile?.Protocol = Models.ProtocolType.Local, System.IO.Path.DirectorySeparatorChar, "/"c)
            Dim otherSeparator As Char = If(separator = "/"c, "\"c, "/"c)

            path = path.Replace(otherSeparator, separator)

            If path = separator.ToString() Then Return path

            Return path.TrimEnd(separator)
        End Function

        Protected Function HandleException(ex As Exception, operationName As String) As Models.OperationResult(Of String)
            Dim errorMsg As String

            Select Case ex
                Case Is Threading.ThreadAbortException
                    errorMsg = $"{operationName} was cancelled."
                Case Is TimeoutException
                    errorMsg = $"{operationName} timed out. The server did not respond in time."
                Case Is System.Net.WebException
                    errorMsg = $"Network error during {operationName}: {ex.Message}"
                Case Is UnauthorizedAccessException
                    errorMsg = $"Access denied. You don't have permission to perform this operation."
                Case Is System.IO.DirectoryNotFoundException, Is System.IO.FileNotFoundException
                    errorMsg = $"The specified path was not found: {ex.Message}"
                Case Is System.IO.IOException
                    errorMsg = $"I/O error: {ex.Message}"
                Case Is ArgumentException
                    errorMsg = $"Invalid argument: {ex.Message}"
                Case Else
                    errorMsg = $"An unexpected error occurred during {operationName}: {ex.Message}"
            End Select

            Return Models.OperationResult(Of String).FailureResult(errorMsg, ex)
        End Function
    End Class
End Namespace
