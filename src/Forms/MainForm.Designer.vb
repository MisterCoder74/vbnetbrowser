Namespace vbnetbrowser.Forms

    Partial Class MainForm
        Private components As System.ComponentModel.IContainer = Nothing

        Protected Overrides Sub Dispose(disposing As Boolean)
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
            MyBase.Dispose(disposing)
        End Sub

        Private Sub InitializeComponent()
            Me.menuStrip = New MenuStrip()
            Me.fileToolStripMenuItem = New ToolStripMenuItem()
            Me.toolsToolStripMenuItem = New ToolStripMenuItem()
            Me.fileManagerToolStripMenuItem = New ToolStripMenuItem()
            Me.helpToolStripMenuItem = New ToolStripMenuItem()
            Me.pnlSidebar = New Panel()
            Me.lblBookmarks = New Label()
            Me.lstBookmarks = New ListBox()
            Me.btnDeleteBookmark = New Button()
            Me.btnSettingsInSidebar = New Button()
            Me.btnToggleSidebar = New Button()
            Me.pnlTop = New Panel()
            Me.btnBack = New Button()
            Me.btnForward = New Button()
            Me.btnRefresh = New Button()
            Me.btnHome = New Button()
            Me.btnGo = New Button()
            Me.txtAddress = New TextBox()
            Me.btnBookmark = New Button()
            Me.btnNewTab = New Button()
            Me.tabControl = New TabControl()
            Me.menuStrip.SuspendLayout()
            Me.pnlSidebar.SuspendLayout()
            Me.pnlTop.SuspendLayout()
            Me.SuspendLayout()
            '
            ' menuStrip
            '
            Me.menuStrip.BackColor = System.Drawing.Color.FromArgb(CType(CType(230, Byte), Integer), CType(CType(230, Byte), Integer), CType(CType(235, Byte), Integer))
            Me.menuStrip.Items.AddRange(New ToolStripItem() {Me.fileToolStripMenuItem, Me.toolsToolStripMenuItem, Me.helpToolStripMenuItem})
            Me.menuStrip.Location = New System.Drawing.Point(0, 0)
            Me.menuStrip.Name = "menuStrip"
            Me.menuStrip.Size = New System.Drawing.Size(850, 24)
            Me.menuStrip.TabIndex = 0
            '
            ' fileToolStripMenuItem
            '
            Me.fileToolStripMenuItem.Name = "fileToolStripMenuItem"
            Me.fileToolStripMenuItem.Size = New System.Drawing.Size(37, 20)
            Me.fileToolStripMenuItem.Text = "&File"
            '
            ' toolsToolStripMenuItem
            '
            Me.toolsToolStripMenuItem.DropDownItems.AddRange(New ToolStripItem() {Me.fileManagerToolStripMenuItem})
            Me.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem"
            Me.toolsToolStripMenuItem.Size = New System.Drawing.Size(46, 20)
            Me.toolsToolStripMenuItem.Text = "&Tools"
            '
            ' fileManagerToolStripMenuItem
            '
            Me.fileManagerToolStripMenuItem.Name = "fileManagerToolStripMenuItem"
            Me.fileManagerToolStripMenuItem.ShortcutKeys = CType((System.Windows.Forms.Keys.Control Or System.Windows.Forms.Keys.M), System.Windows.Forms.Keys)
            Me.fileManagerToolStripMenuItem.Size = New System.Drawing.Size(200, 22)
            Me.fileManagerToolStripMenuItem.Text = "File Manager..."
            '
            ' helpToolStripMenuItem
            '
            Me.helpToolStripMenuItem.Name = "helpToolStripMenuItem"
            Me.helpToolStripMenuItem.Size = New System.Drawing.Size(44, 20)
            Me.helpToolStripMenuItem.Text = "&Help"
            '
            ' pnlSidebar
            '
            Me.pnlSidebar.BackColor = System.Drawing.Color.FromArgb(CType(CType(240, Byte), Integer), CType(CType(240, Byte), Integer), CType(CType(245, Byte), Integer))
            Me.pnlSidebar.Controls.Add(Me.btnSettingsInSidebar)
            Me.pnlSidebar.Controls.Add(Me.lblBookmarks)
            Me.pnlSidebar.Controls.Add(Me.lstBookmarks)
            Me.pnlSidebar.Controls.Add(Me.btnDeleteBookmark)
            Me.pnlSidebar.Controls.Add(Me.btnToggleSidebar)
            Me.pnlSidebar.Dock = System.Windows.Forms.DockStyle.Left
            Me.pnlSidebar.Location = New System.Drawing.Point(0, 24)
            Me.pnlSidebar.Name = "pnlSidebar"
            Me.pnlSidebar.Size = New System.Drawing.Size(250, 556)
            Me.pnlSidebar.TabIndex = 3
            '
            ' lblBookmarks
            '
            Me.lblBookmarks.AutoSize = True
            Me.lblBookmarks.Font = New System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold)
            Me.lblBookmarks.Location = New System.Drawing.Point(10, 10)
            Me.lblBookmarks.Name = "lblBookmarks"
            Me.lblBookmarks.Size = New System.Drawing.Size(79, 19)
            Me.lblBookmarks.TabIndex = 0
            Me.lblBookmarks.Text = "Bookmarks"
            '
            ' lstBookmarks
            '
            Me.lstBookmarks.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.lstBookmarks.Font = New System.Drawing.Font("Segoe UI", 9F)
            Me.lstBookmarks.FormattingEnabled = True
            Me.lstBookmarks.Location = New System.Drawing.Point(10, 35)
            Me.lstBookmarks.Name = "lstBookmarks"
            Me.lstBookmarks.Size = New System.Drawing.Size(230, 416)
            Me.lstBookmarks.TabIndex = 1
            '
            ' btnDeleteBookmark
            '
            Me.btnDeleteBookmark.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.btnDeleteBookmark.Font = New System.Drawing.Font("Segoe UI", 9F)
            Me.btnDeleteBookmark.Location = New System.Drawing.Point(165, 455)
            Me.btnDeleteBookmark.Name = "btnDeleteBookmark"
            Me.btnDeleteBookmark.Size = New System.Drawing.Size(75, 30)
            Me.btnDeleteBookmark.TabIndex = 2
            Me.btnDeleteBookmark.Text = "Delete"
            Me.btnDeleteBookmark.UseVisualStyleBackColor = True
            '
            ' btnSettingsInSidebar
            '
            Me.btnSettingsInSidebar.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.btnSettingsInSidebar.Font = New System.Drawing.Font("Segoe UI", 9F)
            Me.btnSettingsInSidebar.Location = New System.Drawing.Point(10, 455)
            Me.btnSettingsInSidebar.Name = "btnSettingsInSidebar"
            Me.btnSettingsInSidebar.Size = New System.Drawing.Size(75, 30)
            Me.btnSettingsInSidebar.TabIndex = 3
            Me.btnSettingsInSidebar.Text = "Settings"
            Me.btnSettingsInSidebar.UseVisualStyleBackColor = True
            '
            ' btnToggleSidebar
            '
            Me.btnToggleSidebar.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.btnToggleSidebar.Font = New System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold)
            Me.btnToggleSidebar.Location = New System.Drawing.Point(220, 2)
            Me.btnToggleSidebar.Name = "btnToggleSidebar"
            Me.btnToggleSidebar.Size = New System.Drawing.Size(25, 25)
            Me.btnToggleSidebar.TabIndex = 4
            Me.btnToggleSidebar.Text = "«"
            Me.btnToggleSidebar.UseVisualStyleBackColor = True
            '
            ' pnlTop
            '
            Me.pnlTop.BackColor = System.Drawing.Color.FromArgb(CType(CType(230, Byte), Integer), CType(CType(230, Byte), Integer), CType(CType(235, Byte), Integer))
            Me.pnlTop.Controls.Add(Me.btnBookmark)
            Me.pnlTop.Controls.Add(Me.btnNewTab)
            Me.pnlTop.Controls.Add(Me.btnHome)
            Me.pnlTop.Controls.Add(Me.btnGo)
            Me.pnlTop.Controls.Add(Me.btnRefresh)
            Me.pnlTop.Controls.Add(Me.btnForward)
            Me.pnlTop.Controls.Add(Me.btnBack)
            Me.pnlTop.Controls.Add(Me.txtAddress)
            Me.pnlTop.Dock = System.Windows.Forms.DockStyle.Top
            Me.pnlTop.Location = New System.Drawing.Point(0, 0)
            Me.pnlTop.Name = "pnlTop"
            Me.pnlTop.Size = New System.Drawing.Size(800, 60)
            Me.pnlTop.TabIndex = 1
            '
            ' btnBack
            '
            Me.btnBack.Font = New System.Drawing.Font("Segoe UI", 9F)
            Me.btnBack.Location = New System.Drawing.Point(10, 15)
            Me.btnBack.Name = "btnBack"
            Me.btnBack.Size = New System.Drawing.Size(35, 30)
            Me.btnBack.TabIndex = 0
            Me.btnBack.Text = "←"
            Me.btnBack.UseVisualStyleBackColor = True
            '
            ' btnForward
            '
            Me.btnForward.Font = New System.Drawing.Font("Segoe UI", 9F)
            Me.btnForward.Location = New System.Drawing.Point(51, 15)
            Me.btnForward.Name = "btnForward"
            Me.btnForward.Size = New System.Drawing.Size(35, 30)
            Me.btnForward.TabIndex = 1
            Me.btnForward.Text = "→"
            Me.btnForward.UseVisualStyleBackColor = True
            '
            ' btnRefresh
            '
            Me.btnRefresh.Font = New System.Drawing.Font("Segoe UI", 9F)
            Me.btnRefresh.Location = New System.Drawing.Point(92, 15)
            Me.btnRefresh.Name = "btnRefresh"
            Me.btnRefresh.Size = New System.Drawing.Size(35, 30)
            Me.btnRefresh.TabIndex = 2
            Me.btnRefresh.Text = "↻"
            Me.btnRefresh.UseVisualStyleBackColor = True
            '
            ' btnHome
            '
            Me.btnHome.Font = New System.Drawing.Font("Segoe UI", 9F)
            Me.btnHome.Location = New System.Drawing.Point(133, 15)
            Me.btnHome.Name = "btnHome"
            Me.btnHome.Size = New System.Drawing.Size(50, 30)
            Me.btnHome.TabIndex = 3
            Me.btnHome.Text = "Home"
            Me.btnHome.UseVisualStyleBackColor = True
            '
            ' btnGo
            '
            Me.btnGo.Font = New System.Drawing.Font("Segoe UI", 9F)
            Me.btnGo.Location = New System.Drawing.Point(675, 15)
            Me.btnGo.Name = "btnGo"
            Me.btnGo.Size = New System.Drawing.Size(55, 30)
            Me.btnGo.TabIndex = 5
            Me.btnGo.Text = "Go"
            Me.btnGo.UseVisualStyleBackColor = True
            '
            ' txtAddress
            '
            Me.txtAddress.Font = New System.Drawing.Font("Segoe UI", 9F)
            Me.txtAddress.Location = New System.Drawing.Point(189, 18)
            Me.txtAddress.Name = "txtAddress"
            Me.txtAddress.Size = New System.Drawing.Size(480, 23)
            Me.txtAddress.TabIndex = 4
            '
            ' btnBookmark
            '
            Me.btnBookmark.Font = New System.Drawing.Font("Segoe UI", 9F)
            Me.btnBookmark.Location = New System.Drawing.Point(736, 15)
            Me.btnBookmark.Name = "btnBookmark"
            Me.btnBookmark.Size = New System.Drawing.Size(54, 30)
            Me.btnBookmark.TabIndex = 6
            Me.btnBookmark.Text = "★"
            Me.btnBookmark.UseVisualStyleBackColor = True
            '
            ' btnNewTab
            '
            Me.btnNewTab.Font = New System.Drawing.Font("Segoe UI", 9F)
            Me.btnNewTab.Location = New System.Drawing.Point(796, 15)
            Me.btnNewTab.Name = "btnNewTab"
            Me.btnNewTab.Size = New System.Drawing.Size(54, 30)
            Me.btnNewTab.TabIndex = 7
            Me.btnNewTab.Text = "+"
            Me.btnNewTab.UseVisualStyleBackColor = True
            '
            ' tabControl
            '
            Me.tabControl.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.tabControl.Font = New System.Drawing.Font("Segoe UI", 9F)
            Me.tabControl.Location = New System.Drawing.Point(250, 24)
            Me.tabControl.Name = "tabControl"
            Me.tabControl.SelectedIndex = 0
            Me.tabControl.Size = New System.Drawing.Size(600, 556)
            Me.tabControl.TabIndex = 4
            '
            ' MainForm
            '
            Me.AutoScaleDimensions = New System.Drawing.SizeF(7F, 15F)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
            Me.ClientSize = New System.Drawing.Size(850, 580)
            Me.Controls.Add(Me.tabControl)
            Me.Controls.Add(Me.pnlTop)
            Me.Controls.Add(Me.pnlSidebar)
            Me.Controls.Add(Me.menuStrip)
            Me.MainMenuStrip = Me.menuStrip
            Me.MinimumSize = New System.Drawing.Size(800, 600)
            Me.Name = "MainForm"
            Me.Text = "VB.NET Browser"
            Me.menuStrip.ResumeLayout(False)
            Me.menuStrip.PerformLayout()
            Me.pnlSidebar.ResumeLayout(False)
            Me.pnlSidebar.PerformLayout()
            Me.pnlTop.ResumeLayout(False)
            Me.pnlTop.PerformLayout()
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub

        Friend WithEvents menuStrip As MenuStrip
        Friend WithEvents fileToolStripMenuItem As ToolStripMenuItem
        Friend WithEvents toolsToolStripMenuItem As ToolStripMenuItem
        Friend WithEvents fileManagerToolStripMenuItem As ToolStripMenuItem
        Friend WithEvents helpToolStripMenuItem As ToolStripMenuItem
        Friend WithEvents pnlSidebar As Panel
        Friend WithEvents lblBookmarks As Label
        Friend WithEvents lstBookmarks As ListBox
        Friend WithEvents btnDeleteBookmark As Button
        Friend WithEvents btnSettingsInSidebar As Button
        Friend WithEvents btnToggleSidebar As Button
        Friend WithEvents pnlTop As Panel
        Friend WithEvents btnBack As Button
        Friend WithEvents btnForward As Button
        Friend WithEvents btnRefresh As Button
        Friend WithEvents btnHome As Button
        Friend WithEvents btnGo As Button
        Friend WithEvents txtAddress As TextBox
        Friend WithEvents btnBookmark As Button
        Friend WithEvents btnNewTab As Button
        Friend WithEvents tabControl As TabControl
    End Class

End Namespace
