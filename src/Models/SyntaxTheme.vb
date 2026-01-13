'SyntaxTheme.vb - Model for syntax highlighting themes
Namespace vbnetbrowser.Models

    Public Class SyntaxTheme
        Public Property Name As String
        Public Property BackgroundColor As Color
        Public Property ForegroundColor As Color
        Public Property LineNumberBackground As Color
        Public Property LineNumberForeground As Color
        Public Property SelectionBackground As Color
        Public Property KeywordColor As Color
        Public Property StringColor As Color
        Public Property NumberColor As Color
        Public Property CommentColor As Color
        Public Property HtmlTagColor As Color
        Public Property HtmlAttributeColor As Color
        Public Property FunctionColor As Color
        Public Property VariableColor As Color
        Public Property OperatorColor As Color
        Public Property TypeColor As Color

        Public Sub New()
        End Sub

        Public Shared ReadOnly Property Light As SyntaxTheme
            Get
                Return New SyntaxTheme With {
                    .Name = "Light",
                    .BackgroundColor = Color.White,
                    .ForegroundColor = Color.Black,
                    .LineNumberBackground = Color.FromArgb(240, 240, 240),
                    .LineNumberForeground = Color.Gray,
                    .SelectionBackground = Color.FromArgb(173, 214, 255),
                    .KeywordColor = Color.Blue,
                    .StringColor = Color.DarkRed,
                    .NumberColor = Color.Red,
                    .CommentColor = Color.Green,
                    .HtmlTagColor = Color.Blue,
                    .HtmlAttributeColor = Color.Red,
                    .FunctionColor = Color.Black,
                    .VariableColor = Color.Black,
                    .OperatorColor = Color.Black,
                    .TypeColor = Color.Blue
                }
            End Get
        End Property

        Public Shared ReadOnly Property Dark As SyntaxTheme
            Get
                Return New SyntaxTheme With {
                    .Name = "Dark",
                    .BackgroundColor = Color.FromArgb(30, 30, 30),
                    .ForegroundColor = Color.FromArgb(220, 220, 220),
                    .LineNumberBackground = Color.FromArgb(40, 40, 40),
                    .LineNumberForeground = Color.FromArgb(150, 150, 150),
                    .SelectionBackground = Color.FromArgb(70, 70, 70),
                    .KeywordColor = Color.FromArgb(197, 134, 192),
                    .StringColor = Color.FromArgb(206, 145, 120),
                    .NumberColor = Color.FromArgb(181, 206, 168),
                    .CommentColor = Color.FromArgb(106, 153, 85),
                    .HtmlTagColor = Color.FromArgb(197, 134, 192),
                    .HtmlAttributeColor = Color.FromArgb(156, 220, 254),
                    .FunctionColor = Color.FromArgb(220, 220, 170),
                    .VariableColor = Color.FromArgb(156, 220, 254),
                    .OperatorColor = Color.FromArgb(180, 180, 180),
                    .TypeColor = Color.FromArgb(78, 201, 176)
                }
            End Get
        End Property

        Public Shared ReadOnly Property Monokai As SyntaxTheme
            Get
                Return New SyntaxTheme With {
                    .Name = "Monokai",
                    .BackgroundColor = Color.FromArgb(39, 40, 34),
                    .ForegroundColor = Color.FromArgb(248, 248, 242),
                    .LineNumberBackground = Color.FromArgb(50, 50, 45),
                    .LineNumberForeground = Color.FromArgb(120, 120, 120),
                    .SelectionBackground = Color.FromArgb(60, 60, 55),
                    .KeywordColor = Color.FromArgb(249, 38, 114),
                    .StringColor = Color.FromArgb(230, 219, 116),
                    .NumberColor = Color.FromArgb(174, 129, 255),
                    .CommentColor = Color.FromArgb(117, 113, 94),
                    .HtmlTagColor = Color.FromArgb(249, 38, 114),
                    .HtmlAttributeColor = Color.FromArgb(166, 227, 161),
                    .FunctionColor = Color.FromArgb(166, 227, 161),
                    .VariableColor = Color.FromArgb(248, 248, 242),
                    .OperatorColor = Color.FromArgb(248, 248, 242),
                    .TypeColor = Color.FromArgb(102, 217, 239)
                }
            End Get
        End Property
    End Class

    Public Class EditorSettings
        Public Property Theme As String
        Public Property FontFamily As String
        Public Property FontSize As Integer
        Public Property TabSize As Integer
        Public Property UseSpacesInsteadOfTabs As Boolean
        Public Property WordWrap As Boolean
        Public Property ShowLineNumbers As Boolean
        Public Property HighlightCurrentLine As Boolean
        Public Property AutoIndent As Boolean
        Public Property DetectEncoding As Boolean

        Public Sub New()
            Theme = "Dark"
            FontFamily = "Consolas"
            FontSize = 12
            TabSize = 4
            UseSpacesInsteadOfTabs = False
            WordWrap = False
            ShowLineNumbers = True
            HighlightCurrentLine = True
            AutoIndent = True
            DetectEncoding = True
        End Sub
    End Class
End Namespace
