namespace Benita
{
    /// <summary>
    /// The Lexer class is responsible for tokenizing the source code.
    /// </summary>
    public class Lexer
    {
        /// <summary>
        /// The input source code to be tokenized.
        /// </summary>
        private readonly string _source;
        private readonly string? _sourceName;

        /// <summary>
        /// The list to store the generated tokens.
        /// </summary>
        private readonly List<Token> _tokens = new();

        /// <summary>
        /// A set to track included files to avoid duplicate inclusion.
        /// </summary>
        private readonly HashSet<string> _includedFiles = new();

        /// <summary>
        /// Variables to keep track of the current position in the source code.
        /// </summary>
        private int _start = 0;
        private int _current = 0;
        private int _line = 1;

        /// <summary>
        /// A dictionary mapping keywords to their respective token types.
        /// </summary>
        private static readonly Dictionary<string, TokenType> Keywords = new()
        {
            {"pkg", TokenType.PACKAGE},
            {"new", TokenType.NEW},
            {"init", TokenType.INIT},
            {"this", TokenType.THIS},
            {"func", TokenType.FUNC},
            {"_main_", TokenType.MAIN},
            {"return", TokenType.RETURN},
            {"if", TokenType.IF},
            {"else", TokenType.ELSE},
            {"for", TokenType.FOR},
            {"in", TokenType.IN},
            {"while", TokenType.WHILE},
            {"number", TokenType.NUMBER},
            {"string", TokenType.STRING},
            {"let", TokenType.LET},
            {"bool", TokenType.BOOL},
            {"void", TokenType.VOID},
            {"true", TokenType.TRUE_LITERAL},
            {"false", TokenType.FALSE_LITERAL},
            {"break", TokenType.BREAK},
            {"continue", TokenType.CONTINUE},
            {"match", TokenType.MATCH},
            {"public", TokenType.PUBLIC},
            {"private", TokenType.PRIVATE},
        };

        /// <summary>
        /// Constructor that initializes the Lexer with the source code and optionally prints the processed source.
        /// </summary>
        /// <param name="source">The source code to be tokenized.</param>
        /// <param name="sourcePrint">Whether to print the processed source code.</param>
        public Lexer(string source, bool sourcePrint = false, string? sourceName = null)
        {
            _source = source;
            _sourceName = sourceName;
            ProcessIncludes(ref _source); ///< Process included files.
            if (sourcePrint)
                Console.WriteLine(_source);
        }

        /// <summary>
        /// Main method to start tokenization and return the list of tokens.
        /// </summary>
        /// <returns>A list of tokens.</returns>
        public List<Token> Tokenize()
        {
            while (!IsAtEnd())
            {
                _start = _current; ///< Mark the beginning of a new token.
                ScanToken(); ///< Scan a single token.
            }

            _start = _current;
            AddToken(TokenType.EOF, ""); ///< Add end-of-file token.
            return _tokens; ///< Return the list of tokens.
        }

        /// <summary>
        /// Recursively processes "include_once" directives to include the contents of other files.
        /// </summary>
        /// <param name="source">The source code to process.</param>
        private void ProcessIncludes(ref string source)
        {
            var includedSources = new List<string>();
            var lines = source.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            foreach (var line in lines)
            {
                if (line.Trim().StartsWith("include_once"))
                {
                    var parts = line.Split(' ');
                    if (parts.Length == 2)
                    {
                        var filePath = parts[1].Trim('\"', ' ', ';');

                        if (!_includedFiles.Contains(filePath))
                        {
                            _includedFiles.Add(filePath);
                            if (File.Exists(filePath))
                            {
                                var fileContent = File.ReadAllText(filePath);
                                ProcessIncludes(ref fileContent); ///< Recursively process included file.
                                includedSources.Add(fileContent);
                            }
                            else
                            {
                                throw CreateError("BEN1003", $"Included file '{filePath}' was not found.");
                            }
                        }
                    }
                }
                else
                {
                    includedSources.Add(line); ///< Add the line if it's not an include directive.
                }
            }

            source = string.Join(Environment.NewLine, includedSources); ///< Combine the processed lines.
        }

        /// <summary>
        /// Scans a single token from the source code and adds it to the _tokens list.
        /// </summary>
        private void ScanToken()
        {
            char c = Advance();
            switch (c)
            {
                case '(': AddToken(TokenType.LPAREN); break;
                case ')': AddToken(TokenType.RPAREN); break;
                case '{': AddToken(TokenType.LBRACE); break;
                case '}': AddToken(TokenType.RBRACE); break;
                case ';': AddToken(TokenType.SEMICOLON); break;
                case ',': AddToken(TokenType.COMMA); break;
                case '.':
                    if (Match('.'))
                    {
                        AddToken(TokenType.RANGE);
                        break;
                    }
                    if (IsDigit(Peek()))
                        throw CreateError("BEN1005", "Decimal literals must start with a digit; use '0.5' instead of '.5'.");
                    AddToken(TokenType.DOT);
                    break;
                case '+':
                    AddToken(Match('=') ? TokenType.PLUS_EQUAL : Match('+') ? TokenType.PLUS_PLUS : TokenType.PLUS);
                    break;
                case '-':
                    AddToken(Match('=') ? TokenType.MINUS_EQUAL : Match('>') ? TokenType.ARROW : Match('-') ? TokenType.MINUS_MINUS : TokenType.MINUS);
                    break;
                case '*':
                    AddToken(Match('=') ? TokenType.STAR_EQUAL : TokenType.STAR);
                    break;
                case '/':
                    if (Match('/'))
                    {
                        SkipSingleLineComment();
                    }
                    else if (Match('*'))
                    {
                        SkipMultiLineComment(); ///< Skip multi-line comment.
                    }
                    else
                    {
                        AddToken(Match('=') ? TokenType.SLASH_EQUAL : TokenType.SLASH);
                    }
                    break;
                case '%': AddToken(TokenType.PERCENT); break;
                case '<': AddToken(Match('=') ? TokenType.LTE : TokenType.LT); break;
                case '>': AddToken(Match('=') ? TokenType.GTE : TokenType.GT); break;
                case '=': AddToken(Match('=') ? TokenType.EQUAL_EQUAL : Match('>') ? TokenType.FAT_ARROW : TokenType.EQUAL); break;
                case '&':
                    if (!Match('&')) throw CreateError("BEN1001", "Expected '&&' but found '&'.");
                    AddToken(TokenType.AND_AND);
                    break;
                case '|':
                    if (!Match('|')) throw CreateError("BEN1001", "Expected '||' but found '|'.");
                    AddToken(TokenType.OR_OR);
                    break;
                case '!': AddToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG); break;
                case '"': ScanStringLiteral(); break;

                case '[': AddToken(TokenType.LSQUAREBRACE); break;
                case ']': AddToken(TokenType.RSQUAREBRACE); break;

                ///< Ignore whitespace.
                case ' ':
                case '\r':
                case '\t':
                    break;
                case '\n':
                    _line++; ///< Increment line counter on new line.
                    break;

                default:
                    if (IsDigit(c) || c == '.')
                    {
                        ScanNumberLiteral(); ///< Scan number literal.
                    }
                    else if (IsAlpha(c))
                    {
                        ScanIdentifier(); ///< Scan identifier or keyword.
                    }
                    else
                    {
                        throw CreateError("BEN1001", $"Unexpected character '{c}'.");
                    }
                    break;
            }
        }

        /// <summary>
        /// Skips single-line comments.
        /// </summary>
        private void SkipSingleLineComment()
        {
            while (Peek() != '\n' && !IsAtEnd())
                _current++;
        }

        /// <summary>
        /// Skips multi-line comments.
        /// </summary>
        private void SkipMultiLineComment()
        {
            while (!(Peek() == '*' && PeekNext() == '/'))
            {
                if (IsAtEnd())
                    throw CreateError("BEN1004", "Unterminated multiline comment.");
                if (Advance() == '\n') _line++;
            }
            _current += 2;
        }

        /// <summary>
        /// Scans and adds a string literal token.
        /// </summary>
        private void ScanStringLiteral()
        {
            var value = new System.Text.StringBuilder();
            while (!IsAtEnd() && Peek() != '"')
            {
                char current = Advance();
                if (current == '\n')
                    throw CreateError("BEN1002", "String literals cannot span multiple lines.");
                if (current != '\\')
                {
                    value.Append(current);
                    continue;
                }

                if (IsAtEnd()) throw CreateError("BEN1002", "Unterminated string escape sequence.");
                value.Append(Advance() switch
                {
                    'n' => '\n',
                    'r' => '\r',
                    't' => '\t',
                    '"' => '"',
                    '\\' => '\\',
                    char escape => throw CreateError("BEN1006", $"Unsupported escape sequence '\\{escape}'.")
                });
            }

            if (IsAtEnd())
            {
                throw CreateError("BEN1002", "Unterminated string literal.");
            }
            Advance(); 
            AddToken(TokenType.STRING_LITERAL, value.ToString());
        }

        /// <summary>
        /// Scans and processes a numeric literal from the source code.
        /// Handles both integer and floating-point numbers.
        /// </summary>
        private void ScanNumberLiteral()
        {
            // Check if the number starts with a decimal point (e.g., .5)
            if (Peek() == '.' && IsDigit(PeekNext()))
            {
                Advance();
            }

            // Process the integer part of the number
            while (IsDigit(Peek()))
            {
                Advance();
            }

            // Check for a fractional part
            if (Peek() == '.' && IsDigit(PeekNext()))
            {
                Advance();

                // Process the fractional part of the number
                while (IsDigit(Peek()))
                {
                    Advance();
                }
            }

            // Extract the numeric literal from the source
            string lexeme = _source.Substring(_start, _current - _start);

            // Add the numeric literal token to the token list
            AddToken(TokenType.NUMBER_LITERAL, lexeme);
        }

        /// <summary>
        /// Scans and adds an identifier or keyword token.
        /// </summary>
        private void ScanIdentifier()
        {
            while (IsAlphaNumeric(Peek())) Advance();

            string text = _source.Substring(_start, _current - _start);
            AddToken(Keywords.ContainsKey(text) ? Keywords[text] : TokenType.IDENTIFIER);
        }

        /// <summary>
        /// Matches the current character with the expected character.
        /// </summary>
        /// <param name="expected">The expected character to match.</param>
        /// <returns>True if the current character matches the expected character, otherwise false.</returns>
        private bool Match(char expected)
        {
            if (IsAtEnd()) return false;
            if (_source[_current] != expected) return false;
            _current++;
            return true;
        }

        /// <summary>
        /// Peeks at the current character without consuming it.
        /// </summary>
        /// <returns>The current character.</returns>
        private char Peek()
        {
            if (IsAtEnd()) return '\0';
            return _source[_current];
        }

        /// <summary>
        /// Peeks at the next character without consuming it.
        /// </summary>
        /// <returns>The next character.</returns>
        private char PeekNext()
        {
            if (_current + 1 >= _source.Length) return '\0';
            return _source[_current + 1];
        }

        /// <summary>
        /// Advances the current position and returns the current character.
        /// </summary>
        /// <returns>The current character.</returns>
        private char Advance()
        {
            return _source[_current++];
        }

        /// <summary>
        /// Checks if the end of the source code has been reached.
        /// </summary>
        /// <returns>True if at the end of the source code, otherwise false.</returns>
        private bool IsAtEnd()
        {
            return _current >= _source.Length;
        }

        /// <summary>
        /// Adds a token to the _tokens list.
        /// </summary>
        /// <param name="type">The type of the token.</param>
        /// <param name="lexeme">The lexeme of the token.</param>
        private void AddToken(TokenType type, string? lexeme = null)
        {
            lexeme ??= _source.Substring(_start, _current - _start);
            var span = GetCurrentSpan(Math.Max(1, lexeme.Length));
            _tokens.Add(new Token(type, lexeme, span.Line, span.Column, span.FileName, span.LineText));
        }

        private LexerException CreateError(string code, string message) =>
            new(code, message, GetCurrentSpan(Math.Max(1, _current - _start)));

        private SourceSpan GetCurrentSpan(int length)
        {
            var lineStart = _start == 0 ? -1 : _source.LastIndexOf('\n', _start - 1);
            lineStart = lineStart < 0 ? 0 : lineStart + 1;
            var lineEnd = _source.IndexOf('\n', _start);
            if (lineEnd < 0) lineEnd = _source.Length;
            var lineText = _source[lineStart..lineEnd].TrimEnd('\r');
            var line = 1;
            for (var index = 0; index < _start; index++)
            {
                if (_source[index] == '\n') line++;
            }
            return new SourceSpan(_sourceName, line, _start - lineStart + 1, length, lineText);
        }

        /// <summary>
        /// Checks if the character is a digit.
        /// </summary>
        /// <param name="c">The character to check.</param>
        /// <returns>True if the character is a digit, otherwise false.</returns>
        private bool IsDigit(char c)
        {
            return c <= '9' && c >= '0';
        }

        /// <summary>
        /// Checks if the character is an alphabetic character or underscore.
        /// </summary>
        /// <param name="c">The character to check.</param>
        /// <returns>True if the character is alphabetic or an underscore, otherwise false.</returns>
        private bool IsAlpha(char c)
        {
            return (c >= 'a' && c <= 'z') ||
                   (c >= 'A' && c <= 'Z') ||
                   c == '_';
        }

        /// <summary>
        /// Checks if the character is alphanumeric.
        /// </summary>
        /// <param name="c">The character to check.</param>
        /// <returns>True if the character is alphanumeric, otherwise false.</returns>
        private bool IsAlphaNumeric(char c)
        {
            return IsAlpha(c) || IsDigit(c);
        }
    }
}
