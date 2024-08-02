using Benita;

namespace BenitaTestProject
{
    [TestClass]
    public class ParserTests
    {
        [TestMethod]
        public void Parse_MainFunction_Valid()
        {
            // Arrange
            List<Token> tokens = new List<Token>
            {
                new Token(TokenType.MAIN, "_main_", 0),
                new Token(TokenType.LPAREN, "(", 0),
                new Token(TokenType.RPAREN, ")", 0),
                new Token(TokenType.LBRACE, "{", 1),
                new Token(TokenType.RBRACE, "}", 2),
                new Token(TokenType.EOF, "", 3)
            };

            Parser parser = new Parser(tokens);

            // Act
            ProgramNode program = parser.Parse();

            // Assert
            Assert.IsNotNull(program);
            Assert.IsNotNull(program.MainFunction);
            Assert.AreEqual(0, program.GlobalVariables.Count);
            Assert.AreEqual(0, program.Functions.Count);
        }

        [TestMethod]
        public void Parse_GlobalVariableDeclaration_Valid()
        {
            // Arrange
            List<Token> tokens = new List<Token>
            {

                new Token(TokenType.NUMBER, "number", 0),
                new Token(TokenType.IDENTIFIER, "x", 0),
                new Token(TokenType.SEMICOLON, ";", 0),
                new Token(TokenType.MAIN, "_main_", 1),
                new Token(TokenType.LPAREN, "(", 1),
                new Token(TokenType.RPAREN, ")", 1),
                new Token(TokenType.LBRACE, "{", 2),
                new Token(TokenType.RBRACE, "}", 3),
                new Token(TokenType.EOF, "", 4)
            };

            Parser parser = new Parser(tokens);

            // Act
            ProgramNode program = parser.Parse();

            // Assert
            Assert.IsNotNull(program);
            Assert.IsNotNull(program.MainFunction);
            Assert.AreEqual(1, program.GlobalVariables.Count);
            Assert.AreEqual(0, program.Functions.Count);

            var globalVariable = program.GlobalVariables[0];
            Assert.AreEqual("number", globalVariable.Type);
            Assert.AreEqual("x", globalVariable.Name);
            Assert.IsNull(globalVariable.Initializer);
        }

        [TestMethod]
        public void Parse_Function_Valid()
        {
            // Arrange
            List<Token> tokens = new List<Token>
            {
                new Token(TokenType.FUNC, "func", 0),
                new Token(TokenType.IDENTIFIER, "add", 0),
                new Token(TokenType.LPAREN, "(", 0),
                new Token(TokenType.NUMBER, "number", 0),
                new Token(TokenType.IDENTIFIER, "a", 0),
                new Token(TokenType.COMMA, ",", 0),
                new Token(TokenType.NUMBER, "number", 0),
                new Token(TokenType.IDENTIFIER, "b", 0),
                new Token(TokenType.RPAREN, ")", 0),
                new Token(TokenType.ARROW, "->", 0),
                new Token(TokenType.NUMBER, "number",0),
                new Token(TokenType.LBRACE, "{", 0),
                new Token(TokenType.RETURN, "return",1),
                new Token(TokenType.IDENTIFIER, "a", 1),
                new Token(TokenType.PLUS, "+", 1),
                new Token(TokenType.IDENTIFIER, "b", 1),
                new Token(TokenType.SEMICOLON, ";", 1),
                new Token(TokenType.RBRACE, "}", 2),
                new Token(TokenType.MAIN, "_main_", 3),
                new Token(TokenType.LPAREN, "(", 3),
                new Token(TokenType.RPAREN, ")", 3),
                new Token(TokenType.LBRACE, "{", 4),
                new Token(TokenType.RBRACE, "}", 5),
                new Token(TokenType.EOF, "", 6)
            };

            Parser parser = new Parser(tokens);

            // Act
            ProgramNode program = parser.Parse();

            // Assert
            Assert.IsNotNull(program);
            Assert.AreEqual(0, program.GlobalVariables.Count);
            Assert.AreEqual(1, program.Functions.Count);

            var function = program.Functions[0];
            Assert.AreEqual("add", function.Name);
            Assert.AreEqual("number", function.ReturnType);

            Assert.AreEqual(2, function.Parameters.Count);
            Assert.AreEqual("number", function.Parameters[0].Type);
            Assert.AreEqual("a", function.Parameters[0].Name);
            Assert.AreEqual("number", function.Parameters[1].Type);
            Assert.AreEqual("b", function.Parameters[1].Name);

            Assert.IsNotNull(function.Body);
            Assert.AreEqual(0, function.Body.Statements.Count);

            Assert.IsTrue(function.ReturnStatement.ReturnExpression is BinaryExpressionNode);
            var returnStatement = function.ReturnStatement.ReturnExpression as BinaryExpressionNode;
            Assert.IsNotNull(returnStatement);
            Assert.IsNotNull(returnStatement.Left);
            Assert.IsNotNull(returnStatement.Operator);
            Assert.IsNotNull(returnStatement.Right);
        }

        [TestMethod]
        public void Parse_ArrayDeclaration_NoSize_Valid()
        {
            // Arrange
            List<Token> tokens = new List<Token>
            {
                new Token(TokenType.NUMBER, "number", 0),
                new Token(TokenType.LBRACE, "[", 0),
                new Token(TokenType.RBRACE, "]", 0),
                new Token(TokenType.IDENTIFIER, "arr", 0),
                new Token(TokenType.SEMICOLON, ";", 0),
                new Token(TokenType.MAIN, "_main_", 1),
                new Token(TokenType.LPAREN, "(", 1),
                new Token(TokenType.RPAREN, ")", 1),
                new Token(TokenType.LBRACE, "{", 2),
                new Token(TokenType.RBRACE, "}", 3),
                new Token(TokenType.EOF, "", 4)
            };

            Parser parser = new Parser(tokens);

            // Act
            ProgramNode program = parser.Parse();

            // Assert
            Assert.IsNotNull(program);
            Assert.AreEqual(1, program.GlobalVariables.Count);
            Assert.AreEqual(0, program.Functions.Count);

            var globalVariable = program.GlobalVariables[0];
            Assert.AreEqual("number[]", globalVariable.Type);
            Assert.AreEqual("arr", globalVariable.Name);
            Assert.IsNull(globalVariable.Initializer);
        }

        [TestMethod]
        public void Parse_IfStatement_Valid()
        {
            // Arrange
            List<Token> tokens = new List<Token>
            {
                new Token(TokenType.MAIN, "_main_", 0),
                new Token(TokenType.LPAREN, "(", 0),
                new Token(TokenType.RPAREN, ")", 0),
                new Token(TokenType.LBRACE, "{", 1),
                new Token(TokenType.IF, "if", 2),
                new Token(TokenType.LPAREN, "(", 2),
                new Token(TokenType.TRUE_LITERAL, "true", 2),
                new Token(TokenType.RPAREN, ")", 2),
                new Token(TokenType.LBRACE, "{", 3),
                new Token(TokenType.IDENTIFIER, "print", 4),
                new Token(TokenType.LPAREN, "(", 4),
                new Token(TokenType.STRING_LITERAL, "\"True branch\"", 4),
                new Token(TokenType.RPAREN, ")", 4),
                new Token(TokenType.SEMICOLON, ";", 4),
                new Token(TokenType.RBRACE, "}", 5),
                new Token(TokenType.ELSE, "else", 6),
                new Token(TokenType.LBRACE, "{", 6),
                new Token(TokenType.IDENTIFIER, "print", 7),
                new Token(TokenType.LPAREN, "(", 7),
                new Token(TokenType.STRING_LITERAL, "\"False branch\"", 7),
                new Token(TokenType.RPAREN, ")", 7),
                new Token(TokenType.SEMICOLON, ";", 7),
                new Token(TokenType.RBRACE, "}", 8),
                new Token(TokenType.RBRACE, "}", 9),
                new Token(TokenType.EOF, "", 10)
            };

            Parser parser = new Parser(tokens);

            // Act
            ProgramNode program = parser.Parse();

            // Assert
            Assert.IsNotNull(program);
            Assert.AreEqual(0, program.GlobalVariables.Count);
            Assert.AreEqual(0, program.Functions.Count);

            var function = program.MainFunction;
            Assert.IsNotNull(function);
            Assert.AreEqual("_main_", function.Name);
            Assert.AreEqual("void", function.ReturnType);
            Assert.AreEqual(0, function.Parameters.Count);

            Assert.IsNotNull(function.Body);
            Assert.AreEqual(1, function.Body.Statements.Count);

            var ifStatement = function.Body.Statements[0] as IfStatementNode;
            Assert.IsNotNull(ifStatement);
            Assert.IsNotNull(ifStatement.Condition);
            Assert.IsNotNull(ifStatement.ThenBranch);
            Assert.IsNotNull(ifStatement.ElseBranch);

            Assert.IsTrue(ifStatement.Condition is LiteralNode);
            Assert.AreEqual(TokenType.TRUE_LITERAL, (ifStatement.Condition as LiteralNode).Type);

            var thenBranch = ifStatement.ThenBranch as BlockNode;
            Assert.IsNotNull(thenBranch);
            Assert.AreEqual(1, thenBranch.Statements.Count);
            var thenExprStmt = thenBranch.Statements[0] as ExpressionStatementNode;
            Assert.IsNotNull(thenExprStmt);
            var thenFunctionCall = thenExprStmt.Expression as FunctionCallNode;
            Assert.IsNotNull(thenFunctionCall);
            Assert.AreEqual("print", thenFunctionCall.FunctionName);
            Assert.AreEqual(1, thenFunctionCall.Arguments.Count);
            var thenArgument = thenFunctionCall.Arguments[0] as LiteralNode;
            Assert.IsNotNull(thenArgument);
            Assert.AreEqual("\"True branch\"", thenArgument.Value);

            var elseBranch = ifStatement.ElseBranch as BlockNode;
            Assert.IsNotNull(elseBranch);
            Assert.AreEqual(1, elseBranch.Statements.Count);
            var elseExprStmt = elseBranch.Statements[0] as ExpressionStatementNode;
            Assert.IsNotNull(elseExprStmt);
            var elseFunctionCall = elseExprStmt.Expression as FunctionCallNode;
            Assert.IsNotNull(elseFunctionCall);
            Assert.AreEqual("print", elseFunctionCall.FunctionName);
            Assert.AreEqual(1, elseFunctionCall.Arguments.Count);
            var elseArgument = elseFunctionCall.Arguments[0] as LiteralNode;
            Assert.IsNotNull(elseArgument);
            Assert.AreEqual("\"False branch\"", elseArgument.Value);
        }

        [TestMethod]
        public void Parse_WhileLoop_Valid()
        {
            // Arrange
            List<Token> tokens = new List<Token>
            {
                new Token(TokenType.MAIN, "_main_", 0),
                new Token(TokenType.LPAREN, "(", 0),
                new Token(TokenType.RPAREN, ")", 0),
                new Token(TokenType.LBRACE, "{", 1),
                new Token(TokenType.WHILE, "while", 2),
                new Token(TokenType.LPAREN, "(", 2),
                new Token(TokenType.TRUE_LITERAL, "true", 2),
                new Token(TokenType.RPAREN, ")", 2),
                new Token(TokenType.LBRACE, "{", 3),
                new Token(TokenType.IDENTIFIER, "print", 4),
                new Token(TokenType.LPAREN, "(", 4),
                new Token(TokenType.STRING_LITERAL, "\"Loop body\"", 4),
                new Token(TokenType.RPAREN, ")", 4),
                new Token(TokenType.SEMICOLON, ";", 4),
                new Token(TokenType.RBRACE, "}", 5),
                new Token(TokenType.RBRACE, "}", 6),
                new Token(TokenType.EOF, "", 7)
            };

            Parser parser = new Parser(tokens);

            // Act
            ProgramNode program = parser.Parse();

            // Assert
            Assert.IsNotNull(program);
            Assert.AreEqual(0, program.GlobalVariables.Count);
            Assert.AreEqual(0, program.Functions.Count);

            var function = program.MainFunction;
            Assert.IsNotNull(function);
            Assert.AreEqual("_main_", function.Name);
            Assert.AreEqual("void", function.ReturnType);

            Assert.AreEqual(0, function.Parameters.Count);

            Assert.IsNotNull(function.Body);
            Assert.AreEqual(1, function.Body.Statements.Count);

            var whileLoop = function.Body.Statements[0] as WhileStatementNode;
            Assert.IsNotNull(whileLoop);
            Assert.IsNotNull(whileLoop.Condition);
            Assert.IsNotNull(whileLoop.Body);

            Assert.IsTrue(whileLoop.Condition is LiteralNode);
            Assert.AreEqual(TokenType.TRUE_LITERAL, (whileLoop.Condition as LiteralNode).Type);

            var loopBody = whileLoop.Body as BlockNode;
            Assert.IsNotNull(loopBody);
            Assert.AreEqual(1, loopBody.Statements.Count);

            var loopBodyStatement = loopBody.Statements[0] as ExpressionStatementNode;
            Assert.IsNotNull(loopBodyStatement);

            var loopFunctionCall = loopBodyStatement.Expression as FunctionCallNode;
            Assert.IsNotNull(loopFunctionCall);
            Assert.AreEqual("print", loopFunctionCall.FunctionName);

            Assert.AreEqual(1, loopFunctionCall.Arguments.Count);
            var loopArgument = loopFunctionCall.Arguments[0] as LiteralNode;
            Assert.IsNotNull(loopArgument);
            Assert.AreEqual("\"Loop body\"", loopArgument.Value);
        }

        [TestMethod]
        public void Parse_MainFunctionMissing_ThrowsException()
        {
            // Arrange
            List<Token> tokens = new List<Token>
            {
                new Token(TokenType.FUNC, "func", 0),
                new Token(TokenType.IDENTIFIER, "add", 0),
                new Token(TokenType.LPAREN, "(", 0),
                new Token(TokenType.RPAREN, ")", 0),
                new Token(TokenType.ARROW, "->", 0),
                new Token(TokenType.VOID, "void",0),
                new Token(TokenType.LBRACE, "{", 1),
                new Token(TokenType.IDENTIFIER, "print", 2),
                new Token(TokenType.LPAREN, "(", 2),
                new Token(TokenType.STRING_LITERAL, "\"No main function\"", 2),
                new Token(TokenType.RPAREN, ")", 2),
                new Token(TokenType.SEMICOLON, ";", 2),
                new Token(TokenType.RBRACE, "}", 3),
                new Token(TokenType.EOF, "", 4)
            };

            Parser parser = new Parser(tokens);
            // Act & Assert
            var exception = Assert.ThrowsException<Exception>(() => parser.Parse());
            Assert.AreEqual("No main function defined.", exception.Message);
        }

        [TestMethod]
        public void Parse_MainFunctionDuplicated_ThrowsException()
        {
            // Arrange
            List<Token> tokens = new List<Token>
            {
                new Token(TokenType.MAIN, "_main_", 0),
                new Token(TokenType.LPAREN, "(", 0),
                new Token(TokenType.RPAREN, ")", 0),
                new Token(TokenType.LBRACE, "{", 1),
                new Token(TokenType.IDENTIFIER, "print", 2),
                new Token(TokenType.LPAREN, "(", 2),
                new Token(TokenType.STRING_LITERAL, "\"YES\"", 2),
                new Token(TokenType.RPAREN, ")", 2),
                new Token(TokenType.SEMICOLON, ";", 2),
                new Token(TokenType.RBRACE, "}", 3),
                new Token(TokenType.MAIN, "_main_", 4),
                new Token(TokenType.LPAREN, "(", 4),
                new Token(TokenType.RPAREN, ")", 4),
                new Token(TokenType.LBRACE, "{", 5),
                new Token(TokenType.IDENTIFIER, "print", 6),
                new Token(TokenType.LPAREN, "(", 6),
                new Token(TokenType.STRING_LITERAL, "\"NO\"", 6),
                new Token(TokenType.RPAREN, ")", 6),
                new Token(TokenType.SEMICOLON, ";", 6),
                new Token(TokenType.RBRACE, "}", 7),
                new Token(TokenType.EOF, "", 8)
            };

            Parser parser = new Parser(tokens);
            // Act & Assert
            var exception = Assert.ThrowsException<Exception>(() => parser.Parse());
            Assert.AreEqual("Error, Main function is already defined.", exception.Message);
        }

        [TestMethod]
        public void Parse_RecursiveFunction_Valid()
        {
            // Arrange
            List<Token> tokens = new List<Token>
            {
                // Function: Factorial(number n) -> number
                new Token(TokenType.FUNC, "func", 0),
                new Token(TokenType.IDENTIFIER, "Factorial", 0),
                new Token(TokenType.LPAREN, "(", 0),
                new Token(TokenType.NUMBER, "number", 0),
                new Token(TokenType.IDENTIFIER, "n", 0),
                new Token(TokenType.RPAREN, ")", 0),
                new Token(TokenType.ARROW, "->", 0),
                new Token(TokenType.NUMBER, "number", 0),
                new Token(TokenType.LBRACE, "{", 1),

                // number result = 1;
                new Token(TokenType.NUMBER, "number", 2),
                new Token(TokenType.IDENTIFIER, "result", 2),
                new Token(TokenType.EQUAL, "=", 2),
                new Token(TokenType.NUMBER_LITERAL, "1", 2),
                new Token(TokenType.SEMICOLON, ";", 2),

                // if (n > 1)
                new Token(TokenType.IF, "if", 3),
                new Token(TokenType.LPAREN, "(", 3),
                new Token(TokenType.IDENTIFIER, "n", 3),
                new Token(TokenType.GT, ">", 3),
                new Token(TokenType.NUMBER_LITERAL, "1", 3),
                new Token(TokenType.RPAREN, ")", 3),
                new Token(TokenType.LBRACE, "{", 3),

                // result = n * Factorial(n - 1);
                new Token(TokenType.IDENTIFIER, "result", 4),
                new Token(TokenType.EQUAL, "=", 4),
                new Token(TokenType.IDENTIFIER, "n", 4),
                new Token(TokenType.STAR, "*", 4),
                new Token(TokenType.IDENTIFIER, "Factorial", 4),
                new Token(TokenType.LPAREN, "(", 4),
                new Token(TokenType.IDENTIFIER, "n", 4),
                new Token(TokenType.MINUS, "-", 4),
                new Token(TokenType.NUMBER_LITERAL, "1", 4),
                new Token(TokenType.RPAREN, ")", 4),
                new Token(TokenType.SEMICOLON, ";", 4),
                new Token(TokenType.RBRACE, "}", 5),

                // return result;
                new Token(TokenType.RETURN, "return", 6),
                new Token(TokenType.IDENTIFIER, "result", 6),
                new Token(TokenType.SEMICOLON, ";", 6),
                new Token(TokenType.RBRACE, "}", 7),

                // _main_()
                new Token(TokenType.MAIN, "_main_", 8),
                new Token(TokenType.LPAREN, "(", 8),
                new Token(TokenType.RPAREN, ")", 8),
                new Token(TokenType.LBRACE, "{", 9),

                // number a = 5;
                new Token(TokenType.NUMBER, "number", 10),
                new Token(TokenType.IDENTIFIER, "a", 10),
                new Token(TokenType.EQUAL, "=", 10),
                new Token(TokenType.NUMBER_LITERAL, "5", 10),
                new Token(TokenType.SEMICOLON, ";", 10),

                // print(Factorial(a));
                new Token(TokenType.IDENTIFIER, "print", 11),
                new Token(TokenType.LPAREN, "(", 11),
                new Token(TokenType.IDENTIFIER, "Factorial", 11),
                new Token(TokenType.LPAREN, "(", 11),
                new Token(TokenType.IDENTIFIER, "a", 11),
                new Token(TokenType.RPAREN, ")", 11),
                new Token(TokenType.RPAREN, ")", 11),
                new Token(TokenType.SEMICOLON, ";", 11),

                new Token(TokenType.RBRACE, "}", 12),

                // End of file
                new Token(TokenType.EOF, "", 13)
            };

            Parser parser = new Parser(tokens);

            // Act
            ProgramNode program = parser.Parse();

            // Assert
            Assert.IsNotNull(program);
            Assert.AreEqual(0, program.GlobalVariables.Count);
            Assert.AreEqual(1, program.Functions.Count);
            Assert.IsNotNull(program.MainFunction);
            Assert.AreEqual(2, program.MainFunction.Body.Statements.Count);

            // Assert the Factorial function
            var factorialFunction = program.Functions[0];
            Assert.AreEqual("Factorial", factorialFunction.Name);
            Assert.AreEqual("number", factorialFunction.ReturnType);
            Assert.AreEqual(1, factorialFunction.Parameters.Count);
            Assert.AreEqual("n", factorialFunction.Parameters[0].Name);
            Assert.AreEqual("number", factorialFunction.Parameters[0].Type);

            Assert.IsNotNull(factorialFunction.Body);
            Assert.AreEqual(2, factorialFunction.Body.Statements.Count);

            var resultVarDecl = factorialFunction.Body.Statements[0] as VariableDeclarationNode;
            Assert.IsNotNull(resultVarDecl);
            Assert.AreEqual("number", resultVarDecl.Type);
            Assert.AreEqual("result", resultVarDecl.Name);
            Assert.IsNotNull(resultVarDecl.Initializer);
            Assert.AreEqual(TokenType.NUMBER_LITERAL, (resultVarDecl.Initializer as LiteralNode).Type);
            Assert.AreEqual("1", (resultVarDecl.Initializer as LiteralNode).Value);

            var ifStatement = factorialFunction.Body.Statements[1] as IfStatementNode;
            Assert.IsNotNull(ifStatement);
            Assert.IsNotNull(ifStatement.Condition);
            Assert.IsNotNull(ifStatement.ThenBranch);
            Assert.IsNull(ifStatement.ElseBranch);

            var condition = ifStatement.Condition as BinaryExpressionNode;
            Assert.IsNotNull(condition);
            Assert.AreEqual(">", condition.Operator);
            Assert.IsTrue(condition.Left is IdentifierNode);
            Assert.AreEqual("n", (condition.Left as IdentifierNode).Name);
            Assert.IsTrue(condition.Right is LiteralNode);
            Assert.AreEqual("1", (condition.Right as LiteralNode).Value);

            var thenBranch = ifStatement.ThenBranch as BlockNode;
            Assert.IsNotNull(thenBranch);
            Assert.AreEqual(1, thenBranch.Statements.Count);

            var assignmentInIf = thenBranch.Statements[0] as AssignmentNode;
            Assert.IsNotNull(assignmentInIf);
            Assert.AreEqual("result", assignmentInIf.Name);

            var multiplicationInAssignment = assignmentInIf.Expression as BinaryExpressionNode;
            Assert.IsNotNull(multiplicationInAssignment);
            Assert.AreEqual("*", multiplicationInAssignment.Operator);
            Assert.IsTrue(multiplicationInAssignment.Left is IdentifierNode);
            Assert.AreEqual("n", (multiplicationInAssignment.Left as IdentifierNode).Name);
            Assert.IsTrue(multiplicationInAssignment.Right is FunctionCallNode);

            var factorialCall = multiplicationInAssignment.Right as FunctionCallNode;
            Assert.IsNotNull(factorialCall);
            Assert.AreEqual("Factorial", factorialCall.FunctionName);
            Assert.AreEqual(1, factorialCall.Arguments.Count);

            var factorialArgument = factorialCall.Arguments[0] as BinaryExpressionNode;
            Assert.IsNotNull(factorialArgument);
            Assert.AreEqual("-", factorialArgument.Operator);
            Assert.IsTrue(factorialArgument.Left is IdentifierNode);
            Assert.AreEqual("n", (factorialArgument.Left as IdentifierNode).Name);
            Assert.IsTrue(factorialArgument.Right is LiteralNode);
            Assert.AreEqual("1", (factorialArgument.Right as LiteralNode).Value);

            // Assert the _main_ function
            var mainFunction = program.MainFunction;
            Assert.AreEqual("_main_", mainFunction.Name);
            Assert.AreEqual("void", mainFunction.ReturnType);
            Assert.AreEqual(2, mainFunction.Body.Statements.Count);

            var varDeclarationInMain = mainFunction.Body.Statements[0] as VariableDeclarationNode;
            Assert.IsNotNull(varDeclarationInMain);
            Assert.AreEqual("number", varDeclarationInMain.Type);
            Assert.AreEqual("a", varDeclarationInMain.Name);
            Assert.IsNotNull(varDeclarationInMain.Initializer);
            Assert.AreEqual(TokenType.NUMBER_LITERAL, (varDeclarationInMain.Initializer as LiteralNode).Type);
            Assert.AreEqual("5", (varDeclarationInMain.Initializer as LiteralNode).Value);

            var printStatement = mainFunction.Body.Statements[1] as ExpressionStatementNode;
            Assert.IsNotNull(printStatement);
            var printFunctionCall = printStatement.Expression as FunctionCallNode;
            Assert.IsNotNull(printFunctionCall);
            Assert.AreEqual("print", printFunctionCall.FunctionName);
            Assert.AreEqual(1, printFunctionCall.Arguments.Count);

            var printArgument = printFunctionCall.Arguments[0] as FunctionCallNode;
            Assert.IsNotNull(printArgument);
            Assert.AreEqual("Factorial", printArgument.FunctionName);
            Assert.AreEqual(1, printArgument.Arguments.Count);
            Assert.IsTrue(printArgument.Arguments[0] is IdentifierNode);
            Assert.AreEqual("a", (printArgument.Arguments[0] as IdentifierNode).Name);
        }


    }
}




