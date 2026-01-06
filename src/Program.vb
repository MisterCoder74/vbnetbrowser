Imports Microsoft.Web.WebView2.Core
Imports vbnetbrowser.Forms

Namespace vbnetbrowser

    Module Program
        <STAThread>
        Public Sub Main()
            Application.EnableVisualStyles()
            Application.SetCompatibleTextRenderingDefault(False)

            ' Ensure WebView2 runtime is available
            EnsureWebView2Runtime()

            ' Show login form first
            Dim loginForm As New LoginForm()
            If loginForm.ShowDialog() = DialogResult.OK Then
                ' Login successful, show main form
                Application.Run(New MainForm())
            End If
        End Sub

        Private Sub EnsureWebView2Runtime()
            Try
                Dim environment As CoreWebView2Environment = CoreWebView2Environment.CreateAsync().Result
            Catch ex As Exception
                MessageBox.Show(
                    "WebView2 Runtime is not installed. Please install it from: " &
                    "https://developer.microsoft.com/microsoft-edge/webview2/" &
                    vbCrLf & vbCrLf & "Error: " & ex.Message,
                    "WebView2 Runtime Required",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                )
                Environment.Exit(1)
            End Try
        End Sub
    End Module

End Namespace
