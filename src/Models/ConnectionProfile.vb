'ConnectionProfile.vb - Model for storing connection profiles
Namespace vbnetbrowser.Models

    Public Class ConnectionProfile
        Public Property Id As String
        Public Property Name As String
        Public Property Host As String
        Public Property Port As Integer
        Public Property Protocol As ProtocolType
        Public Property Username As String
        Public Property Password As String
        Public Property PrivateKeyPath As String
        Public Property BasePath As String
        Public Property IsDefault As Boolean
        Public Property LastConnected As DateTime?
        Public Property CreatedAt As DateTime
        Public Property UpdatedAt As DateTime

        Public Sub New()
            Id = Guid.NewGuid().ToString()
            Protocol = ProtocolType.Sftp
            Port = 22
            CreatedAt = DateTime.UtcNow
            UpdatedAt = DateTime.UtcNow
        End Sub

        Public Overrides Function ToString() As String
            Return $"{Name} ({Protocol}://{Host}:{Port})"
        End Function
    End Class

    Public Enum ProtocolType
        Http
        Https
        Ftp
        Ftps
        Sftp
        Local
    End Enum

    Public Class ConnectionProfileCollection
        Public Property Profiles As New List(Of ConnectionProfile)
        Public Property DefaultProfileId As String

        Public Function GetDefaultProfile() As ConnectionProfile
            If String.IsNullOrEmpty(DefaultProfileId) Then
                Return Profiles.FirstOrDefault()
            End If
            Return Profiles.FirstOrDefault(Function(p) p.Id = DefaultProfileId)
        End Function

        Public Function GetProfileById(id As String) As ConnectionProfile
            Return Profiles.FirstOrDefault(Function(p) p.Id = id)
        End Function
    End Class
End Namespace
