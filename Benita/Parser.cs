namespace Benita
{
    /// <summary>
    /// Represents a parser for analyzing a list of tokens and constructing an abstract syntax tree (AST).
    /// </summary>
    public class Parser
    {
        private readonly List<Token> _tokens; ///< The list of tokens to be parsed.
        private int _current = 0; ///< The index of the current token being processed.
        readonly List<FunctionNode?> _functions;///< The list of functions to be parsed.

        /// <summary>
        /// Initializes a new instance of the <see cref="Parser"/> class.
        /// </summary>
        /// <param name="tokens">The list of tokens to parse.</param>
        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
            _functions = new List<FunctionNode?>();
        }

        /// <summary>
        /// Parses the tokens to produce a <see cref="ProgramNode"/> representing the entire program.
        /// </summary>
        /// <returns>A <see cref="ProgramNode"/> representing the parsed program.</returns>
        /// <exception cref="Exception">Thrown if multiple main functions are defined or no main function is found.</exception>
        public ProgramNode Parse()
        {
            List<VariableDeclarationNode?> globalVariables = new List<VariableDeclarationNode?>();

            List<StatementNode?> statements = new List<StatementNode?>();
            List<PackageNode?> packages = new List<PackageNode?>();
            FunctionNode? mainFunction = null;

            while (!IsAtEnd())
            {
                if (Check(TokenType.MAIN))
                {
                    if (mainFunction != null)
                        throw new Exception("Error, Main function is already defined.");

                    mainFunction = ParseMainFunction();
                }
                else if (Match(TokenType.PACKAGE))
                {
                    packages.Add(ParsePackage());
                }
                else if (Match(TokenType.FUNC))
                {
                    _functions.Add(ParseFunction());
                }
                else if (Check(TokenType.LET) &&
                    NextToken().Type == TokenType.IDENTIFIER &&
                    NextToken(2).Type == TokenType.EQUAL &&
                    NextToken(3).Type == TokenType.NEW)
                {
                    statements.Add(ParseStatement());
                }
                else if (Check(TokenType.VOID, TokenType.BOOL, TokenType.NUMBER, TokenType.STRING, TokenType.LET))
                {
                    globalVariables.Add(ParseVariableDeclaration());
                }
                else
                {
                    statements.Add(ParseStatement());
                }
            }

            if (mainFunction == null && statements.Count == 0)
            {
                throw new Exception("No main function defined.");
            }

            return new ProgramNode(globalVariables, packages, _functions, mainFunction, statements);
        }

        /// <summary>
        /// Parses a package declaration, including its name and members (functions and variables).
        /// This function expects the package declaration to follow a specific syntax:
        /// <code>
        /// package PackageName {
        ///     func FunctionName(params) { ... }
        ///     Type VariableName = Initializer;
        /// }
        /// </code>
        /// The function will consume tokens to parse the package name, package body, and package members,
        /// including functions and variable declarations. It supports both explicitly-typed and implicitly-typed
        /// variable declarations.
        /// </summary>
        /// <returns>
        /// A <see cref="PackageNode"/> representing the parsed package, including its name and members.
        /// Returns null if the package cannot be parsed correctly.
        /// </returns>
        /// <exception cref="Exception">
        /// Thrown if the package syntax is incorrect, including missing braces, missing semicolons,
        /// or unexpected tokens within the package.
        /// </exception>
        private PackageNode? ParsePackage()
        {
            string packageName = Consume(TokenType.IDENTIFIER, "Expected package name").Lexeme;
            Consume(TokenType.LBRACE, "Expected '{' after package name.");

            List<PackageMemberNode> members = new List<PackageMemberNode>();
            while (!Check(TokenType.RBRACE) && !IsAtEnd())
            {
                if (Match(TokenType.FUNC))
                {
                    var function = ParseFunction();
                    members.Add(new PackageFunctionNode(function.Name, function.Parameters, function.ReturnType,
                        function.Body, function.ReturnStatement));
                }
                else if (Match(TokenType.NUMBER, TokenType.STRING, TokenType.BOOL, TokenType.LET))
                {
                    var type = PreviousToken().Lexeme;
                    if (Match(TokenType.IDENTIFIER))
                    {
                        var name = PreviousToken().Lexeme;
                        ExpressionNode? initializer = null;

                        if (Match(TokenType.EQUAL))
                        {
                            initializer = ParseExpression();
                        }
                        #region 'initilize Implicitly-typed variables'
                        if (type == "let")
                        {
                            if (initializer != null)
                            {
                                if (initializer is LiteralNode literalNode)
                                    type = ParseTokenType(literalNode.Type);
                                else if (initializer is FunctionCallNode functionCallNode)
                                {
                                    foreach (var function in _functions)
                                    {
                                        if (function.Name == functionCallNode.FunctionName)
                                            type = function.ReturnType;
                                    }
                                }
                            }
                            else
                            {
                                throw new Exception("Implicitly-typed variables must be initialized");
                            }
                        }
                        #endregion
                        Consume(TokenType.SEMICOLON, "Expected ';' after variable declaration");
                        members.Add(new PackageVariableDeclarationNode(type, name, initializer));
                    }
                    else
                    {
                        throw new Exception("Expected variable or function declaration.");
                    }
                }
                else
                {
                    throw new Exception("Unexpected token inside package.");
                }
            }
            Consume(TokenType.RBRACE, "Expected '}' after package body.");

            return new PackageNode(packageName, members);
        }

        /// <summary>
        /// Parses a  variable declaration, which could be a regular variable or an array.
        /// </summary>
        /// <returns>A <see cref="VariableDeclarationNode"/> representing the global variable declaration.</returns>
        /// <exception cref="Exception">Thrown if the declaration is malformed.</exception>
        private VariableDeclarationNode ParseVariableDeclaration()
        {
            string type = ParseType(); ///< The type of the variable (e.g., "number[]").
            if (Check(TokenType.IDENTIFIER))
            {
                string name = Consume(TokenType.IDENTIFIER, "Expected variable name").Lexeme;
                ExpressionNode initializer = null;

                if (Match(TokenType.EQUAL))
                {
                    if (type.Contains("[]") && !Check(TokenType.IDENTIFIER))
                    {
                        initializer = ParseArrayInitializer();
                    }
                    else
                    {
                        initializer = ParseExpression();
                    }
                }

                if (type == "let")
                {
                    if (initializer != null)
                    {
                        if (initializer is LiteralNode literalNode)
                            type = ParseTokenType(literalNode.Type);
                        else if (initializer is FunctionCallNode functionCallNode)
                        {
                            foreach (var function in _functions)
                            {
                                if (function.Name == functionCallNode.FunctionName)
                                    type = function.ReturnType;
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("Implicitly-typed variables must be initialized");
                    }
                }

                Consume(TokenType.SEMICOLON, "Expected ';' after variable declaration");

                return new VariableDeclarationNode(type, name, initializer);
            }

            throw new Exception("Expected array or variable declaration");
        }

        /// <summary>
        /// Parses the main function of the program.
        /// </summary>
        /// <returns>A <see cref="FunctionNode"/> representing the main function.</returns>
        /// <exception cref="Exception">Thrown if the syntax of the main function is incorrect.</exception>
        private FunctionNode? ParseMainFunction()
        {
            Consume(TokenType.MAIN, "Expected 'main'");
            Consume(TokenType.LPAREN, "Expected '(' after 'main'");
            List<ParameterNode> parameters = ParseParameters();
            Consume(TokenType.RPAREN, "Expected ')' after parameters");
            Consume(TokenType.LBRACE, "Expected '{' before main function body");

            List<StatementNode?> statements = new List<StatementNode?>();

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
        private FunctionNode? ParseFunction()
        {
            string name = Consume(TokenType.IDENTIFIER, "Expected function name").Lexeme;
            Consume(TokenType.LPAREN, "Expected '(' after function name");
            List<ParameterNode> parameters = ParseParameters();
            Consume(TokenType.RPAREN, "Expected ')' after parameters");
            Consume(TokenType.ARROW, "Expected '->' after parameters");
            string? returnType = ParseType();
            Consume(TokenType.LBRACE, "Expected '{' before function body");

            List<StatementNode?> statements = new List<StatementNode?>();
            ReturnStatementNode? returnExpression = null;

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
        private ReturnStatementNode? ParseReturnStatement(string? returnType, ref List<StatementNode?> statements)
        {
            ReturnStatementNode? returnStatement = null;
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
        private ReturnStatementNode? ParseReturnStatement()
        {
            ExpressionNode? returnExpression = null;
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
                    string type = ParseType();
                    string name = Consume(TokenType.IDENTIFIER, "Expected parameter name").Lexeme;
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
            string? type = string.Empty;
            if (Match(TokenType.NUMBER,TokenType.STRING,TokenType.BOOL,TokenType.LET,TokenType.VOID)) 
                type = ParseTokenType(PreviousToken().Type);

            if (Check(TokenType.LSQUAREBRACE))
            {
                Advance(); // Consume '['
                if (!Check(TokenType.RSQUAREBRACE))
                    throw new Exception("Expected ']' after '[' for array declaration.");
                Advance(); // Consume ']'
                type += "[]";
            }
            if (string.IsNullOrEmpty(type))
                throw new Exception("Expected type");

            return type;
        }

        /// <summary>
        /// Converts a given <see cref="TokenType"/> to its corresponding string representation
        /// for use in the language's type system.
        /// </summary>
        /// <param name="token">
        /// The <see cref="TokenType"/> that represents a specific data type or keyword in the language.
        /// </param>
        /// <returns>
        /// A string representing the corresponding data type ("number", "string", "bool", "void", "let").
        /// Returns null if the provided token does not match any known type.
        /// </returns>
        /// <exception cref="Exception">
        /// Thrown if the token does not correspond to any recognized type.
        /// </exception>
        private string? ParseTokenType(TokenType token)
        {
            if (token is TokenType.NUMBER or TokenType.NUMBER_LITERAL) return "number";
            if (token is TokenType.STRING or TokenType.STRING_LITERAL) return "string";
            if (token == TokenType.BOOL) return "bool";
            if (token == TokenType.VOID) return "void";
            if (token == TokenType.LET) return "let";
            throw new Exception("Expected type");
        }

        /// <summary>
        /// Parses a statement, which can be an if, while, for, block, variable declaration, expression, or return statement.
        /// </summary>
        /// <returns>A <see cref="StatementNode"/> representing the parsed statement.</returns>
        /// <exception cref="Exception">Thrown if an unexpected token is encountered.</exception>
        private StatementNode? ParseStatement()
        {
            if (Match(TokenType.BREAK))
            {
                Consume(TokenType.SEMICOLON, "Expected ';' after break");
                return new BreakStatementNode();
            }
            if (Match(TokenType.CONTINUE))
            {
                Consume(TokenType.SEMICOLON, "Expected ';' after continue");
                return new ContinueStatementNode();
            }
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

            if (Check(TokenType.LET) &&
                NextToken().Type == TokenType.IDENTIFIER &&
                NextToken(2).Type == TokenType.EQUAL &&
                NextToken(3).Type == TokenType.NEW)
            {
                Consume(TokenType.LET, "Object Instantiation checked");
                if (Match(TokenType.IDENTIFIER) && Check(TokenType.EQUAL))
                    return ParseObjectInstantiationOrAssignment();
            }


            if (Check(TokenType.VOID, TokenType.BOOL, TokenType.NUMBER, TokenType.STRING, TokenType.LET))
            {
                return ParseVariableDeclaration();
            }
            if (Match(TokenType.IDENTIFIER))
            {
                if (Match(TokenType.IDENTIFIER) && Check(TokenType.EQUAL))
                {
                    return ParseObjectInstantiationOrAssignment();
                }
                return ParseExpressionStatementOrAssignment();
            }
            if (Match(TokenType.RETURN))
            {
                return ParseReturnStatement();
            }

            throw new Exception($"Unexpected token: {CurrentToken().Type}");
        }

        /// <summary>
        /// Parses an object instantiation or assignment statement from the token stream.
        /// </summary>
        /// <returns>
        /// A <see cref="StatementNode"/> representing either an object instantiation or an assignment.
        /// Returns <see cref="ObjectInstantiationNode"/> if the statement is an object instantiation,
        /// otherwise returns <see cref="AssignmentNode"/> for an assignment statement.
        /// </returns>
        /// <exception cref="Exception">
        /// Thrown in the following cases:
        /// <list type="bullet">
        /// <item><description>If the expected '=' token is missing after the identifier.</description></item>
        /// <item><description>If the expected package name is missing after the 'new' keyword during object instantiation.</description></item>
        /// <item><description>If the expected '(' or ')' tokens are missing around the argument list during object instantiation.</description></item>
        /// <item><description>If the expected ';' token is missing after object instantiation or assignment.</description></item>
        /// <item><description>If there is a type mismatch between the initially declared package and the package being instantiated.</description></item>
        /// </list>
        /// </exception>
        private StatementNode? ParseObjectInstantiationOrAssignment()
        {
            string initialPackageName = PreviousToken(2).Lexeme;
            string name = PreviousToken().Lexeme;

            Consume(TokenType.EQUAL, "Expected '=' after identifier");

            if (Match(TokenType.NEW))
            {
                // Object instantiation
                string packageName = Consume(TokenType.IDENTIFIER, "Expected package name").Lexeme;
                Consume(TokenType.LPAREN, "Expected '(' after package name");
                List<ExpressionNode?> arguments = new List<ExpressionNode?>();
                if (!Check(TokenType.RPAREN))
                {
                    do
                    {
                        arguments.Add(ParseExpression());
                    } while (Match(TokenType.COMMA));
                }

                Consume(TokenType.RPAREN, "Expected ')' after arguments");
                Consume(TokenType.SEMICOLON, "Expected ';' after object instantiation");

                if (initialPackageName != packageName && initialPackageName != "let")
                    throw new Exception($"Cannot implicitly convert type '{packageName}' to '{initialPackageName}'");

                return new ObjectInstantiationNode(name, packageName, arguments);
            }

            // Assignment
            ExpressionNode? value = ParseExpression();
            Consume(TokenType.SEMICOLON, "Expected ';' after assignment");
            return new AssignmentNode(name, value);
        }

        /// <summary>
        /// Parses an if statement.
        /// </summary>
        /// <returns>A <see cref="IfStatementNode"/> representing the if statement.</returns>
        private StatementNode? ParseIfStatement()
        {
            Consume(TokenType.LPAREN, "Expected '(' after 'if'");
            ExpressionNode? condition = ParseExpression();
            Consume(TokenType.RPAREN, "Expected ')' after if condition");
            StatementNode? thenBranch = ParseStatement();
            StatementNode? elseBranch = null;
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
        private StatementNode? ParseWhileStatement()
        {
            Consume(TokenType.LPAREN, "Expected '(' after 'while'");
            ExpressionNode? condition = ParseExpression();
            Consume(TokenType.RPAREN, "Expected ')' after while condition");
            StatementNode? body = ParseStatement();
            return new WhileStatementNode(condition, body);
        }

        /// <summary>
        /// Parses a for statement.
        /// </summary>
        /// <returns>A <see cref="ForStatementNode"/> representing the for statement.</returns>
        private StatementNode? ParseForStatement()
        {
            Consume(TokenType.LPAREN, "Expected '(' after 'for'");

            // Regular variable declaration
            var initializer = ParseStatement();
            ExpressionNode? condition = ParseExpression();
            Consume(TokenType.SEMICOLON, "Expected ';' after condition in 'for'");
            StatementNode? increment = null;
            if (Match(TokenType.IDENTIFIER))
            {
                increment = ParseExpressionStatementOrAssignment(true);
            }

            Consume(TokenType.RPAREN, "Expected ')' after for increment");
            StatementNode? body = ParseStatement();
            return new ForStatementNode(initializer, condition, increment, body);
        }

        /// <summary>
        /// Parses a block statement enclosed in braces.
        /// </summary>
        /// <returns>A <see cref="BlockNode"/> representing the block statement.</returns>
        private StatementNode? ParseBlockStatement()
        {
            List<StatementNode?> statements = new List<StatementNode?>();
            while (!Check(TokenType.RBRACE) && !IsAtEnd())
            {
                statements.Add(ParseStatement());
            }
            Consume(TokenType.RBRACE, "Expected '}' after block");
            return new BlockNode(statements);
        }

        /// <summary>
        /// Parses an array initializer, which includes a list of expressions enclosed in braces.
        /// </summary>
        /// <returns>A <see cref="ArrayInitializerNode"/> representing the array initializer.</returns>
        private ExpressionNode? ParseArrayInitializer()
        {
            List<ExpressionNode?> elements = new List<ExpressionNode?>();
            if (Check(TokenType.LSQUAREBRACE))
            {
                Consume(TokenType.LSQUAREBRACE, "Expected '[' to start array initializer");
            }

            // Check if the array initializer is empty
            if (!Check(TokenType.RSQUAREBRACE))
            {
                do
                {
                    elements.Add(ParseExpression());
                } while (Match(TokenType.COMMA));
            }
            Consume(TokenType.RSQUAREBRACE, "Expected ']' after array initializer");

            return new ArrayInitializerNode(elements);
        }

        /// <summary>
        /// Parses an expression statement or an assignment statement.
        /// </summary>
        /// <returns>A <see cref="StatementNode"/> representing the expression or assignment.</returns>
        /// <exception cref="Exception">Thrown if the syntax is incorrect.</exception>
        private StatementNode? ParseExpressionStatementOrAssignment(bool isExpression = false)
        {
            string name = PreviousToken().Lexeme;

            if (Match(TokenType.LSQUAREBRACE)) // Check for array access
            {
                // Parse the array index expression
                ExpressionNode? index = ParseExpression();
                Consume(TokenType.RSQUAREBRACE, "Expected ']' after array index");

                // Check for assignment to array element
                if (Match(TokenType.EQUAL))
                {
                    ExpressionNode? value = ParseExpression();
                    if (!isExpression)
                        Consume(TokenType.SEMICOLON, "Expected ';' after assignment");
                    return new ArrayAssignmentNode(name, index, value);
                }

                throw new Exception("Expected '=' after array index");
            }

            if (Match(TokenType.PLUS_PLUS, TokenType.MINUS_MINUS))
            {
                string operation = PreviousToken().Lexeme;
                if (!isExpression)
                    Consume(TokenType.SEMICOLON, "Expected ';' after increment/decrement");
                return new IncrementDecrementNode(name, operation);
            }

            ExpressionNode? expression;
            if (Match(TokenType.EQUAL)) // Regular variable assignment
            {
                expression = ParseExpression();
                if (!isExpression)
                    Consume(TokenType.SEMICOLON, "Expected ';' after assignment");
                return new AssignmentNode(name, expression);
            }

            if (Match(TokenType.PLUS_EQUAL, TokenType.MINUS_EQUAL, TokenType.STAR_EQUAL, TokenType.SLASH_EQUAL))
            {
                string operation = PreviousToken().Lexeme;
                expression = ParseExpression();
                if (!isExpression)
                    Consume(TokenType.SEMICOLON, "Expected ';' after compound assignment");
                return new CompoundAssignmentNode(name, operation, expression);
            }

            expression = ParseExpression();
            if (Check(TokenType.SEMICOLON))
                Consume(TokenType.SEMICOLON, "Expected ';' after expression");
            return new ExpressionStatementNode(expression);
        }

        /// <summary>
        /// Parses an expression starting with logical OR operations.
        /// </summary>
        /// <returns>An <see cref="ExpressionNode"/> representing the parsed expression.</returns>
        private ExpressionNode? ParseExpression()
        {
            return ParseLogicalOr();
        }

        /// <summary>
        /// Parses logical OR operations in expressions.
        /// </summary>
        /// <returns>An <see cref="ExpressionNode"/> representing the logical OR expression.</returns>
        private ExpressionNode? ParseLogicalOr()
        {
            ExpressionNode? expr = ParseLogicalAnd();

            while (Match(TokenType.OR_OR))
            {
                string op = PreviousToken().Lexeme;
                ExpressionNode? right = ParseLogicalAnd();
                expr = new LogicalExpressionNode(expr, op, right);
            }

            return expr;
        }

        /// <summary>
        /// Parses logical AND operations in expressions.
        /// </summary>
        /// <returns>An <see cref="ExpressionNode"/> representing the logical AND expression.</returns>
        private ExpressionNode? ParseLogicalAnd()
        {
            ExpressionNode? expr = ParseEquality();

            while (Match(TokenType.AND_AND))
            {
                string op = PreviousToken().Lexeme;
                ExpressionNode? right = ParseEquality();
                expr = new LogicalExpressionNode(expr, op, right);
            }

            return expr;
        }

        /// <summary>
        /// Parses equality operations in expressions.
        /// </summary>
        /// <returns>An <see cref="ExpressionNode"/> representing the equality expression.</returns>
        private ExpressionNode? ParseEquality()
        {
            ExpressionNode? expr = ParseComparison();

            while (Match(TokenType.EQUAL_EQUAL, TokenType.BANG_EQUAL))
            {
                string op = PreviousToken().Lexeme;
                ExpressionNode? right = ParseComparison();
                expr = new BinaryExpressionNode(expr, op, right);
            }

            return expr;
        }

        /// <summary>
        /// Parses comparison operations in expressions.
        /// </summary>
        /// <returns>An <see cref="ExpressionNode"/> representing the comparison expression.</returns>
        private ExpressionNode? ParseComparison()
        {
            ExpressionNode? expr = ParseTerm();

            while (Match(TokenType.GT, TokenType.GTE, TokenType.LT, TokenType.LTE))
            {
                string op = PreviousToken().Lexeme;
                ExpressionNode? right = ParseTerm();
                expr = new BinaryExpressionNode(expr, op, right);
            }

            return expr;
        }

        /// <summary>
        /// Parses term operations in expressions (e.g., addition, subtraction).
        /// </summary>
        /// <returns>An <see cref="ExpressionNode"/> representing the term expression.</returns>
        private ExpressionNode? ParseTerm()
        {
            ExpressionNode? expr = ParseFactor();

            while (Match(TokenType.PLUS, TokenType.MINUS))
            {
                string op = PreviousToken().Lexeme;
                ExpressionNode? right = ParseFactor();
                expr = new BinaryExpressionNode(expr, op, right);
            }

            return expr;
        }

        /// <summary>
        /// Parses factor operations in expressions (e.g., multiplication, division).
        /// </summary>
        /// <returns>An <see cref="ExpressionNode"/> representing the factor expression.</returns>
        private ExpressionNode? ParseFactor()
        {
            ExpressionNode? expr = ParseUnary();

            while (Match(TokenType.STAR, TokenType.SLASH, TokenType.PERCENT))
            {
                string op = PreviousToken().Lexeme;
                ExpressionNode? right = ParseUnary();
                expr = new BinaryExpressionNode(expr, op, right);
            }

            return expr;
        }

        /// <summary>
        /// Parses unary operations in expressions (e.g., negation).
        /// </summary>
        /// <returns>An <see cref="ExpressionNode"/> representing the unary expression.</returns>
        private ExpressionNode? ParseUnary()
        {
            if (Match(TokenType.BANG, TokenType.MINUS))
            {
                string op = PreviousToken().Lexeme;
                ExpressionNode? right = ParseUnary();
                return new UnaryExpressionNode(op, right);
            }
            return ParsePrimary();
        }

        /// <summary>
        /// Parses primary expressions, including literals, identifiers, and function calls.
        /// </summary>
        /// <returns>An <see cref="ExpressionNode"/> representing the primary expression.</returns>
        private ExpressionNode? ParsePrimary()
        {
            if (Match(TokenType.NUMBER_LITERAL, TokenType.STRING_LITERAL, TokenType.TRUE_LITERAL, TokenType.FALSE_LITERAL))
            {
                return new LiteralNode(PreviousToken().Lexeme, PreviousToken().Type);
            }

            if (Match(TokenType.IDENTIFIER) && !Check(TokenType.DOT))
            {
                return ParseIdentifier();
            }

            if (Check(TokenType.DOT))
            {
                string objectName = PreviousToken().Lexeme;
                Consume(TokenType.DOT, "Expected dot separator");
                if (NextToken().Type == TokenType.LPAREN)
                {
                    var expressionNode = ParseExpression();
                    return new MemberAccessNode(objectName, expressionNode);
                }

                Consume(TokenType.IDENTIFIER, "Expected member name");
                if (NextToken().Type == TokenType.SEMICOLON || NextToken().Type == TokenType.RPAREN)
                {
                    var state = ParseIdentifier();
                    return new MemberAccessNode(objectName, state);
                }
                else
                {
                    var expression = ParseIdentifier();
                    if (Match(TokenType.PLUS, TokenType.MINUS, TokenType.STAR, TokenType.SLASH, TokenType.PERCENT))
                    {
                        do
                        {
                            string op = PreviousToken().Lexeme;
                            ExpressionNode? right = ParseFactor();
                            expression = new BinaryExpressionNode(expression, op, right);
                        } while (Match(TokenType.PLUS, TokenType.MINUS, TokenType.STAR, TokenType.SLASH, TokenType.PERCENT));

                        return new MemberAccessNode(objectName, new ExpressionStatementNode(expression));
                    }

                    var state = ParseExpressionStatementOrAssignment(true);
                    return new MemberAccessNode(objectName, state);
                }
            }

            if (Match(TokenType.LSQUAREBRACE))
            {
                return ParseArrayInitializer();
            }

            if (Check(TokenType.LPAREN))
            {
                string name = PreviousToken().Lexeme;
                Consume(TokenType.LPAREN, "Expected '(' before arguments");
                List<ExpressionNode?> arguments = new List<ExpressionNode?>();
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
        private ExpressionNode? ParseIdentifier()
        {
            string name = PreviousToken().Lexeme;

            if (Match(TokenType.LSQUAREBRACE))
            {
                // Array access case
                ExpressionNode? index = ParseExpression();
                Consume(TokenType.RSQUAREBRACE, "Expected ']' after array index.");
                return new ArrayAccessNode(name, index);
            }

            if (Match(TokenType.LPAREN))
            {
                List<ExpressionNode?> arguments = new List<ExpressionNode?>();
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
        /// <param name="types">The token type to check against.</param>
        /// <returns>True if the current token matches the specified type; otherwise, false.</returns>
        private bool Check(params TokenType[] types)
        {
            foreach (var type in types)
            {
                if (IsAtEnd()) return false;
                if (CurrentToken().Type == type)
                {
                    return true;
                }
            }
            return false;
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
        private Token PreviousToken(int i = 1)
        {
            return _tokens[_current - i];
        }

        /// <summary>
        /// Retrieves the next token after the current one.
        /// </summary>
        /// <returns>The next token.</returns>
        private Token NextToken(int i = 1)
        {
            return _tokens[_current + i];
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
