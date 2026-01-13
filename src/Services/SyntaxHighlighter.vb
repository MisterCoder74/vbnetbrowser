'SyntaxHighlighter.vb - Enhanced syntax highlighting for multiple languages
Imports System.Text
Imports System.Text.RegularExpressions

Namespace vbnetbrowser.Services

    Public Class SyntaxHighlighter

        Private ReadOnly _theme As Models.SyntaxTheme

        Public Sub New(Optional theme As Models.SyntaxTheme = Nothing)
            _theme = theme Or Models.SyntaxTheme.Dark
        End Sub

        Public Function GetLanguageFromExtension(extension As String) As String
            Select Case extension.ToLowerInvariant()
                Case ".html", ".htm" : Return "html"
                Case ".css" : Return "css"
                Case ".js", ".ts" : Return "javascript"
                Case ".json" : Return "json"
                Case ".xml" : Return "xml"
                Case ".vb" : Return "vbnet"
                Case ".cs" : Return "csharp"
                Case ".py" : Return "python"
                Case ".yaml", ".yml" : Return "yaml"
                Case ".md" : Return "markdown"
                Case ".sql" : Return "sql"
                Case ".ini", ".cfg", ".conf" : Return "ini"
                Case ".log" : Return "log"
                Case ".sh", ".bash" : Return "bash"
                Case Else : Return "plaintext"
            End Select
        End Function

        Public Function GetLanguageFromFileName(fileName As String) As String
            Dim extension As String = Path.GetExtension(fileName)
            Return GetLanguageFromExtension(extension)
        End Function

        Public Function Highlight(content As String, language As String) As String
            Select Case language.ToLowerInvariant()
                Case "html" : Return HighlightHtml(content)
                Case "css" : Return HighlightCss(content)
                Case "javascript", "js" : Return HighlightJavaScript(content)
                Case "json" : Return HighlightJson(content)
                Case "xml" : Return HighlightXml(content)
                Case "vbnet", "vb" : Return HighlightVbNet(content)
                Case "csharp", "cs" : Return HighlightCSharp(content)
                Case "python", "py" : Return HighlightPython(content)
                Case "yaml" : Return HighlightYaml(content)
                Case "markdown", "md" : Return HighlightMarkdown(content)
                Case "sql" : Return HighlightSql(content)
                Case "ini" : Return HighlightIni(content)
                Case Else : Return EscapeHtml(content)
            End Select
        End Function

        Private Function EscapeHtml(text As String) As String
            Return text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;")
        End Function

        Private Function WrapSpan(text As String, cssClass As String) As String
            Return $"<span class=""{cssClass}"">{text}</span>"
        End Function

        Private Function HighlightHtml(content As String) As String
            Dim result As New StringBuilder()
            Dim currentIndex As Integer = 0

            ' Match HTML comments
            Dim commentPattern As New Regex("&lt;!--.*?--&gt;", RegexOptions.Singleline)
            content = EscapeHtml(content)

            ' Match tags
            Dim tagPattern As New Regex("&lt;/?([a-zA-Z][a-zA-Z0-9]*)([^&gt;]*)&gt;")
            Dim attrPattern As New Regex("([a-zA-Z_:][a-zA-Z0-9_:.-]*)=(&quot;[^&quot;]*&quot;|'[^']*'|[^&lt;&gt;\s]+)")

            ' Process content
            Dim remaining As String = content
            Dim lastEnd As Integer = 0

            For Each match As Match In tagPattern.Matches(content)
                result.Append(content.Substring(lastEnd, match.Index - lastEnd))

                Dim fullTag As String = match.Value
                Dim tagName As String = match.Groups(1).Value
                Dim attrs As String = match.Groups(2).Value

                ' Highlight tag name
                result.Append(WrapSpan("&lt;", "operator"))
                result.Append(WrapSpan(tagName, "tag"))
                result.Append(WrapSpan(attrs, "attribute"))
                result.Append(WrapSpan("&gt;", "operator"))

                lastEnd = match.Index + match.Length
            Next

            result.Append(content.Substring(lastEnd))

            ' Highlight attributes
            result.Replace("class=""", WrapSpan("class=""", "attribute") & WrapSpan("""", "attribute"))
            result.Replace("id=""", WrapSpan("id=""", "attribute") & WrapSpan("""", "attribute"))
            result.Replace("href=""", WrapSpan("href=""", "attribute") & WrapSpan("""", "attribute"))
            result.Replace("src=""", WrapSpan("src=""", "attribute") & WrapSpan("""", "attribute"))

            Return result.ToString()
        End Function

        Private Function HighlightCss(content As String) As String
            content = EscapeHtml(content)

            ' Highlight selectors
            Dim selectorPattern As New Regex("([a-zA-Z0-9_\.#][a-zA-Z0-9_\.#:\s-]*)\s*\{")
            For Each match As Match In selectorPattern.Matches(content)
                content = content.Replace(match.Value, WrapSpan(match.Groups(1).Value, "selector") & "{")
            Next

            ' Highlight properties
            Dim propPattern As New Regex("([a-zA-Z-]+)\s*:")
            content = propPattern.Replace(content, Function(m) WrapSpan(m.Groups(1).Value, "property") & ":")

            ' Highlight values
            Dim valuePattern As New Regex(":\s*([^;]+);")
            content = valuePattern.Replace(content, Function(m) ": " & WrapSpan(m.Groups(1).Value.Trim(), "value") & ";")

            Return content
        End Function

        Private Function HighlightJavaScript(content As String) As String
            content = EscapeHtml(content)

            ' Keywords
            Dim keywords As String() = {
                "var", "let", "const", "function", "return", "if", "else", "for", "while",
                "do", "switch", "case", "break", "continue", "try", "catch", "finally",
                "throw", "new", "this", "class", "extends", "import", "export", "default",
                "async", "await", "true", "false", "null", "undefined", "typeof", "instanceof"
            }

            For Each keyword As String In keywords
                Dim pattern As New Regex($"\b({keyword})\b")
                content = pattern.Replace(content, WrapSpan("$1", "keyword"))
            Next

            ' Strings
            Dim stringPattern As New Regex("""([^""\\]*(?:\\.[^""\\]*)*)""")
            content = stringPattern.Replace(content, """$1""".Replace("$1", WrapSpan("$1", "string")))

            ' Single quotes
            content = content.Replace("'([^'\\]*(?:\\.[^'\\]*)*)'", Function(m) "'" & WrapSpan(m.Groups(1).Value, "string") & "'")

            ' Numbers
            Dim numberPattern As New Regex("\b(\d+\.?\d*)\b")
            content = numberPattern.Replace(content, WrapSpan("$1", "number"))

            ' Comments
            Dim singleCommentPattern As New Regex("//.*$", RegexOptions.Multiline)
            content = singleCommentPattern.Replace(content, WrapSpan("$0", "comment"))

            Dim multiCommentPattern As New Regex("/\*.*?\*/", RegexOptions.Singleline)
            content = multiCommentPattern.Replace(content, WrapSpan("$0", "comment"))

            Return content
        End Function

        Private Function HighlightJson(content As String) As String
            content = EscapeHtml(content)

            ' Keys
            Dim keyPattern As New Regex("""([^""]+)"":")
            content = keyPattern.Replace(content, """$1"":".Replace("$1", WrapSpan("$1", "key")))

            ' Strings
            Dim stringPattern As New Regex("""([^""\\]*(?:\\.[^""\\]*)*)""")
            content = stringPattern.Replace(content, """$1""".Replace("$1", WrapSpan("$1", "string")))

            ' Numbers
            Dim numberPattern As New Regex("\b(-?\d+\.?\d*)\b")
            content = numberPattern.Replace(content, WrapSpan("$1", "number"))

            ' Boolean and null
            content = content.Replace("true", WrapSpan("true", "keyword"))
            content = content.Replace("false", WrapSpan("false", "keyword"))
            content = content.Replace("null", WrapSpan("null", "keyword"))

            Return content
        End Function

        Private Function HighlightXml(content As String) As String
            Return HighlightHtml(content)
        End Function

        Private Function HighlightVbNet(content As String) As String
            content = EscapeHtml(content)

            ' Keywords
            Dim keywords As String() = {
                "Dim", "As", "New", "Public", "Private", "Protected", "Friend",
                "Sub", "Function", "Property", "Class", "Module", "Structure",
                "If", "Then", "Else", "ElseIf", "End If", "Select", "Case", "End Select",
                "For", "Next", "Each", "In", "While", "Do", "Loop",
                "Try", "Catch", "Finally", "Throw", "Exit",
                "Return", "Async", "Await", "Using", "Imports",
                "Namespace", "End Namespace", "Interface", "End Interface",
                "Enum", "End Enum", "Const", "Shared", "ReadOnly", "Overridable",
                "Overloads", "Overrides", "Override", "MustOverride", "NotInheritable",
                "NotOverridable", "Partial", "Shadows", "Shadows", "Widening", "Narrowing",
                "Operator", "True", "False", "Nothing", "Me", "MyBase", "MyClass",
                "And", "Or", "Not", "AndAlso", "OrElse", "Xor"
            }

            For Each keyword As String In keywords
                Dim pattern As New Regex($"\b({keyword})\b", RegexOptions.IgnoreCase)
                content = pattern.Replace(content, WrapSpan("$1", "keyword"))
            Next

            ' Strings
            Dim stringPattern As New Regex("""([^""\\]*(?:""[^""\\]*)*)""")
            content = stringPattern.Replace(content, """$1""".Replace("$1", WrapSpan("$1", "string")))

            ' Comments
            Dim commentPattern As New Regex("'.*$", RegexOptions.Multiline)
            content = commentPattern.Replace(content, WrapSpan("$0", "comment"))

            ' Numbers
            Dim numberPattern As New Regex("\b(&H[0-9A-Fa-f]+|\d+\.?\d*)\b")
            content = numberPattern.Replace(content, WrapSpan("$1", "number"))

            Return content
        End Function

        Private Function HighlightCSharp(content As String) As String
            content = EscapeHtml(content)

            ' Keywords
            Dim keywords As String() = {
                "using", "namespace", "class", "struct", "interface", "enum",
                "public", "private", "protected", "internal", "static", "readonly",
                "const", "new", "virtual", "abstract", "override", "sealed",
                "partial", "void", "int", "long", "short", "byte", "float", "double",
                "decimal", "bool", "char", "string", "object", "var", "dynamic",
                "if", "else", "for", "foreach", "in", "while", "do", "switch",
                "case", "break", "continue", "return", "try", "catch", "finally",
                "throw", "async", "await", "get", "set", "init", "where",
                "this", "base", "null", "true", "false", "typeof", "sizeof",
                "is", "as", "ref", "out", "params", "in", "nameof"
            }

            For Each keyword As String In keywords
                Dim pattern As New Regex($"\b({keyword})\b")
                content = pattern.Replace(content, WrapSpan("$1", "keyword"))
            Next

            ' Strings
            Dim stringPattern As New Regex("@?""([^""\\]*(?:""[^""\\]*)*)""")
            content = stringPattern.Replace(content, """$1""".Replace("$1", WrapSpan("$1", "string")))

            ' Comments
            Dim singleCommentPattern As New Regex("//.*$", RegexOptions.Multiline)
            content = singleCommentPattern.Replace(content, WrapSpan("$0", "comment"))

            Dim multiCommentPattern As New Regex("/\*.*?\*/", RegexOptions.Singleline)
            content = multiCommentPattern.Replace(content, WrapSpan("$0", "comment"))

            ' Numbers
            Dim numberPattern As New Regex("\b(0x[0-9A-Fa-f]+|\d+\.?\d*)\b")
            content = numberPattern.Replace(content, WrapSpan("$1", "number"))

            Return content
        End Function

        Private Function HighlightPython(content As String) As String
            content = EscapeHtml(content)

            ' Keywords
            Dim keywords As String() = {
                "import", "from", "as", "class", "def", "return", "if", "elif",
                "else", "for", "while", "break", "continue", "pass", "try",
                "except", "finally", "raise", "with", "yield", "lambda", "and",
                "or", "not", "in", "is", "True", "False", "None", "global",
                "nonlocal", "assert", "del", "async", "await"
            }

            For Each keyword As String In keywords
                Dim pattern As New Regex($"\b({keyword})\b")
                content = pattern.Replace(content, WrapSpan("$1", "keyword"))
            Next

            ' Strings (single, double, triple)
            Dim stringPattern As New Regex("(?:r|f)?(""[^""\\]*(?:\\.[^""\\]*)*"")|(?:r|f)?('[^'\\]*(?:\\.[^'\\]*)*')|(?:r|f)?("""[^""\\]*(?:\\.[^""\\]*)*""")|(?:r|f)?('''[^'''\\]*(?:\\.[^'''\\]*)*''')")
            content = stringPattern.Replace(content, Function(m) WrapSpan(m.Value, "string"))

            ' Numbers
            Dim numberPattern As New Regex("\b(\d+\.?\d*)\b")
            content = numberPattern.Replace(content, WrapSpan("$1", "number"))

            ' Comments
            Dim commentPattern As New Regex("#.*$", RegexOptions.Multiline)
            content = commentPattern.Replace(content, WrapSpan("$0", "comment"))

            Return content
        End Function

        Private Function HighlightYaml(content As String) As String
            content = EscapeHtml(content)

            ' Keys
            Dim keyPattern As New Regex("^(\s*)([a-zA-Z0-9_-]+):", RegexOptions.Multiline)
            content = keyPattern.Replace(content, "$1$2:".Replace("$2", WrapSpan("$2", "key")))

            ' Strings
            Dim stringPattern As New Regex(":\s*(&quot;[^&quot;]*&quot;|'[^']*')")
            content = stringPattern.Replace(content, Function(m) ": " & WrapSpan(m.Groups(1).Value, "string"))

            ' Booleans and null
            content = content.Replace(": true", ": " & WrapSpan("true", "keyword"))
            content = content.Replace(": false", ": " & WrapSpan("false", "keyword"))
            content = content.Replace(": null", ": " & WrapSpan("null", "keyword"))
            content = content.Replace(": ~", ": " & WrapSpan("~", "keyword"))

            Return content
        End Function

        Private Function HighlightMarkdown(content As String) As String
            content = EscapeHtml(content)

            ' Headers
            content = Regex.Replace(content, "^(#+)\s+(.+)$", Function(m) WrapSpan(m.Groups(1).Value, "keyword") & " " & WrapSpan(m.Groups(2).Value, "function"), RegexOptions.Multiline)

            ' Bold
            content = Regex.Replace(content, "(\*\*|__)(.*?)\1", Function(m) WrapSpan(m.Groups(2).Value, "number"))

            ' Italic
            content = Regex.Replace(content, "(\*|_)(.*?)\1", Function(m) WrapSpan(m.Groups(2).Value, "string"))

            ' Code blocks
            content = Regex.Replace(content, "```(\w*)\s*([\s\S]*?)```", Function(m) WrapSpan(m.Groups(2).Value, "comment"), RegexOptions.Singleline)

            ' Inline code
            content = Regex.Replace(content, "`([^`]+)`", Function(m) WrapSpan(m.Groups(1).Value, "comment"))

            ' Links
            Dim linkPattern As New Regex("\[([^\]]+)\]\(([^)]+)\)")
            content = linkPattern.Replace(content, Function(m) WrapSpan(m.Groups(1).Value, "tag") & "(" & WrapSpan(m.Groups(2).Value, "attribute") & ")")

            Return content
        End Function

        Private Function HighlightSql(content As String) As String
            content = EscapeHtml(content)

            ' Keywords
            Dim keywords As String() = {
                "SELECT", "FROM", "WHERE", "AND", "OR", "NOT", "IN", "LIKE",
                "INSERT", "INTO", "VALUES", "UPDATE", "SET", "DELETE", "CREATE",
                "TABLE", "INDEX", "DROP", "ALTER", "ADD", "CONSTRAINT", "PRIMARY",
                "KEY", "FOREIGN", "REFERENCES", "JOIN", "LEFT", "RIGHT", "INNER",
                "OUTER", "ON", "GROUP", "BY", "HAVING", "ORDER", "ASC", "DESC",
                "DISTINCT", "TOP", "LIMIT", "OFFSET", "AS", "IS", "NULL", "BETWEEN",
                "UNION", "ALL", "EXCEPT", "INTERSECT", "CASE", "WHEN", "THEN", "END",
                "CAST", "CONVERT", "COALESCE", "IFNULL", "NVL", "COUNT", "SUM",
                "AVG", "MIN", "MAX", "NOW", "CURRENT_DATE", "CURRENT_TIMESTAMP"
            }

            For Each keyword As String In keywords
                Dim pattern As New Regex($"\b({keyword})\b", RegexOptions.IgnoreCase)
                content = pattern.Replace(content, WrapSpan("$1", "keyword"))
            Next

            ' Strings
            Dim stringPattern As New Regex("N?'([^'\\]*(?:''[^'\\]*)*)'")
            content = stringPattern.Replace(content, Function(m) "N'" & WrapSpan(m.Groups(1).Value, "string") & "'")

            ' Comments
            Dim singleCommentPattern As New Regex("--.*$", RegexOptions.Multiline)
            content = singleCommentPattern.Replace(content, WrapSpan("$0", "comment"))

            Dim multiCommentPattern As New Regex("/\*.*?\*/", RegexOptions.Singleline)
            content = multiCommentPattern.Replace(content, WrapSpan("$0", "comment"))

            Return content
        End Function

        Private Function HighlightIni(content As String) As String
            content = EscapeHtml(content)

            ' Sections
            Dim sectionPattern As New Regex("^\[([^\]]+)\]$")
            content = sectionPattern.Replace(content, Function(m) WrapSpan("[" & m.Groups(1).Value & "]", "tag"), RegexOptions.Multiline)

            ' Keys
            Dim keyPattern As New Regex("^([a-zA-Z0-9_-]+)\s*=")
            content = keyPattern.Replace(content, Function(m) WrapSpan(m.Groups(1).Value, "key") & " =", RegexOptions.Multiline)

            ' Values
            Dim valuePattern As New Regex("=\s*([^;]+)")
            content = valuePattern.Replace(content, Function(m) "= " & WrapSpan(m.Groups(1).Value.Trim(), "string"), RegexOptions.Multiline)

            ' Comments
            content = Regex.Replace(content, "(^|;)(.*)$", Function(m) If(m.Groups(1).Value = ";", WrapSpan(";" & m.Groups(2).Value, "comment"), m.Value), RegexOptions.Multiline)

            Return content
        End Function

        Public Function GenerateCss() As String
            Return $"
.highlight {{ 
    font-family: 'Consolas', 'Monaco', 'Courier New', monospace; 
    font-size: 14px; 
    line-height: 1.5; 
    white-space: pre-wrap; 
    background-color: {_theme.BackgroundColor.Name}; 
    color: {_theme.ForegroundColor.Name}; 
    padding: 10px; 
    overflow: auto;
}}
.line-number {{
    display: inline-block;
    width: 40px;
    text-align: right;
    margin-right: 15px;
    color: {_theme.LineNumberForeground.Name};
    user-select: none;
    background-color: {_theme.LineNumberBackground.Name};
}}
.highlight .keyword {{ color: {_theme.KeywordColor.Name}; font-weight: bold; }}
.highlight .string {{ color: {_theme.StringColor.Name}; }}
.highlight .number {{ color: {_theme.NumberColor.Name}; }}
.highlight .comment {{ color: {_theme.CommentColor.Name}; font-style: italic; }}
.highlight .tag {{ color: {_theme.HtmlTagColor.Name}; }}
.highlight .attribute {{ color: {_theme.HtmlAttributeColor.Name}; }}
.highlight .value {{ color: {_theme.VariableColor.Name}; }}
.highlight .function {{ color: {_theme.FunctionColor.Name}; }}
.highlight .property {{ color: {_theme.TypeColor.Name}; }}
.highlight .selector {{ color: {_theme.VariableColor.Name}; }}
.highlight .key {{ color: {_theme.KeywordColor.Name}; font-weight: bold; }}
.highlight .operator {{ color: {_theme.OperatorColor.Name}; }}
"
        End Function
    End Class
End Namespace
