Namespace vbnetbrowser.Models

    Public Class Bookmark
        Public Property Id As String
        Public Property Title As String
        Public Property Url As String
        Public Property CreatedAt As DateTime

        Public Sub New()
            Id = Guid.NewGuid().ToString()
            CreatedAt = DateTime.Now
        End Sub

        Public Sub New(title As String, url As String)
            Me.New()
            Me.Title = title
            Me.Url = url
        End Sub
    End Class

End Namespace
