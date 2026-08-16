using Benita;

namespace BenitaTestProject
{
    [TestClass]
    /// <summary>ارزیابی گره‌های اصلی AST و تغییر state توسط مفسر را بررسی می‌کند.</summary>
    public class InterpreterTests
    {

        private Interpreter _interpreter;

        [TestInitialize]
        public void Setup()
        {
            _interpreter = new Interpreter();
        }

        [TestMethod]
        public void Visit_WithNumericLiteral_ReturnsNumericValue()
        {
            var node = new LiteralNode("42", TokenType.NUMBER_LITERAL);
            var result = _interpreter.Visit(node);
            Assert.AreEqual(42d, result);
        }

        [TestMethod]
        public void Visit_WithDeclaredIdentifier_ReturnsVariableValue()
        {
            _interpreter.Visit(new VariableDeclarationNode("number", "x", new LiteralNode("5", TokenType.NUMBER_LITERAL)));
            var node = new IdentifierNode("x");
            var result = _interpreter.Visit(node);
            Assert.AreEqual(5d, result);
        }

        [TestMethod]
        public void Visit_WithAdditionExpression_ReturnsSum()
        {
            var leftNode = new LiteralNode("10", TokenType.NUMBER_LITERAL);
            var rightNode = new LiteralNode("20", TokenType.NUMBER_LITERAL);
            var node = new BinaryExpressionNode(leftNode, "+", rightNode);

            var result = _interpreter.Visit(node);

            Assert.AreEqual(30d, result);
        }

        [TestMethod]
        public void Visit_WithUnaryNegation_ReturnsNegativeValue()
        {
            var operandNode = new LiteralNode("10", TokenType.NUMBER_LITERAL);
            var node = new UnaryExpressionNode("-", operandNode);

            var result = _interpreter.Visit(node);

            Assert.AreEqual(-10d, result);
        }

        [TestMethod]
        public void Visit_WithLogicalAndExpression_ReturnsFalse()
        {
            var leftNode = new LiteralNode("true", TokenType.TRUE_LITERAL);
            var rightNode = new LiteralNode("false", TokenType.FALSE_LITERAL);
            var node = new LogicalExpressionNode(leftNode, "&&", rightNode);

            var result = _interpreter.Visit(node);

            Assert.AreEqual(false, (bool)result);
        }

        [TestMethod]
        public void Visit_WithFunctionCall_ReturnsFunctionResult()
        {
            var functionBody = new BlockNode(new List<StatementNode>());
            var returnExpression = new LiteralNode("10", TokenType.NUMBER_LITERAL);
            var returnStatement = new ReturnStatementNode(returnExpression);
            var functionNode = new FunctionNode("foo", new List<ParameterNode>(), "number", functionBody, returnStatement);
            _interpreter.Visit(functionNode);

            var callNode = new FunctionCallNode("foo", new List<ExpressionNode>());
            var result = _interpreter.Visit(callNode);

            Assert.AreEqual(10d, result);
        }

        [TestMethod]
        public void Visit_WithVariableDeclaration_StoresInitialValue()
        {
            var node = new VariableDeclarationNode("number", "x", new LiteralNode("10", TokenType.NUMBER_LITERAL));

            _interpreter.Visit(node);

            var result = _interpreter.Visit(new IdentifierNode("x"));
            Assert.AreEqual(10d, result);
        }

        [TestMethod]
        public void Visit_WithAssignment_UpdatesVariableValue()
        {
            _interpreter.Visit(new VariableDeclarationNode("number", "x", new LiteralNode("10", TokenType.NUMBER_LITERAL)));
            var node = new AssignmentNode("x", new LiteralNode("20", TokenType.NUMBER_LITERAL));

            _interpreter.Visit(node);

            var result = _interpreter.Visit(new IdentifierNode("x"));
            Assert.AreEqual(20d, result);
        }

        [TestMethod]
        public void Visit_WithAdditionAssignment_UpdatesVariableValue()
        {
            _interpreter.Visit(new VariableDeclarationNode("number", "x", new LiteralNode("10", TokenType.NUMBER_LITERAL)));
            var node = new CompoundAssignmentNode("x", "+=", new LiteralNode("5", TokenType.NUMBER_LITERAL));

            _interpreter.Visit(node);

            var result = _interpreter.Visit(new IdentifierNode("x"));
            Assert.AreEqual(15d, result);
        }

        [TestMethod]
        public void Visit_WithIncrement_UpdatesVariableValue()
        {
            _interpreter.Visit(new VariableDeclarationNode("number", "x", new LiteralNode("10", TokenType.NUMBER_LITERAL)));
            var node = new IncrementDecrementNode("x", "++");

            _interpreter.Visit(node);

            var result = _interpreter.Visit(new IdentifierNode("x"));
            Assert.AreEqual(11d, result);
        }

        [TestMethod]
        public void Visit_WithTrueIfCondition_ExecutesThenBranch()
        {
            var condition = new LiteralNode("true", TokenType.TRUE_LITERAL);
            var thenBranch = new AssignmentNode("x", new LiteralNode("10", TokenType.NUMBER_LITERAL));
            var node = new IfStatementNode(condition, thenBranch, null);

            _interpreter.Visit(new VariableDeclarationNode("number", "x", new LiteralNode("0", TokenType.NUMBER_LITERAL)));
            _interpreter.Visit(node);

            var result = _interpreter.Visit(new IdentifierNode("x"));
            Assert.AreEqual(10d, result);
        }

        [TestMethod]
        public void Visit_WithWhileLoop_ExecutesUntilConditionIsFalse()
        {
            var condition = new BinaryExpressionNode(new IdentifierNode("x"), "<", new LiteralNode("10", TokenType.NUMBER_LITERAL));
            var body = new IncrementDecrementNode("x", "++");
            var node = new WhileStatementNode(condition, body);

            _interpreter.Visit(new VariableDeclarationNode("number", "x", new LiteralNode("0", TokenType.NUMBER_LITERAL)));
            _interpreter.Visit(node);

            var result = _interpreter.Visit(new IdentifierNode("x"));
            Assert.AreEqual(10d, result);
        }

        [TestMethod]
        public void Visit_WithArrayAccess_ReturnsElementAtIndex()
        {
            _interpreter.Visit(new VariableDeclarationNode("number[]", "arr", new ArrayInitializerNode(new List<ExpressionNode>
            {
                new LiteralNode("1", TokenType.NUMBER_LITERAL),
                new LiteralNode("2", TokenType.NUMBER_LITERAL),
                new LiteralNode("3", TokenType.NUMBER_LITERAL)
            }, new LiteralNode("3", TokenType.NUMBER_LITERAL))));
            var node = new ArrayAccessNode("arr", new LiteralNode("1", TokenType.NUMBER_LITERAL));

            var result = _interpreter.Visit(node);

            Assert.AreEqual(2d, result);
        }

        [TestMethod]
        public void Visit_WithArrayAssignment_UpdatesElementAtIndex()
        {
            _interpreter.Visit(new VariableDeclarationNode("number[]", "arr", new ArrayInitializerNode(new List<ExpressionNode>
            {
                new LiteralNode("1", TokenType.NUMBER_LITERAL),
                new LiteralNode("2", TokenType.NUMBER_LITERAL),
                new LiteralNode("3", TokenType.NUMBER_LITERAL)
            }, new LiteralNode("3", TokenType.NUMBER_LITERAL))));
            var node = new ArrayAssignmentNode("arr", new LiteralNode("1", TokenType.NUMBER_LITERAL), new LiteralNode("10", TokenType.NUMBER_LITERAL));

            _interpreter.Visit(node);

            var result = _interpreter.Visit(new ArrayAccessNode("arr", new LiteralNode("1", TokenType.NUMBER_LITERAL)));
            Assert.AreEqual(10d, result);
        }
    }
}
