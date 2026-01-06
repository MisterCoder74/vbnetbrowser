Imports System.Windows.Forms
Imports vbnetbrowser.Services
Imports vbnetbrowser.Models

Namespace vbnetbrowser.Forms

    Public Partial Class SettingsForm
        Inherits Form

        Private _configService As ConfigService
        Private _config As AppConfig

        Public Sub New(configService As ConfigService)
            _configService = configService
            _config = configService.GetConfig()
            InitializeComponent()
            LoadSettings()
        End Sub

        Private Sub LoadSettings()
            txtHomePage.Text = _config.HomePage
            txtSearchEngine.Text = _config.DefaultSearchEngine
            txtSidebarWidth.Text = _config.SidebarWidth.ToString()
            chkRememberSession.Checked = _config.RememberSession
            chkEnableHistory.Checked = _config.EnableHistory
            chkEnableBookmarks.Checked = _config.EnableBookmarks
            cmbTheme.SelectedItem = _config.Theme
        End Sub

        Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
            Try
                Dim newConfig As New AppConfig() With {
                    .HomePage = txtHomePage.Text.Trim(),
                    .DefaultSearchEngine = txtSearchEngine.Text.Trim(),
                    .SidebarWidth = Integer.Parse(txtSidebarWidth.Text),
                    .RememberSession = chkRememberSession.Checked,
                    .SidebarCollapsed = _config.SidebarCollapsed,
                    .Theme = If(cmbTheme.SelectedItem?.ToString(), "Light"),
                    .EnableHistory = chkEnableHistory.Checked,
                    .EnableBookmarks = chkEnableBookmarks.Checked
                }

                If _configService.SaveConfig(newConfig) Then
                    MessageBox.Show(
                        "Settings saved successfully!",
                        "Success",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    )
                    Me.DialogResult = DialogResult.OK
                    Me.Close()
                Else
                    MessageBox.Show(
                        "Failed to save settings.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    )
                End If
            Catch ex As Exception
                MessageBox.Show(
                    "Invalid settings. Please check your input." & vbCrLf & vbCrLf & "Error: " & ex.Message,
                    "Validation Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                )
            End Try
        End Sub

        Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
            Me.DialogResult = DialogResult.Cancel
            Me.Close()
        End Sub

        Private Sub btnReset_Click(sender As Object, e As EventArgs) Handles btnReset.Click
            If MessageBox.Show(
                "Are you sure you want to reset all settings to defaults?",
                "Confirm Reset",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            ) = DialogResult.Yes Then
                Dim defaultConfig As New AppConfig()
                txtHomePage.Text = defaultConfig.HomePage
                txtSearchEngine.Text = defaultConfig.DefaultSearchEngine
                txtSidebarWidth.Text = defaultConfig.SidebarWidth.ToString()
                chkRememberSession.Checked = defaultConfig.RememberSession
                chkEnableHistory.Checked = defaultConfig.EnableHistory
                chkEnableBookmarks.Checked = defaultConfig.EnableBookmarks
                cmbTheme.SelectedItem = defaultConfig.Theme
            End If
        End Sub

        Private Sub SettingsForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
            cmbTheme.Items.AddRange({"Light", "Dark"})
            If cmbTheme.Items.Contains(_config.Theme) Then
                cmbTheme.SelectedItem = _config.Theme
            Else
                cmbTheme.SelectedIndex = 0
            End If
        End Sub
    End Class

End Namespace
