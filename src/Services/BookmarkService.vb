Imports System.IO
Imports System.Text.Json
Imports vbnetbrowser.Models

Namespace vbnetbrowser.Services

    Public Class BookmarkService
        Private Const BOOKMARKS_FILE As String = "data/bookmarks.json"
        Private _bookmarks As List(Of Bookmark)

        Public Sub New()
            _bookmarks = New List(Of Bookmark)()
            LoadBookmarks()
        End Sub

        Public Function GetBookmarks() As List(Of Bookmark)
            Return New List(Of Bookmark)(_bookmarks)
        End Function

        Public Function AddBookmark(title As String, url As String) As Boolean
            Try
                Dim bookmark As New Bookmark(title, url)
                _bookmarks.Add(bookmark)
                SaveBookmarks()
                Return True
            Catch ex As Exception
                Return False
            End Try
        End Function

        Public Function DeleteBookmark(id As String) As Boolean
            Try
                Dim bookmark As Bookmark = _bookmarks.FirstOrDefault(Function(b) b.Id = id)
                If bookmark IsNot Nothing Then
                    _bookmarks.Remove(bookmark)
                    SaveBookmarks()
                    Return True
                End If
                Return False
            Catch ex As Exception
                Return False
            End Try
        End Function

        Private Sub LoadBookmarks()
            Try
                If File.Exists(BOOKMARKS_FILE) Then
                    Dim json As String = File.ReadAllText(BOOKMARKS_FILE)
                    _bookmarks = JsonSerializer.Deserialize(Of List(Of Bookmark))(json)
                    If _bookmarks Is Nothing Then
                        _bookmarks = New List(Of Bookmark)()
                    End If
                Else
                    _bookmarks = New List(Of Bookmark)()
                    SaveBookmarks()
                End If
            Catch ex As Exception
                _bookmarks = New List(Of Bookmark)()
            End Try
        End Sub

        Private Sub SaveBookmarks()
            Try
                Dim directory As String = Path.GetDirectoryName(BOOKMARKS_FILE)
                If Not Directory.Exists(directory) Then
                    Directory.CreateDirectory(directory)
                End If

                Dim options As New JsonSerializerOptions With {
                    .WriteIndented = True
                }
                Dim json As String = JsonSerializer.Serialize(_bookmarks, options)
                File.WriteAllText(BOOKMARKS_FILE, json)
            Catch ex As Exception
                Throw
            End Try
        End Sub
    End Class

End Namespace
