'OperationProgress.vb - Model for progress reporting
Namespace vbnetbrowser.Models

    Public Class OperationProgress
        Public Property OperationType As OperationType
        Public Property CurrentBytes As Long
        Public Property TotalBytes As Long
        Public Property CurrentItem As String
        Public Property StatusMessage As String
        Public Property Percentage As Integer
        Public Property SpeedBytesPerSecond As Long
        Public Property TimeRemaining As TimeSpan?
        Public Property IsCancelled As Boolean
        Public Property HasError As Boolean
        Public Property ErrorMessage As String

        Public Sub New()
        End Sub

        Public Sub New(operationType As OperationType)
            Me.OperationType = operationType
        End Sub

        Public ReadOnly Property PercentageText As String
            Get
                If TotalBytes > 0 Then
                    Return $"{Percentage}%"
                End If
                Return ""
            End Get
        End Property

        Public ReadOnly Property SpeedFormatted As String
            Get
                If SpeedBytesPerSecond <= 0 Then Return ""
                Return $"{FileItem.FormatFileSize(SpeedBytesPerSecond)}/s"
            End Get
        End Property

        Public Shared Function CalculatePercentage(current As Long, total As Long) As Integer
            If total <= 0 Then Return 0
            Dim percent As Double = (current / CDbl(total)) * 100
            Return Math.Min(100, CInt(Math.Floor(percent)))
        End Function

        Public Shared Function CalculateTimeRemaining(bytesProcessed As Long, bytesPerSecond As Long, totalBytes As Long) As TimeSpan?
            If bytesPerSecond <= 0 Then Return Nothing
            Dim remainingBytes As Long = totalBytes - bytesProcessed
            If remainingBytes <= 0 Then Return TimeSpan.Zero
            Dim secondsRemaining As Double = remainingBytes / CDbl(bytesPerSecond)
            Return TimeSpan.FromSeconds(secondsRemaining)
        End Function
    End Class

    Public Enum OperationType
        Download
        Upload
        ListDirectory
        Delete
        Create
        Rename
        Connect
        Disconnect
    End Class

    Public Class OperationResult(Of T)
        Public Property Success As Boolean
        Public Property Data As T
        Public Property ErrorMessage As String
        Public Property Exception As Exception
        Public Property ElapsedTime As TimeSpan

        Public Sub New()
        End Sub

        Public Sub New(success As Boolean, data As T, Optional errorMsg As String = "")
            Me.Success = success
            Me.Data = data
            Me.ErrorMessage = errorMsg
        End Sub

        Public Shared Function SuccessResult(data As T) As OperationResult(Of T)
            Return New OperationResult(Of T)(True, data)
        End Function

        Public Shared Function FailureResult(errorMsg As String, Optional ex As Exception = Nothing) As OperationResult(Of T)
            Return New OperationResult(Of T)(False, Nothing, errorMsg) With {.Exception = ex}
        End Function
    End Class
End Namespace
