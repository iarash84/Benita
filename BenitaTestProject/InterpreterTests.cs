using Benita;

namespace BenitaTestProject
{
    [TestClass]
    public class InterpreterTests
    {

        private Interpreter _interpreter;

        [TestInitialize]
        public void Setup()
        {
            _interpreter = new Interpreter();
        }

        [TestMethod]
        public void TestVisitLiteralNode()
        {
            var node = new LiteralNode("42", TokenType.NUMBER_LITERAL);
            var result = _interpreter.Visit(node);
            Assert.AreEqual(42, result);
        }

        [TestMethod]
        public void TestVisitIdentifierNode()
        {
            _interpreter.Visit(new VariableDeclarationNode("number", "x", new LiteralNode("5", TokenType.NUMBER_LITERAL)));
            var node = new IdentifierNode("x");
            var result = _interpreter.Visit(node);
            Assert.AreEqual(5, result);
        }

        [TestMethod]
        public void TestVisitBinaryExpressionNode()
        {
            var leftNode = new LiteralNode("10", TokenType.NUMBER_LITERAL);
            var rightNode = new LiteralNode("20", TokenType.NUMBER_LITERAL);
            var node = new BinaryExpressionNode(leftNode, "+", rightNode);

            var result = _interpreter.Visit(node);

            Assert.AreEqual(30, result);
        }

        [TestMethod]
        public void TestVisitUnaryExpressionNode()
        {
            var operandNode = new LiteralNode("10", TokenType.NUMBER_LITERAL);
            var node = new UnaryExpressionNode("-", operandNode);

            var result = _interpreter.Visit(node);

            Assert.AreEqual(-10, result);
        }

        [TestMethod]
        public void TestVisitLogicalExpressionNode()
        {
            var leftNode = new LiteralNode("true", TokenType.TRUE_LITERAL);
            var rightNode = new LiteralNode("false", TokenType.FALSE_LITERAL);
            var node = new LogicalExpressionNode(leftNode, "&&", rightNode);

            var result = _interpreter.Visit(node);

            Assert.AreEqual(false, (bool)result);
        }

        [TestMethod]
        public void TestVisitFunctionCallNode()
        {
            var functionBody = new BlockNode(new List<StatementNode?>());
            var returnExpression = new LiteralNode("10", TokenType.NUMBER_LITERAL);
            var returnStatement = new ReturnStatementNode(returnExpression);
            var functionNode = new FunctionNode("foo", new List<ParameterNode?>(), "number", functionBody, returnStatement);
            _interpreter.Visit(functionNode);

            var callNode = new FunctionCallNode("foo", new List<ExpressionNode?>());
            var result = _interpreter.Visit(callNode);

            Assert.AreEqual(10, result);
        }

        [TestMethod]
        public void TestVisitVariableDeclarationNode()
        {
            var node = new VariableDeclarationNode("number", "x", new LiteralNode("10", TokenType.NUMBER_LITERAL));

            _interpreter.Visit(node);

            var result = _interpreter.Visit(new IdentifierNode("x"));
            Assert.AreEqual(10, result);
        }

        [TestMethod]
        public void TestVisitAssignmentNode()
        {
            _interpreter.Visit(new VariableDeclarationNode("number", "x", new LiteralNode("10", TokenType.NUMBER_LITERAL)));
            var node = new AssignmentNode("x", new LiteralNode("20", TokenType.NUMBER_LITERAL));

            _interpreter.Visit(node);

            var result = _interpreter.Visit(new IdentifierNode("x"));
            Assert.AreEqual(20, result);
        }

        [TestMethod]
        public void TestVisitCompoundAssignmentNode()
        {
            _interpreter.Visit(new VariableDeclarationNode("number", "x", new LiteralNode("10", TokenType.NUMBER_LITERAL)));
            var node = new CompoundAssignmentNode("x", "+=", new LiteralNode("5", TokenType.NUMBER_LITERAL));

            _interpreter.Visit(node);

            var result = _interpreter.Visit(new IdentifierNode("x"));
            Assert.AreEqual(15, result);
        }

        [TestMethod]
        public void TestVisitIncrementDecrementNode()
        {
            _interpreter.Visit(new VariableDeclarationNode("number", "x", new LiteralNode("10", TokenType.NUMBER_LITERAL)));
            var node = new IncrementDecrementNode("x", "++");

            _interpreter.Visit(node);

            var result = _interpreter.Visit(new IdentifierNode("x"));
            Assert.AreEqual(11, result);
        }

        [TestMethod]
        public void TestVisitIfStatementNode()
        {
            var condition = new LiteralNode("true", TokenType.TRUE_LITERAL);
            var thenBranch = new AssignmentNode("x", new LiteralNode("10", TokenType.NUMBER_LITERAL));
            var node = new IfStatementNode(condition, thenBranch, null);

            _interpreter.Visit(new VariableDeclarationNode("number", "x", new LiteralNode("0", TokenType.NUMBER_LITERAL)));
            _interpreter.Visit(node);

            var result = _interpreter.Visit(new IdentifierNode("x"));
            Assert.AreEqual(10, result);
        }

        [TestMethod]
        public void TestVisitWhileStatementNode()
        {
            var condition = new BinaryExpressionNode(new IdentifierNode("x"), "<", new LiteralNode("10", TokenType.NUMBER_LITERAL));
            var body = new IncrementDecrementNode("x", "++");
            var node = new WhileStatementNode(condition, body);

            _interpreter.Visit(new VariableDeclarationNode("number", "x", new LiteralNode("0", TokenType.NUMBER_LITERAL)));
            _interpreter.Visit(node);

            var result = _interpreter.Visit(new IdentifierNode("x"));
            Assert.AreEqual(10, result);
        }

        [TestMethod]
        public void TestVisitArrayAccessNode()
        {
            _interpreter.Visit(new VariableDeclarationNode("number[]", "arr", new ArrayInitializerNode(new List<ExpressionNode?>
            {
                new LiteralNode("1", TokenType.NUMBER_LITERAL),
                new LiteralNode("2", TokenType.NUMBER_LITERAL),
                new LiteralNode("3", TokenType.NUMBER_LITERAL)
            })));
            var node = new ArrayAccessNode("arr", new LiteralNode("1", TokenType.NUMBER_LITERAL));

            var result = _interpreter.Visit(node);

            Assert.AreEqual(2, result);
        }

        [TestMethod]
        public void TestVisitArrayAssignmentNode()
        {
            _interpreter.Visit(new VariableDeclarationNode("number[]", "arr", new ArrayInitializerNode(new List<ExpressionNode?>
            {
                new LiteralNode("1", TokenType.NUMBER_LITERAL),
                new LiteralNode("2", TokenType.NUMBER_LITERAL),
                new LiteralNode("3", TokenType.NUMBER_LITERAL)
            })));
            var node = new ArrayAssignmentNode("arr", new LiteralNode("1", TokenType.NUMBER_LITERAL), new LiteralNode("10", TokenType.NUMBER_LITERAL));

            _interpreter.Visit(node);

            var result = _interpreter.Visit(new ArrayAccessNode("arr", new LiteralNode("1", TokenType.NUMBER_LITERAL)));
            Assert.AreEqual(10, result);
        }
    }
}
