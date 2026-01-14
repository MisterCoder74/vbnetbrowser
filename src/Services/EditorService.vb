'EditorService.vb - Editor operations and file content management
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Threading
Imports System.Threading.Tasks

Namespace vbnetbrowser.Services

    Public Class EditorService

        Private ReadOnly _connectionManager As ConnectionManager
        Private _currentContent As String = ""
        Private _currentFilePath As String = ""
        Private _currentFileItem As Models.FileItem
        Private ReadOnly _undoStack As New Stack(Of String)()
        Private ReadOnly _redoStack As New Stack(Of String)()
        Private Const MaxUndoSteps As Integer = 100

        Public Event ContentChanged As EventHandler(Of String)
        Public Event FileLoaded As EventHandler(Of Models.FileItem)
        Public Event SaveCompleted As EventHandler(Of Boolean)

        Public Sub New(connectionManager As ConnectionManager)
            _connectionManager = connectionManager
        End Sub

        Public ReadOnly Property CurrentContent As String
            Get
                Return _currentContent
            End Get
        End Property

        Public ReadOnly Property CurrentFilePath As String
            Get
                Return _currentFilePath
            End Get
        End Property

        Public ReadOnly Property CurrentFileItem As Models.FileItem
            Get
                Return _currentFileItem
            End Get
        End Property

        Public ReadOnly Property HasChanges As Boolean
            Get
                Return Not String.IsNullOrEmpty(_currentContent)
            End Get
        End Property

        Public ReadOnly Property CanUndo As Boolean
            Get
                Return _undoStack.Count > 0
            End Get
        End Property

        Public ReadOnly Property CanRedo As Boolean
            Get
                Return _redoStack.Count > 0
            End Get
        End Property

        Public Async Function LoadFileAsync(path As String, Optional showProgress As Boolean = True) As Task(Of Boolean)
            If _connectionManager.ActiveService Is Nothing Then Return False

            Try
                Dim progress As IProgress(Of Models.OperationProgress) = Nothing
                If showProgress Then
                    progress = New Progress(Of Models.OperationProgress)(Sub(p)
                        ' Progress handled by caller
                    End Sub)
                End If

                Dim content As String = Await _connectionManager.ActiveService.DownloadFileAsync(path, progress, CancellationToken.None)
                _currentContent = content
                _currentFilePath = path
                _undoStack.Clear()
                _redoStack.Clear()

                RaiseEvent ContentChanged(Me, _currentContent)
                RaiseEvent FileLoaded(Me, _currentFileItem)

                Return True
            Catch ex As Exception
                Return False
            End Try
        End Function

        Public Async Function SaveFileAsync(Optional newContent As String = Nothing) As Task(Of Boolean)
            If _connectionManager.ActiveService Is Nothing OrElse String.IsNullOrEmpty(_currentFilePath) Then
                Return False
            End If

            Dim contentToSave As String = If(newContent IsNot Nothing, newContent, _currentContent)

            If String.IsNullOrEmpty(contentToSave) Then
                contentToSave = ""
            End If

            Try
                Dim progress As IProgress(Of Models.OperationProgress) = Nothing
                Await _connectionManager.ActiveService.UploadFileAsync(_currentFilePath, contentToSave, progress, CancellationToken.None)

                _currentContent = contentToSave
                _redoStack.Clear()

                RaiseEvent SaveCompleted(Me, True)
                Return True
            Catch ex As Exception
                RaiseEvent SaveCompleted(Me, False)
                Return False
            End Try
        End Function

        Public Sub SetContent(content As String)
            _currentContent = content
            RaiseEvent ContentChanged(Me, _currentContent)
        End Sub

        Public Sub Undo()
            If CanUndo Then
                _redoStack.Push(_currentContent)
                _currentContent = _undoStack.Pop()
                RaiseEvent ContentChanged(Me, _currentContent)
            End If
        End Sub

        Public Sub Redo()
            If CanRedo Then
                _undoStack.Push(_currentContent)
                _currentContent = _redoStack.Pop()
                RaiseEvent ContentChanged(Me, _currentContent)
            End If
        End Sub

        Public Sub PushUndoState()
            If _undoStack.Count >= MaxUndoSteps Then
                _undoStack.Pop()
            End If
            _undoStack.Push(_currentContent)
            _redoStack.Clear()
        End Sub

        Public Sub Cut()
            If Not String.IsNullOrEmpty(_currentContent) Then
                Clipboard.SetText(_currentContent)
                _currentContent = ""
                RaiseEvent ContentChanged(Me, _currentContent)
            End If
        End Sub

        Public Sub Copy()
            If Not String.IsNullOrEmpty(_currentContent) Then
                Clipboard.SetText(_currentContent)
            End If
        End Sub

        Public Sub Paste()
            If Clipboard.ContainsText() Then
                PushUndoState()
                _currentContent &= Clipboard.GetText()
                RaiseEvent ContentChanged(Me, _currentContent)
            End If
        End Sub

        Public Sub Delete()
            PushUndoState()
            _currentContent = ""
            RaiseEvent ContentChanged(Me, _currentContent)
        End Sub

        Public Sub SelectAll()
            ' Selection handled by RichTextBox
        End Sub

        Public Function FindNext(searchText As String, Optional useRegex As Boolean = False, Optional matchCase As Boolean = False) As Integer
            If String.IsNullOrEmpty(searchText) OrElse String.IsNullOrEmpty(_currentContent) Then
                Return -1
            End If

            Try
                Dim options As RegexOptions = RegexOptions.None
                If Not matchCase Then options = options Or RegexOptions.IgnoreCase

                Dim pattern As String = If(useRegex, searchText, Regex.Escape(searchText))
                Dim regex As New Regex(pattern, options)

                Dim match As Match = regex.Match(_currentContent)
                If match.Success Then
                    Return match.Index
                End If
            Catch
                ' Invalid regex pattern
            End Try

            Return -1
        End Function

        Public Function FindAll(searchText As String, Optional useRegex As Boolean = False, Optional matchCase As Boolean = False) As List(Of Integer)
            Dim positions As New List(Of Integer)()

            If String.IsNullOrEmpty(searchText) OrElse String.IsNullOrEmpty(_currentContent) Then
                Return positions
            End If

            Try
                Dim options As RegexOptions = RegexOptions.None
                If Not matchCase Then options = options Or RegexOptions.IgnoreCase

                Dim pattern As String = If(useRegex, searchText, Regex.Escape(searchText))
                Dim regex As New Regex(pattern, options)

                For Each match As Match In regex.Matches(_currentContent)
                    positions.Add(match.Index)
                Next
            Catch
                ' Invalid regex pattern
            End Try

            Return positions
        End Function

        Public Function Replace(searchText As String, replaceText As String, Optional useRegex As Boolean = False, Optional matchCase As Boolean = False, Optional replaceAll As Boolean = False) As Integer
            If String.IsNullOrEmpty(searchText) OrElse String.IsNullOrEmpty(_currentContent) Then
                Return 0
            End If

            PushUndoState()

            Try
                Dim options As RegexOptions = RegexOptions.None
                If Not matchCase Then options = options Or RegexOptions.IgnoreCase

                Dim pattern As String = If(useRegex, searchText, Regex.Escape(searchText))
                Dim regex As New Regex(pattern, options)

                If replaceAll Then
                    _currentContent = regex.Replace(_currentContent, replaceText)
                Else
                    Dim match As Match = regex.Match(_currentContent)
                    If match.Success Then
                        _currentContent = _currentContent.Remove(match.Index, match.Length).Insert(match.Index, replaceText)
                    End If
                End If

                RaiseEvent ContentChanged(Me, _currentContent)

                Return If(replaceAll, regex.Matches(_currentContent).Count, 1)
            Catch
                Return 0
            End Try
        End Function

        Public Function GetLineNumber(position As Integer) As Integer
            If String.IsNullOrEmpty(_currentContent) OrElse position < 0 Then Return 1

            Dim lineCount As Integer = 1
            For i As Integer = 0 To Math.Min(position, _currentContent.Length - 1)
                If _currentContent(i) = vbCrLf OrElse _currentContent(i) = vbLf Then
                    lineCount += 1
                End If
            Next

            Return lineCount
        End Function

        Public Function GetLineContent(lineNumber As Integer) As String
            Dim lines As String() = _currentContent.Split({vbCrLf, vbCr, vbLf}, StringSplitOptions.None)
            If lineNumber > 0 AndAlso lineNumber <= lines.Length Then
                Return lines(lineNumber - 1)
            End If
            Return ""
        End Function

        Public Function GetTotalLines() As Integer
            If String.IsNullOrEmpty(_currentContent) Then Return 1
            Return _currentContent.Split({vbCrLf, vbCr, vbLf}, StringSplitOptions.None).Length
        End Function

        Public Function GetTextUpToPosition(position As Integer) As String
            If position <= 0 Then Return ""
            Return _currentContent.Substring(0, Math.Min(position, _currentContent.Length))
        End Function

        Public Function GetTextFromPosition(position As Integer) As String
            If position >= _currentContent.Length Then Return ""
            Return _currentContent.Substring(position)
        End Function

        Public Function DetectEncoding(content As String) As Encoding
            If String.IsNullOrEmpty(content) Then Return Encoding.UTF8

            ' Check for BOM
            If content.Length > 0 Then
                Dim bytes() As Byte = Encoding.UTF8.GetBytes(content.Substring(0, Math.Min(1024, content.Length)))

                If bytes.Length >= 3 AndAlso bytes(0) = &HEF AndAlso bytes(1) = &HBB AndAlso bytes(2) = &HBF Then
                    Return Encoding.UTF8
                End If

                If bytes.Length >= 2 Then
                    If bytes(0) = &HFE AndAlso bytes(1) = &HFF Then
                        Return Encoding.BigEndianUTF32
                    End If
                    If bytes(0) = &HFF AndAlso bytes(1) = &HFE Then
                        Return Encoding.UTF32
                    End If

                    ' Check for UTF-16/UTF-32 patterns
                    If bytes.Length >= 4 Then
                        If bytes(0) = 0 AndAlso bytes(1) = 0 AndAlso bytes(2) = &HFE AndAlso bytes(3) = &HFF Then
                            Return Encoding.UTF32
                        End If
                    End If
                End If
            End If

            Return Encoding.UTF8
        End Function

        Public Function ConvertLineEndings(content As String, targetStyle As LineEndingStyle) As String
            Dim newLine As String = targetStyle.ToString()

            Select Case targetStyle
                Case LineEndingStyle.Windows
                    newLine = vbCrLf
                Case LineEndingStyle.Unix
                    newLine = vbLf
                Case LineEndingStyle.Mac
                    newLine = vbCr
                Case Else
                    newLine = vbCrLf
            End Select

            ' Normalize first
            Dim normalized As String = content.Replace(vbCrLf, vbLf).Replace(vbCr, vbLf)
            Return normalized.Replace(vbLf, newLine)
        End Function

        Public Function ConvertTabsToSpaces(content As String, tabSize As Integer) As String
            Dim result As New StringBuilder()
            Dim currentColumn As Integer = 0

            For Each c As Char In content
                If c = vbTab Then
                    Dim spaces As Integer = tabSize - (currentColumn Mod tabSize)
                    result.Append(New String(" "c, spaces))
                    currentColumn += spaces
                ElseIf c = vbCr Then
                    result.Append(c)
                    currentColumn = 0
                ElseIf c = vbLf Then
                    result.Append(c)
                    currentColumn = 0
                Else
                    result.Append(c)
                    currentColumn += 1
                End If
            Next

            Return result.ToString()
        End Function

        Public Function ConvertSpacesToTabs(content As String, tabSize As Integer) As String
            Dim result As New StringBuilder()
            Dim spacesInLine As Integer = 0

            For Each c As Char In content
                If c = " "c Then
                    spacesInLine += 1
                    If spacesInLine = tabSize Then
                        result.Append(vbTab)
                        spacesInLine = 0
                    End If
                ElseIf c = vbTab Then
                    result.Append(c)
                    spacesInLine = 0
                ElseIf c = vbCr OrElse c = vbLf Then
                    result.Append(c)
                    spacesInLine = 0
                Else
                    result.Append(New String(" "c, spacesInLine))
                    spacesInLine = 0
                    result.Append(c)
                End If
            Next

            Return result.ToString()
        End Function

        Public Function ApplyAutoIndent(content As String, Optional tabSize As Integer = 4) As String
            Dim lines As String() = content.Split({vbCrLf, vbCr, vbLf}, StringSplitOptions.None)
            Dim result As New List(Of String)()
            Dim currentIndent As Integer = 0

            For i As Integer = 0 To lines.Length - 1
                Dim line As String = lines(i)
                Dim trimmed As String = line.TrimStart()

                ' Calculate indent based on previous line
                Dim previousLine As String = If(i > 0, lines(i - 1), "")
                Dim previousTrimmed As String = previousLine.TrimStart()

                ' Check if previous line increases indent
                If previousTrimmed.EndsWith("{") OrElse
                   previousTrimmed.EndsWith(":") OrElse
                   previousTrimmed.StartsWith("Case ") OrElse
                   previousTrimmed.StartsWith("default:") Then
                    currentIndent += 1
                End If

                ' Remove existing indentation and apply current
                Dim newIndent As String = New String(" "c, currentIndent * tabSize)
                Dim newLine As String = newIndent & trimmed

                ' Check if current line decreases indent
                If trimmed.EndsWith("}") OrElse
                   trimmed.EndsWith("];") OrElse
                   trimmed.EndsWith("end") OrElse
                   trimmed.StartsWith("Case ") OrElse
                   trimmed.StartsWith("Else") OrElse
                   trimmed.StartsWith("ElseIf") Then
                    currentIndent = Math.Max(0, currentIndent - 1)
                    newIndent = New String(" "c, currentIndent * tabSize)
                    newLine = newIndent & trimmed
                End If

                result.Add(newLine)
            Next

            Return String.Join(vbCrLf, result)
        End Function

        Public Sub Clear()
            _currentContent = ""
            _currentFilePath = ""
            _currentFileItem = Nothing
            _undoStack.Clear()
            _redoStack.Clear()
            RaiseEvent ContentChanged(Me, "")
        End Sub
    End Class

    Public Enum LineEndingStyle
        Windows
        Unix
        Mac
    End Enum
End Namespace
