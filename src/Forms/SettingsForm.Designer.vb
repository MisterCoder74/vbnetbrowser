Namespace vbnetbrowser.Forms

    Partial Class SettingsForm
        Private components As System.ComponentModel.IContainer = Nothing

        Protected Overrides Sub Dispose(disposing As Boolean)
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
            MyBase.Dispose(disposing)
        End Sub

        Private Sub InitializeComponent()
            Me.btnSettings = New Button()
            Me.txtHomePage = New TextBox()
            Me.txtSearchEngine = New TextBox()
            Me.txtSidebarWidth = New TextBox()
            Me.chkRememberSession = New CheckBox()
            Me.chkEnableHistory = New CheckBox()
            Me.chkEnableBookmarks = New CheckBox()
            Me.cmbTheme = New ComboBox()
            Me.btnSave = New Button()
            Me.btnCancel = New Button()
            Me.btnReset = New Button()
            Me.grpGeneral = New GroupBox()
            Me.grpAppearance = New GroupBox()
            Me.grpFeatures = New GroupBox()
            Me.Label1 = New Label()
            Me.Label2 = New Label()
            Me.Label3 = New Label()
            Me.Label4 = New Label()
            Me.grpGeneral.SuspendLayout()
            Me.grpAppearance.SuspendLayout()
            Me.grpFeatures.SuspendLayout()
            Me.SuspendLayout()
            '
            ' Label1
            '
            Me.Label1.AutoSize = True
            Me.Label1.Font = New Font("Segoe UI", 9F)
            Me.Label1.Location = New Point(20, 30)
            Me.Label1.Name = "Label1"
            Me.Label1.Size = New Size(63, 15)
            Me.Label1.TabIndex = 0
            Me.Label1.Text = "Home Page:"
            '
            ' Label2
            '
            Me.Label2.AutoSize = True
            Me.Label2.Font = New Font("Segoe UI", 9F)
            Me.Label2.Location = New Point(20, 70)
            Me.Label2.Name = "Label2"
            Me.Label2.Size = New Size(91, 15)
            Me.Label2.TabIndex = 1
            Me.Label2.Text = "Search Engine:"
            '
            ' Label3
            '
            Me.Label3.AutoSize = True
            Me.Label3.Font = New Font("Segoe UI", 9F)
            Me.Label3.Location = New Point(20, 30)
            Me.Label3.Name = "Label3"
            Me.Label3.Size = New Size(99, 15)
            Me.Label3.TabIndex = 2
            Me.Label3.Text = "Sidebar Width:"
            '
            ' Label4
            '
            Me.Label4.AutoSize = True
            Me.Label4.Font = New Font("Segoe UI", 9F)
            Me.Label4.Location = New Point(20, 70)
            Me.Label4.Name = "Label4"
            Me.Label4.Size = New Size(41, 15)
            Me.Label4.TabIndex = 3
            Me.Label4.Text = "Theme:"
            '
            ' grpGeneral
            '
            Me.grpGeneral.Controls.Add(Me.txtHomePage)
            Me.grpGeneral.Controls.Add(Me.Label2)
            Me.grpGeneral.Controls.Add(Me.Label1)
            Me.grpGeneral.Controls.Add(Me.txtSearchEngine)
            Me.grpGeneral.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
            Me.grpGeneral.Location = New Point(20, 20)
            Me.grpGeneral.Name = "grpGeneral"
            Me.grpGeneral.Size = New Size(440, 120)
            Me.grpGeneral.TabIndex = 0
            Me.grpGeneral.TabStop = False
            Me.grpGeneral.Text = "General"
            '
            ' txtHomePage
            '
            Me.txtHomePage.Font = New Font("Segoe UI", 9F)
            Me.txtHomePage.Location = New Point(120, 27)
            Me.txtHomePage.Name = "txtHomePage"
            Me.txtHomePage.Size = New Size(300, 23)
            Me.txtHomePage.TabIndex = 0
            '
            ' txtSearchEngine
            '
            Me.txtSearchEngine.Font = New Font("Segoe UI", 9F)
            Me.txtSearchEngine.Location = New Point(120, 67)
            Me.txtSearchEngine.Name = "txtSearchEngine"
            Me.txtSearchEngine.Size = New Size(300, 23)
            Me.txtSearchEngine.TabIndex = 1
            '
            ' grpAppearance
            '
            Me.grpAppearance.Controls.Add(Me.Label4)
            Me.grpAppearance.Controls.Add(Me.cmbTheme)
            Me.grpAppearance.Controls.Add(Me.Label3)
            Me.grpAppearance.Controls.Add(Me.txtSidebarWidth)
            Me.grpAppearance.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
            Me.grpAppearance.Location = New Point(20, 150)
            Me.grpAppearance.Name = "grpAppearance"
            Me.grpAppearance.Size = New Size(440, 130)
            Me.grpAppearance.TabIndex = 1
            Me.grpAppearance.TabStop = False
            Me.grpAppearance.Text = "Appearance"
            '
            ' txtSidebarWidth
            '
            Me.txtSidebarWidth.Font = New Font("Segoe UI", 9F)
            Me.txtSidebarWidth.Location = New Point(120, 27)
            Me.txtSidebarWidth.Name = "txtSidebarWidth"
            Me.txtSidebarWidth.Size = New Size(100, 23)
            Me.txtSidebarWidth.TabIndex = 0
            '
            ' cmbTheme
            '
            Me.cmbTheme.Font = New Font("Segoe UI", 9F)
            Me.cmbTheme.FormattingEnabled = True
            Me.cmbTheme.Location = New Point(120, 67)
            Me.cmbTheme.Name = "cmbTheme"
            Me.cmbTheme.Size = New Size(150, 23)
            Me.cmbTheme.TabIndex = 1
            '
            ' grpFeatures
            '
            Me.grpFeatures.Controls.Add(Me.chkEnableBookmarks)
            Me.grpFeatures.Controls.Add(Me.chkEnableHistory)
            Me.grpFeatures.Controls.Add(Me.chkRememberSession)
            Me.grpFeatures.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
            Me.grpFeatures.Location = New Point(20, 290)
            Me.grpFeatures.Name = "grpFeatures"
            Me.grpFeatures.Size = New Size(440, 100)
            Me.grpFeatures.TabIndex = 2
            Me.grpFeatures.TabStop = False
            Me.grpFeatures.Text = "Features"
            '
            ' chkRememberSession
            '
            Me.chkRememberSession.AutoSize = True
            Me.chkRememberSession.Font = New Font("Segoe UI", 9F)
            Me.chkRememberSession.Location = New Point(20, 30)
            Me.chkRememberSession.Name = "chkRememberSession"
            Me.chkRememberSession.Size = New Size(120, 19)
            Me.chkRememberSession.TabIndex = 0
            Me.chkRememberSession.Text = "Remember Session"
            Me.chkRememberSession.UseVisualStyleBackColor = True
            '
            ' chkEnableHistory
            '
            Me.chkEnableHistory.AutoSize = True
            Me.chkEnableHistory.Font = New Font("Segoe UI", 9F)
            Me.chkEnableHistory.Location = New Point(160, 30)
            Me.chkEnableHistory.Name = "chkEnableHistory"
            Me.chkEnableHistory.Size = New Size(100, 19)
            Me.chkEnableHistory.TabIndex = 1
            Me.chkEnableHistory.Text = "Enable History"
            Me.chkEnableHistory.UseVisualStyleBackColor = True
            '
            ' chkEnableBookmarks
            '
            Me.chkEnableBookmarks.AutoSize = True
            Me.chkEnableBookmarks.Font = New Font("Segoe UI", 9F)
            Me.chkEnableBookmarks.Location = New Point(20, 60)
            Me.chkEnableBookmarks.Name = "chkEnableBookmarks"
            Me.chkEnableBookmarks.Size = New Size(110, 19)
            Me.chkEnableBookmarks.TabIndex = 2
            Me.chkEnableBookmarks.Text = "Enable Bookmarks"
            Me.chkEnableBookmarks.UseVisualStyleBackColor = True
            '
            ' btnSave
            '
            Me.btnSave.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
            Me.btnSave.Location = New Point(200, 410)
            Me.btnSave.Name = "btnSave"
            Me.btnSave.Size = New Size(100, 35)
            Me.btnSave.TabIndex = 3
            Me.btnSave.Text = "Save"
            Me.btnSave.UseVisualStyleBackColor = True
            '
            ' btnCancel
            '
            Me.btnCancel.Font = New Font("Segoe UI", 9F)
            Me.btnCancel.Location = New Point(310, 410)
            Me.btnCancel.Name = "btnCancel"
            Me.btnCancel.Size = New Size(100, 35)
            Me.btnCancel.TabIndex = 4
            Me.btnCancel.Text = "Cancel"
            Me.btnCancel.UseVisualStyleBackColor = True
            '
            ' btnReset
            '
            Me.btnReset.Font = New Font("Segoe UI", 9F)
            Me.btnReset.Location = New Point(90, 410)
            Me.btnReset.Name = "btnReset"
            Me.btnReset.Size = New Size(100, 35)
            Me.btnReset.TabIndex = 5
            Me.btnReset.Text = "Reset"
            Me.btnReset.UseVisualStyleBackColor = True
            '
            ' btnSettings
            '
            Me.btnSettings.Font = New Font("Segoe UI", 9F)
            Me.btnSettings.Location = New Point(20, 410)
            Me.btnSettings.Name = "btnSettings"
            Me.btnSettings.Size = New Size(100, 35)
            Me.btnSettings.TabIndex = 6
            Me.btnSettings.Text = "Settings"
            Me.btnSettings.Visible = False
            '
            ' SettingsForm
            '
            Me.AutoScaleDimensions = New SizeF(7F, 15F)
            Me.AutoScaleMode = AutoScaleMode.Font
            Me.ClientSize = New Size(480, 460)
            Me.Controls.Add(Me.btnReset)
            Me.Controls.Add(Me.btnCancel)
            Me.Controls.Add(Me.btnSave)
            Me.Controls.Add(Me.grpFeatures)
            Me.Controls.Add(Me.grpAppearance)
            Me.Controls.Add(Me.grpGeneral)
            Me.Controls.Add(Me.btnSettings)
            Me.FormBorderStyle = FormBorderStyle.FixedDialog
            Me.MaximizeBox = False
            Me.MinimizeBox = False
            Me.Name = "SettingsForm"
            Me.StartPosition = FormStartPosition.CenterParent
            Me.Text = "Settings"
            Me.grpGeneral.ResumeLayout(False)
            Me.grpGeneral.PerformLayout()
            Me.grpAppearance.ResumeLayout(False)
            Me.grpAppearance.PerformLayout()
            Me.grpFeatures.ResumeLayout(False)
            Me.grpFeatures.PerformLayout()
            Me.ResumeLayout(False)

        End Sub

        Friend WithEvents btnSettings As Button
        Friend WithEvents txtHomePage As TextBox
        Friend WithEvents txtSearchEngine As TextBox
        Friend WithEvents txtSidebarWidth As TextBox
        Friend WithEvents chkRememberSession As CheckBox
        Friend WithEvents chkEnableHistory As CheckBox
        Friend WithEvents chkEnableBookmarks As CheckBox
        Friend WithEvents cmbTheme As ComboBox
        Friend WithEvents btnSave As Button
        Friend WithEvents btnCancel As Button
        Friend WithEvents btnReset As Button
        Friend WithEvents grpGeneral As GroupBox
        Friend WithEvents grpAppearance As GroupBox
        Friend WithEvents grpFeatures As GroupBox
        Friend WithEvents Label1 As Label
        Friend WithEvents Label2 As Label
        Friend WithEvents Label3 As Label
        Friend WithEvents Label4 As Label
    End Class

End Namespace
