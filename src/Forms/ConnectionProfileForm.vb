'ConnectionProfileForm.vb - Form for managing connection profiles
Imports System.IO
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks

Namespace vbnetbrowser.Forms

    Public Class ConnectionProfileForm
        Inherits Form

        Private ReadOnly _connectionManager As ConnectionManager
        Private _editingProfile As Models.ConnectionProfile

        ' Controls
        Private txtName As TextBox
        Private txtHost As TextBox
        Private numPort As NumericUpDown
        Private cmbProtocol As ComboBox
        Private txtUsername As TextBox
        Private txtPassword As TextBox
        Private txtPrivateKeyPath As TextBox
        Private btnBrowseKey As Button
        Private txtBasePath As TextBox
        Private chkSavePassword As CheckBox
        Private btnTest As Button
        Private btnSave As Button
        Private btnCancel As Button
        Private lblStatus As Label
        Private pnlCredentials As Panel
        Private pnlSftp As Panel

        Public Property ResultProfile As Models.ConnectionProfile

        Public Sub New(connectionManager As ConnectionManager, Optional profile As Models.ConnectionProfile = Nothing)
            _connectionManager = connectionManager
            _editingProfile = profile
            InitializeComponent()
            If profile IsNot Nothing Then
                LoadProfile(profile)
            Else
                cmbProtocol.SelectedIndex = 0
            End If
        End Sub

        Private Sub InitializeComponent()
            Me.Text = If(_editingProfile IsNot Nothing, "Edit Connection Profile", "New Connection Profile")
            Me.Size = New Size(450, 420)
            Me.StartPosition = FormStartPosition.CenterParent
            Me.FormBorderStyle = FormBorderStyle.FixedDialog
            Me.MaximizeBox = False
            Me.MinimizeBox = False

            Dim y As Integer = 10
            Dim labelWidth As Integer = 100
            Dim controlLeft As Integer = 120
            Dim controlWidth As Integer = 280

            ' Name
            Dim lblName As New Label() With {
                .Text = "Name:",
                .Location = New Point(10, y),
                .AutoSize = True
            }
            txtName = New TextBox() With {
                .Location = New Point(controlLeft, y - 2),
                .Width = controlWidth
            }
            y += 30

            ' Protocol
            Dim lblProtocol As New Label() With {
                .Text = "Protocol:",
                .Location = New Point(10, y),
                .AutoSize = True
            }
            cmbProtocol = New ComboBox() With {
                .Location = New Point(controlLeft, y - 2),
                .Width = controlWidth,
                .DropDownStyle = ComboBoxStyle.DropDownList
            }
            cmbProtocol.Items.AddRange({
                "HTTP", "HTTPS", "FTP", "FTPS", "SFTP", "Local"
            })
            AddHandler cmbProtocol.SelectedIndexChanged, AddressOf CmbProtocol_SelectedIndexChanged
            y += 30

            ' Host
            Dim lblHost As New Label() With {
                .Text = "Host:",
                .Location = New Point(10, y),
                .AutoSize = True
            }
            txtHost = New TextBox() With {
                .Location = New Point(controlLeft, y - 2),
                .Width = controlWidth - 80
            }
            numPort = New NumericUpDown() With {
                .Location = New Point(controlLeft + controlWidth - 70, y - 2),
                .Width = 70,
                .Minimum = 0,
                .Maximum = 65535,
                .Value = 22
            }
            y += 30

            ' Credentials panel
            pnlCredentials = New Panel() With {
                .Location = New Point(10, y),
                .Width = 420,
                .Height = 100,
                .BorderStyle = BorderStyle.FixedSingle
            }

            Dim credY As Integer = 5

            Dim lblUsername As New Label() With {
                .Text = "Username:",
                .Location = New Point(10, credY),
                .AutoSize = True
            }
            txtUsername = New TextBox() With {
                .Location = New Point(100, credY - 2),
                .Width = 290
            }

            credY += 30

            Dim lblPassword As New Label() With {
                .Text = "Password:",
                .Location = New Point(10, credY),
                .AutoSize = True
            }
            txtPassword = New TextBox() With {
                .Location = New Point(100, credY - 2),
                .Width = 290,
                .PasswordChar = "*"c
            }

            credY += 30

            chkSavePassword = New CheckBox() With {
                .Text = "Save password (not encrypted)",
                .Location = New Point(10, credY),
                .AutoSize = True
            }

            pnlCredentials.Controls.AddRange({lblUsername, txtUsername, lblPassword, txtPassword, chkSavePassword})

            ' SFTP panel
            pnlSftp = New Panel() With {
                .Location = New Point(10, y + 110),
                .Width = 420,
                .Height = 70,
                .BorderStyle = BorderStyle.FixedSingle,
                .Visible = False
            }

            Dim sftpY As Integer = 5

            Dim lblPrivateKey As New Label() With {
                .Text = "Private Key:",
                .Location = New Point(10, sftpY),
                .AutoSize = True
            }
            txtPrivateKeyPath = New TextBox() With {
                .Location = New Point(100, sftpY - 2),
                .Width = 250
            }
            btnBrowseKey = New Button() With {
                .Text = "Browse...",
                .Location = New Point(360, sftpY - 3),
                .Width = 50
            }
            AddHandler btnBrowseKey.Click, AddressOf BtnBrowseKey_Click

            pnlSftp.Controls.AddRange({lblPrivateKey, txtPrivateKeyPath, btnBrowseKey})
            y += 180

            ' Base path
            Dim lblBasePath As New Label() With {
                .Text = "Base Path:",
                .Location = New Point(10, y),
                .AutoSize = True
            }
            txtBasePath = New TextBox() With {
                .Location = New Point(controlLeft, y - 2),
                .Width = controlWidth
            }
            y += 30

            ' Status
            lblStatus = New Label() With {
                .Text = "",
                .Location = New Point(10, y),
                .AutoSize = True,
                .ForeColor = Color.Blue
            }
            y += 30

            ' Buttons
            btnTest = New Button() With {
                .Text = "Test Connection",
                .Location = New Point(100, y),
                .Width = 110,
                .Height = 30,
                .DialogResult = DialogResult.None
            }

            btnSave = New Button() With {
                .Text = "Save",
                .Location = New Point(220, y),
                .Width = 80,
                .Height = 30,
                .DialogResult = DialogResult.OK
            }

            btnCancel = New Button() With {
                .Text = "Cancel",
                .Location = New Point(310, y),
                .Width = 80,
                .Height = 30,
                .DialogResult = DialogResult.Cancel
            }

            Me.Controls.AddRange({
                lblName, txtName, lblProtocol, cmbProtocol,
                lblHost, txtHost, numPort, pnlCredentials,
                pnlSftp, lblBasePath, txtBasePath, lblStatus,
                btnTest, btnSave, btnCancel
            })

            AddHandler btnTest.Click, AddressOf BtnTest_Click
            AddHandler btnSave.Click, AddressOf BtnSave_Click
            AddHandler Me.FormClosing, AddressOf ConnectionProfileForm_FormClosing

            Me.AcceptButton = btnSave
            Me.CancelButton = btnCancel
        End Sub

        Private Sub LoadProfile(profile As Models.ConnectionProfile)
            txtName.Text = profile.Name
            txtHost.Text = profile.Host
            numPort.Value = profile.Port
            cmbProtocol.SelectedItem = profile.Protocol.ToString()
            txtUsername.Text = profile.Username
            txtPassword.Text = profile.Password
            txtPrivateKeyPath.Text = profile.PrivateKeyPath
            txtBasePath.Text = profile.BasePath
        End Sub

        Private Sub CmbProtocol_SelectedIndexChanged(sender As Object, e As EventArgs)
            Dim protocol As String = cmbProtocol.SelectedItem.ToString()

            Select Case protocol
                Case "SFTP"
                    numPort.Value = 22
                    pnlSftp.Visible = True
                    pnlCredentials.Visible = True
                Case "FTP", "FTPS"
                    numPort.Value = If(protocol = "FTPS", 990, 21)
                    pnlSftp.Visible = False
                    pnlCredentials.Visible = True
                Case "HTTP", "HTTPS"
                    numPort.Value = If(protocol = "HTTPS", 443, 80)
                    pnlSftp.Visible = False
                    pnlCredentials.Visible = String.IsNullOrEmpty(txtUsername.Text)
                Case "Local"
                    numPort.Value = 0
                    pnlSftp.Visible = False
                    pnlCredentials.Visible = False
            End Select
        End Sub

        Private Sub BtnBrowseKey_Click(sender As Object, e As EventArgs)
            Using openDialog As New OpenFileDialog()
                openDialog.Filter = "Private Key Files (*.ppk, *.pem)|*.ppk;*.pem|All Files (*.*)|*.*"
                openDialog.Title = "Select Private Key File"
                openDialog.CheckFileExists = True

                If openDialog.ShowDialog() = DialogResult.OK Then
                    txtPrivateKeyPath.Text = openDialog.FileName
                End If
            End Using
        End Sub

        Private Async Sub BtnTest_Click(sender As Object, e As EventArgs)
            If Not ValidateInput() Then Return

            lblStatus.Text = "Testing connection..."
            btnTest.Enabled = False

            Dim profile As Models.ConnectionProfile = CreateProfile()

            Using tempConnection As New ConnectionManager()
                Try
                    Dim connected As Boolean = Await tempConnection.ConnectAsync(profile, CancellationToken.None)

                    If connected Then
                        lblStatus.Text = "Connection successful!"
                        lblStatus.ForeColor = Color.Green
                        tempConnection.Disconnect()
                    Else
                        lblStatus.Text = "Connection failed."
                        lblStatus.ForeColor = Color.Red
                    End If
                Catch ex As Exception
                    lblStatus.Text = $"Error: {ex.Message}"
                    lblStatus.ForeColor = Color.Red
                Finally
                    btnTest.Enabled = True
                End Try
            End Using
        End Sub

        Private Sub BtnSave_Click(sender As Object, e As EventArgs)
            If Not ValidateInput() Then
                DialogResult = DialogResult.None
                Return
            End If

            ResultProfile = CreateProfile()

            If _editingProfile IsNot Nothing Then
                ResultProfile.Id = _editingProfile.Id
            End If

            _connectionManager.AddProfile(ResultProfile)
        End Sub

        Private Sub ConnectionProfileForm_FormClosing(sender As Object, e As FormClosingEventArgs)
            If DialogResult = DialogResult.OK Then
                If Not ValidateInput() Then
                    e.Cancel = True
                End If
            End If
        End Sub

        Private Function ValidateInput() As Boolean
            If String.IsNullOrWhiteSpace(txtName.Text) Then
                lblStatus.Text = "Please enter a name for this connection."
                lblStatus.ForeColor = Color.Red
                txtName.Focus()
                Return False
            End If

            Dim protocol As String = cmbProtocol.SelectedItem.ToString()

            If protocol <> "Local" Then
                If String.IsNullOrWhiteSpace(txtHost.Text) Then
                    lblStatus.Text = "Please enter a host address."
                    lblStatus.ForeColor = Color.Red
                    txtHost.Focus()
                    Return False
                End If
            End If

            If protocol = "SFTP" OrElse protocol = "FTP" OrElse protocol = "FTPS" Then
                If String.IsNullOrWhiteSpace(txtUsername.Text) Then
                    lblStatus.Text = "Username is required for this protocol."
                    lblStatus.ForeColor = Color.Red
                    txtUsername.Focus()
                    Return False
                End If
            End If

            lblStatus.Text = ""
            Return True
        End Function

        Private Function CreateProfile() As Models.ConnectionProfile
            Dim profile As Models.ConnectionProfile

            If _editingProfile IsNot Nothing Then
                profile = _editingProfile
            Else
                profile = New Models.ConnectionProfile()
            End If

            profile.Name = txtName.Text.Trim()
            profile.Host = txtHost.Text.Trim()
            profile.Port = CInt(numPort.Value)
            profile.Protocol = ParseProtocol(cmbProtocol.SelectedItem.ToString())
            profile.Username = txtUsername.Text.Trim()
            profile.Password = If(chkSavePassword.Checked, txtPassword.Text, "")
            profile.PrivateKeyPath = txtPrivateKeyPath.Text.Trim()
            profile.BasePath = txtBasePath.Text.Trim()
            profile.UpdatedAt = DateTime.UtcNow

            Return profile
        End Function

        Private Function ParseProtocol(protocol As String) As Models.ProtocolType
            Select Case protocol
                Case "HTTP" : Return Models.ProtocolType.Http
                Case "HTTPS" : Return Models.ProtocolType.Https
                Case "FTP" : Return Models.ProtocolType.Ftp
                Case "FTPS" : Return Models.ProtocolType.Ftps
                Case "SFTP" : Return Models.ProtocolType.Sftp
                Case "Local" : Return Models.ProtocolType.Local
                Case Else : Return Models.ProtocolType.Local
            End Select
        End Function
    End Class
End Namespace
