'FileItem.vb - Model for file/folder items
Namespace vbnetbrowser.Models

    Public Class FileItem
        Public Property Name As String
        Public Property FullPath As String
        Public Property IsDirectory As Boolean
        Public Property Size As Long
        Public Property LastModified As DateTime?
        Public Property Permissions As String
        Public Property Owner As String
        Public Property Group As String
        Public Property IconIndex As Integer
        Public Property Protocol As ProtocolType
        Public Property ChildrenLoaded As Boolean
        Public Property Children As New List(Of FileItem)

        Public Sub New()
        End Sub

        Public Sub New(name As String, isDirectory As Boolean, Optional protocol As ProtocolType = ProtocolType.Local)
            Me.Name = name
            Me.IsDirectory = isDirectory
            Me.Protocol = protocol
            Me.FullPath = name
        End Sub

        Public ReadOnly Property Extension As String
            Get
                If IsDirectory Then Return ""
                Dim ext As String = System.IO.Path.GetExtension(Name)
                Return If(ext Is Nothing, "", ext.ToLowerInvariant())
            End Get
        End Property

        Public ReadOnly Property SizeFormatted As String
            Get
                If IsDirectory Then Return ""
                Return FormatFileSize(Size)
            End Get
        End Property

        Public Shared Function FormatFileSize(bytes As Long) As String
            If bytes < 1024 Then Return $"{bytes} B"
            If bytes < 1024 * 1024 Then Return $"{bytes / 1024:F1} KB"
            If bytes < 1024 * 1024 * 1024 Then Return $"{bytes / (1024 * 1024):F1} MB"
            Return $"{bytes / (1024 * 1024 * 1024):F1} GB"
        End Function

        Public ReadOnly Property IsTextFile As Boolean
            Get
                Dim textExtensions = {".txt", ".html", ".htm", ".css", ".js", ".json", ".xml", ".vb", ".cs", ".py", ".md", ".yaml", ".yml", ".ini", ".config", ".log"}
                Return textExtensions.Contains(Extension)
            End Get
        End Property

        Public ReadOnly Property IconKey As String
            Get
                If IsDirectory Then Return "folder"
                Select Case Extension
                    Case ".html", ".htm" : Return "html"
                    Case ".css" : Return "css"
                    Case ".js", ".ts" : Return "js"
                    Case ".json" : Return "json"
                    Case ".xml" : Return "xml"
                    Case ".txt", ".md", ".log" : Return "text"
                    Case ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".ico" : Return "image"
                    Case ".zip", ".rar", ".7z", ".tar", ".gz" : Return "archive"
                    Case ".exe", ".dll" : Return "binary"
                    Case ".pdf" : Return "pdf"
                    Case Else : Return "file"
                End Select
            End Get
        End Property
    End Class

    Public Class FileItemCollection
        Inherits List(Of FileItem)

        Public Function FindByPath(path As String) As FileItem
            Return Me.FirstOrDefault(Function(f) f.FullPath = path)
        End Function

        Public Function FindByName(name As String) As FileItem
            Return Me.FirstOrDefault(Function(f) f.Name = name)
        End Function
    End Class
End Namespace
