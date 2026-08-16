namespace Benita
{
    /// <summary>انواع توکن‌هایی را مشخص می‌کند که Lexer برای Parser تولید می‌کند.</summary>
    public enum TokenType
    {
        // کلیدواژه‌های ساختار برنامه و کنترل جریان.
        FUNC, MAIN, RETURN, IF, ELSE, WHILE, FOR, IN, PACKAGE, NEW, INIT, THIS, BREAK, CONTINUE, MATCH,
        PUBLIC, PRIVATE, INTERFACE, TRY, CATCH, FINALLY, THROW, ASYNC, AWAIT,

        // انواع داخلی و کلیدواژهٔ استنتاج نوع.
        NUMBER, STRING, BOOL, VOID, LET,

        // عملگرهای حسابی، مقایسه‌ای، منطقی و نحوی.
        PLUS, MINUS, STAR, SLASH, PERCENT,
        LT, GT, LTE, GTE, EQUAL_EQUAL, BANG_EQUAL,
        AND_AND, OR_OR,
        BANG, PLUS_PLUS, MINUS_MINUS, ARROW, FAT_ARROW, DOT, RANGE,

        // عملگرهای انتساب ساده و مرکب.
        EQUAL, PLUS_EQUAL, MINUS_EQUAL, STAR_EQUAL, SLASH_EQUAL,

        // شناسه‌ها و مقادیر لفظی.
        IDENTIFIER, NUMBER_LITERAL, STRING_LITERAL, TRUE_LITERAL, FALSE_LITERAL,

        // علائم جداکننده و نشانه‌گذاری ساختار زبان.
        LPAREN, RPAREN, LBRACE, RBRACE, LSQUAREBRACE, RSQUAREBRACE, SEMICOLON, COMMA, COLON,

        // پایان جریان توکن‌ها.
        EOF
    }

    /// <summary>نوع، متن و موقعیت مبدأ یک توکن Benita را نگه می‌دارد.</summary>
    public class Token
    {
        /// <summary>نوع توکن را دریافت می‌کند.</summary>
        public TokenType Type { get; }

        /// <summary>متن دقیق توکن در کد مبدأ را دریافت می‌کند.</summary>
        public string Lexeme { get; }

        /// <summary>شمارهٔ خط یک‌مبنای توکن را دریافت می‌کند.</summary>
        public int Line { get; }

        /// <summary>شمارهٔ ستون یک‌مبنای توکن را دریافت می‌کند.</summary>
        public int Column { get; }

        /// <summary>اطلاعات کامل موقعیت توکن را برای گزارش عیب‌یابی دریافت می‌کند.</summary>
        public SourceSpan Span { get; }

        /// <summary>توکنی با نوع، متن و اطلاعات مبدأ مشخص می‌سازد.</summary>
        public Token(TokenType type, string lexeme, int line, int column = 1, string? fileName = null, string? lineText = null)
        {
            Type = type;
            Lexeme = lexeme;
            Line = line;
            Column = column;
            Span = new SourceSpan(fileName, line, column, Math.Max(1, lexeme.Length), lineText);
        }

        /// <summary>نمایش خوانای توکن و موقعیت آن را تولید می‌کند.</summary>
        public override string ToString()
        {
            return $"{Type} {Lexeme} (Line: {Line}, Column: {Column})";
        }
    }

}
