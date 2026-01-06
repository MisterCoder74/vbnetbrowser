Imports System.IO
Imports System.Text.Json
Imports vbnetbrowser.Models

Namespace vbnetbrowser.Services

    Public Class ConfigService
        Private Const CONFIG_FILE As String = "data/config.json"
        Private _config As AppConfig

        Public Sub New()
            _config = New AppConfig()
            LoadConfig()
        End Sub

        Public Function GetConfig() As AppConfig
            Return _config
        End Function

        Public Function SaveConfig(config As AppConfig) As Boolean
            Try
                _config = config
                SaveConfigToFile()
                Return True
            Catch ex As Exception
                Return False
            End Try
        End Function

        Public Function UpdateHomePage(url As String) As Boolean
            Try
                _config.HomePage = url
                SaveConfigToFile()
                Return True
            Catch ex As Exception
                Return False
            End Try
        End Function

        Public Function UpdateSearchEngine(url As String) As Boolean
            Try
                _config.DefaultSearchEngine = url
                SaveConfigToFile()
                Return True
            Catch ex As Exception
                Return False
            End Try
        End Function

        Public Function UpdateSidebarWidth(width As Integer) As Boolean
            Try
                _config.SidebarWidth = width
                SaveConfigToFile()
                Return True
            Catch ex As Exception
                Return False
            End Try
        End Function

        Public Function UpdateSidebarCollapsed(collapsed As Boolean) As Boolean
            Try
                _config.SidebarCollapsed = collapsed
                SaveConfigToFile()
                Return True
            End Try
        End Function

        Public Function UpdateTheme(theme As String) As Boolean
            Try
                _config.Theme = theme
                SaveConfigToFile()
                Return True
            Catch ex As Exception
                Return False
            End Try
        End Function

        Private Sub LoadConfig()
            Try
                If File.Exists(CONFIG_FILE) Then
                    Dim json As String = File.ReadAllText(CONFIG_FILE)
                    _config = JsonSerializer.Deserialize(Of AppConfig)(json)
                    If _config Is Nothing Then
                        _config = New AppConfig()
                    End If
                Else
                    _config = New AppConfig()
                    SaveConfigToFile()
                End If
            Catch ex As Exception
                _config = New AppConfig()
            End Try
        End Sub

        Private Sub SaveConfigToFile()
            Try
                Dim directory As String = Path.GetDirectoryName(CONFIG_FILE)
                If Not Directory.Exists(directory) Then
                    Directory.CreateDirectory(directory)
                End If

                Dim options As New JsonSerializerOptions With {
                    .WriteIndented = True
                }
                Dim json As String = JsonSerializer.Serialize(_config, options)
                File.WriteAllText(CONFIG_FILE, json)
            Catch ex As Exception
                Throw
            End Try
        End Sub
    End Class

End Namespace
