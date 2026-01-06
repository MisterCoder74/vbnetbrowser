Imports System.Windows.Forms

Namespace vbnetbrowser.Forms

    Public Partial Class LoginForm
        Inherits Form

        Public Property Username As String = ""

        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub btnLogin_Click(sender As Object, e As EventArgs) Handles btnLogin.Click
            If String.IsNullOrWhiteSpace(txtUsername.Text) Then
                lblError.Text = "Please enter a username"
                lblError.Visible = True
                Return
            End If

            If String.IsNullOrWhiteSpace(txtPassword.Text) Then
                lblError.Text = "Please enter a password"
                lblError.Visible = True
                Return
            End If

            ' For now, accept any credentials (stub implementation)
            ' In production, implement proper authentication
            Username = txtUsername.Text.Trim()
            Me.DialogResult = DialogResult.OK
            Me.Close()
        End Sub

        Private Sub txtUsername_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtUsername.KeyPress
            If e.KeyChar = Convert.ToChar(13) Then
                txtPassword.Focus()
            End If
        End Sub

        Private Sub txtPassword_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtPassword.KeyPress
            If e.KeyChar = Convert.ToChar(13) Then
                btnLogin.PerformClick()
            End If
        End Sub

        Private Sub txtUsername_TextChanged(sender As Object, e As EventArgs) Handles txtUsername.TextChanged
            lblError.Visible = False
        End Sub

        Private Sub txtPassword_TextChanged(sender As Object, e As EventArgs) Handles txtPassword.TextChanged
            lblError.Visible = False
        End Sub

        Private Sub LoginForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
            lblError.Visible = False
            txtUsername.Focus()
        End Sub
    End Class

End Namespace
