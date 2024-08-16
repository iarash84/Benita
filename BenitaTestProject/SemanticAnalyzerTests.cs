using Benita;

namespace BenitaTestProject
{
    [TestClass]
    public class SemanticAnalyzerTests
    {
        [TestMethod]
        public void TestAnalyze_GlobalVariables()
        {
            // TODO : Notimpelimented
        }

        [TestMethod]
        public void TestAnalyze_Functions()
        {
            // TODO : Notimpelimented
        }

        [TestMethod]
        public void TestAnalyze_DuplicateFunction()
        {
            // TODO : Notimpelimented
        }

        [TestMethod]
        public void TestAnalyze_MainFunction()
        {
            // Arrange
            var semanticAnalyzer = new SemanticAnalyzer();
            var mainFunction = new FunctionNode("main", new List<ParameterNode>(), "void", new BlockNode(new List<StatementNode>()), null);
            var program = new ProgramNode(
                new List<VariableDeclarationNode?>(),
                new List<PackageNode?>(),
                new List<FunctionNode?>(),
                mainFunction,
                statements: null
            );

            // Act & Assert (no exception expected)
            semanticAnalyzer.Analyze(program);
        }

        [TestMethod]
        public void TestAnalyze_ReturnTypeMismatch()
        {
            // Arrange
            var semanticAnalyzer = new SemanticAnalyzer();
            var program = new ProgramNode(
                new List<VariableDeclarationNode?>(),
                new List<PackageNode?>(),
                new List<FunctionNode?>
                {
                    new FunctionNode("foo", new List<ParameterNode>(), "number",
                    new BlockNode(new List<StatementNode>()),
                    new ReturnStatementNode(new LiteralNode("true", TokenType.TRUE_LITERAL)))
                },
                null,
                statements: null
            );

            // Act & Assert (expecting an exception)
            Assert.ThrowsException<Exception>(() => semanticAnalyzer.Analyze(program));
        }

        [TestMethod]
        public void TestAnalyze_UndeclaredVariable()
        {
            // Arrange
            var semanticAnalyzer = new SemanticAnalyzer();
            var program = new ProgramNode(
                new List<VariableDeclarationNode?>(),
                new List<PackageNode?>(),
                new List<FunctionNode?>
                {
                    new FunctionNode("foo", new List<ParameterNode>(), "void", new BlockNode(new List<StatementNode?>
                    {
                        new AssignmentNode("x", new LiteralNode("10", TokenType.NUMBER_LITERAL))
                    }), null),
                },
                null,
                statements: null
            );

            // Act & Assert (expecting an exception)
            Assert.ThrowsException<Exception>(() => semanticAnalyzer.Analyze(program));
        }

    }
}
