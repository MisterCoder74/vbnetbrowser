Namespace vbnetbrowser.Forms

    Partial Class LoginForm
        Private components As System.ComponentModel.IContainer = Nothing

        Protected Overrides Sub Dispose(disposing As Boolean)
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
            MyBase.Dispose(disposing)
        End Sub

        Private Sub InitializeComponent()
            Me.lblTitle = New Label()
            Me.lblUsername = New Label()
            Me.txtUsername = New TextBox()
            Me.lblPassword = New Label()
            Me.txtPassword = New TextBox()
            Me.btnLogin = New Button()
            Me.lblError = New Label()
            Me.SuspendLayout()
            '
            ' lblTitle
            '
            Me.lblTitle.AutoSize = True
            Me.lblTitle.Font = New Font("Segoe UI", 16F, FontStyle.Bold)
            Me.lblTitle.Location = New Point(85, 30)
            Me.lblTitle.Name = "lblTitle"
            Me.lblTitle.Size = New Size(200, 31)
            Me.lblTitle.TabIndex = 0
            Me.lblTitle.Text = "VB.NET Browser"
            '
            ' lblUsername
            '
            Me.lblUsername.AutoSize = True
            Me.lblUsername.Font = New Font("Segoe UI", 9F)
            Me.lblUsername.Location = New Point(50, 90)
            Me.lblUsername.Name = "lblUsername"
            Me.lblUsername.Size = New Size(56, 15)
            Me.lblUsername.TabIndex = 1
            Me.lblUsername.Text = "Username:"
            '
            ' txtUsername
            '
            Me.txtUsername.Font = New Font("Segoe UI", 9F)
            Me.txtUsername.Location = New Point(120, 87)
            Me.txtUsername.Name = "txtUsername"
            Me.txtUsername.Size = New Size(200, 23)
            Me.txtUsername.TabIndex = 2
            '
            ' lblPassword
            '
            Me.lblPassword.AutoSize = True
            Me.lblPassword.Font = New Font("Segoe UI", 9F)
            Me.lblPassword.Location = New Point(50, 130)
            Me.lblPassword.Name = "lblPassword"
            Me.lblPassword.Size = New Size(56, 15)
            Me.lblPassword.TabIndex = 3
            Me.lblPassword.Text = "Password:"
            '
            ' txtPassword
            '
            Me.txtPassword.Font = New Font("Segoe UI", 9F)
            Me.txtPassword.Location = New Point(120, 127)
            Me.txtPassword.Name = "txtPassword"
            Me.txtPassword.PasswordChar = "*"
            Me.txtPassword.Size = New Size(200, 23)
            Me.txtPassword.TabIndex = 4
            '
            ' btnLogin
            '
            Me.btnLogin.Font = New Font("Segoe UI", 9F, FontStyle.Bold)
            Me.btnLogin.Location = New Point(120, 170)
            Me.btnLogin.Name = "btnLogin"
            Me.btnLogin.Size = New Size(200, 35)
            Me.btnLogin.TabIndex = 5
            Me.btnLogin.Text = "Login"
            Me.btnLogin.UseVisualStyleBackColor = True
            '
            ' lblError
            '
            Me.lblError.AutoSize = True
            Me.lblError.Font = New Font("Segoe UI", 9F)
            Me.lblError.ForeColor = Color.Red
            Me.lblError.Location = New Point(50, 215)
            Me.lblError.Name = "lblError"
            Me.lblError.Size = New Size(270, 15)
            Me.lblError.TabIndex = 6
            Me.lblError.Text = ""
            Me.lblError.Visible = False
            '
            ' LoginForm
            '
            Me.AutoScaleDimensions = New SizeF(7F, 15F)
            Me.AutoScaleMode = AutoScaleMode.Font
            Me.ClientSize = New Size(370, 250)
            Me.Controls.Add(Me.lblError)
            Me.Controls.Add(Me.btnLogin)
            Me.Controls.Add(Me.txtPassword)
            Me.Controls.Add(Me.lblPassword)
            Me.Controls.Add(Me.txtUsername)
            Me.Controls.Add(Me.lblUsername)
            Me.Controls.Add(Me.lblTitle)
            Me.FormBorderStyle = FormBorderStyle.FixedDialog
            Me.MaximizeBox = False
            Me.MinimizeBox = False
            Me.Name = "LoginForm"
            Me.StartPosition = FormStartPosition.CenterScreen
            Me.Text = "Login"
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub

        Friend WithEvents lblTitle As Label
        Friend WithEvents lblUsername As Label
        Friend WithEvents txtUsername As TextBox
        Friend WithEvents lblPassword As Label
        Friend WithEvents txtPassword As TextBox
        Friend WithEvents btnLogin As Button
        Friend WithEvents lblError As Label
    End Class

End Namespace
