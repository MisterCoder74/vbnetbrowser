'ConnectionManager.vb - Manages connections and profiles
Imports System.IO
Imports System.Text
Imports System.Text.Json
Imports System.Threading
Imports System.Threading.Tasks

Namespace vbnetbrowser.Services

    Public Class ConnectionManager
        Private ReadOnly _profilesFile As String = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "connections.json")
        Private ReadOnly _settingsFile As String = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "editor_settings.json")
        Private _profiles As Models.ConnectionProfileCollection
        Private _editorSettings As Models.EditorSettings
        Private _activeService As FileTransferService
        Private _activeProfile As Models.ConnectionProfile
        Private ReadOnly _lock As New Object()

        Public Event ConnectionChanged As EventHandler(Of FileTransferService)
        Public Event ProfilesChanged As EventHandler
        Public Event SettingsChanged As EventHandler(Of Models.EditorSettings)

        Public Sub New()
            LoadProfiles()
            LoadEditorSettings()
        End Sub

        Public ReadOnly Property Profiles As List(Of Models.ConnectionProfile)
            Get
                Return _profiles.Profiles
            End Get
        End Property

        Public ReadOnly Property EditorSettings As Models.EditorSettings
            Get
                Return _editorSettings
            End Get
        End Property

        Public ReadOnly Property ActiveService As FileTransferService
            Get
                Return _activeService
            End Get
        End Property

        Public ReadOnly Property ActiveProfile As Models.ConnectionProfile
            Get
                Return _activeProfile
            End Get
        End Property

        Public ReadOnly Property IsConnected As Boolean
            Get
                Return _activeService IsNot Nothing AndAlso _activeService.IsConnected
            End Get
        End Property

        Public Sub LoadProfiles()
            SyncLock _lock
                Try
                    If File.Exists(_profilesFile) Then
                        Dim json As String = File.ReadAllText(_profilesFile)
                        _profiles = JsonSerializer.Deserialize(Of Models.ConnectionProfileCollection)(json) ?? New Models.ConnectionProfileCollection()
                    Else
                        _profiles = New Models.ConnectionProfileCollection()
                    End If
                Catch ex As Exception
                    _profiles = New Models.ConnectionProfileCollection()
                End Try
            End SyncLock
        End Sub

        Public Sub SaveProfiles()
            SyncLock _lock
                Try
                    Dim options As New JsonSerializerOptions With {.WriteIndented = True}
                    Dim json As String = JsonSerializer.Serialize(_profiles, options)
                    Directory.CreateDirectory(Path.GetDirectoryName(_profilesFile))
                    File.WriteAllText(_profilesFile, json)
                Catch ex As Exception
                    ' Log error but don't throw
                End Try
            End SyncLock
        End Sub

        Public Sub LoadEditorSettings()
            SyncLock _lock
                Try
                    If File.Exists(_settingsFile) Then
                        Dim json As String = File.ReadAllText(_settingsFile)
                        _editorSettings = JsonSerializer.Deserialize(Of Models.EditorSettings)(json) ?? New Models.EditorSettings()
                    Else
                        _editorSettings = New Models.EditorSettings()
                    End If
                Catch ex As Exception
                    _editorSettings = New Models.EditorSettings()
                End Try
            End SyncLock
        End Sub

        Public Sub SaveEditorSettings()
            SyncLock _lock
                Try
                    Dim options As New JsonSerializerOptions With {.WriteIndented = True}
                    Dim json As String = JsonSerializer.Serialize(_editorSettings, options)
                    Directory.CreateDirectory(Path.GetDirectoryName(_settingsFile))
                    File.WriteAllText(_settingsFile, json)
                Catch ex As Exception
                    ' Log error but don't throw
                End Try
            End SyncLock
        End Sub

        Public Function AddProfile(profile As Models.ConnectionProfile) As Boolean
            SyncLock _lock
                If _profiles.Profiles.Any(Function(p) p.Id = profile.Id) Then
                    ' Update existing
                    Dim existing As Models.ConnectionProfile = _profiles.Profiles.First(Function(p) p.Id = profile.Id)
                    existing.Name = profile.Name
                    existing.Host = profile.Host
                    existing.Port = profile.Port
                    existing.Protocol = profile.Protocol
                    existing.Username = profile.Username
                    existing.Password = profile.Password
                    existing.PrivateKeyPath = profile.PrivateKeyPath
                    existing.BasePath = profile.BasePath
                    existing.UpdatedAt = DateTime.UtcNow
                Else
                    ' Add new
                    _profiles.Profiles.Add(profile)
                End If
                SaveProfiles()
                RaiseEvent ProfilesChanged(Me, EventArgs.Empty)
                Return True
            End SyncLock
        End Function

        Public Function DeleteProfile(profileId As String) As Boolean
            SyncLock _lock
                Dim profile As Models.ConnectionProfile = _profiles.Profiles.FirstOrDefault(Function(p) p.Id = profileId)
                If profile IsNot Nothing Then
                    _profiles.Profiles.Remove(profile)
                    SaveProfiles()
                    RaiseEvent ProfilesChanged(Me, EventArgs.Empty)
                    Return True
                End If
                Return False
            End SyncLock
        End Function

        Public Function GetProfileById(profileId As String) As Models.ConnectionProfile
            Return _profiles.GetProfileById(profileId)
        End Function

        Public Function GetDefaultProfile() As Models.ConnectionProfile
            Return _profiles.GetDefaultProfile()
        End Function

        Public Sub SetDefaultProfile(profileId As String)
            _profiles.DefaultProfileId = profileId
            SaveProfiles()
        End Sub

        Public Async Function ConnectAsync(profileId As String, cancellationToken As CancellationToken) As Task(Of Boolean)
            Dim profile As Models.ConnectionProfile = GetProfileById(profileId)
            If profile Is Nothing Then Return False

            Return Await ConnectAsync(profile, cancellationToken)
        End Function

        Public Async Function ConnectAsync(profile As Models.ConnectionProfile, cancellationToken As CancellationToken) As Task(Of Boolean)
            Disconnect()

            _activeProfile = profile
            _activeService = CreateServiceForProfile(profile)

            AddHandler _activeService.ConnectionStateChanged, AddressOf OnConnectionStateChanged

            Dim connected As Boolean = Await _activeService.ConnectAsync(profile, cancellationToken)

            If connected Then
                profile.LastConnected = DateTime.UtcNow
                profile.UpdatedAt = DateTime.UtcNow
                AddProfile(profile)
                RaiseEvent ConnectionChanged(Me, _activeService)
            Else
                _activeService = Nothing
                _activeProfile = Nothing
            End If

            Return connected
        End Function

        Public Sub Disconnect()
            If _activeService IsNot Nothing Then
                RemoveHandler _activeService.ConnectionStateChanged, AddressOf OnConnectionStateChanged
                _activeService.Disconnect()
                _activeService = Nothing
            End If
            _activeProfile = Nothing
            RaiseEvent ConnectionChanged(Me, Nothing)
        End Sub

        Public Sub UpdateEditorSettings(settings As Models.EditorSettings)
            _editorSettings = settings
            SaveEditorSettings()
            RaiseEvent SettingsChanged(Me, settings)
        End Function

        Private Function CreateServiceForProfile(profile As Models.ConnectionProfile) As FileTransferService
            Select Case profile.Protocol
                Case Models.ProtocolType.Http, Models.ProtocolType.Https
                    Return New HttpFileService()
                Case Models.ProtocolType.Ftp, Models.ProtocolType.Ftps
                    Return New FtpFileService()
                Case Models.ProtocolType.Sftp
                    Return New SftpFileService()
                Case Models.ProtocolType.Local
                    Return New LocalFileService()
                Case Else
                    Return New LocalFileService()
            End Select
        End Function

        Private Sub OnConnectionStateChanged(sender As Object, e As Models.ConnectionProfile)
            RaiseEvent ConnectionChanged(Me, _activeService)
        End Sub

        Public Function DetectProtocolFromUrl(url As String) As Models.ProtocolType
            If String.IsNullOrEmpty(url) Then Return Models.ProtocolType.Local

            url = url.Trim().ToLowerInvariant()

            If url.StartsWith("sftp://") Then Return Models.ProtocolType.Sftp
            If url.StartsWith("ftp://") Then Return Models.ProtocolType.Ftp
            If url.StartsWith("ftps://") Then Return Models.ProtocolType.Ftps
            If url.StartsWith("https://") Then Return Models.ProtocolType.Https
            If url.StartsWith("http://") Then Return Models.ProtocolType.Http

            ' Default based on port or common patterns
            If url.Contains(":22") OrElse url.Contains(":2222") Then Return Models.ProtocolType.Sftp
            If url.Contains(":21") Then Return Models.ProtocolType.Ftp

            Return Models.ProtocolType.Local
        End Function
    End Class
End Namespace
