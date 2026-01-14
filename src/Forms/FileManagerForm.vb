'FileManagerForm.vb - Modern file manager with SSH/SFTP support and async operations
Imports System.IO
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms

Namespace vbnetbrowser.Forms

    Public Class FileManagerForm
        Inherits Form

        Private ReadOnly _connectionManager As ConnectionManager
        Private ReadOnly _editorService As EditorService
        Private _currentPath As String = "/"
        Private _cancellationTokenSource As CancellationTokenSource
        Private ReadOnly _recentFiles As New List(Of String)()
        Private Const MaxRecentFiles As Integer = 10

        ' Controls
        Private WithEvents splitContainer As SplitContainer
        Private WithEvents pnlQuickAccess As Panel
        Private WithEvents cmbConnection As ComboBox
        Private WithEvents btnConnect As Button
        Private WithEvents btnNewConnection As Button
        Private WithEvents btnDisconnect As Button
        Private WithEvents btnRefresh As Button
        Private WithEvents btnHome As Button
        Private WithEvents btnUp As Button
        Private WithEvents txtPath As TextBox
        Private WithEvents treeFiles As TreeView
        Private WithEvents pnlEditor As Panel
        Private WithEvents rtbEditor As RichTextBox
        Private WithEvents pnlLineNumbers As Panel
        Private WithEvents lblLineNumbers As Label
        Private WithEvents statusStrip As StatusStrip
        Private WithEvents lblStatus As ToolStripStatusLabel
        Private WithEvents lblProgress As ToolStripProgressBar
        Private WithEvents lblConnectionStatus As ToolStripStatusLabel
        Private WithEvents pnlToolbar As Panel
        Private WithEvents btnSave As Button
        Private WithEvents btnUndo As Button
        Private WithEvents btnRedo As Button
        Private WithEvents btnCut As Button
        Private WithEvents btnCopy As Button
        Private WithEvents btnPaste As Button
        Private WithEvents btnFind As Button
        Private WithEvents btnReplace As Button
        Private WithEvents cmbEncoding As ComboBox
        Private WithEvents cmbLineEndings As ComboBox
        Private WithEvents cmsFileTree As ContextMenuStrip
        Private WithEvents mnuOpen As ToolStripMenuItem
        Private WithEvents mnuEdit As ToolStripMenuItem
        Private WithEvents mnuDelete As ToolStripMenuItem
        Private WithEvents mnuRename As ToolStripMenuItem
        Private WithEvents mnuRefresh As ToolStripMenuItem
        Private WithEvents cmsEditor As ContextMenuStrip
        Private WithEvents mnuEditorUndo As ToolStripMenuItem
        Private WithEvents mnuEditorRedo As ToolStripMenuItem
        Private WithEvents toolStripSeparator1 As ToolStripSeparator
        Private WithEvents mnuEditorCut As ToolStripMenuItem
        Private WithEvents mnuEditorCopy As ToolStripMenuItem
        Private WithEvents mnuEditorPaste As ToolStripMenuItem
        Private WithEvents toolStripSeparator2 As ToolStripSeparator
        Private WithEvents mnuEditorDelete As ToolStripMenuItem
        Private WithEvents mnuEditorSelectAll As ToolStripMenuItem
        Private WithEvents btnCancel As Button
        Private WithEvents pnlRecent As Panel
        Private WithEvents lstRecentFiles As ListBox

        Public Sub New()
            InitializeComponent()
            _connectionManager = New ConnectionManager()
            _editorService = New EditorService(_connectionManager)
            InitializeFileManager()
        End Sub

        Private Sub InitializeComponent()
            Me.SuspendLayout()

            ' Form
            Me.Text = "File Manager"
            Me.Name = "FileManagerForm"
            Me.Size = New Size(1200, 800)
            Me.StartPosition = FormStartPosition.CenterParent
            Me.WindowState = FormWindowState.Maximized

            ' splitContainer
            splitContainer = New SplitContainer() With {
                .Dock = DockStyle.Fill,
                .BorderStyle = BorderStyle.None,
                .SplitterWidth = 5,
                .Panel1MinSize = 200,
                .Panel2MinSize = 300
            }

            ' pnlQuickAccess
            pnlQuickAccess = New Panel() With {
                .Dock = DockStyle.Top,
                .Height = 40,
                .BackColor = Color.FromArgb(45, 45, 48)
            }

            ' cmbConnection
            cmbConnection = New ComboBox() With {
                .Location = New Point(10, 8),
                .Width = 200,
                .DropDownStyle = ComboBoxStyle.DropDownList,
                .FlatStyle = FlatStyle.System
            }

            ' btnConnect
            btnConnect = New Button() With {
                .Location = New Point(220, 7),
                .Text = "Connect",
                .Width = 80,
                .Height = 26,
                .FlatStyle = FlatStyle.System
            }

            ' btnNewConnection
            btnNewConnection = New Button() With {
                .Location = New Point(310, 7),
                .Text = "New",
                .Width = 60,
                .Height = 26,
                .FlatStyle = FlatStyle.System
            }

            ' btnDisconnect
            btnDisconnect = New Button() With {
                .Location = New Point(380, 7),
                .Text = "Disconnect",
                .Width = 90,
                .Height = 26,
                .FlatStyle = FlatStyle.System,
                .Enabled = False
            }

            ' btnHome
            btnHome = New Button() With {
                .Location = New Point(480, 7),
                .Text = "🏠",
                .Width = 30,
                .Height = 26,
                .FlatStyle = FlatStyle.System
            }

            ' btnUp
            btnUp = New Button() With {
                .Location = New Point(520, 7),
                .Text = "⬆",
                .Width = 30,
                .Height = 26,
                .FlatStyle = FlatStyle.System
            }

            ' btnRefresh
            btnRefresh = New Button() With {
                .Location = New Point(560, 7),
                .Text = "↻",
                .Width = 30,
                .Height = 26,
                .FlatStyle = FlatStyle.System
            }

            ' btnCancel
            btnCancel = New Button() With {
                .Location = New Point(600, 7),
                .Text = "✕",
                .Width = 30,
                .Height = 26,
                .FlatStyle = FlatStyle.System,
                .Enabled = False
            }

            ' txtPath
            txtPath = New TextBox() With {
                .Location = New Point(640, 9),
                .Width = 400,
                .Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
            }

            pnlQuickAccess.Controls.AddRange({
                cmbConnection, btnConnect, btnNewConnection, btnDisconnect,
                btnHome, btnUp, btnRefresh, btnCancel, txtPath
            })

            ' treeFiles
            treeFiles = New TreeView() With {
                .Dock = DockStyle.Fill,
                .BorderStyle = BorderStyle.None,
                .BackColor = Color.FromArgb(30, 30, 30),
                .ForeColor = Color.White,
                .Font = New Font("Segoe UI", 9),
                .ShowLines = True,
                .ShowPlusMinus = True
            })

            ' Context menu for treeFiles
            cmsFileTree = New ContextMenuStrip()
            mnuOpen = New ToolStripMenuItem("Open")
            mnuEdit = New ToolStripMenuItem("Edit")
            mnuDelete = New ToolStripMenuItem("Delete")
            mnuRename = New ToolStripMenuItem("Rename")
            mnuRefresh = New ToolStripMenuItem("Refresh")
            cmsFileTree.Items.AddRange({mnuOpen, mnuEdit, mnuDelete, mnuRename, New ToolStripSeparator(), mnuRefresh})
            treeFiles.ContextMenuStrip = cmsFileTree

            ' pnlRecent
            pnlRecent = New Panel() With {
                .Dock = DockStyle.Bottom,
                .Height = 120,
                .BackColor = Color.FromArgb(40, 40, 40)
            }

            lstRecentFiles = New ListBox() With {
                .Dock = DockStyle.Fill,
                .BackColor = Color.FromArgb(40, 40, 40),
                .ForeColor = Color.White,
                .BorderStyle = BorderStyle.None,
                .Font = New Font("Segoe UI", 9)
            }

            pnlRecent.Controls.Add(lstRecentFiles)

            ' Panel1 of splitContainer
            splitContainer.Panel1.Controls.Add(pnlQuickAccess)
            splitContainer.Panel1.Controls.Add(treeFiles)
            splitContainer.Panel1.Controls.Add(pnlRecent)

            ' pnlToolbar
            pnlToolbar = New Panel() With {
                .Dock = DockStyle.Top,
                .Height = 35,
                .BackColor = Color.FromArgb(50, 50, 50)
            }

            btnSave = New Button() With {
                .Location = New Point(10, 5),
                .Text = "💾 Save",
                .Width = 70,
                .Height = 25,
                .FlatStyle = FlatStyle.System,
                .ForeColor = Color.White,
                .Enabled = False
            }

            btnUndo = New Button() With {
                .Location = New Point(90, 5),
                .Text = "↶ Undo",
                .Width = 70,
                .Height = 25,
                .FlatStyle = FlatStyle.System,
                .ForeColor = Color.White,
                .Enabled = False
            }

            btnRedo = New Button() With {
                .Location = New Point(170, 5),
                .Text = "↷ Redo",
                .Width = 70,
                .Height = 25,
                .FlatStyle = FlatStyle.System,
                .ForeColor = Color.White,
                .Enabled = False
            }

            btnCut = New Button() With {
                .Location = New Point(250, 5),
                .Text = "✂ Cut",
                .Width = 60,
                .Height = 25,
                .FlatStyle = FlatStyle.System,
                .ForeColor = Color.White
            }

            btnCopy = New Button() With {
                .Location = New Point(320, 5),
                .Text = "📋 Copy",
                .Width = 70,
                .Height = 25,
                .FlatStyle = FlatStyle.System,
                .ForeColor = Color.White
            }

            btnPaste = New Button() With {
                .Location = New Point(400, 5),
                .Text = "📄 Paste",
                .Width = 70,
                .Height = 25,
                .FlatStyle = FlatStyle.System,
                .ForeColor = Color.White
            }

            btnFind = New Button() With {
                .Location = New Point(480, 5),
                .Text = "🔍 Find",
                .Width = 70,
                .Height = 25,
                .FlatStyle = FlatStyle.System,
                .ForeColor = Color.White
            }

            btnReplace = New Button() With {
                .Location = New Point(560, 5),
                .Text = "🔄 Replace",
                .Width = 80,
                .Height = 25,
                .FlatStyle = FlatStyle.System,
                .ForeColor = Color.White
            }

            cmbEncoding = New ComboBox() With {
                .Location = New Point(650, 7),
                .Width = 100,
                .FlatStyle = FlatStyle.System
            }
            cmbEncoding.Items.AddRange({"UTF-8", "UTF-16", "ASCII", "Auto"})

            cmbLineEndings = New ComboBox() With {
                .Location = New Point(760, 7),
                .Width = 100,
                .FlatStyle = FlatStyle.System
            }
            cmbLineEndings.Items.AddRange({"Windows (CRLF)", "Unix (LF)", "Mac (CR)"})
            cmbLineEndings.SelectedIndex = 0

            pnlToolbar.Controls.AddRange({
                btnSave, btnUndo, btnRedo, btnCut, btnCopy, btnPaste,
                btnFind, btnReplace, cmbEncoding, cmbLineEndings
            })

            ' pnlEditor
            pnlEditor = New Panel() With {
                .Dock = DockStyle.Fill,
                .BackColor = Color.FromArgb(30, 30, 30)
            }

            ' pnlLineNumbers
            pnlLineNumbers = New Panel() With {
                .Dock = DockStyle.Left,
                .Width = 50,
                .BackColor = Color.FromArgb(40, 40, 40)
            }

            lblLineNumbers = New Label() With {
                .Dock = DockStyle.Fill,
                .ForeColor = Color.Gray,
                .Font = New Font("Consolas", 10),
                .TextAlign = ContentAlignment.TopRight,
                .Padding = New Padding(0, 5, 5, 0)
            }

            pnlLineNumbers.Controls.Add(lblLineNumbers)

            ' rtbEditor
            rtbEditor = New RichTextBox() With {
                .Dock = DockStyle.Fill,
                .BackColor = Color.FromArgb(30, 30, 30),
                .ForeColor = Color.White,
                .Font = New Font("Consolas", 11),
                .BorderStyle = BorderStyle.None,
                .Multiline = True,
                .ScrollBars = RichTextBoxScrollBars.Both,
                .WordWrap = False,
                .DetectUrls = False
            }

            ' Context menu for editor
            cmsEditor = New ContextMenuStrip()
            mnuEditorUndo = New ToolStripMenuItem("Undo")
            mnuEditorRedo = New ToolStripMenuItem("Redo")
            toolStripSeparator1 = New ToolStripSeparator()
            mnuEditorCut = New ToolStripMenuItem("Cut")
            mnuEditorCopy = New ToolStripMenuItem("Copy")
            mnuEditorPaste = New ToolStripMenuItem("Paste")
            toolStripSeparator2 = New ToolStripSeparator()
            mnuEditorDelete = New ToolStripMenuItem("Delete")
            mnuEditorSelectAll = New ToolStripMenuItem("Select All")
            cmsEditor.Items.AddRange({
                mnuEditorUndo, mnuEditorRedo, toolStripSeparator1,
                mnuEditorCut, mnuEditorCopy, mnuEditorPaste,
                toolStripSeparator2, mnuEditorDelete, mnuEditorSelectAll
            })
            rtbEditor.ContextMenuStrip = cmsEditor

            pnlEditor.Controls.Add(rtbEditor)
            pnlEditor.Controls.Add(pnlLineNumbers)

            ' statusStrip
            statusStrip = New StatusStrip() With {
                .BackColor = Color.FromArgb(45, 45, 48),
                .ForeColor = Color.White,
                .Dock = DockStyle.Bottom,
                .Height = 25
            }

            lblStatus = New ToolStripStatusLabel("Ready") With {
                .ForeColor = Color.White
            }

            lblProgress = New ToolStripProgressBar() With {
                .Width = 200,
                .Visible = False
            }

            lblConnectionStatus = New ToolStripStatusLabel("Disconnected") With {
                .ForeColor = Color.Red,
                .Alignment = ToolStripItemAlignment.Right
            }

            statusStrip.Items.AddRange({lblStatus, lblProgress, lblConnectionStatus})

            ' Panel2 of splitContainer
            splitContainer.Panel2.Controls.Add(pnlToolbar)
            splitContainer.Panel2.Controls.Add(pnlEditor)
            splitContainer.Panel2.Controls.Add(statusStrip)

            ' Main controls
            Me.Controls.Add(splitContainer)

            AddHandlers()
            Me.ResumeLayout(True)
        End Sub

        Private Sub AddHandlers()
            AddHandler btnConnect.Click, AddressOf BtnConnect_Click
            AddHandler btnNewConnection.Click, AddressOf BtnNewConnection_Click
            AddHandler btnDisconnect.Click, AddressOf BtnDisconnect_Click
            AddHandler btnRefresh.Click, AddressOf BtnRefresh_Click
            AddHandler btnHome.Click, AddressOf BtnHome_Click
            AddHandler btnUp.Click, AddressOf BtnUp_Click
            AddHandler btnCancel.Click, AddressOf BtnCancel_Click
            AddHandler btnSave.Click, AddressOf BtnSave_Click
            AddHandler btnUndo.Click, AddressOf BtnUndo_Click
            AddHandler btnRedo.Click, AddressOf BtnRedo_Click
            AddHandler btnCut.Click, AddressOf BtnCut_Click
            AddHandler btnCopy.Click, AddressOf BtnCopy_Click
            AddHandler btnPaste.Click, AddressOf BtnPaste_Click
            AddHandler btnFind.Click, AddressOf BtnFind_Click
            AddHandler btnReplace.Click, AddressOf BtnReplace_Click
            AddHandler txtPath.KeyPress, AddressOf TxtPath_KeyPress
            AddHandler treeFiles.AfterSelect, AddressOf TreeFiles_AfterSelect
            AddHandler treeFiles.NodeMouseClick, AddressOf TreeFiles_NodeMouseClick
            AddHandler treeFiles.BeforeExpand, AddressOf TreeFiles_BeforeExpand
            AddHandler rtbEditor.TextChanged, AddressOf RtbEditor_TextChanged
            AddHandler rtbEditor.SelectionChanged, AddressOf RtbEditor_SelectionChanged
            AddHandler rtbEditor.KeyDown, AddressOf RtbEditor_KeyDown
            AddHandler lstRecentFiles.MouseDoubleClick, AddressOf LstRecentFiles_MouseDoubleClick
            AddHandler mnuOpen.Click, AddressOf MnuOpen_Click
            AddHandler mnuEdit.Click, AddressOf MnuEdit_Click
            AddHandler mnuDelete.Click, AddressOf MnuDelete_Click
            AddHandler mnuRename.Click, AddressOf MnuRename_Click
            AddHandler mnuRefresh.Click, AddressOf MnuRefresh_Click
            AddHandler mnuEditorUndo.Click, AddressOf MnuEditorUndo_Click
            AddHandler mnuEditorRedo.Click, AddressOf MnuEditorRedo_Click
            AddHandler mnuEditorCut.Click, AddressOf MnuEditorCut_Click
            AddHandler mnuEditorCopy.Click, AddressOf MnuEditorCopy_Click
            AddHandler mnuEditorPaste.Click, AddressOf MnuEditorPaste_Click
            AddHandler mnuEditorDelete.Click, AddressOf MnuEditorDelete_Click
            AddHandler mnuEditorSelectAll.Click, AddressOf MnuEditorSelectAll_Click
            AddHandler _connectionManager.ProfilesChanged, AddressOf ConnectionManager_ProfilesChanged
            AddHandler _connectionManager.ConnectionChanged, AddressOf ConnectionManager_ConnectionChanged
            AddHandler _connectionManager.SettingsChanged, AddressOf ConnectionManager_SettingsChanged
        End Sub

        Private Sub InitializeFileManager()
            LoadConnectionProfiles()
            UpdateConnectionStatus(False)
            UpdateLineNumbers()

            ' Set default theme
            ApplyTheme(_connectionManager.EditorSettings)
        End Sub

        Private Sub LoadConnectionProfiles()
            cmbConnection.Items.Clear()
            For Each profile As Models.ConnectionProfile In _connectionManager.Profiles
                cmbConnection.Items.Add(profile)
            Next

            If cmbConnection.Items.Count > 0 Then
                cmbConnection.SelectedIndex = 0
            End If
        End Sub

        Private Sub ApplyTheme(settings As Models.EditorSettings)
            Dim theme As Models.SyntaxTheme = If(
                settings.Theme = "Dark",
                Models.SyntaxTheme.Dark,
                Models.SyntaxTheme.Light
            )

            rtbEditor.BackColor = theme.BackgroundColor
            rtbEditor.ForeColor = theme.ForegroundColor
            pnlLineNumbers.BackColor = theme.LineNumberBackground
            lblLineNumbers.ForeColor = theme.LineNumberForeground

            rtbEditor.Font = New Font(settings.FontFamily, settings.FontSize)
        End Sub

        Private Async Sub BtnConnect_Click(sender As Object, e As EventArgs)
            If cmbConnection.SelectedItem Is Nothing Then
                MessageBox.Show("Please select a connection profile.", "No Connection Selected", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            Dim profile As Models.ConnectionProfile = DirectCast(cmbConnection.SelectedItem, Models.ConnectionProfile)
            Await ConnectAsync(profile)
        End Sub

        Private Async Function ConnectAsync(profile As Models.ConnectionProfile) As Task
            SetControlsEnabled(False, "Connecting...")
            btnCancel.Enabled = True

            _cancellationTokenSource = New CancellationTokenSource()

            Try
                Dim connected As Boolean = Await _connectionManager.ConnectAsync(profile, _cancellationTokenSource.Token)

                If connected Then
                    UpdateConnectionStatus(True)
                    lblStatus.Text = $"Connected to {profile.Name}"
                    btnDisconnect.Enabled = True
                    Await LoadDirectoryAsync("/")
                Else
                    UpdateConnectionStatus(False)
                    lblStatus.Text = "Connection failed"
                End If
            Catch ex As OperationCanceledException
                lblStatus.Text = "Connection cancelled"
            Catch ex As Exception
                lblStatus.Text = $"Connection error: {ex.Message}"
                MessageBox.Show($"Failed to connect: {ex.Message}", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Finally
                SetControlsEnabled(True)
                btnCancel.Enabled = False
                lblProgress.Visible = False
            End Try
        End Function

        Private Sub BtnNewConnection_Click(sender As Object, e As EventArgs)
            Dim profileForm As New ConnectionProfileForm(_connectionManager)
            If profileForm.ShowDialog() = DialogResult.OK Then
                LoadConnectionProfiles()
                cmbConnection.SelectedIndex = cmbConnection.Items.Count - 1
            End If
        End Sub

        Private Sub BtnDisconnect_Click(sender As Object, e As EventArgs)
            Disconnect()
        End Sub

        Private Sub Disconnect()
            _connectionManager.Disconnect()
            UpdateConnectionStatus(False)
            lblStatus.Text = "Disconnected"
            btnDisconnect.Enabled = False
            treeFiles.Nodes.Clear()
            _editorService.Clear()
            rtbEditor.Text = ""
            UpdateLineNumbers()
        End Sub

        Private Async Sub BtnRefresh_Click(sender As Object, e As EventArgs)
            Await LoadDirectoryAsync(_currentPath)
        End Sub

        Private Sub BtnHome_Click(sender As Object, e As EventArgs)
            If _connectionManager.IsConnected Then
                txtPath.Text = _connectionManager.ActiveService.GetHomeDirectory()
                Await LoadDirectoryAsync(txtPath.Text)
            End If
        End Sub

        Private Sub BtnUp_Click(sender As Object, e As EventArgs)
            If Not _connectionManager.IsConnected Then Return

            Dim currentPath As String = _currentPath
            Dim separator As Char = If(currentPath.Contains("/"), "/"c, "\"c)
            Dim lastSeparator As Integer = currentPath.LastIndexOf(separator)

            If lastSeparator > 0 Then
                txtPath.Text = currentPath.Substring(0, lastSeparator)
                Await LoadDirectoryAsync(txtPath.Text)
            End If
        End Sub

        Private Sub BtnCancel_Click(sender As Object, e As EventArgs)
            _cancellationTokenSource?.Cancel()
        End Sub

        Private Async Function LoadDirectoryAsync(path As String) As Task
            If Not _connectionManager.IsConnected Then Return

            SetControlsEnabled(False, "Loading...")
            lblProgress.Visible = True

            Try
                Dim progress As New Progress(Of Models.OperationProgress)(Sub(p)
                    lblStatus.Text = p.StatusMessage
                    If p.TotalBytes > 0 Then
                        lblProgress.Value = p.Percentage
                    End If
                End Sub)

                Dim items As List(Of Models.FileItem) = Await _connectionManager.ActiveService.ListDirectoryAsync(path, progress, CancellationToken.None)

                treeFiles.BeginUpdate()
                treeFiles.Nodes.Clear()

                For Each item As Models.FileItem In items
                    If item.IsDirectory Then
                        Dim node As TreeNode = treeFiles.Nodes.Add(item.Name)
                        node.Tag = item
                        node.ImageKey = "folder"
                        node.SelectedImageKey = "folder"
                        ' Add dummy child for lazy loading
                        node.Nodes.Add("")
                    Else
                        Dim node As TreeNode = treeFiles.Nodes.Add(item.Name)
                        node.Tag = item
                        node.ImageKey = item.IconKey
                        node.SelectedImageKey = item.IconKey
                    End If
                Next

                treeFiles.EndUpdate()

                _currentPath = path
                txtPath.Text = path
                lblStatus.Text = $"Loaded {items.Count} items"
            Catch ex As Exception
                lblStatus.Text = $"Error: {ex.Message}"
                MessageBox.Show($"Failed to load directory: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Finally
                SetControlsEnabled(True)
                lblProgress.Visible = False
            End Try
        End Function

        Private Sub TreeFiles_AfterSelect(sender As Object, e As TreeViewEventArgs)
            If e.Node?.Tag IsNot Nothing Then
                Dim item As Models.FileItem = DirectCast(e.Node.Tag, Models.FileItem)
                If Not item.IsDirectory Then
                    txtPath.Text = item.FullPath
                End If
            End If
        End Sub

        Private Sub TreeFiles_NodeMouseClick(sender As Object, e As TreeNodeMouseClickEventArgs)
            treeFiles.SelectedNode = e.Node
        End Sub

        Private Async Sub TreeFiles_BeforeExpand(sender As Object, e As TreeViewCancelEventArgs)
            If e.Node?.Nodes.Count = 1 AndAlso String.IsNullOrEmpty(e.Node.Nodes(0).Text) Then
                e.Node.Nodes.Clear()

                If _connectionManager.ActiveService Is Nothing Then Return

                Dim item As Models.FileItem = DirectCast(e.Node.Tag, Models.FileItem)
                Dim parentPath As String = item.FullPath

                Try
                    Dim progress As New Progress(Of Models.OperationProgress)(Sub(p)
                        lblStatus.Text = p.StatusMessage
                    End Sub)

                    Dim items As List(Of Models.FileItem) = Await _connectionManager.ActiveService.ListDirectoryAsync(parentPath, progress, CancellationToken.None)

                    For Each subItem As Models.FileItem In items
                        If subItem.IsDirectory Then
                            Dim node As TreeNode = e.Node.Nodes.Add(subItem.Name)
                            node.Tag = subItem
                            node.ImageKey = "folder"
                            node.SelectedImageKey = "folder"
                            node.Nodes.Add("")
                        End If
                    Next
                Catch ex As Exception
                    lblStatus.Text = $"Error loading directory: {ex.Message}"
                End Try
            End If
        End Sub

        Private Async Sub MnuOpen_Click(sender As Object, e As EventArgs)
            If treeFiles.SelectedNode?.Tag Is Nothing Then Return

            Dim item As Models.FileItem = DirectCast(treeFiles.SelectedNode.Tag, Models.FileItem)

            If item.IsDirectory Then
                txtPath.Text = item.FullPath
                Await LoadDirectoryAsync(item.FullPath)
            End If
        End Sub

        Private Async Sub MnuEdit_Click(sender As Object, e As EventArgs)
            If treeFiles.SelectedNode?.Tag Is Nothing Then Return

            Dim item As Models.FileItem = DirectCast(treeFiles.SelectedNode.Tag, Models.FileItem)

            If Not item.IsDirectory Then
                Await OpenFileAsync(item)
            End If
        End Sub

        Private Async Function OpenFileAsync(item As Models.FileItem) As Task
            If Not item.IsTextFile Then
                MessageBox.Show("This file type cannot be edited in the text editor.", "Cannot Edit", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            SetControlsEnabled(False, "Loading file...")
            lblProgress.Visible = True

            Try
                Dim loaded As Boolean = Await _editorService.LoadFileAsync(item.FullPath)

                If loaded Then
                    rtbEditor.Text = _editorService.CurrentContent
                    UpdateLineNumbers()
                    btnSave.Enabled = False
                    lblStatus.Text = $"Opened: {item.Name}"

                    AddToRecentFiles(item.FullPath)
                Else
                    lblStatus.Text = "Failed to load file"
                End If
            Catch ex As Exception
                lblStatus.Text = $"Error: {ex.Message}"
                MessageBox.Show($"Failed to open file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Finally
                SetControlsEnabled(True)
                lblProgress.Visible = False
            End Try
        End Function

        Private Async Sub MnuDelete_Click(sender As Object, e As EventArgs)
            If treeFiles.SelectedNode?.Tag Is Nothing Then Return

            Dim item As Models.FileItem = DirectCast(treeFiles.SelectedNode.Tag, Models.FileItem)

            Dim result As DialogResult = MessageBox.Show(
                $"Are you sure you want to delete '{item.Name}'?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            )

            If result = DialogResult.Yes Then
                Try
                    Await _connectionManager.ActiveService.DeleteAsync(item.FullPath, CancellationToken.None)
                    Await LoadDirectoryAsync(_currentPath)
                    lblStatus.Text = $"Deleted: {item.Name}"
                Catch ex As Exception
                    MessageBox.Show($"Failed to delete: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End Try
            End If
        End Sub

        Private Sub MnuRename_Click(sender As Object, e As EventArgs)
            If treeFiles.SelectedNode?.Tag Is Nothing Then Return

            Dim item As Models.FileItem = DirectCast(treeFiles.SelectedNode.Tag, Models.FileItem)

            Using renameDialog As New RenameDialog(item.Name)
                If renameDialog.ShowDialog() = DialogResult Then
                    Try
                        Await _connectionManager.ActiveService.RenameAsync(item.FullPath, renameDialog.NewName, CancellationToken.None)
                        Await LoadDirectoryAsync(_currentPath)
                        lblStatus.Text = $"Renamed to: {renameDialog.NewName}"
                    Catch ex As Exception
                        MessageBox.Show($"Failed to rename: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    End Try
                End If
            End Using
        End Sub

        Private Async Sub MnuRefresh_Click(sender As Object, e As EventArgs)
            Await LoadDirectoryAsync(_currentPath)
        End Sub

        Private Async Sub TxtPath_KeyPress(sender As Object, e As KeyPressEventArgs)
            If e.KeyChar = Convert.ToChar(13) Then
                Await LoadDirectoryAsync(txtPath.Text)
            End If
        End Sub

        Private Sub RtbEditor_TextChanged(sender As Object, e As EventArgs)
            UpdateLineNumbers()
            btnSave.Enabled = True
        End Sub

        Private Sub RtbEditor_SelectionChanged(sender As Object, e As EventArgs)
            UpdateButtonStates()
        End Sub

        Private Sub RtbEditor_KeyDown(sender As Object, e As KeyEventArgs)
            If e.Control Then
                Select Case e.KeyCode
                    Case Keys.S
                        e.SuppressKeyPress = True
                        BtnSave_Click(sender, EventArgs.Empty)
                    Case Keys.Z
                        e.SuppressKeyPress = True
                        If e.Shift Then
                            _editorService.Redo()
                        Else
                            _editorService.Undo()
                        End If
                        UpdateButtonStates()
                    Case Keys.Y
                        e.SuppressKeyPress = True
                        _editorService.Redo()
                        UpdateButtonStates()
                    Case Keys.F
                        e.SuppressKeyPress = True
                        BtnFind_Click(sender, EventArgs.Empty)
                    Case Keys.H
                        e.SuppressKeyPress = True
                        BtnReplace_Click(sender, EventArgs.Empty)
                    Case Keys.A
                        e.SuppressKeyPress = True
                        rtbEditor.SelectAll()
                End Select
            End If
        End Sub

        Private Async Sub BtnSave_Click(sender As Object, e As EventArgs)
            If String.IsNullOrEmpty(_editorService.CurrentFilePath) Then Return

            btnSave.Enabled = False
            lblStatus.Text = "Saving..."

            Dim saved As Boolean = Await _editorService.SaveFileAsync(rtbEditor.Text)

            If saved Then
                lblStatus.Text = "Saved successfully"
            Else
                lblStatus.Text = "Save failed"
                btnSave.Enabled = True
            End If
        End Sub

        Private Sub BtnUndo_Click(sender As Object, e As EventArgs)
            _editorService.Undo()
            rtbEditor.Text = _editorService.CurrentContent
            UpdateButtonStates()
        End Sub

        Private Sub BtnRedo_Click(sender As Object, e As EventArgs)
            _editorService.Redo()
            rtbEditor.Text = _editorService.CurrentContent
            UpdateButtonStates()
        End Sub

        Private Sub BtnCut_Click(sender As Object, e As EventArgs)
            rtbEditor.Cut()
        End Sub

        Private Sub BtnCopy_Click(sender As Object, e As EventArgs)
            rtbEditor.Copy()
        End Sub

        Private Sub BtnPaste_Click(sender As Object, e As EventArgs)
            rtbEditor.Paste()
        End Sub

        Private Sub BtnFind_Click(sender As Object, e As EventArgs)
            Using findDialog As New FindDialog()
                If findDialog.ShowDialog() = DialogResult.OK Then
                    Dim startIndex As Integer = rtbEditor.SelectionStart + rtbEditor.SelectionLength
                    Dim searchText As String = findDialog.FindText

                    Dim index As Integer = _editorService.FindNext(searchText, findDialog.UseRegex, findDialog.MatchCase)

                    If index >= 0 Then
                        rtbEditor.Select(index, searchText.Length)
                        rtbEditor.ScrollToCaret()
                    Else
                        MessageBox.Show("Text not found.", "Find", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    End If
                End If
            End Using
        End Sub

        Private Sub BtnReplace_Click(sender As Object, e As EventArgs)
            Using replaceDialog As New ReplaceDialog()
                If replaceDialog.ShowDialog() = DialogResult.OK Then
                    Dim count As Integer = _editorService.Replace(
                        replaceDialog.FindText,
                        replaceDialog.ReplaceText,
                        replaceDialog.UseRegex,
                        replaceDialog.MatchCase,
                        replaceDialog.ReplaceAll
                    )

                    rtbEditor.Text = _editorService.CurrentContent
                    MessageBox.Show($"Replaced {count} occurrence(s).", "Replace", MessageBoxButtons.OK, MessageBoxIcon.Information)
                End If
            End Using
        End Sub

        Private Sub UpdateLineNumbers()
            Dim lineCount As Integer = _editorService.GetTotalLines()
            Dim lineNumbersText As New StringBuilder()

            For i As Integer = 1 To lineCount
                lineNumbersText.AppendLine(i.ToString())
            Next

            lblLineNumbers.Text = lineNumbersText.ToString().TrimEnd()
        End Sub

        Private Sub UpdateButtonStates()
            btnUndo.Enabled = _editorService.CanUndo
            btnRedo.Enabled = _editorService.CanRedo
        End Sub

        Private Sub AddToRecentFiles(path As String)
            _recentFiles.Remove(path)
            _recentFiles.Insert(0, path)

            While _recentFiles.Count > MaxRecentFiles
                _recentFiles.RemoveAt(_recentFiles.Count - 1)
            End While

            lstRecentFiles.Items.Clear()
            For Each file As String In _recentFiles
                lstRecentFiles.Items.Add(Path.GetFileName(file))
            Next
        End Sub

        Private Sub LstRecentFiles_MouseDoubleClick(sender As Object, e As MouseEventArgs)
            If lstRecentFiles.SelectedIndex < 0 Then Return

            Dim path As String = _recentFiles(lstRecentFiles.SelectedIndex)
            txtPath.Text = path
        End Sub

        ' Editor context menu handlers
        Private Sub MnuEditorUndo_Click(sender As Object, e As EventArgs)
            _editorService.Undo()
            rtbEditor.Text = _editorService.CurrentContent
            UpdateButtonStates()
        End Sub

        Private Sub MnuEditorRedo_Click(sender As Object, e As EventArgs)
            _editorService.Redo()
            rtbEditor.Text = _editorService.CurrentContent
            UpdateButtonStates()
        End Sub

        Private Sub MnuEditorCut_Click(sender As Object, e As EventArgs)
            rtbEditor.Cut()
        End Sub

        Private Sub MnuEditorCopy_Click(sender As Object, e As EventArgs)
            rtbEditor.Copy()
        End Sub

        Private Sub MnuEditorPaste_Click(sender As Object, e As EventArgs)
            rtbEditor.Paste()
        End Sub

        Private Sub MnuEditorDelete_Click(sender As Object, e As EventArgs)
            rtbEditor.SelectedText = ""
        End Sub

        Private Sub MnuEditorSelectAll_Click(sender As Object, e As EventArgs)
            rtbEditor.SelectAll()
        End Sub

        Private Sub ConnectionManager_ProfilesChanged(sender As Object, e As EventArgs)
            LoadConnectionProfiles()
        End Sub

        Private Sub ConnectionManager_ConnectionChanged(sender As Object, e As FileTransferService)
            UpdateConnectionStatus(e?.IsConnected ?? False)
        End Sub

        Private Sub ConnectionManager_SettingsChanged(sender As Object, e As Models.EditorSettings)
            ApplyTheme(e)
        End Sub

        Private Sub UpdateConnectionStatus(connected As Boolean)
            lblConnectionStatus.Text = If(connected, "Connected", "Disconnected")
            lblConnectionStatus.ForeColor = If(connected, Color.Lime, Color.Red)
        End Sub

        Private Sub SetControlsEnabled(enabled As Boolean, Optional statusText As String = "")
            btnConnect.Enabled = enabled
            btnNewConnection.Enabled = enabled
            btnRefresh.Enabled = enabled
            btnHome.Enabled = enabled
            btnUp.Enabled = enabled
            txtPath.Enabled = enabled
            treeFiles.Enabled = enabled

            If Not String.IsNullOrEmpty(statusText) Then
                lblStatus.Text = statusText
            End If
        End Sub

        Private Sub FileManagerForm_FormClosing(sender As Object, e As FormClosingEventArgs)
            If btnSave.Enabled Then
                Dim result As DialogResult = MessageBox.Show(
                    "You have unsaved changes. Do you want to save before closing?",
                    "Unsaved Changes",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question
                )

                If result = DialogResult.Cancel Then
                    e.Cancel = True
                    Return
                ElseIf result = DialogResult.Yes Then
                    BtnSave_Click(sender, EventArgs.Empty)
                End If
            End If

            _connectionManager.Disconnect()
        End Sub
    End Class

    ' Helper dialog classes
    Public Class RenameDialog
        Inherits Form

        Private txtName As TextBox
        Private btnOK As Button
        Private btnCancel As Button

        Public Property NewName As String

        Public Sub New(currentName As String)
            Me.Text = "Rename"
            Me.Size = New Size(300, 120)
            Me.StartPosition = FormStartPosition.CenterParent
            Me.FormBorderStyle = FormBorderStyle.FixedDialog
            Me.MaximizeBox = False
            Me.MinimizeBox = False

            Dim lblPrompt As New Label() With {
                .Text = "Enter new name:",
                .Location = New Point(10, 10),
                .AutoSize = True
            }

            txtName = New TextBox() With {
                .Text = currentName,
                .Location = New Point(10, 35),
                .Width = 260
            }

            btnOK = New Button() With {
                .Text = "OK",
                .Location = New Point(110, 70),
                .Width = 70,
                .DialogResult = DialogResult.OK
            }

            btnCancel = New Button() With {
                .Text = "Cancel",
                .Location = New Point(190, 70),
                .Width = 70,
                .DialogResult = DialogResult.Cancel
            }

            Me.Controls.AddRange({lblPrompt, txtName, btnOK, btnCancel})
            Me.AcceptButton = btnOK
            Me.CancelButton = btnCancel
        End Sub

        Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
            If DialogResult = DialogResult.OK Then
                NewName = txtName.Text
            End If
            MyBase.OnFormClosing(e)
        End Sub
    End Class

    Public Class FindDialog
        Inherits Form

        Private txtFind As TextBox
        Private chkRegex As CheckBox
        Private chkMatchCase As CheckBox
        Private btnFind As Button
        Private btnCancel As Button

        Public Property FindText As String
        Public Property UseRegex As Boolean
        Public Property MatchCase As Boolean

        Public Sub New()
            Me.Text = "Find"
            Me.Size = New Size(350, 150)
            Me.StartPosition = FormStartPosition.CenterParent
            Me.FormBorderStyle = FormBorderStyle.FixedDialog
            Me.MaximizeBox = False
            Me.MinimizeBox = False

            Dim lblPrompt As New Label() With {
                .Text = "Find what:",
                .Location = New Point(10, 10),
                .AutoSize = True
            }

            txtFind = New TextBox() With {
                .Location = New Point(80, 7),
                .Width = 240
            }

            chkRegex = New CheckBox() With {
                .Text = "Use regular expressions",
                .Location = New Point(10, 40),
                .AutoSize = True
            }

            chkMatchCase = New CheckBox() With {
                .Text = "Match case",
                .Location = New Point(10, 65),
                .AutoSize = True
            }

            btnFind = New Button() With {
                .Text = "Find Next",
                .Location = New Point(160, 90),
                .Width = 80,
                .DialogResult = DialogResult.OK
            }

            btnCancel = New Button() With {
                .Text = "Cancel",
                .Location = New Point(250, 90),
                .Width = 80,
                .DialogResult = DialogResult.Cancel
            }

            Me.Controls.AddRange({lblPrompt, txtFind, chkRegex, chkMatchCase, btnFind, btnCancel})
            Me.AcceptButton = btnFind
            Me.CancelButton = btnCancel
        End Sub

        Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
            If DialogResult = DialogResult.OK Then
                FindText = txtFind.Text
                UseRegex = chkRegex.Checked
                MatchCase = chkMatchCase.Checked
            End If
            MyBase.OnFormClosing(e)
        End Sub
    End Class

    Public Class ReplaceDialog
        Inherits Form

        Private txtFind As TextBox
        Private txtReplace As TextBox
        Private chkRegex As CheckBox
        Private chkMatchCase As CheckBox
        Private chkReplaceAll As CheckBox
        Private btnReplace As Button
        Private btnCancel As Button

        Public Property FindText As String
        Public Property ReplaceText As String
        Public Property UseRegex As Boolean
        Public Property MatchCase As Boolean
        Public Property ReplaceAll As Boolean

        Public Sub New()
            Me.Text = "Replace"
            Me.Size = New Size(350, 200)
            Me.StartPosition = FormStartPosition.CenterParent
            Me.FormBorderStyle = FormBorderStyle.FixedDialog
            Me.MaximizeBox = False
            Me.MinimizeBox = False

            Dim lblFind As New Label() With {
                .Text = "Find what:",
                .Location = New Point(10, 10),
                .AutoSize = True
            }

            txtFind = New TextBox() With {
                .Location = New Point(80, 7),
                .Width = 240
            }

            Dim lblReplace As New Label() With {
                .Text = "Replace with:",
                .Location = New Point(10, 40),
                .AutoSize = True
            }

            txtReplace = New TextBox() With {
                .Location = New Point(80, 37),
                .Width = 240
            }

            chkRegex = New CheckBox() With {
                .Text = "Use regular expressions",
                .Location = New Point(10, 70),
                .AutoSize = True
            }

            chkMatchCase = New CheckBox() With {
                .Text = "Match case",
                .Location = New Point(10, 95),
                .AutoSize = True
            }

            chkReplaceAll = New CheckBox() With {
                .Text = "Replace all",
                .Location = New Point(10, 120),
                .AutoSize = True
            }

            btnReplace = New Button() With {
                .Text = "Replace",
                .Location = New Point(160, 145),
                .Width = 80,
                .DialogResult = DialogResult.OK
            }

            btnCancel = New Button() With {
                .Text = "Cancel",
                .Location = New Point(250, 145),
                .Width = 80,
                .DialogResult = DialogResult.Cancel
            }

            Me.Controls.AddRange({lblFind, txtFind, lblReplace, txtReplace, chkRegex, chkMatchCase, chkReplaceAll, btnReplace, btnCancel})
            Me.AcceptButton = btnReplace
            Me.CancelButton = btnCancel
        End Sub

        Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
            If DialogResult = DialogResult.OK Then
                FindText = txtFind.Text
                ReplaceText = txtReplace.Text
                UseRegex = chkRegex.Checked
                MatchCase = chkMatchCase.Checked
                ReplaceAll = chkReplaceAll.Checked
            End If
            MyBase.OnFormClosing(e)
        End Sub
    End Class
End Namespace
