namespace Benita
{
    public enum TokenType
    {
        // Keywords
        FUNC, MAIN, RETURN, IF, ELSE, WHILE, FOR, PACKAGE, NEW, BREAK, CONTINUE,

        // Types
        NUMBER, STRING, BOOL, VOID, LET,

        // Operators
        PLUS, MINUS, STAR, SLASH, PERCENT,
        LT, GT, LTE, GTE, EQUAL_EQUAL, BANG_EQUAL,
        AND_AND, OR_OR,
        BANG, PLUS_PLUS, MINUS_MINUS, ARROW, DOT,

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

        public Token(TokenType type, string lexeme, int line)
        {
            Type = type;
            Lexeme = lexeme;
            Line = line;
        }

        public override string ToString()
        {
            return $"{Type} {Lexeme} (Line: {Line})";
        }
    }

}