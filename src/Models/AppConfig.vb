Namespace vbnetbrowser.Models

    Public Class AppConfig
        Public Property HomePage As String = "https://www.bing.com"
        Public Property DefaultSearchEngine As String = "https://www.bing.com/search?q="
        Public Property RememberSession As Boolean = True
        Public Property SidebarWidth As Integer = 250
        Public Property SidebarCollapsed As Boolean = False
        Public Property Theme As String = "Light"
        Public Property EnableHistory As Boolean = True
        Public Property EnableBookmarks As Boolean = True
    End Class

End Namespace
