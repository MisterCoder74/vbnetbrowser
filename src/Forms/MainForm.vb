Imports System.Windows.Forms
Imports Microsoft.Web.WebView2.WinForms
Imports Microsoft.Web.WebView2.Core
Imports System.Windows.Forms.VisualStyles
Imports vbnetbrowser.Services
Imports vbnetbrowser.Models

Namespace vbnetbrowser.Forms

    Public Partial Class MainForm
        Inherits Form

        Private _configService As ConfigService
        Private _bookmarkService As BookmarkService
        Private _sidebarCollapsed As Boolean = False
        Private _sidebarAnimating As Boolean = False
        Private _sidebarWidth As Integer = 250
        Private _animationTimer As Timer

        Public Sub New()
            InitializeComponent()
            _configService = New ConfigService()
            _bookmarkService = New BookmarkService()
            InitializeBrowser()
            InitializeSidebar()
            LoadBookmarksList()
            AddNewTab()
        End Sub

        Private Sub InitializeBrowser()
            ' Load saved configuration
            Dim config As AppConfig = _configService.GetConfig()
            _sidebarWidth = config.SidebarWidth
            _sidebarCollapsed = config.SidebarCollapsed

            If _sidebarCollapsed Then
                CollapseSidebar(True)
            End If

            ' Setup animation timer
            _animationTimer = New Timer()
            _animationTimer.Interval = 10
            AddHandler _animationTimer.Tick, AddressOf AnimationTimer_Tick
        End Sub

        Private Sub InitializeSidebar()
            pnlSidebar.Width = _sidebarWidth
        End Sub

        Private Async Sub InitializeWebView2(webView As WebView2, url As String)
            Try
                Dim environment As CoreWebView2Environment = Await CoreWebView2Environment.CreateAsync()
                Await webView.EnsureCoreWebView2Async(environment)

                AddHandler webView.CoreWebView2.NavigationStarting, AddressOf WebView_NavigationStarting
                AddHandler webView.CoreWebView2.NavigationCompleted, AddressOf WebView_NavigationCompleted
                AddHandler webView.CoreWebView2.DocumentTitleChanged, AddressOf WebView_DocumentTitleChanged

                If String.IsNullOrWhiteSpace(url) Then
                    Dim config As AppConfig = _configService.GetConfig()
                    url = config.HomePage
                End If

                webView.CoreWebView2.Navigate(url)
            Catch ex As Exception
                MessageBox.Show(
                    "Failed to initialize WebView2: " & ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                )
            End Try
        End Sub

        Private Sub AddNewTab(Optional url As String = "", Optional title As String = "New Tab")
            Dim tabPage As New TabPage(title)
            tabPage.Tag = New TabInfo() With {
                .Url = url,
                .WebView = Nothing
            }

            Dim webView As New WebView2()
            webView.Dock = DockStyle.Fill
            tabPage.Controls.Add(webView)

            ' Store WebView reference
            Dim tabInfo As TabInfo = DirectCast(tabPage.Tag, TabInfo)
            tabInfo.WebView = webView

            ' Initialize WebView2
            InitializeWebView2(webView, url)

            tabControl.TabPages.Add(tabPage)
            tabControl.SelectedTab = tabPage

            AddCloseButtonToTab(tabPage)
        End Sub

        Private Sub AddCloseButtonToTab(tabPage As TabPage)
            ' Create custom tab with close button
            Dim rect As Rectangle = tabControl.GetTabRect(tabControl.TabPages.IndexOf(tabPage))
            Dim closeButton As New Button()
            closeButton.Text = "×"
            closeButton.Size = New Size(18, 18)
            closeButton.Location = New Point(rect.Right - 20, rect.Top + 3)
            closeButton.FlatStyle = FlatStyle.Flat
            closeButton.FlatAppearance.BorderSize = 0
            closeButton.Font = New Font("Segoe UI", 10F, FontStyle.Bold)
            closeButton.Tag = tabPage
            AddHandler closeButton.Click, AddressOf CloseButton_Click

            tabControl.Tag = tabControl.Tag & "|" & tabPage.GetHashCode().ToString()
        End Sub

        Private Sub CloseButton_Click(sender As Object, e As EventArgs)
            Dim closeButton As Button = DirectCast(sender, Button)
            Dim tabPage As TabPage = DirectCast(closeButton.Tag, TabPage)

            If tabControl.TabPages.Count > 1 Then
                tabControl.TabPages.Remove(tabPage)
            Else
                MessageBox.Show(
                    "Cannot close the last tab",
                    "Info",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                )
            End If
        End Sub

        Private Sub btnNewTab_Click(sender As Object, e As EventArgs) Handles btnNewTab.Click
            AddNewTab()
        End Sub

        Private Sub btnGo_Click(sender As Object, e As EventArgs) Handles btnGo.Click
            NavigateToUrl(txtAddress.Text.Trim())
        End Sub

        Private Sub txtAddress_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtAddress.KeyPress
            If e.KeyChar = Convert.ToChar(13) Then
                NavigateToUrl(txtAddress.Text.Trim())
            End If
        End Sub

        Private Sub NavigateToUrl(url As String)
            If String.IsNullOrWhiteSpace(url) Then Return

            If Not url.StartsWith("http://") AndAlso Not url.StartsWith("https://") Then
                ' Check if it looks like a URL
                If url.Contains(".") AndAlso Not url.Contains(" ") Then
                    url = "https://" & url
                Else
                    ' Treat as search query
                    Dim config As AppConfig = _configService.GetConfig()
                    url = config.DefaultSearchEngine & Uri.EscapeDataString(url)
                End If
            End If

            Dim currentWebView As WebView2 = GetActiveWebView()
            If currentWebView IsNot Nothing AndAlso currentWebView.CoreWebView2 IsNot Nothing Then
                currentWebView.CoreWebView2.Navigate(url)
            End If
        End Sub

        Private Sub btnBack_Click(sender As Object, e As EventArgs) Handles btnBack.Click
            Dim currentWebView As WebView2 = GetActiveWebView()
            If currentWebView IsNot Nothing AndAlso currentWebView.CoreWebView2 IsNot Nothing Then
                If currentWebView.CoreWebView2.CanGoBack Then
                    currentWebView.CoreWebView2.GoBack()
                End If
            End If
        End Sub

        Private Sub btnForward_Click(sender As Object, e As EventArgs) Handles btnForward.Click
            Dim currentWebView As WebView2 = GetActiveWebView()
            If currentWebView IsNot Nothing AndAlso currentWebView.CoreWebView2 IsNot Nothing Then
                If currentWebView.CoreWebView2.CanGoForward Then
                    currentWebView.CoreWebView2.GoForward()
                End If
            End If
        End Sub

        Private Sub btnRefresh_Click(sender As Object, e As EventArgs) Handles btnRefresh.Click
            Dim currentWebView As WebView2 = GetActiveWebView()
            If currentWebView IsNot Nothing AndAlso currentWebView.CoreWebView2 IsNot Nothing Then
                currentWebView.CoreWebView2.Reload()
            End If
        End Sub

        Private Sub btnHome_Click(sender As Object, e As EventArgs) Handles btnHome.Click
            Dim config As AppConfig = _configService.GetConfig()
            Dim currentWebView As WebView2 = GetActiveWebView()
            If currentWebView IsNot Nothing AndAlso currentWebView.CoreWebView2 IsNot Nothing Then
                currentWebView.CoreWebView2.Navigate(config.HomePage)
            End If
        End Sub

        Private Sub btnBookmark_Click(sender As Object, e As EventArgs) Handles btnBookmark.Click
            Dim currentWebView As WebView2 = GetActiveWebView()
            If currentWebView IsNot Nothing AndAlso currentWebView.CoreWebView2 IsNot Nothing Then
                Dim title As String = currentWebView.CoreWebView2.DocumentTitle
                Dim url As String = currentWebView.CoreWebView2.Source

                If _bookmarkService.AddBookmark(title, url) Then
                    LoadBookmarksList()
                    MessageBox.Show(
                        "Bookmark added successfully!",
                        "Success",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    )
                Else
                    MessageBox.Show(
                        "Failed to add bookmark.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    )
                End If
            End If
        End Sub

        Private Sub btnToggleSidebar_Click(sender As Object, e As EventArgs) Handles btnToggleSidebar.Click
            If _sidebarCollapsed Then
                ExpandSidebar()
            Else
                CollapseSidebar()
            End If
        End Sub

        Private Sub CollapseSidebar(Optional immediate As Boolean = False)
            If _sidebarAnimating AndAlso Not immediate Then Return

            If immediate Then
                pnlSidebar.Width = 30
                btnToggleSidebar.Text = "»"
                lstBookmarks.Visible = False
                lblBookmarks.Visible = False
                btnSettingsInSidebar.Visible = False
                _sidebarCollapsed = True
                _configService.UpdateSidebarCollapsed(True)
                Return
            End If

            _sidebarAnimating = True
            Dim targetWidth As Integer = 30
            _animationTimer.Start()
        End Sub

        Private Sub ExpandSidebar()
            If _sidebarAnimating Then Return

            _sidebarAnimating = True
            Dim targetWidth As Integer = _sidebarWidth
            _animationTimer.Start()
        End Sub

        Private Sub AnimationTimer_Tick(sender As Object, e As EventArgs)
            If _sidebarCollapsed Then
                pnlSidebar.Width += 10
                If pnlSidebar.Width >= _sidebarWidth Then
                    pnlSidebar.Width = _sidebarWidth
                    lstBookmarks.Visible = True
                    lblBookmarks.Visible = True
                    btnSettingsInSidebar.Visible = True
                    btnToggleSidebar.Text = "«"
                    _sidebarCollapsed = False
                    _animationTimer.Stop()
                    _sidebarAnimating = False
                    _configService.UpdateSidebarCollapsed(False)
                End If
            Else
                pnlSidebar.Width -= 10
                If pnlSidebar.Width <= 30 Then
                    pnlSidebar.Width = 30
                    lstBookmarks.Visible = False
                    lblBookmarks.Visible = False
                    btnSettingsInSidebar.Visible = False
                    btnToggleSidebar.Text = "»"
                    _sidebarCollapsed = True
                    _animationTimer.Stop()
                    _sidebarAnimating = False
                    _configService.UpdateSidebarCollapsed(True)
                End If
            End If
        End Sub

        Private Sub lstBookmarks_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles lstBookmarks.MouseDoubleClick
            If lstBookmarks.SelectedItem IsNot Nothing Then
                Dim bookmark As Bookmark = DirectCast(lstBookmarks.SelectedItem, Bookmark)
                NavigateToUrl(bookmark.Url)
            End If
        End Sub

        Private Sub btnDeleteBookmark_Click(sender As Object, e As EventArgs) Handles btnDeleteBookmark.Click
            If lstBookmarks.SelectedItem IsNot Nothing Then
                Dim bookmark As Bookmark = DirectCast(lstBookmarks.SelectedItem, Bookmark)
                If MessageBox.Show(
                    "Delete this bookmark?",
                    "Confirm",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                ) = DialogResult.Yes Then
                    If _bookmarkService.DeleteBookmark(bookmark.Id) Then
                        LoadBookmarksList()
                    End If
                End If
            End If
        End Sub

        Private Sub btnSettingsInSidebar_Click(sender As Object, e As EventArgs) Handles btnSettingsInSidebar.Click
            Dim settingsForm As New SettingsForm(_configService)
            If settingsForm.ShowDialog() = DialogResult.OK Then
                ' Reload configuration
                Dim config As AppConfig = _configService.GetConfig()
                _sidebarWidth = config.SidebarWidth
                If Not _sidebarCollapsed Then
                    pnlSidebar.Width = _sidebarWidth
                End If
            End If
        End Sub

        Private Sub LoadBookmarksList()
            lstBookmarks.Items.Clear()
            Dim bookmarks As List(Of Bookmark) = _bookmarkService.GetBookmarks()
            For Each bookmark As Bookmark In bookmarks
                lstBookmarks.Items.Add(bookmark)
            Next
        End Sub

        Private Sub tabControl_SelectedIndexChanged(sender As Object, e As EventArgs) Handles tabControl.SelectedIndexChanged
            Dim webView As WebView2 = GetActiveWebView()
            If webView IsNot Nothing AndAlso webView.CoreWebView2 IsNot Nothing Then
                txtAddress.Text = webView.CoreWebView2.Source
            End If
        End Sub

        Private Sub WebView_NavigationStarting(sender As Object, e As CoreWebView2NavigationStartingEventArgs)
            txtAddress.Text = e.Uri
        End Sub

        Private Sub WebView_NavigationCompleted(sender As Object, e As CoreWebView2NavigationCompletedEventArgs)
            UpdateNavigationButtons()
        End Sub

        Private Sub WebView_DocumentTitleChanged(sender As Object, e As Object)
            Dim webView As WebView2 = DirectCast(sender, WebView2)
            For Each tabPage As TabPage In tabControl.TabPages
                Dim tabInfo As TabInfo = DirectCast(tabPage.Tag, TabInfo)
                If tabInfo.WebView Is webView Then
                    tabPage.Text = If(webView.CoreWebView2.DocumentTitle, "New Tab")
                    Exit For
                End If
            Next

            If webView Is GetActiveWebView() Then
                txtAddress.Text = webView.CoreWebView2.Source
            End If
        End Sub

        Private Sub UpdateNavigationButtons()
            Dim webView As WebView2 = GetActiveWebView()
            If webView IsNot Nothing AndAlso webView.CoreWebView2 IsNot Nothing Then
                btnBack.Enabled = webView.CoreWebView2.CanGoBack
                btnForward.Enabled = webView.CoreWebView2.CanGoForward
            End If
        End Sub

        Private Function GetActiveWebView() As WebView2
            If tabControl.SelectedTab IsNot Nothing Then
                Dim tabInfo As TabInfo = DirectCast(tabControl.SelectedTab.Tag, TabInfo)
                Return tabInfo.WebView
            End If
            Return Nothing
        End Function

        Private Sub MainForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
            If _animationTimer IsNot Nothing Then
                _animationTimer.Stop()
                _animationTimer.Dispose()
            End If
        End Sub

        Private Sub MainForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
            txtAddress.Focus()
        End Sub
    End Class

    Public Class TabInfo
        Public Property Url As String
        Public Property WebView As WebView2
    End Class

End Namespace
