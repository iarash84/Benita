namespace Benita
{
    public enum TokenType
    {
        // Keywords
        FUNC, MAIN, RETURN, IF, ELSE, WHILE, FOR, IN, PACKAGE, NEW, INIT, THIS, BREAK, CONTINUE, MATCH,
        PUBLIC, PRIVATE,

        // Types
        NUMBER, STRING, BOOL, VOID, LET,

        // Operators
        PLUS, MINUS, STAR, SLASH, PERCENT,
        LT, GT, LTE, GTE, EQUAL_EQUAL, BANG_EQUAL,
        AND_AND, OR_OR,
        BANG, PLUS_PLUS, MINUS_MINUS, ARROW, FAT_ARROW, DOT, RANGE,

        // Assignment operators
        EQUAL, PLUS_EQUAL, MINUS_EQUAL, STAR_EQUAL, SLASH_EQUAL,

        // Literals
        IDENTIFIER, NUMBER_LITERAL, STRING_LITERAL, TRUE_LITERAL, FALSE_LITERAL,

        // Symbols
        LPAREN, RPAREN, LBRACE, RBRACE, LSQUAREBRACE, RSQUAREBRACE, SEMICOLON, COMMA,

        // Comments
        //COMMENT,MULTI_LINE_COMMENT

        EOF
    }

    public class Token
    {
        public TokenType Type { get; }
        public string Lexeme { get; }
        public int Line { get; }
        public int Column { get; }
        public SourceSpan Span { get; }

        public Token(TokenType type, string lexeme, int line, int column = 1, string? fileName = null, string? lineText = null)
        {
            Type = type;
            Lexeme = lexeme;
            Line = line;
            Column = column;
            Span = new SourceSpan(fileName, line, column, Math.Max(1, lexeme.Length), lineText);
        }

        public override string ToString()
        {
            return $"{Type} {Lexeme} (Line: {Line}, Column: {Column})";
        }
    }

}
