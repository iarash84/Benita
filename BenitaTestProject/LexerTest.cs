using Benita;

namespace BenitaTestProject
{
    [TestClass]
    public class LexerTest
    {
        [TestMethod]
        public void Tokenize_SimpleCode_ReturnsCorrectTokens()
        {
            // Arrange
            string code = "_main_() { return 42; }";
            Lexer lexer = new Lexer(code);

            // Act
            List<Token> tokens = lexer.Tokenize();

            // Assert
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
            // Arrange
            string code = "if (true) { return 1; } else { return 0; }";
            Lexer lexer = new Lexer(code);

            // Act
            List<Token> tokens = lexer.Tokenize();

            // Assert
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
            // Arrange
            string code = "string str = \"hello world\";";
            Lexer lexer = new Lexer(code);

            // Act
            List<Token> tokens = lexer.Tokenize();

            // Assert
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
            // Arrange
            string code = "_main_() { // this is a comment\n return 42; }";
            Lexer lexer = new Lexer(code);

            // Act
            List<Token> tokens = lexer.Tokenize();

            // Assert
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
            // Arrange
            string code = "_main_() { /* multi\nline\ncomment */ return 42; }";
            Lexer lexer = new Lexer(code);

            // Act
            List<Token> tokens = lexer.Tokenize();

            // Assert
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
        public void Tokenize_UnterminatedStringLiteral_ShouldHandleError()
        {
            // Arrange
            string code = "string str = \"hello world";
            Lexer lexer = new Lexer(code);

            // Act
            List<Token> tokens = lexer.Tokenize();

            // Assert
            Assert.AreEqual(4, tokens.Count);
            Assert.AreEqual(TokenType.STRING, tokens[0].Type);
            Assert.AreEqual(TokenType.IDENTIFIER, tokens[1].Type);
            Assert.AreEqual(TokenType.EQUAL, tokens[2].Type);
            // Check for EOF as it couldn't find the closing "
            Assert.AreEqual(TokenType.EOF, tokens[3].Type);
        }

        [TestMethod]
        public void Tokenize_WhileLoop_ReturnsCorrectTokens()
        {
            // Arrange
            string code = "while (true) { return 42; }";
            Lexer lexer = new Lexer(code);

            // Act
            List<Token> tokens = lexer.Tokenize();

            // Assert
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
            // Arrange
            string code = @"
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

                    print(message);

                    bool check = is_even(sum);
                    if (check) {
                        print(""Sum is even."");
                    } 
                }
            "
            ;

            Lexer lexer = new Lexer(code);

            // Act
            List<Token> tokens = lexer.Tokenize();

            // Assert
            Assert.AreEqual(123, tokens.Count); // Adjusted based on the actual token count in the code

            // Validate specific tokens and their types
            AssertToken(tokens[0], TokenType.FUNC, "func");
            AssertToken(tokens[1], TokenType.IDENTIFIER, "add");
            AssertToken(tokens[2], TokenType.LPAREN, "(");
            AssertToken(tokens[3], TokenType.NUMBER, "number");
            AssertToken(tokens[4], TokenType.IDENTIFIER, "a");
            AssertToken(tokens[5], TokenType.COMMA, ",");
            AssertToken(tokens[6], TokenType.NUMBER, "number");
            AssertToken(tokens[7], TokenType.IDENTIFIER, "b");
            AssertToken(tokens[8], TokenType.RPAREN, ")");
            AssertToken(tokens[9], TokenType.ARROW, "->");
            AssertToken(tokens[10], TokenType.NUMBER, "number");
            AssertToken(tokens[11], TokenType.LBRACE, "{");
            AssertToken(tokens[12], TokenType.RETURN, "return");
            AssertToken(tokens[13], TokenType.IDENTIFIER, "a");
            AssertToken(tokens[14], TokenType.PLUS, "+");
            AssertToken(tokens[15], TokenType.IDENTIFIER, "b");
            AssertToken(tokens[16], TokenType.SEMICOLON, ";");
            AssertToken(tokens[17], TokenType.RBRACE, "}");

            AssertToken(tokens[18], TokenType.FUNC, "func");
            AssertToken(tokens[19], TokenType.IDENTIFIER, "concatenate");
            AssertToken(tokens[20], TokenType.LPAREN, "(");
            AssertToken(tokens[21], TokenType.STRING, "string");
            AssertToken(tokens[22], TokenType.IDENTIFIER, "a");
            AssertToken(tokens[23], TokenType.COMMA, ",");
            AssertToken(tokens[24], TokenType.STRING, "string");
            AssertToken(tokens[25], TokenType.IDENTIFIER, "b");
            AssertToken(tokens[26], TokenType.RPAREN, ")");
            AssertToken(tokens[27], TokenType.ARROW, "->");
            AssertToken(tokens[28], TokenType.STRING, "string");
            AssertToken(tokens[29], TokenType.LBRACE, "{");
            AssertToken(tokens[30], TokenType.RETURN, "return");
            AssertToken(tokens[31], TokenType.IDENTIFIER, "a");
            AssertToken(tokens[32], TokenType.PLUS, "+");
            AssertToken(tokens[33], TokenType.IDENTIFIER, "b");
            AssertToken(tokens[34], TokenType.SEMICOLON, ";");
            AssertToken(tokens[35], TokenType.RBRACE, "}");

            AssertToken(tokens[36], TokenType.FUNC, "func");
            AssertToken(tokens[37], TokenType.IDENTIFIER, "is_even");
            AssertToken(tokens[38], TokenType.LPAREN, "(");
            AssertToken(tokens[39], TokenType.NUMBER, "number");
            AssertToken(tokens[40], TokenType.IDENTIFIER, "a");
            AssertToken(tokens[41], TokenType.RPAREN, ")");
            AssertToken(tokens[42], TokenType.ARROW, "->");
            AssertToken(tokens[43], TokenType.BOOL, "bool");
            AssertToken(tokens[44], TokenType.LBRACE, "{");
            AssertToken(tokens[45], TokenType.RETURN, "return");
            AssertToken(tokens[46], TokenType.IDENTIFIER, "a");
            AssertToken(tokens[47], TokenType.PERCENT, "%");
            AssertToken(tokens[48], TokenType.NUMBER_LITERAL, "2");
            AssertToken(tokens[49], TokenType.EQUAL_EQUAL, "==");
            AssertToken(tokens[50], TokenType.NUMBER_LITERAL, "0");
            AssertToken(tokens[51], TokenType.SEMICOLON, ";");
            AssertToken(tokens[52], TokenType.RBRACE, "}");

            AssertToken(tokens[53], TokenType.MAIN, "_main_");
            AssertToken(tokens[54], TokenType.LPAREN, "(");
            AssertToken(tokens[55], TokenType.RPAREN, ")");
            AssertToken(tokens[56], TokenType.LBRACE, "{");

            AssertToken(tokens[57], TokenType.NUMBER, "number");
            AssertToken(tokens[58], TokenType.IDENTIFIER, "x");
            AssertToken(tokens[59], TokenType.EQUAL, "=");
            AssertToken(tokens[60], TokenType.NUMBER_LITERAL, "10");
            AssertToken(tokens[61], TokenType.SEMICOLON, ";");

            AssertToken(tokens[62], TokenType.NUMBER, "number");
            AssertToken(tokens[63], TokenType.IDENTIFIER, "y");
            AssertToken(tokens[64], TokenType.EQUAL, "=");
            AssertToken(tokens[65], TokenType.NUMBER_LITERAL, "20");
            AssertToken(tokens[66], TokenType.SEMICOLON, ";");

            AssertToken(tokens[67], TokenType.NUMBER, "number");
            AssertToken(tokens[68], TokenType.IDENTIFIER, "sum");
            AssertToken(tokens[69], TokenType.EQUAL, "=");
            AssertToken(tokens[70], TokenType.IDENTIFIER, "add");
            AssertToken(tokens[71], TokenType.LPAREN, "(");
            AssertToken(tokens[72], TokenType.IDENTIFIER, "x");
            AssertToken(tokens[73], TokenType.COMMA, ",");
            AssertToken(tokens[74], TokenType.IDENTIFIER, "y");
            AssertToken(tokens[75], TokenType.RPAREN, ")");
            AssertToken(tokens[76], TokenType.SEMICOLON, ";");

            AssertToken(tokens[77], TokenType.STRING, "string");
            AssertToken(tokens[78], TokenType.IDENTIFIER, "hello");
            AssertToken(tokens[79], TokenType.EQUAL, "=");
            AssertToken(tokens[80], TokenType.STRING_LITERAL, "Hello, ");
            AssertToken(tokens[81], TokenType.SEMICOLON, ";");

            AssertToken(tokens[82], TokenType.STRING, "string");
            AssertToken(tokens[83], TokenType.IDENTIFIER, "world");
            AssertToken(tokens[84], TokenType.EQUAL, "=");
            AssertToken(tokens[85], TokenType.STRING_LITERAL, "world!");
            AssertToken(tokens[86], TokenType.SEMICOLON, ";");

            AssertToken(tokens[87], TokenType.STRING, "string");
            AssertToken(tokens[88], TokenType.IDENTIFIER, "message");
            AssertToken(tokens[89], TokenType.EQUAL, "=");
            AssertToken(tokens[90], TokenType.IDENTIFIER, "concatenate");
            AssertToken(tokens[91], TokenType.LPAREN, "(");
            AssertToken(tokens[92], TokenType.IDENTIFIER, "hello");
            AssertToken(tokens[93], TokenType.COMMA, ",");
            AssertToken(tokens[94], TokenType.IDENTIFIER, "world");
            AssertToken(tokens[95], TokenType.RPAREN, ")");
            AssertToken(tokens[96], TokenType.SEMICOLON, ";");

            AssertToken(tokens[97], TokenType.IDENTIFIER, "print");
            AssertToken(tokens[98], TokenType.LPAREN, "(");
            AssertToken(tokens[99], TokenType.IDENTIFIER, "message");
            AssertToken(tokens[100], TokenType.RPAREN, ")");
            AssertToken(tokens[101], TokenType.SEMICOLON, ";");

            AssertToken(tokens[102], TokenType.BOOL, "bool");
            AssertToken(tokens[103], TokenType.IDENTIFIER, "check");
            AssertToken(tokens[104], TokenType.EQUAL, "=");
            AssertToken(tokens[105], TokenType.IDENTIFIER, "is_even");
            AssertToken(tokens[106], TokenType.LPAREN, "(");
            AssertToken(tokens[107], TokenType.IDENTIFIER, "sum");
            AssertToken(tokens[108], TokenType.RPAREN, ")");
            AssertToken(tokens[109], TokenType.SEMICOLON, ";");

            AssertToken(tokens[110], TokenType.IF, "if");
            AssertToken(tokens[111], TokenType.LPAREN, "(");
            AssertToken(tokens[112], TokenType.IDENTIFIER, "check");
            AssertToken(tokens[113], TokenType.RPAREN, ")");
            AssertToken(tokens[114], TokenType.LBRACE, "{");
            AssertToken(tokens[115], TokenType.IDENTIFIER, "print");
            AssertToken(tokens[116], TokenType.LPAREN, "(");
            AssertToken(tokens[117], TokenType.STRING_LITERAL, "Sum is even.");
            AssertToken(tokens[118], TokenType.RPAREN, ")");
            AssertToken(tokens[119], TokenType.SEMICOLON, ";");
            AssertToken(tokens[120], TokenType.RBRACE, "}");

            // Add more assertions as needed for the remaining tokens

        }

        [TestMethod]
        public void Tokenize_FibonacciCode_ReturnsCorrectTokens()
        {
            // Arrange
            string code = @"
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
                      print(i + "" => "" + fib(i));
                      i++;
                   }
                }
            "
            ;

            Lexer lexer = new Lexer(code);

            // Act
            List<Token> tokens = lexer.Tokenize();

            // Assert
            Assert.AreEqual(90, tokens.Count); // Adjusted based on the actual token count in the code

            // Validate specific tokens and their types
            AssertToken(tokens[0], TokenType.FUNC, "func");
            AssertToken(tokens[1], TokenType.IDENTIFIER, "fib");
            AssertToken(tokens[2], TokenType.LPAREN, "(");
            AssertToken(tokens[3], TokenType.NUMBER, "number");
            AssertToken(tokens[4], TokenType.IDENTIFIER, "x");
            AssertToken(tokens[5], TokenType.RPAREN, ")");
            AssertToken(tokens[6], TokenType.ARROW, "->");
            AssertToken(tokens[7], TokenType.NUMBER, "number");
            AssertToken(tokens[8], TokenType.LBRACE, "{");

            // Tokens inside the function definition
            AssertToken(tokens[9], TokenType.NUMBER, "number");
            AssertToken(tokens[10], TokenType.IDENTIFIER, "result");
            AssertToken(tokens[11], TokenType.SEMICOLON, ";");
            AssertToken(tokens[12], TokenType.IF, "if");
            AssertToken(tokens[13], TokenType.LPAREN, "(");
            AssertToken(tokens[14], TokenType.IDENTIFIER, "x");
            AssertToken(tokens[15], TokenType.EQUAL_EQUAL, "==");
            AssertToken(tokens[16], TokenType.NUMBER_LITERAL, "1");
            AssertToken(tokens[17], TokenType.OR_OR, "||");
            AssertToken(tokens[18], TokenType.IDENTIFIER, "x");
            AssertToken(tokens[19], TokenType.EQUAL_EQUAL, "==");
            AssertToken(tokens[20], TokenType.NUMBER_LITERAL, "0");
            AssertToken(tokens[21], TokenType.RPAREN, ")");
            AssertToken(tokens[22], TokenType.LBRACE, "{");
            AssertToken(tokens[23], TokenType.IDENTIFIER, "result");
            AssertToken(tokens[24], TokenType.EQUAL, "=");
            AssertToken(tokens[25], TokenType.IDENTIFIER, "x");
            AssertToken(tokens[26], TokenType.SEMICOLON, ";");
            AssertToken(tokens[27], TokenType.RBRACE, "}");
            AssertToken(tokens[28], TokenType.ELSE, "else");
            AssertToken(tokens[29], TokenType.LBRACE, "{");
            AssertToken(tokens[30], TokenType.IDENTIFIER, "result");
            AssertToken(tokens[31], TokenType.EQUAL, "=");
            AssertToken(tokens[32], TokenType.IDENTIFIER, "fib");
            AssertToken(tokens[33], TokenType.LPAREN, "(");
            AssertToken(tokens[34], TokenType.IDENTIFIER, "x");
            AssertToken(tokens[35], TokenType.MINUS, "-");
            AssertToken(tokens[36], TokenType.NUMBER_LITERAL, "1");
            AssertToken(tokens[37], TokenType.RPAREN, ")");
            AssertToken(tokens[38], TokenType.PLUS, "+");
            AssertToken(tokens[39], TokenType.IDENTIFIER, "fib");
            AssertToken(tokens[40], TokenType.LPAREN, "(");
            AssertToken(tokens[41], TokenType.IDENTIFIER, "x");
            AssertToken(tokens[42], TokenType.MINUS, "-");
            AssertToken(tokens[43], TokenType.NUMBER_LITERAL, "2");
            AssertToken(tokens[44], TokenType.RPAREN, ")");
            AssertToken(tokens[45], TokenType.SEMICOLON, ";");
            AssertToken(tokens[46], TokenType.RBRACE, "}");
            AssertToken(tokens[47], TokenType.RETURN, "return");
            AssertToken(tokens[48], TokenType.IDENTIFIER, "result");
            AssertToken(tokens[49], TokenType.SEMICOLON, ";");
            AssertToken(tokens[50], TokenType.RBRACE, "}");

            // Tokens inside the _main_ function
            AssertToken(tokens[51], TokenType.MAIN, "_main_");
            AssertToken(tokens[52], TokenType.LPAREN, "(");
            AssertToken(tokens[53], TokenType.RPAREN, ")");
            AssertToken(tokens[54], TokenType.LBRACE, "{");
            AssertToken(tokens[55], TokenType.NUMBER, "number");
            AssertToken(tokens[56], TokenType.IDENTIFIER, "x");
            AssertToken(tokens[57], TokenType.EQUAL, "=");
            AssertToken(tokens[58], TokenType.NUMBER_LITERAL, "20");
            AssertToken(tokens[59], TokenType.SEMICOLON, ";");
            AssertToken(tokens[60], TokenType.NUMBER, "number");
            AssertToken(tokens[61], TokenType.IDENTIFIER, "i");
            AssertToken(tokens[62], TokenType.EQUAL, "=");
            AssertToken(tokens[63], TokenType.NUMBER_LITERAL, "0");
            AssertToken(tokens[64], TokenType.SEMICOLON, ";");
            AssertToken(tokens[65], TokenType.WHILE, "while");
            AssertToken(tokens[66], TokenType.LPAREN, "(");
            AssertToken(tokens[67], TokenType.IDENTIFIER, "i");
            AssertToken(tokens[68], TokenType.LTE, "<=");
            AssertToken(tokens[69], TokenType.IDENTIFIER, "x");
            AssertToken(tokens[70], TokenType.RPAREN, ")");
            AssertToken(tokens[71], TokenType.LBRACE, "{");
            AssertToken(tokens[72], TokenType.IDENTIFIER, "print");
            AssertToken(tokens[73], TokenType.LPAREN, "(");
            AssertToken(tokens[74], TokenType.IDENTIFIER, "i");
            AssertToken(tokens[75], TokenType.PLUS, "+");
            AssertToken(tokens[76], TokenType.STRING_LITERAL, " => ");
            AssertToken(tokens[77], TokenType.PLUS, "+");
            AssertToken(tokens[78], TokenType.IDENTIFIER, "fib");
            AssertToken(tokens[79], TokenType.LPAREN, "(");
            AssertToken(tokens[80], TokenType.IDENTIFIER, "i");
            AssertToken(tokens[81], TokenType.RPAREN, ")");
            AssertToken(tokens[82], TokenType.RPAREN, ")");
            AssertToken(tokens[83], TokenType.SEMICOLON, ";");
            AssertToken(tokens[84], TokenType.IDENTIFIER, "i");
            AssertToken(tokens[85], TokenType.PLUS_PLUS, "++");
            AssertToken(tokens[86], TokenType.SEMICOLON, ";");
            AssertToken(tokens[87], TokenType.RBRACE, "}");
            AssertToken(tokens[88], TokenType.RBRACE, "}");
        }



        private void AssertToken(Token token, TokenType expectedType, string? expectedLexeme)
        {
            Assert.AreEqual(expectedType, token.Type);
            Assert.AreEqual(expectedLexeme, token.Lexeme);
        }
    }
}

