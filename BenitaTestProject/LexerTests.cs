using Benita;

namespace BenitaTestProject
{
    [TestClass]
    /// <summary>توکن‌سازی کلیدواژه‌ها، مقادیر، عملگرها و خطاهای واژگانی را بررسی می‌کند.</summary>
    public class LexerTests
    {
        [TestMethod]
        public void Tokenize_SimpleCode_ReturnsCorrectTokens()
        {
            string code = "_main_() { return 42; }";
            Lexer lexer = new Lexer(code);

            List<Token> tokens = lexer.Tokenize();

            Assert.AreEqual(9, tokens.Count);
            Assert.AreEqual(TokenType.MAIN, tokens[0].Type);
            Assert.AreEqual(TokenType.LPAREN, tokens[1].Type);
            Assert.AreEqual(TokenType.RPAREN, tokens[2].Type);
            Assert.AreEqual(TokenType.LBRACE, tokens[3].Type);
            Assert.AreEqual(TokenType.RETURN, tokens[4].Type);
            Assert.AreEqual(TokenType.NUMBER_LITERAL, tokens[5].Type);
            Assert.AreEqual("42", tokens[5].Lexeme);
            Assert.AreEqual(TokenType.SEMICOLON, tokens[6].Type);
            Assert.AreEqual(TokenType.RBRACE, tokens[7].Type);
        }

        [TestMethod]
        public void Tokenize_IfElseCode_ReturnsCorrectTokens()
        {
            string code = "if (true) { return 1; } else { return 0; }";
            Lexer lexer = new Lexer(code);

            List<Token> tokens = lexer.Tokenize();

            Assert.AreEqual(16, tokens.Count);
            Assert.AreEqual(TokenType.IF, tokens[0].Type);
            Assert.AreEqual(TokenType.LPAREN, tokens[1].Type);
            Assert.AreEqual(TokenType.TRUE_LITERAL, tokens[2].Type);
            Assert.AreEqual(TokenType.RPAREN, tokens[3].Type);
            Assert.AreEqual(TokenType.LBRACE, tokens[4].Type);
            Assert.AreEqual(TokenType.RETURN, tokens[5].Type);
            Assert.AreEqual(TokenType.NUMBER_LITERAL, tokens[6].Type);
            Assert.AreEqual("1", tokens[6].Lexeme);
            Assert.AreEqual(TokenType.SEMICOLON, tokens[7].Type);
            Assert.AreEqual(TokenType.RBRACE, tokens[8].Type);
            Assert.AreEqual(TokenType.ELSE, tokens[9].Type);
            Assert.AreEqual(TokenType.LBRACE, tokens[10].Type);
            Assert.AreEqual(TokenType.RETURN, tokens[11].Type);
            Assert.AreEqual(TokenType.NUMBER_LITERAL, tokens[12].Type);
            Assert.AreEqual("0", tokens[12].Lexeme);
            Assert.AreEqual(TokenType.SEMICOLON, tokens[13].Type);
            Assert.AreEqual(TokenType.RBRACE, tokens[14].Type);
        }

        [TestMethod]
        public void Tokenize_StringLiteral_ReturnsCorrectToken()
        {
            string code = "string str = \"hello world\";";
            Lexer lexer = new Lexer(code);

            List<Token> tokens = lexer.Tokenize();

            Assert.AreEqual(6, tokens.Count);
            Assert.AreEqual(TokenType.STRING, tokens[0].Type);
            Assert.AreEqual(TokenType.IDENTIFIER, tokens[1].Type);
            Assert.AreEqual(TokenType.EQUAL, tokens[2].Type);
            Assert.AreEqual(TokenType.STRING_LITERAL, tokens[3].Type);
            Assert.AreEqual("hello world", tokens[3].Lexeme);
            Assert.AreEqual(TokenType.SEMICOLON, tokens[4].Type);
        }

        [TestMethod]
        public void Tokenize_Comment_ShouldSkipComment()
        {
            string code = "_main_() { // this is a comment\n return 42; }";
            Lexer lexer = new Lexer(code);

            List<Token> tokens = lexer.Tokenize();

            Assert.AreEqual(9, tokens.Count);
            Assert.AreEqual(TokenType.MAIN, tokens[0].Type);
            Assert.AreEqual(TokenType.LPAREN, tokens[1].Type);
            Assert.AreEqual(TokenType.RPAREN, tokens[2].Type);
            Assert.AreEqual(TokenType.LBRACE, tokens[3].Type);
            Assert.AreEqual(TokenType.RETURN, tokens[4].Type);
            Assert.AreEqual(TokenType.NUMBER_LITERAL, tokens[5].Type);
            Assert.AreEqual("42", tokens[5].Lexeme);
            Assert.AreEqual(TokenType.SEMICOLON, tokens[6].Type);
            Assert.AreEqual(TokenType.RBRACE, tokens[7].Type);
        }

        [TestMethod]
        public void Tokenize_MultiLineComment_ShouldSkipMultiLineComment()
        {
            string code = "_main_() { /* multi\nline\ncomment */ return 42; }";
            Lexer lexer = new Lexer(code);

            List<Token> tokens = lexer.Tokenize();

            Assert.AreEqual(9, tokens.Count);
            Assert.AreEqual(TokenType.MAIN, tokens[0].Type);
            Assert.AreEqual(TokenType.LPAREN, tokens[1].Type);
            Assert.AreEqual(TokenType.RPAREN, tokens[2].Type);
            Assert.AreEqual(TokenType.LBRACE, tokens[3].Type);
            Assert.AreEqual(TokenType.RETURN, tokens[4].Type);
            Assert.AreEqual(TokenType.NUMBER_LITERAL, tokens[5].Type);
            Assert.AreEqual("42", tokens[5].Lexeme);
            Assert.AreEqual(TokenType.SEMICOLON, tokens[6].Type);
            Assert.AreEqual(TokenType.RBRACE, tokens[7].Type);
        }

        [TestMethod]
        public void Tokenize_WithUnterminatedStringLiteral_ThrowsInvalidOperationException()
        {
            string code = "string str = \"hello world";
            Lexer lexer = new Lexer(code);

            Assert.ThrowsException<LexerException>(() => lexer.Tokenize());
        }

        [TestMethod]
        public void Tokenize_WhileLoop_ReturnsCorrectTokens()
        {
            string code = "while (true) { return 42; }";
            Lexer lexer = new Lexer(code);

            List<Token> tokens = lexer.Tokenize();

            Assert.AreEqual(10, tokens.Count);
            Assert.AreEqual(TokenType.WHILE, tokens[0].Type);
            Assert.AreEqual(TokenType.LPAREN, tokens[1].Type);
            Assert.AreEqual(TokenType.TRUE_LITERAL, tokens[2].Type);
            Assert.AreEqual(TokenType.RPAREN, tokens[3].Type);
            Assert.AreEqual(TokenType.LBRACE, tokens[4].Type);
            Assert.AreEqual(TokenType.RETURN, tokens[5].Type);
            Assert.AreEqual(TokenType.NUMBER_LITERAL, tokens[6].Type);
            Assert.AreEqual("42", tokens[6].Lexeme);
            Assert.AreEqual(TokenType.SEMICOLON, tokens[7].Type);
            Assert.AreEqual(TokenType.RBRACE, tokens[8].Type);
            Assert.AreEqual(TokenType.EOF, tokens[9].Type);
        }

        [TestMethod]
        public void Tokenize_ComplexCode_ReturnsCorrectTokens()
        {
            const int IMPORT_OFFSET = 3;
            string code = @"                import Terminal;

                func add(number a, number b) -> number {
                    return a + b;
                }

                func concatenate(string a, string b) -> string {
                    return a + b;
                }

                func is_even(number a) -> bool {
                    return a % 2 == 0;
                }

                _main_() {
                    number x = 10;
                    number y = 20;
                    number sum = add(x, y);
                    string hello = ""Hello, "";
                    string world = ""world!"";
                    string message = concatenate(hello, world);

                    Terminal.print(message);

                    bool check = is_even(sum);
                    if (check) {
                        Terminal.print(""Sum is even."");
                    } 
                }
            "
            ;

            Lexer lexer = new Lexer(code);

            List<Token> tokens = lexer.Tokenize();

            Assert.AreEqual(130, tokens.Count); // Adjusted based on the actual token count in the code

            // Validate specific tokens and their types
            AssertToken(tokens[IMPORT_OFFSET + 0], TokenType.FUNC, "func");
            AssertToken(tokens[IMPORT_OFFSET + 1], TokenType.IDENTIFIER, "add");
            AssertToken(tokens[IMPORT_OFFSET + 2], TokenType.LPAREN, "(");
            AssertToken(tokens[IMPORT_OFFSET + 3], TokenType.NUMBER, "number");
            AssertToken(tokens[IMPORT_OFFSET + 4], TokenType.IDENTIFIER, "a");
            AssertToken(tokens[IMPORT_OFFSET + 5], TokenType.COMMA, ",");
            AssertToken(tokens[IMPORT_OFFSET + 6], TokenType.NUMBER, "number");
            AssertToken(tokens[IMPORT_OFFSET + 7], TokenType.IDENTIFIER, "b");
            AssertToken(tokens[IMPORT_OFFSET + 8], TokenType.RPAREN, ")");
            AssertToken(tokens[IMPORT_OFFSET + 9], TokenType.ARROW, "->");
            AssertToken(tokens[IMPORT_OFFSET + 10], TokenType.NUMBER, "number");
            AssertToken(tokens[IMPORT_OFFSET + 11], TokenType.LBRACE, "{");
            AssertToken(tokens[IMPORT_OFFSET + 12], TokenType.RETURN, "return");
            AssertToken(tokens[IMPORT_OFFSET + 13], TokenType.IDENTIFIER, "a");
            AssertToken(tokens[IMPORT_OFFSET + 14], TokenType.PLUS, "+");
            AssertToken(tokens[IMPORT_OFFSET + 15], TokenType.IDENTIFIER, "b");
            AssertToken(tokens[IMPORT_OFFSET + 16], TokenType.SEMICOLON, ";");
            AssertToken(tokens[IMPORT_OFFSET + 17], TokenType.RBRACE, "}");

            AssertToken(tokens[IMPORT_OFFSET + 18], TokenType.FUNC, "func");
            AssertToken(tokens[IMPORT_OFFSET + 19], TokenType.IDENTIFIER, "concatenate");
            AssertToken(tokens[IMPORT_OFFSET + 20], TokenType.LPAREN, "(");
            AssertToken(tokens[IMPORT_OFFSET + 21], TokenType.STRING, "string");
            AssertToken(tokens[IMPORT_OFFSET + 22], TokenType.IDENTIFIER, "a");
            AssertToken(tokens[IMPORT_OFFSET + 23], TokenType.COMMA, ",");
            AssertToken(tokens[IMPORT_OFFSET + 24], TokenType.STRING, "string");
            AssertToken(tokens[IMPORT_OFFSET + 25], TokenType.IDENTIFIER, "b");
            AssertToken(tokens[IMPORT_OFFSET + 26], TokenType.RPAREN, ")");
            AssertToken(tokens[IMPORT_OFFSET + 27], TokenType.ARROW, "->");
            AssertToken(tokens[IMPORT_OFFSET + 28], TokenType.STRING, "string");
            AssertToken(tokens[IMPORT_OFFSET + 29], TokenType.LBRACE, "{");
            AssertToken(tokens[IMPORT_OFFSET + 30], TokenType.RETURN, "return");
            AssertToken(tokens[IMPORT_OFFSET + 31], TokenType.IDENTIFIER, "a");
            AssertToken(tokens[IMPORT_OFFSET + 32], TokenType.PLUS, "+");
            AssertToken(tokens[IMPORT_OFFSET + 33], TokenType.IDENTIFIER, "b");
            AssertToken(tokens[IMPORT_OFFSET + 34], TokenType.SEMICOLON, ";");
            AssertToken(tokens[IMPORT_OFFSET + 35], TokenType.RBRACE, "}");

            AssertToken(tokens[IMPORT_OFFSET + 36], TokenType.FUNC, "func");
            AssertToken(tokens[IMPORT_OFFSET + 37], TokenType.IDENTIFIER, "is_even");
            AssertToken(tokens[IMPORT_OFFSET + 38], TokenType.LPAREN, "(");
            AssertToken(tokens[IMPORT_OFFSET + 39], TokenType.NUMBER, "number");
            AssertToken(tokens[IMPORT_OFFSET + 40], TokenType.IDENTIFIER, "a");
            AssertToken(tokens[IMPORT_OFFSET + 41], TokenType.RPAREN, ")");
            AssertToken(tokens[IMPORT_OFFSET + 42], TokenType.ARROW, "->");
            AssertToken(tokens[IMPORT_OFFSET + 43], TokenType.BOOL, "bool");
            AssertToken(tokens[IMPORT_OFFSET + 44], TokenType.LBRACE, "{");
            AssertToken(tokens[IMPORT_OFFSET + 45], TokenType.RETURN, "return");
            AssertToken(tokens[IMPORT_OFFSET + 46], TokenType.IDENTIFIER, "a");
            AssertToken(tokens[IMPORT_OFFSET + 47], TokenType.PERCENT, "%");
            AssertToken(tokens[IMPORT_OFFSET + 48], TokenType.NUMBER_LITERAL, "2");
            AssertToken(tokens[IMPORT_OFFSET + 49], TokenType.EQUAL_EQUAL, "==");
            AssertToken(tokens[IMPORT_OFFSET + 50], TokenType.NUMBER_LITERAL, "0");
            AssertToken(tokens[IMPORT_OFFSET + 51], TokenType.SEMICOLON, ";");
            AssertToken(tokens[IMPORT_OFFSET + 52], TokenType.RBRACE, "}");

            AssertToken(tokens[IMPORT_OFFSET + 53], TokenType.MAIN, "_main_");
            AssertToken(tokens[IMPORT_OFFSET + 54], TokenType.LPAREN, "(");
            AssertToken(tokens[IMPORT_OFFSET + 55], TokenType.RPAREN, ")");
            AssertToken(tokens[IMPORT_OFFSET + 56], TokenType.LBRACE, "{");

            AssertToken(tokens[IMPORT_OFFSET + 57], TokenType.NUMBER, "number");
            AssertToken(tokens[IMPORT_OFFSET + 58], TokenType.IDENTIFIER, "x");
            AssertToken(tokens[IMPORT_OFFSET + 59], TokenType.EQUAL, "=");
            AssertToken(tokens[IMPORT_OFFSET + 60], TokenType.NUMBER_LITERAL, "10");
            AssertToken(tokens[IMPORT_OFFSET + 61], TokenType.SEMICOLON, ";");

            AssertToken(tokens[IMPORT_OFFSET + 62], TokenType.NUMBER, "number");
            AssertToken(tokens[IMPORT_OFFSET + 63], TokenType.IDENTIFIER, "y");
            AssertToken(tokens[IMPORT_OFFSET + 64], TokenType.EQUAL, "=");
            AssertToken(tokens[IMPORT_OFFSET + 65], TokenType.NUMBER_LITERAL, "20");
            AssertToken(tokens[IMPORT_OFFSET + 66], TokenType.SEMICOLON, ";");

            AssertToken(tokens[IMPORT_OFFSET + 67], TokenType.NUMBER, "number");
            AssertToken(tokens[IMPORT_OFFSET + 68], TokenType.IDENTIFIER, "sum");
            AssertToken(tokens[IMPORT_OFFSET + 69], TokenType.EQUAL, "=");
            AssertToken(tokens[IMPORT_OFFSET + 70], TokenType.IDENTIFIER, "add");
            AssertToken(tokens[IMPORT_OFFSET + 71], TokenType.LPAREN, "(");
            AssertToken(tokens[IMPORT_OFFSET + 72], TokenType.IDENTIFIER, "x");
            AssertToken(tokens[IMPORT_OFFSET + 73], TokenType.COMMA, ",");
            AssertToken(tokens[IMPORT_OFFSET + 74], TokenType.IDENTIFIER, "y");
            AssertToken(tokens[IMPORT_OFFSET + 75], TokenType.RPAREN, ")");
            AssertToken(tokens[IMPORT_OFFSET + 76], TokenType.SEMICOLON, ";");

            AssertToken(tokens[IMPORT_OFFSET + 77], TokenType.STRING, "string");
            AssertToken(tokens[IMPORT_OFFSET + 78], TokenType.IDENTIFIER, "hello");
            AssertToken(tokens[IMPORT_OFFSET + 79], TokenType.EQUAL, "=");
            AssertToken(tokens[IMPORT_OFFSET + 80], TokenType.STRING_LITERAL, "Hello, ");
            AssertToken(tokens[IMPORT_OFFSET + 81], TokenType.SEMICOLON, ";");

            AssertToken(tokens[IMPORT_OFFSET + 82], TokenType.STRING, "string");
            AssertToken(tokens[IMPORT_OFFSET + 83], TokenType.IDENTIFIER, "world");
            AssertToken(tokens[IMPORT_OFFSET + 84], TokenType.EQUAL, "=");
            AssertToken(tokens[IMPORT_OFFSET + 85], TokenType.STRING_LITERAL, "world!");
            AssertToken(tokens[IMPORT_OFFSET + 86], TokenType.SEMICOLON, ";");

            AssertToken(tokens[IMPORT_OFFSET + 87], TokenType.STRING, "string");
            AssertToken(tokens[IMPORT_OFFSET + 88], TokenType.IDENTIFIER, "message");
            AssertToken(tokens[IMPORT_OFFSET + 89], TokenType.EQUAL, "=");
            AssertToken(tokens[IMPORT_OFFSET + 90], TokenType.IDENTIFIER, "concatenate");
            AssertToken(tokens[IMPORT_OFFSET + 91], TokenType.LPAREN, "(");
            AssertToken(tokens[IMPORT_OFFSET + 92], TokenType.IDENTIFIER, "hello");
            AssertToken(tokens[IMPORT_OFFSET + 93], TokenType.COMMA, ",");
            AssertToken(tokens[IMPORT_OFFSET + 94], TokenType.IDENTIFIER, "world");
            AssertToken(tokens[IMPORT_OFFSET + 95], TokenType.RPAREN, ")");
            AssertToken(tokens[IMPORT_OFFSET + 96], TokenType.SEMICOLON, ";");

            AssertToken(tokens[IMPORT_OFFSET + 97], TokenType.IDENTIFIER, "Terminal");
            AssertToken(tokens[IMPORT_OFFSET + 98], TokenType.DOT, ".");
            AssertToken(tokens[IMPORT_OFFSET + 99], TokenType.IDENTIFIER, "print");
            AssertToken(tokens[IMPORT_OFFSET + 100], TokenType.LPAREN, "(");
            AssertToken(tokens[IMPORT_OFFSET + 101], TokenType.IDENTIFIER, "message");
            AssertToken(tokens[IMPORT_OFFSET + 102], TokenType.RPAREN, ")");
            AssertToken(tokens[IMPORT_OFFSET + 103], TokenType.SEMICOLON, ";");

            AssertToken(tokens[IMPORT_OFFSET + 104], TokenType.BOOL, "bool");
            AssertToken(tokens[IMPORT_OFFSET + 105], TokenType.IDENTIFIER, "check");
            AssertToken(tokens[IMPORT_OFFSET + 106], TokenType.EQUAL, "=");
            AssertToken(tokens[IMPORT_OFFSET + 107], TokenType.IDENTIFIER, "is_even");
            AssertToken(tokens[IMPORT_OFFSET + 108], TokenType.LPAREN, "(");
            AssertToken(tokens[IMPORT_OFFSET + 109], TokenType.IDENTIFIER, "sum");
            AssertToken(tokens[IMPORT_OFFSET + 110], TokenType.RPAREN, ")");
            AssertToken(tokens[IMPORT_OFFSET + 111], TokenType.SEMICOLON, ";");

            AssertToken(tokens[IMPORT_OFFSET + 112], TokenType.IF, "if");
            AssertToken(tokens[IMPORT_OFFSET + 113], TokenType.LPAREN, "(");
            AssertToken(tokens[IMPORT_OFFSET + 114], TokenType.IDENTIFIER, "check");
            AssertToken(tokens[IMPORT_OFFSET + 115], TokenType.RPAREN, ")");
            AssertToken(tokens[IMPORT_OFFSET + 116], TokenType.LBRACE, "{");
            AssertToken(tokens[IMPORT_OFFSET + 117], TokenType.IDENTIFIER, "Terminal");
            AssertToken(tokens[IMPORT_OFFSET + 118], TokenType.DOT, ".");
            AssertToken(tokens[IMPORT_OFFSET + 119], TokenType.IDENTIFIER, "print");
            AssertToken(tokens[IMPORT_OFFSET + 120], TokenType.LPAREN, "(");
            AssertToken(tokens[IMPORT_OFFSET + 121], TokenType.STRING_LITERAL, "Sum is even.");
            AssertToken(tokens[IMPORT_OFFSET + 122], TokenType.RPAREN, ")");
            AssertToken(tokens[IMPORT_OFFSET + 123], TokenType.SEMICOLON, ";");
            AssertToken(tokens[IMPORT_OFFSET + 124], TokenType.RBRACE, "}");

            // Add more assertions as needed for the remaining tokens

        }

        [TestMethod]
        public void Tokenize_FibonacciCode_ReturnsCorrectTokens()
        {
            const int IMPORT_OFFSET = 3;
            string code = @"                import Terminal;

                func fib(number x) -> number 
                {
                   number result; 
                   if (x == 1 || x == 0) {
                      result = x;
                   } else {
                      result = fib(x - 1) + fib(x - 2);
                   }
                   return result;                                      
                }

                _main_() {
                   number x = 20; 
                   number i = 0;
                   while(i <= x) {
                      Terminal.print(i + "" => "" + fib(i));
                      i++;
                   }
                }
            "
            ;

            Lexer lexer = new Lexer(code);

            List<Token> tokens = lexer.Tokenize();

            Assert.AreEqual(95, tokens.Count); // Adjusted based on the actual token count in the code

            // Validate specific tokens and their types
            AssertToken(tokens[IMPORT_OFFSET + 0], TokenType.FUNC, "func");
            AssertToken(tokens[IMPORT_OFFSET + 1], TokenType.IDENTIFIER, "fib");
            AssertToken(tokens[IMPORT_OFFSET + 2], TokenType.LPAREN, "(");
            AssertToken(tokens[IMPORT_OFFSET + 3], TokenType.NUMBER, "number");
            AssertToken(tokens[IMPORT_OFFSET + 4], TokenType.IDENTIFIER, "x");
            AssertToken(tokens[IMPORT_OFFSET + 5], TokenType.RPAREN, ")");
            AssertToken(tokens[IMPORT_OFFSET + 6], TokenType.ARROW, "->");
            AssertToken(tokens[IMPORT_OFFSET + 7], TokenType.NUMBER, "number");
            AssertToken(tokens[IMPORT_OFFSET + 8], TokenType.LBRACE, "{");

            // Tokens inside the function definition
            AssertToken(tokens[IMPORT_OFFSET + 9], TokenType.NUMBER, "number");
            AssertToken(tokens[IMPORT_OFFSET + 10], TokenType.IDENTIFIER, "result");
            AssertToken(tokens[IMPORT_OFFSET + 11], TokenType.SEMICOLON, ";");
            AssertToken(tokens[IMPORT_OFFSET + 12], TokenType.IF, "if");
            AssertToken(tokens[IMPORT_OFFSET + 13], TokenType.LPAREN, "(");
            AssertToken(tokens[IMPORT_OFFSET + 14], TokenType.IDENTIFIER, "x");
            AssertToken(tokens[IMPORT_OFFSET + 15], TokenType.EQUAL_EQUAL, "==");
            AssertToken(tokens[IMPORT_OFFSET + 16], TokenType.NUMBER_LITERAL, "1");
            AssertToken(tokens[IMPORT_OFFSET + 17], TokenType.OR_OR, "||");
            AssertToken(tokens[IMPORT_OFFSET + 18], TokenType.IDENTIFIER, "x");
            AssertToken(tokens[IMPORT_OFFSET + 19], TokenType.EQUAL_EQUAL, "==");
            AssertToken(tokens[IMPORT_OFFSET + 20], TokenType.NUMBER_LITERAL, "0");
            AssertToken(tokens[IMPORT_OFFSET + 21], TokenType.RPAREN, ")");
            AssertToken(tokens[IMPORT_OFFSET + 22], TokenType.LBRACE, "{");
            AssertToken(tokens[IMPORT_OFFSET + 23], TokenType.IDENTIFIER, "result");
            AssertToken(tokens[IMPORT_OFFSET + 24], TokenType.EQUAL, "=");
            AssertToken(tokens[IMPORT_OFFSET + 25], TokenType.IDENTIFIER, "x");
            AssertToken(tokens[IMPORT_OFFSET + 26], TokenType.SEMICOLON, ";");
            AssertToken(tokens[IMPORT_OFFSET + 27], TokenType.RBRACE, "}");
            AssertToken(tokens[IMPORT_OFFSET + 28], TokenType.ELSE, "else");
            AssertToken(tokens[IMPORT_OFFSET + 29], TokenType.LBRACE, "{");
            AssertToken(tokens[IMPORT_OFFSET + 30], TokenType.IDENTIFIER, "result");
            AssertToken(tokens[IMPORT_OFFSET + 31], TokenType.EQUAL, "=");
            AssertToken(tokens[IMPORT_OFFSET + 32], TokenType.IDENTIFIER, "fib");
            AssertToken(tokens[IMPORT_OFFSET + 33], TokenType.LPAREN, "(");
            AssertToken(tokens[IMPORT_OFFSET + 34], TokenType.IDENTIFIER, "x");
            AssertToken(tokens[IMPORT_OFFSET + 35], TokenType.MINUS, "-");
            AssertToken(tokens[IMPORT_OFFSET + 36], TokenType.NUMBER_LITERAL, "1");
            AssertToken(tokens[IMPORT_OFFSET + 37], TokenType.RPAREN, ")");
            AssertToken(tokens[IMPORT_OFFSET + 38], TokenType.PLUS, "+");
            AssertToken(tokens[IMPORT_OFFSET + 39], TokenType.IDENTIFIER, "fib");
            AssertToken(tokens[IMPORT_OFFSET + 40], TokenType.LPAREN, "(");
            AssertToken(tokens[IMPORT_OFFSET + 41], TokenType.IDENTIFIER, "x");
            AssertToken(tokens[IMPORT_OFFSET + 42], TokenType.MINUS, "-");
            AssertToken(tokens[IMPORT_OFFSET + 43], TokenType.NUMBER_LITERAL, "2");
            AssertToken(tokens[IMPORT_OFFSET + 44], TokenType.RPAREN, ")");
            AssertToken(tokens[IMPORT_OFFSET + 45], TokenType.SEMICOLON, ";");
            AssertToken(tokens[IMPORT_OFFSET + 46], TokenType.RBRACE, "}");
            AssertToken(tokens[IMPORT_OFFSET + 47], TokenType.RETURN, "return");
            AssertToken(tokens[IMPORT_OFFSET + 48], TokenType.IDENTIFIER, "result");
            AssertToken(tokens[IMPORT_OFFSET + 49], TokenType.SEMICOLON, ";");
            AssertToken(tokens[IMPORT_OFFSET + 50], TokenType.RBRACE, "}");

            // Tokens inside the _main_ function
            AssertToken(tokens[IMPORT_OFFSET + 51], TokenType.MAIN, "_main_");
            AssertToken(tokens[IMPORT_OFFSET + 52], TokenType.LPAREN, "(");
            AssertToken(tokens[IMPORT_OFFSET + 53], TokenType.RPAREN, ")");
            AssertToken(tokens[IMPORT_OFFSET + 54], TokenType.LBRACE, "{");
            AssertToken(tokens[IMPORT_OFFSET + 55], TokenType.NUMBER, "number");
            AssertToken(tokens[IMPORT_OFFSET + 56], TokenType.IDENTIFIER, "x");
            AssertToken(tokens[IMPORT_OFFSET + 57], TokenType.EQUAL, "=");
            AssertToken(tokens[IMPORT_OFFSET + 58], TokenType.NUMBER_LITERAL, "20");
            AssertToken(tokens[IMPORT_OFFSET + 59], TokenType.SEMICOLON, ";");
            AssertToken(tokens[IMPORT_OFFSET + 60], TokenType.NUMBER, "number");
            AssertToken(tokens[IMPORT_OFFSET + 61], TokenType.IDENTIFIER, "i");
            AssertToken(tokens[IMPORT_OFFSET + 62], TokenType.EQUAL, "=");
            AssertToken(tokens[IMPORT_OFFSET + 63], TokenType.NUMBER_LITERAL, "0");
            AssertToken(tokens[IMPORT_OFFSET + 64], TokenType.SEMICOLON, ";");
            AssertToken(tokens[IMPORT_OFFSET + 65], TokenType.WHILE, "while");
            AssertToken(tokens[IMPORT_OFFSET + 66], TokenType.LPAREN, "(");
            AssertToken(tokens[IMPORT_OFFSET + 67], TokenType.IDENTIFIER, "i");
            AssertToken(tokens[IMPORT_OFFSET + 68], TokenType.LTE, "<=");
            AssertToken(tokens[IMPORT_OFFSET + 69], TokenType.IDENTIFIER, "x");
            AssertToken(tokens[IMPORT_OFFSET + 70], TokenType.RPAREN, ")");
            AssertToken(tokens[IMPORT_OFFSET + 71], TokenType.LBRACE, "{");
            AssertToken(tokens[IMPORT_OFFSET + 72], TokenType.IDENTIFIER, "Terminal");
            AssertToken(tokens[IMPORT_OFFSET + 73], TokenType.DOT, ".");
            AssertToken(tokens[IMPORT_OFFSET + 74], TokenType.IDENTIFIER, "print");
            AssertToken(tokens[IMPORT_OFFSET + 75], TokenType.LPAREN, "(");
            AssertToken(tokens[IMPORT_OFFSET + 76], TokenType.IDENTIFIER, "i");
            AssertToken(tokens[IMPORT_OFFSET + 77], TokenType.PLUS, "+");
            AssertToken(tokens[IMPORT_OFFSET + 78], TokenType.STRING_LITERAL, " => ");
            AssertToken(tokens[IMPORT_OFFSET + 79], TokenType.PLUS, "+");
            AssertToken(tokens[IMPORT_OFFSET + 80], TokenType.IDENTIFIER, "fib");
            AssertToken(tokens[IMPORT_OFFSET + 81], TokenType.LPAREN, "(");
            AssertToken(tokens[IMPORT_OFFSET + 82], TokenType.IDENTIFIER, "i");
            AssertToken(tokens[IMPORT_OFFSET + 83], TokenType.RPAREN, ")");
            AssertToken(tokens[IMPORT_OFFSET + 84], TokenType.RPAREN, ")");
            AssertToken(tokens[IMPORT_OFFSET + 85], TokenType.SEMICOLON, ";");
            AssertToken(tokens[IMPORT_OFFSET + 86], TokenType.IDENTIFIER, "i");
            AssertToken(tokens[IMPORT_OFFSET + 87], TokenType.PLUS_PLUS, "++");
            AssertToken(tokens[IMPORT_OFFSET + 88], TokenType.SEMICOLON, ";");
            AssertToken(tokens[IMPORT_OFFSET + 89], TokenType.RBRACE, "}");
            AssertToken(tokens[IMPORT_OFFSET + 90], TokenType.RBRACE, "}");
        }
        [TestMethod]
        public void Tokenize_LargeMultilineSource_PreservesFinalTokenLocation()
        {
            const int declarationCount = 5000;
            string source = string.Join('\n', Enumerable.Range(0, declarationCount)
                .Select(index => $"number value{index} = {index};"));

            List<Token> tokens = new Lexer(source, sourceName: "large.ben").Tokenize();
            Token finalIdentifier = tokens.Last(token => token.Type == TokenType.IDENTIFIER);

            Assert.AreEqual(declarationCount, finalIdentifier.Span.Line);
            Assert.AreEqual(8, finalIdentifier.Span.Column);
            Assert.AreEqual($"value{declarationCount - 1}", finalIdentifier.Lexeme);
            Assert.AreEqual(Path.GetFullPath("large.ben"), finalIdentifier.Span.FileName);
        }

        private void AssertToken(Token token, TokenType expectedType, string? expectedLexeme)
        {
            Assert.AreEqual(expectedType, token.Type);
            Assert.AreEqual(expectedLexeme, token.Lexeme);
        }
    }
}

