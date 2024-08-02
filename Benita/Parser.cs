namespace Benita
{
    /// <summary>
    /// Represents a parser for analyzing a list of tokens and constructing an abstract syntax tree (AST).
    /// </summary>
    public class Parser
    {
        private readonly List<Token> _tokens; ///< The list of tokens to be parsed.
        private int _current = 0; ///< The index of the current token being processed.

        /// <summary>
        /// Initializes a new instance of the <see cref="Parser"/> class.
        /// </summary>
        /// <param name="tokens">The list of tokens to parse.</param>
        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
        }

        /// <summary>
        /// Parses the tokens to produce a <see cref="ProgramNode"/> representing the entire program.
        /// </summary>
        /// <returns>A <see cref="ProgramNode"/> representing the parsed program.</returns>
        /// <exception cref="Exception">Thrown if multiple main functions are defined or no main function is found.</exception>
        public ProgramNode Parse()
        {
            List<VariableDeclarationNode> globalVariables = new List<VariableDeclarationNode>();
            List<FunctionNode> functions = new List<FunctionNode>();
            FunctionNode mainFunction = null;

            while (!IsAtEnd())
            {
                if (Check(TokenType.MAIN))
                {
                    if (mainFunction != null)
                        throw new Exception("Error, Main function is already defined.");

                    mainFunction = ParseMainFunction();
                }
                else if (Match(TokenType.FUNC))
                {
                    functions.Add(ParseFunction());
                }
                else
                {
                    globalVariables.Add(ParseGlobalVariableDeclaration());
                }
            }

            if (mainFunction == null)
            {
                throw new Exception("No main function defined.");
            }
            return new ProgramNode(globalVariables, functions, mainFunction);
        }

        /// <summary>
        /// Parses a global variable declaration, which could be a regular variable or an array.
        /// </summary>
        /// <returns>A <see cref="VariableDeclarationNode"/> representing the global variable declaration.</returns>
        /// <exception cref="Exception">Thrown if the declaration is malformed.</exception>
        private VariableDeclarationNode ParseGlobalVariableDeclaration()
        {
            string? type = ParseType(); ///< The type of the variable (e.g., "number[]").
            string? name;
            if (Check(TokenType.LBRACE))
            {
                // Array declaration
                Advance(); // Consume '['

                if (Check(TokenType.RBRACE))
                {
                    // Array declaration without size
                    Advance(); // Consume ']'
                }
                else
                {
                    throw new Exception("Expected ']' after '[' for array declaration.");
                }

                name = Consume(TokenType.IDENTIFIER, "Expected variable name").Lexeme;

                ExpressionNode initializer = null;

                if (Match(TokenType.EQUAL))
                {
                    initializer = ParseArrayInitializer();
                }

                Consume(TokenType.SEMICOLON, "Expected ';' after variable declaration");

                return new VariableDeclarationNode(type + "[]", name, initializer);
            }
            else if (Check(TokenType.IDENTIFIER))
            {
                // Regular variable declaration
                name = Consume(TokenType.IDENTIFIER, "Expected variable name").Lexeme;
                ExpressionNode initializer = null;

                if (Match(TokenType.EQUAL))
                {
                    initializer = ParseExpression();
                }

                Consume(TokenType.SEMICOLON, "Expected ';' after variable declaration");

                return new VariableDeclarationNode(type, name, initializer);
            }
            else
            {
                throw new Exception("Expected array or variable declaration");
            }
        }

        /// <summary>
        /// Parses the main function of the program.
        /// </summary>
        /// <returns>A <see cref="FunctionNode"/> representing the main function.</returns>
        /// <exception cref="Exception">Thrown if the syntax of the main function is incorrect.</exception>
        private FunctionNode ParseMainFunction()
        {
            Consume(TokenType.MAIN, "Expected 'main'");
            Consume(TokenType.LPAREN, "Expected '(' after 'main'");
            List<ParameterNode> parameters = ParseParameters();
            Consume(TokenType.RPAREN, "Expected ')' after parameters");
            Consume(TokenType.LBRACE, "Expected '{' before main function body");

            List<StatementNode> statements = new List<StatementNode>();

            while (!IsAtEnd() && !Match(TokenType.RBRACE))
            {
                statements.Add(ParseStatement());
            }

            return new FunctionNode("_main_", parameters, "void", new BlockNode(statements), null);
        }

        /// <summary>
        /// Parses a function declaration.
        /// </summary>
        /// <returns>A <see cref="FunctionNode"/> representing the function declaration.</returns>
        private FunctionNode ParseFunction()
        {
            string? name = Consume(TokenType.IDENTIFIER, "Expected function name").Lexeme;
            Consume(TokenType.LPAREN, "Expected '(' after function name");
            List<ParameterNode> parameters = ParseParameters();
            Consume(TokenType.RPAREN, "Expected ')' after parameters");
            Consume(TokenType.ARROW, "Expected '->' after parameters");
            string? returnType = ParseType();
            Consume(TokenType.LBRACE, "Expected '{' before function body");

            List<StatementNode> statements = new List<StatementNode>();
            ReturnStatementNode returnExpression = null;

            while (!IsAtEnd() && !Check(TokenType.RBRACE))
            {
                if (Check(TokenType.RETURN))
                {
                    returnExpression = ParseReturnStatement(returnType, ref statements);
                    SkipRemainingCode();
                    break;
                }
                statements.Add(ParseStatement());
            }

            Consume(TokenType.RBRACE, "Expected '}' after function body");
            return new FunctionNode(name, parameters, returnType, new BlockNode(statements), returnExpression);
        }

        /// <summary>
        /// Parses a return statement within a function.
        /// </summary>
        /// <param name="returnType">The return type of the function.</param>
        /// <param name="statements">The list of statements in the function.</param>
        /// <returns>A <see cref="ReturnStatementNode"/> representing the return statement.</returns>
        private ReturnStatementNode ParseReturnStatement(string? returnType, ref List<StatementNode> statements)
        {
            ReturnStatementNode returnStatement = null;
            if (returnType != "void")
            {
                Consume(TokenType.RETURN, "Expected 'return' statement in function");
                if (!Check(TokenType.SEMICOLON))
                {
                    var returnExpression = ParseExpression();
                    returnStatement = new ReturnStatementNode(returnExpression);
                }
                Consume(TokenType.SEMICOLON, "Expected ';' after return value");
            }
            else
            {
                statements.Add(ParseStatement());
            }

            return returnStatement;
        }

        /// <summary>
        /// Parses a return statement without considering the return type.
        /// </summary>
        /// <returns>A <see cref="ReturnStatementNode"/> representing the return statement.</returns>
        private ReturnStatementNode ParseReturnStatement()
        {
            ExpressionNode returnExpression = null;
            if (!Check(TokenType.SEMICOLON))
            {
                returnExpression = ParseExpression();
            }
            var returnStatement = new ReturnStatementNode(returnExpression);
            Consume(TokenType.SEMICOLON, "Expected ';' after return value");
            return returnStatement;
        }

        /// <summary>
        /// Skips the remaining code until the closing brace is found.
        /// </summary>
        private void SkipRemainingCode()
        {
            int openBlock = 0;
            while (!IsAtEnd() && !(Check(TokenType.RBRACE) && openBlock == 0))
            {
                if (Check(TokenType.LBRACE)) openBlock++;
                if (Check(TokenType.RBRACE)) openBlock--;
                Advance(); // Skip tokens until the closing brace
            }
        }

        /// <summary>
        /// Parses the parameters of a function.
        /// </summary>
        /// <returns>A list of <see cref="ParameterNode"/> representing the function parameters.</returns>
        private List<ParameterNode> ParseParameters()
        {
            List<ParameterNode> parameters = new List<ParameterNode>();
            if (!Check(TokenType.RPAREN))
            {
                do
                {
                    string? type = ParseType();
                    string? name = Consume(TokenType.IDENTIFIER, "Expected parameter name").Lexeme;
                    parameters.Add(new ParameterNode(type, name));
                } while (Match(TokenType.COMMA));
            }
            return parameters;
        }

        /// <summary>
        /// Parses a type token (e.g., "number", "string", "bool", "void").
        /// </summary>
        /// <returns>The type as a string.</returns>
        /// <exception cref="Exception">Thrown if an unexpected token is encountered.</exception>
        private string? ParseType()
        {
            if (Match(TokenType.NUMBER)) return "number";
            if (Match(TokenType.STRING)) return "string";
            if (Match(TokenType.BOOL)) return "bool";
            if (Match(TokenType.VOID)) return "void";

            throw new Exception("Expected type");
        }

        /// <summary>
        /// Parses a statement, which can be an if, while, for, block, variable declaration, expression, or return statement.
        /// </summary>
        /// <returns>A <see cref="StatementNode"/> representing the parsed statement.</returns>
        /// <exception cref="Exception">Thrown if an unexpected token is encountered.</exception>
        private StatementNode ParseStatement()
        {
            if (Match(TokenType.IF))
            {
                return ParseIfStatement();
            }
            if (Match(TokenType.WHILE))
            {
                return ParseWhileStatement();
            }
            if (Match(TokenType.FOR))
            {
                return ParseForStatement();
            }
            if (Match(TokenType.LBRACE))
            {
                return ParseBlockStatement();
            }
            if (Match(TokenType.VOID, TokenType.BOOL, TokenType.NUMBER, TokenType.STRING))
            {
                return ParseVariableDeclaration();
            }
            if (Match(TokenType.IDENTIFIER))
            {
                return ParseExpressionStatementOrAssignment();
            }
            if (Match(TokenType.RETURN))
            {
                return ParseReturnStatement();
            }

            throw new Exception($"Unexpected token: {CurrentToken().Type}");
        }

        /// <summary>
        /// Parses an if statement.
        /// </summary>
        /// <returns>A <see cref="IfStatementNode"/> representing the if statement.</returns>
        private StatementNode ParseIfStatement()
        {
            Consume(TokenType.LPAREN, "Expected '(' after 'if'");
            ExpressionNode condition = ParseExpression();
            Consume(TokenType.RPAREN, "Expected ')' after if condition");
            StatementNode thenBranch = ParseStatement();
            StatementNode elseBranch = null;
            if (Match(TokenType.ELSE))
            {
                elseBranch = ParseStatement();
            }

            return new IfStatementNode(condition, thenBranch, elseBranch);
        }

        /// <summary>
        /// Parses a while statement.
        /// </summary>
        /// <returns>A <see cref="WhileStatementNode"/> representing the while statement.</returns>
        private StatementNode ParseWhileStatement()
        {
            Consume(TokenType.LPAREN, "Expected '(' after 'while'");
            ExpressionNode condition = ParseExpression();
            Consume(TokenType.RPAREN, "Expected ')' after while condition");
            StatementNode body = ParseStatement();
            return new WhileStatementNode(condition, body);
        }

        /// <summary>
        /// Parses a for statement.
        /// </summary>
        /// <returns>A <see cref="ForStatementNode"/> representing the for statement.</returns>
        private StatementNode ParseForStatement()
        {
            Consume(TokenType.LPAREN, "Expected '(' after 'for'");

            // Regular variable declaration
            var initializer = ParseStatement();
            ExpressionNode condition = ParseExpression();
            Consume(TokenType.SEMICOLON, "Expected ';' after condition in 'for'");
            StatementNode increment = null;
            if (Match(TokenType.IDENTIFIER))
            {
                increment = ParseExpressionStatementOrAssignment();
            }

            Consume(TokenType.RPAREN, "Expected ')' after for increment");
            StatementNode body = ParseStatement();
            return new ForStatementNode(initializer, condition, increment, body);
        }

        /// <summary>
        /// Parses a block statement enclosed in braces.
        /// </summary>
        /// <returns>A <see cref="BlockNode"/> representing the block statement.</returns>
        private StatementNode ParseBlockStatement()
        {
            List<StatementNode> statements = new List<StatementNode>();
            while (!Check(TokenType.RBRACE) && !IsAtEnd())
            {
                statements.Add(ParseStatement());
            }
            Consume(TokenType.RBRACE, "Expected '}' after block");
            return new BlockNode(statements);
        }

        /// <summary>
        /// Parses a variable declaration, which can be an array or a regular variable.
        /// </summary>
        /// <returns>A <see cref="VariableDeclarationNode"/> representing the variable declaration.</returns>
        /// <exception cref="Exception">Thrown if the declaration is malformed.</exception>
        private StatementNode ParseVariableDeclaration()
        {
            string? type = PreviousToken().Lexeme;
            string? name;

            if (Check(TokenType.LBRACE))
            {
                // Array declaration
                Advance(); // Consume '['

                if (Check(TokenType.RBRACE))
                {
                    // Array declaration without size
                    Advance(); // Consume ']'
                }
                else
                {
                    throw new Exception("Expected ']' after '[' for array declaration.");
                }

                name = Consume(TokenType.IDENTIFIER, "Expected variable name").Lexeme;

                ExpressionNode initializer = null;

                if (Match(TokenType.EQUAL))
                {
                    initializer = ParseArrayInitializer();
                }

                Consume(TokenType.SEMICOLON, "Expected ';' after variable declaration");

                return new VariableDeclarationNode(type + "[]", name, initializer);
            }
            else if (Check(TokenType.IDENTIFIER))
            {
                // Regular variable declaration
                name = Consume(TokenType.IDENTIFIER, "Expected variable name").Lexeme;
                ExpressionNode initializer = null;

                if (Match(TokenType.EQUAL))
                {
                    initializer = ParseExpression();
                }

                Consume(TokenType.SEMICOLON, "Expected ';' after variable declaration");

                return new VariableDeclarationNode(type, name, initializer);
            }
            else
            {
                throw new Exception("Expected array or variable declaration");
            }
        }

        /// <summary>
        /// Parses an array initializer, which includes a list of expressions enclosed in braces.
        /// </summary>
        /// <returns>A <see cref="ArrayInitializerNode"/> representing the array initializer.</returns>
        private ExpressionNode ParseArrayInitializer()
        {
            List<ExpressionNode> elements = new List<ExpressionNode>();
            if (Check(TokenType.LBRACE))
            {
                Consume(TokenType.LBRACE, "Expected '[' to start array initializer");
            }

            // Check if the array initializer is empty
            if (!Check(TokenType.RBRACE))
            {
                do
                {
                    elements.Add(ParseExpression());
                } while (Match(TokenType.COMMA));
            }
            Consume(TokenType.RBRACE, "Expected ']' after array initializer");

            return new ArrayInitializerNode(elements);
        }

        /// <summary>
        /// Parses an expression statement or an assignment statement.
        /// </summary>
        /// <returns>A <see cref="StatementNode"/> representing the expression or assignment.</returns>
        /// <exception cref="Exception">Thrown if the syntax is incorrect.</exception>
        private StatementNode ParseExpressionStatementOrAssignment()
        {
            string? name = PreviousToken().Lexeme;

            if (Match(TokenType.LBRACE)) // Check for array access
            {
                // Parse the array index expression
                ExpressionNode index = ParseExpression();
                Consume(TokenType.RBRACE, "Expected ']' after array index");

                // Check for assignment to array element
                if (Match(TokenType.EQUAL))
                {
                    ExpressionNode value = ParseExpression();
                    Consume(TokenType.SEMICOLON, "Expected ';' after assignment");
                    return new ArrayAssignmentNode(name, index, value);
                }

                throw new Exception("Expected '=' after array index");
            }

            if (Match(TokenType.PLUS_PLUS, TokenType.MINUS_MINUS))
            {
                string? operation = PreviousToken().Lexeme;
                Consume(TokenType.SEMICOLON, "Expected ';' after increment/decrement");
                return new IncrementDecrementNode(name, operation);
            }

            ExpressionNode expression;
            if (Match(TokenType.EQUAL)) // Regular variable assignment
            {
                expression = ParseExpression();
                Consume(TokenType.SEMICOLON, "Expected ';' after assignment");
                return new AssignmentNode(name, expression);
            }

            if (Match(TokenType.PLUS_EQUAL, TokenType.MINUS_EQUAL, TokenType.STAR_EQUAL, TokenType.SLASH_EQUAL))
            {
                string? operation = PreviousToken().Lexeme;
                expression = ParseExpression();
                Consume(TokenType.SEMICOLON, "Expected ';' after compound assignment");
                return new CompoundAssignmentNode(name, operation, expression);
            }

            expression = ParseExpression();
            Consume(TokenType.SEMICOLON, "Expected ';' after expression");
            return new ExpressionStatementNode(expression);
        }

        /// <summary>
        /// Parses an expression starting with logical OR operations.
        /// </summary>
        /// <returns>An <see cref="ExpressionNode"/> representing the parsed expression.</returns>
        private ExpressionNode ParseExpression()
        {
            return ParseLogicalOr();
        }

        /// <summary>
        /// Parses logical OR operations in expressions.
        /// </summary>
        /// <returns>An <see cref="ExpressionNode"/> representing the logical OR expression.</returns>
        private ExpressionNode ParseLogicalOr()
        {
            ExpressionNode expr = ParseLogicalAnd();

            while (Match(TokenType.OR_OR))
            {
                string? op = PreviousToken().Lexeme;
                ExpressionNode right = ParseLogicalAnd();
                expr = new LogicalExpressionNode(expr, op, right);
            }

            return expr;
        }

        /// <summary>
        /// Parses logical AND operations in expressions.
        /// </summary>
        /// <returns>An <see cref="ExpressionNode"/> representing the logical AND expression.</returns>
        private ExpressionNode ParseLogicalAnd()
        {
            ExpressionNode expr = ParseEquality();

            while (Match(TokenType.AND_AND))
            {
                string? op = PreviousToken().Lexeme;
                ExpressionNode right = ParseEquality();
                expr = new LogicalExpressionNode(expr, op, right);
            }

            return expr;
        }

        /// <summary>
        /// Parses equality operations in expressions.
        /// </summary>
        /// <returns>An <see cref="ExpressionNode"/> representing the equality expression.</returns>
        private ExpressionNode ParseEquality()
        {
            ExpressionNode expr = ParseComparison();

            while (Match(TokenType.EQUAL_EQUAL, TokenType.BANG_EQUAL))
            {
                string? op = PreviousToken().Lexeme;
                ExpressionNode right = ParseComparison();
                expr = new BinaryExpressionNode(expr, op, right);
            }

            return expr;
        }

        /// <summary>
        /// Parses comparison operations in expressions.
        /// </summary>
        /// <returns>An <see cref="ExpressionNode"/> representing the comparison expression.</returns>
        private ExpressionNode ParseComparison()
        {
            ExpressionNode expr = ParseTerm();

            while (Match(TokenType.GT, TokenType.GTE, TokenType.LT, TokenType.LTE))
            {
                string? op = PreviousToken().Lexeme;
                ExpressionNode right = ParseTerm();
                expr = new BinaryExpressionNode(expr, op, right);
            }

            return expr;
        }

        /// <summary>
        /// Parses term operations in expressions (e.g., addition, subtraction).
        /// </summary>
        /// <returns>An <see cref="ExpressionNode"/> representing the term expression.</returns>
        private ExpressionNode ParseTerm()
        {
            ExpressionNode expr = ParseFactor();

            while (Match(TokenType.PLUS, TokenType.MINUS))
            {
                string? op = PreviousToken().Lexeme;
                ExpressionNode right = ParseFactor();
                expr = new BinaryExpressionNode(expr, op, right);
            }

            return expr;
        }

        /// <summary>
        /// Parses factor operations in expressions (e.g., multiplication, division).
        /// </summary>
        /// <returns>An <see cref="ExpressionNode"/> representing the factor expression.</returns>
        private ExpressionNode ParseFactor()
        {
            ExpressionNode expr = ParseUnary();

            while (Match(TokenType.STAR, TokenType.SLASH, TokenType.PERCENT))
            {
                string? op = PreviousToken().Lexeme;
                ExpressionNode right = ParseUnary();
                expr = new BinaryExpressionNode(expr, op, right);
            }

            return expr;
        }

        /// <summary>
        /// Parses unary operations in expressions (e.g., negation).
        /// </summary>
        /// <returns>An <see cref="ExpressionNode"/> representing the unary expression.</returns>
        private ExpressionNode ParseUnary()
        {
            if (Match(TokenType.BANG, TokenType.MINUS))
            {
                string? op = PreviousToken().Lexeme;
                ExpressionNode right = ParseUnary();
                return new UnaryExpressionNode(op, right);
            }
            return ParsePrimary();
        }

        /// <summary>
        /// Parses primary expressions, including literals, identifiers, and function calls.
        /// </summary>
        /// <returns>An <see cref="ExpressionNode"/> representing the primary expression.</returns>
        private ExpressionNode ParsePrimary()
        {
            if (Match(TokenType.NUMBER_LITERAL, TokenType.STRING_LITERAL, TokenType.TRUE_LITERAL, TokenType.FALSE_LITERAL))
            {
                return new LiteralNode(PreviousToken().Lexeme, PreviousToken().Type);
            }

            if (Match(TokenType.IDENTIFIER))
            {
                return ParseIdentifier();
            }

            if (Match(TokenType.LBRACE))
            {
                return ParseArrayInitializer();
            }

            if (Check(TokenType.LPAREN))
            {
                string? name = PreviousToken().Lexeme;
                Consume(TokenType.LPAREN, "Expected '(' before arguments");
                List<ExpressionNode> arguments = new List<ExpressionNode>();
                if (!Check(TokenType.RPAREN))
                {
                    do
                    {
                        arguments.Add(ParseExpression());
                    } while (Match(TokenType.COMMA));
                }
                Consume(TokenType.RPAREN, "Expected ')' after arguments");
                return new FunctionCallNode(name, arguments);
            }

            throw new Exception($"Unexpected token: {CurrentToken().Type}");
        }

        /// <summary>
        /// Parses identifiers, including array access and function calls.
        /// </summary>
        /// <returns>An <see cref="ExpressionNode"/> representing the identifier expression.</returns>
        private ExpressionNode ParseIdentifier()
        {
            string? name = PreviousToken().Lexeme;
            if (Match(TokenType.LBRACE))
            {
                // Array access case
                ExpressionNode index = ParseExpression();
                Consume(TokenType.RBRACE, "Expected ']' after array index.");
                return new ArrayAccessNode(name, index);
            }
            else if (Match(TokenType.LPAREN))
            {
                List<ExpressionNode> arguments = new List<ExpressionNode>();
                if (!Check(TokenType.RPAREN))
                {
                    do
                    {
                        arguments.Add(ParseExpression());
                    } while (Match(TokenType.COMMA));
                }
                Consume(TokenType.RPAREN, "Expected ')' after arguments");
                return new FunctionCallNode(name, arguments);
            }
            return new IdentifierNode(name);
        }

        /// <summary>
        /// Attempts to match the current token with any of the specified token types.
        /// </summary>
        /// <param name="types">The token types to match against.</param>
        /// <returns>True if a match is found; otherwise, false.</returns>
        private bool Match(params TokenType[] types)
        {
            foreach (var type in types)
            {
                if (Check(type))
                {
                    Advance();
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if the current token matches the specified token type.
        /// </summary>
        /// <param name="type">The token type to check against.</param>
        /// <returns>True if the current token matches the specified type; otherwise, false.</returns>
        private bool Check(TokenType type)
        {
            if (IsAtEnd()) return false;
            return CurrentToken().Type == type;
        }

        /// <summary>
        /// Advances to the next token and returns the previous token.
        /// </summary>
        /// <returns>The previous token.</returns>
        private Token Advance()
        {
            if (!IsAtEnd()) _current++;
            return PreviousToken();
        }

        /// <summary>
        /// Checks if the parser has reached the end of the token list.
        /// </summary>
        /// <returns>True if the end of the token list is reached; otherwise, false.</returns>
        private bool IsAtEnd()
        {
            return CurrentToken().Type == TokenType.EOF;
        }

        /// <summary>
        /// Retrieves the current token being processed.
        /// </summary>
        /// <returns>The current token.</returns>
        private Token CurrentToken()
        {
            return _tokens[_current];
        }

        /// <summary>
        /// Retrieves the previous token before the current one.
        /// </summary>
        /// <returns>The previous token.</returns>
        private Token PreviousToken()
        {
            return _tokens[_current - 1];
        }

        /// <summary>
        /// Retrieves the next token after the current one.
        /// </summary>
        /// <returns>The next token.</returns>
        private Token NextToken()
        {
            return _tokens[_current + 1];
        }

        /// <summary>
        /// Consumes the current token if it matches the specified type; otherwise, throws an exception.
        /// </summary>
        /// <param name="type">The token type to consume.</param>
        /// <param name="message">The error message to display if the token type does not match.</param>
        /// <returns>The consumed token.</returns>
        /// <exception cref="Exception">Thrown if the current token does not match the specified type.</exception>
        private Token Consume(TokenType type, string message)
        {
            if (Check(type)) return Advance();
            throw new Exception(message);
        }
    }
}
