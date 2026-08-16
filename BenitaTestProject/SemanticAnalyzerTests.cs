using Benita;

namespace BenitaTestProject
{
    [TestClass]
    public class SemanticAnalyzerTests
    {
        [TestMethod]
        public void Analyze_WithGlobalVariables_CompletesSuccessfully()
        {
            // TODO : Notimpelimented
        }

        [TestMethod]
        public void Analyze_WithFunctions_CompletesSuccessfully()
        {
            // TODO : Notimpelimented
        }

        [TestMethod]
        public void Analyze_WithDuplicateFunction_ThrowsException()
        {
            // TODO : Notimpelimented
        }

        [TestMethod]
        public void Analyze_WithValidMainFunction_CompletesSuccessfully()
        {
            var semanticAnalyzer = new SemanticAnalyzer();
            var mainFunction = new FunctionNode("main", new List<ParameterNode>(), "void", new BlockNode(new List<StatementNode>()), null);
            var program = new ProgramNode(
                new List<VariableDeclarationNode?>(),
                new List<PackageNode?>(),
                new List<FunctionNode?>(),
                mainFunction,
                statements: null
            );

            semanticAnalyzer.Analyze(program);
        }

        [TestMethod]
        public void Analyze_WithMismatchedReturnType_ThrowsException()
        {
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

            Assert.ThrowsException<Exception>(() => semanticAnalyzer.Analyze(program));
        }

        [TestMethod]
        public void Analyze_WithUndeclaredVariable_ThrowsException()
        {
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

            Assert.ThrowsException<Exception>(() => semanticAnalyzer.Analyze(program));
        }

    }
}
