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
            ArgumentNullException.ThrowIfNull(tokens);
            if (tokens.Count == 0 || tokens[^1].Type != TokenType.EOF)
                throw new ArgumentException("The token stream must end with an EOF token.", nameof(tokens));

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
                AccessModifier? declaredAccess = ParseAccessModifier();
                if (Check(TokenType.MAIN))
                {
                    if (declaredAccess is not null)
                        throw Error("BEN2009", "The _main_ entry point cannot have an access modifier.");
                    if (mainFunction != null)
                        throw Error("BEN2002", "The main function is already defined.");

                    mainFunction = ParseMainFunction();
                }
                else if (Match(TokenType.PACKAGE))
                {
                    if (declaredAccess is not null)
                        throw Error("BEN2001", "Access modifiers can only be applied to variable and function declarations.");
                    packages.Add(ParsePackage());
                }
                else if (Match(TokenType.FUNC))
                {
                    _functions.Add(ParseFunction(declaredAccess ?? AccessModifier.Private));
                }
                else if (Check(TokenType.LET) &&
                    NextToken().Type == TokenType.IDENTIFIER &&
                    NextToken(2).Type == TokenType.EQUAL &&
                    NextToken(3).Type == TokenType.NEW)
                {
                    statements.Add(ParseStatement());
                }
                else if (Check(TokenType.BOOL, TokenType.NUMBER, TokenType.STRING, TokenType.LET))
                {
                    globalVariables.Add(ParseVariableDeclaration(declaredAccess ?? AccessModifier.Private));
                }
                else
                {
                    if (declaredAccess is not null)
                        throw Error("BEN2001", "Access modifiers can only be applied to variable and function declarations.");
                    statements.Add(ParseStatement());
                }
            }

            if (mainFunction == null && statements.Count == 0)
            {
                throw Error("BEN2003", "No main function or top-level executable statement was found.");
            }

            return new ProgramNode(globalVariables, packages, _functions, mainFunction, statements);
        }

        private AccessModifier? ParseAccessModifier()
        {
            if (Match(TokenType.PUBLIC)) return AccessModifier.Public;
            if (Match(TokenType.PRIVATE)) return AccessModifier.Private;
            return null;
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
                AccessModifier? declaredAccess = ParseAccessModifier();
                AccessModifier accessModifier = declaredAccess ?? AccessModifier.Private;
                if (Match(TokenType.FUNC))
                {
                    var function = ParseFunction(accessModifier);
                    members.Add(new PackageFunctionNode(function.Name, function.Parameters, function.ReturnType,
                        function.Body, function.ReturnStatement, function.AccessModifier));
                }
                else if (Match(TokenType.INIT))
                {
                    if (declaredAccess is not null)
                        throw Error("BEN2001", "The init constructor cannot have an access modifier.");
                    if (members.OfType<PackageFunctionNode>().Any(member => member.Name == "init"))
                        throw Error("BEN2008", "A package can declare only one init constructor.");
                    members.Add(ParsePackageInitializer());
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
                                throw Error("BEN2004", "Implicitly-typed variables must have an initializer.");
                            }
                        }
                        #endregion
                        Consume(TokenType.SEMICOLON, "Expected ';' after variable declaration");
                        members.Add(new PackageVariableDeclarationNode(type, name, initializer, accessModifier));
                    }
                    else
                    {
                        throw Error("BEN2001", "Expected a variable or function declaration.");
                    }
                }
                else if (Check(TokenType.IDENTIFIER) && NextToken().Type == TokenType.IDENTIFIER)
                {
                    string type = Advance().Lexeme;
                    string name = Advance().Lexeme;
                    ExpressionNode? initializer = Match(TokenType.EQUAL) ? ParseExpression() : null;
                    Consume(TokenType.SEMICOLON, "Expected ';' after package field declaration");
                    members.Add(new PackageVariableDeclarationNode(type, name, initializer, accessModifier));
                }
                else
                {
                    throw Error("BEN2001", "Unexpected token inside package.");
                }
            }
            Consume(TokenType.RBRACE, "Expected '}' after package body.");

            return new PackageNode(packageName, members);
        }

        private PackageFunctionNode ParsePackageInitializer()
        {
            Consume(TokenType.LPAREN, "Expected '(' after 'init'");
            List<ParameterNode> parameters = ParseParameters();
            Consume(TokenType.RPAREN, "Expected ')' after init parameters");
            Consume(TokenType.LBRACE, "Expected '{' before init body");
            List<StatementNode?> statements = new();
            while (!IsAtEnd() && !Check(TokenType.RBRACE))
                statements.Add(ParseStatement());
            Consume(TokenType.RBRACE, "Expected '}' after init body");
            return new PackageFunctionNode("init", parameters, "void", new BlockNode(statements), null);
        }

        /// <summary>
        /// Parses a  variable declaration, which could be a regular variable or an array.
        /// </summary>
        /// <returns>A <see cref="VariableDeclarationNode"/> representing the global variable declaration.</returns>
        /// <exception cref="Exception">Thrown if the declaration is malformed.</exception>
        private VariableDeclarationNode ParseVariableDeclaration(AccessModifier accessModifier = AccessModifier.Private)
        {
            string type = ParseType(allowLet: true); ///< The type of the variable (e.g., "number[]").
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
                        throw Error("BEN2004", "Implicitly-typed variables must have an initializer.");
                    }
                }

                Consume(TokenType.SEMICOLON, "Expected ';' after variable declaration");

                return new VariableDeclarationNode(type, name, initializer, accessModifier);
            }

            throw Error("BEN2001", "Expected an array or variable declaration.");
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
            if (!Check(TokenType.RPAREN))
                throw Error("BEN2006", "The _main_ entry point cannot declare parameters.");
            List<ParameterNode> parameters = [];
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
        private FunctionNode? ParseFunction(AccessModifier accessModifier = AccessModifier.Private)
        {
            string name = Consume(TokenType.IDENTIFIER, "Expected function name").Lexeme;
            Consume(TokenType.LPAREN, "Expected '(' after function name");
            List<ParameterNode> parameters = ParseParameters();
            Consume(TokenType.RPAREN, "Expected ')' after parameters");
            Consume(TokenType.ARROW, "Expected '->' after parameters");
            string? returnType = ParseType(allowVoid: true, allowCustom: true);
            Consume(TokenType.LBRACE, "Expected '{' before function body");

            List<StatementNode?> statements = new List<StatementNode?>();
            ReturnStatementNode? returnExpression = null;

            while (!IsAtEnd() && !Check(TokenType.RBRACE))
            {
                StatementNode? statement = ParseStatement();
                statements.Add(statement);
                if (statement is ReturnStatementNode returnStatement)
                    returnExpression = returnStatement;
            }

            Consume(TokenType.RBRACE, "Expected '}' after function body");
            return new FunctionNode(name, parameters, returnType, new BlockNode(statements), returnExpression, accessModifier);
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
                    string type = ParseType(allowCustom: true);
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
        private string ParseType(bool allowVoid = false, bool allowLet = false, bool allowCustom = false)
        {
            string type = string.Empty;
            if (Match(TokenType.NUMBER, TokenType.STRING, TokenType.BOOL))
                type = ParseTokenType(PreviousToken().Type);
            else if (allowVoid && Match(TokenType.VOID))
                type = "void";
            else if (allowLet && Match(TokenType.LET))
                type = "let";
            else if (allowCustom && Match(TokenType.IDENTIFIER))
                type = PreviousToken().Lexeme;

            if (Check(TokenType.LSQUAREBRACE))
            {
                if (type is "void" or "let" || string.IsNullOrEmpty(type))
                    throw Error("BEN2001", "Only value types can be used as array elements.");
                Advance(); // Consume '['
                if (!Check(TokenType.RSQUAREBRACE))
                    throw Error("BEN2001", "Expected ']' after '[' in array declaration.");
                Advance(); // Consume ']'
                type += "[]";
            }
            if (string.IsNullOrEmpty(type))
                throw Error("BEN2001", "Expected a type.");

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
            throw Error("BEN2001", "Expected a type.");
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
            if (Match(TokenType.MATCH))
            {
                return ParseMatchStatement();
            }
            if (Match(TokenType.LBRACE))
            {
                return ParseBlockStatement();
            }

            if (Check(TokenType.BOOL, TokenType.NUMBER, TokenType.STRING, TokenType.LET))
            {
                return ParseVariableDeclaration();
            }
            if (Check(TokenType.IDENTIFIER) && NextToken().Type == TokenType.IDENTIFIER)
            {
                string type = Advance().Lexeme;
                string name = Advance().Lexeme;
                ExpressionNode? initializer = Match(TokenType.EQUAL) ? ParseExpression() : null;
                Consume(TokenType.SEMICOLON, "Expected ';' after variable declaration");
                return new VariableDeclarationNode(type, name, initializer);
            }
            if (Match(TokenType.IDENTIFIER))
            {
                return ParseExpressionStatementOrAssignment();
            }
            if (Check(TokenType.NUMBER_LITERAL, TokenType.STRING_LITERAL, TokenType.TRUE_LITERAL,
                    TokenType.FALSE_LITERAL, TokenType.LPAREN, TokenType.LSQUAREBRACE,
                    TokenType.BANG, TokenType.MINUS, TokenType.NEW, TokenType.THIS))
            {
                ExpressionNode? expression = ParseExpression();
                Consume(TokenType.SEMICOLON, "Expected ';' after expression");
                return new ExpressionStatementNode(expression);
            }
            if (Match(TokenType.RETURN))
            {
                return ParseReturnStatement();
            }

            throw Error("BEN2001", $"Unexpected token '{CurrentToken().Lexeme}' ({CurrentToken().Type}).");
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
                    throw Error("BEN2005", $"Cannot implicitly convert type '{packageName}' to '{initialPackageName}'.");

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

        /// <summary>یک match دستوری با بدنه‌های بلوکی را تجزیه می‌کند.</summary>
        private MatchStatementNode ParseMatchStatement()
        {
            ExpressionNode value = ParseExpression();
            Consume(TokenType.LBRACE, "Expected '{' after match value");
            List<MatchArm> arms = new();
            bool hasDefault = false;

            while (!Check(TokenType.RBRACE) && !IsAtEnd())
            {
                (List<MatchPatternNode> patterns, bool isDefault) = ParseMatchPatterns();
                if (isDefault && hasDefault)
                    throw Error("BEN2007", "A match can contain only one default '_' arm.");
                if (hasDefault)
                    throw Error("BEN2007", "The default '_' arm must be the last match arm.");
                hasDefault |= isDefault;

                Consume(TokenType.FAT_ARROW, "Expected '=>' after match pattern");
                StatementNode body = ParseStatement();
                arms.Add(new MatchArm(patterns, body, isDefault));
                Match(TokenType.COMMA, TokenType.SEMICOLON);
            }

            Consume(TokenType.RBRACE, "Expected '}' after match arms");
            if (arms.Count == 0)
                throw Error("BEN2007", "A match must contain at least one arm.");
            return new MatchStatementNode(value, arms);
        }

        /// <summary>الگوهای مقداری یا بازه‌ای یک شاخه match را می‌خواند.</summary>
        private (List<MatchPatternNode> Patterns, bool IsDefault) ParseMatchPatterns()
        {
            if (Check(TokenType.IDENTIFIER) && CurrentToken().Lexeme == "_" &&
                NextToken().Type == TokenType.FAT_ARROW)
            {
                Advance();
                return (new List<MatchPatternNode>(), true);
            }

            List<MatchPatternNode> patterns = new();
            do
            {
                ExpressionNode start = ParseExpression();
                if (Match(TokenType.RANGE))
                {
                    ExpressionNode end = ParseExpression();
                    patterns.Add(new RangeMatchPatternNode(start, end));
                }
                else
                {
                    patterns.Add(new ValueMatchPatternNode(start));
                }
            } while (Match(TokenType.COMMA));

            return (patterns, false);
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
            if (Check(TokenType.IDENTIFIER) && NextToken().Type == TokenType.IN)
            {
                string variableName = Advance().Lexeme;
                Consume(TokenType.IN, "Expected 'in' after iteration variable");
                ExpressionNode iterable = ParseExpression();
                Consume(TokenType.RPAREN, "Expected ')' after for-in iterable");
                StatementNode forEachBody = ParseStatement();
                return new ForEachStatementNode(variableName, iterable, forEachBody);
            }

            StatementNode? initializer = null;
            if (Match(TokenType.SEMICOLON))
            {
                // Empty initializer.
            }
            else if (Check(TokenType.NUMBER, TokenType.STRING, TokenType.BOOL, TokenType.LET))
            {
                initializer = ParseVariableDeclaration();
            }
            else if (Match(TokenType.IDENTIFIER))
            {
                initializer = ParseExpressionStatementOrAssignment();
            }
            else
            {
                throw Error("BEN2001", "Expected a variable declaration, assignment, or ';' in for initializer.");
            }

            ExpressionNode? condition = Check(TokenType.SEMICOLON) ? null : ParseExpression();
            Consume(TokenType.SEMICOLON, "Expected ';' after condition in 'for'");
            StatementNode? increment = null;
            if (!Check(TokenType.RPAREN))
            {
                if (!Match(TokenType.IDENTIFIER))
                    throw Error("BEN2001", "Expected assignment or increment expression in for increment.");
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
            List<ExpressionNode> elements = new List<ExpressionNode>();
            ExpressionNode sizeExpression = new LiteralNode(0.ToString(), TokenType.NUMBER_LITERAL);
            if (Check(TokenType.LSQUAREBRACE))
            {
                Consume(TokenType.LSQUAREBRACE, "Expected '[' to start array initializer");
            }

            // Check if the array initializer is empty
            if (!Check(TokenType.RSQUAREBRACE))
            {
                if (Check(TokenType.NUMBER, TokenType.STRING, TokenType.BOOL))
                {
                    var elementType = CurrentToken().Type;
                    Advance();
                    Consume(TokenType.LSQUAREBRACE, "Expected '[' to start array initializer");
                    sizeExpression = ParseExpression();

                    if (sizeExpression is LiteralNode sizeToken)
                    {
                        for (int i = 0; i < int.Parse(sizeToken.Value); i++)
                        {
                            elements.Add(new LiteralNode("0", elementType));
                        }
                    }
                }
                else
                {
                    do
                    {
                        elements.Add(ParseExpression());
                    } while (Match(TokenType.COMMA));

                    sizeExpression = new LiteralNode(elements.Count.ToString(), TokenType.NUMBER_LITERAL);
                }
            }
            Consume(TokenType.RSQUAREBRACE, "Expected ']' after array initializer");

            return new ArrayInitializerNode(elements, sizeExpression);
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

                throw Error("BEN2001", "Expected '=' after array index.");
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

            // The leading identifier was consumed by ParseStatement. Rewind it so the
            // regular precedence parser can handle calls, member access and binary expressions.
            _current--;
            expression = ParseExpression();
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
            if (Match(TokenType.NEW))
                return ParseNewExpression();

            if (Match(TokenType.THIS))
            {
                if (Check(TokenType.DOT))
                    return ParseMemberAccess("this");
                return new IdentifierNode("this");
            }

            if (Match(TokenType.MATCH))
                return ParseMatchExpression();

            if (Match(TokenType.NUMBER_LITERAL, TokenType.STRING_LITERAL, TokenType.TRUE_LITERAL, TokenType.FALSE_LITERAL))
            {
                return new LiteralNode(PreviousToken().Lexeme, PreviousToken().Type);
            }

            if (Match(TokenType.IDENTIFIER))
            {
                if (Check(TokenType.DOT))
                    return ParseMemberAccess(PreviousToken().Lexeme);
                return ParseIdentifier();
            }

            if (Match(TokenType.LSQUAREBRACE))
            {
                return ParseArrayInitializer();
            }

            if (Match(TokenType.LPAREN))
            {
                ExpressionNode? expression = ParseExpression();
                Consume(TokenType.RPAREN, "Expected ')' after expression");
                return expression;
            }

            throw Error("BEN2001", $"Unexpected token '{CurrentToken().Lexeme}' ({CurrentToken().Type}).");
        }

        private NewExpressionNode ParseNewExpression()
        {
            string packageName = Consume(TokenType.IDENTIFIER, "Expected package name after 'new'").Lexeme;
            Consume(TokenType.LPAREN, "Expected '(' after package name");
            List<ExpressionNode?> arguments = new();
            if (!Check(TokenType.RPAREN))
            {
                do
                {
                    arguments.Add(ParseExpression());
                } while (Match(TokenType.COMMA));
            }
            Consume(TokenType.RPAREN, "Expected ')' after constructor arguments");
            return new NewExpressionNode(packageName, arguments);
        }

        /// <summary>یک match مقدارساز را تجزیه می‌کند.</summary>
        private MatchExpressionNode ParseMatchExpression()
        {
            ExpressionNode value = ParseExpression();
            Consume(TokenType.LBRACE, "Expected '{' after match value");
            List<MatchArm> arms = new();
            bool hasDefault = false;

            while (!Check(TokenType.RBRACE) && !IsAtEnd())
            {
                (List<MatchPatternNode> patterns, bool isDefault) = ParseMatchPatterns();
                if (isDefault && hasDefault)
                    throw Error("BEN2007", "A match can contain only one default '_' arm.");
                if (hasDefault)
                    throw Error("BEN2007", "The default '_' arm must be the last match arm.");
                hasDefault |= isDefault;

                Consume(TokenType.FAT_ARROW, "Expected '=>' after match pattern");
                ExpressionNode result = ParseExpression();
                arms.Add(new MatchArm(patterns, result, isDefault));

                if (!Match(TokenType.COMMA, TokenType.SEMICOLON) && !Check(TokenType.RBRACE))
                    throw Error("BEN2007", "Expected ',' or ';' between match expression arms.");
            }

            Consume(TokenType.RBRACE, "Expected '}' after match arms");
            if (arms.Count == 0)
                throw Error("BEN2007", "A match must contain at least one arm.");
            if (!hasDefault)
                throw Error("BEN2007", "A match expression requires a default '_' arm.");
            return new MatchExpressionNode(value, arms);
        }

        /// <summary>
        /// یک دسترسی مستقیم به عضو package را پس از خواندن نام شیء تجزیه می‌کند.
        /// </summary>
        private ExpressionNode ParseMemberAccess(string objectName)
        {
            Consume(TokenType.DOT, "Expected '.' before member name");
            string memberName = Consume(TokenType.IDENTIFIER, "Expected member name after '.'").Lexeme;

            if (Match(TokenType.LPAREN))
            {
                List<ExpressionNode?> arguments = new();
                if (!Check(TokenType.RPAREN))
                {
                    do
                    {
                        arguments.Add(ParseExpression());
                    } while (Match(TokenType.COMMA));
                }
                Consume(TokenType.RPAREN, "Expected ')' after member arguments");
                return new MemberAccessNode(objectName, new FunctionCallNode(memberName, arguments));
            }

            if (Match(TokenType.EQUAL))
                return new MemberAccessNode(objectName, new AssignmentNode(memberName, ParseExpression()));

            if (Match(TokenType.PLUS_EQUAL, TokenType.MINUS_EQUAL, TokenType.STAR_EQUAL, TokenType.SLASH_EQUAL))
            {
                string operation = PreviousToken().Lexeme;
                return new MemberAccessNode(objectName,
                    new CompoundAssignmentNode(memberName, operation, ParseExpression()));
            }

            if (Match(TokenType.PLUS_PLUS, TokenType.MINUS_MINUS))
                return new MemberAccessNode(objectName,
                    new IncrementDecrementNode(memberName, PreviousToken().Lexeme));

            return new MemberAccessNode(objectName, new IdentifierNode(memberName));
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
            var index = Math.Min(_current + i, _tokens.Count - 1);
            return _tokens[index];
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
            throw Error("BEN2001", message);
        }

        private ParserException Error(string code, string message) =>
            new(code, message, CurrentToken().Span);
    }
}
