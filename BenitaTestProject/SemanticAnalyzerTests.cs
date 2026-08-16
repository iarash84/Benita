using Benita;

namespace BenitaTestProject
{
    [TestClass]
    /// <summary>ثبت نمادها و تشخیص خطاهای پایه توسط تحلیل‌گر معنایی را بررسی می‌کند.</summary>
    public class SemanticAnalyzerTests
    {
        [TestMethod]
        public void Analyze_WithGlobalVariables_CompletesSuccessfully()
        {
            const string source = """
                number answer = 42;
                _main_() { print(answer); }
                """;
            var program = new Parser(new Lexer(source).Tokenize()).Parse();

            new SemanticAnalyzer().Analyze(program);
        }

        [TestMethod]
        public void Analyze_WithFunctions_CompletesSuccessfully()
        {
            const string source = """
                func add(number left, number right) -> number {
                    return left + right;
                }

                _main_() { number result = add(20, 22); }
                """;
            var program = new Parser(new Lexer(source).Tokenize()).Parse();

            new SemanticAnalyzer().Analyze(program);
        }

        [TestMethod]
        public void Analyze_WithDuplicateFunction_ThrowsException()
        {
            const string source = """
                func calculate() -> number { return 1; }
                func calculate() -> number { return 2; }
                _main_() { }
                """;
            var program = new Parser(new Lexer(source).Tokenize()).Parse();
            var analyzer = new SemanticAnalyzer();

            var exception = Assert.ThrowsException<Exception>(() => analyzer.Analyze(program));

            StringAssert.Contains(exception.Message, "Function 'calculate' is already declared");
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
