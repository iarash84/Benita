using Benita;

namespace BenitaTestProject;

[TestClass]
/// <summary>لبه‌های مشترک تحلیل معنایی و runtime را که در تست‌های happy path دیده نمی‌شوند پوشش می‌دهد.</summary>
public class SemanticAndRuntimeEdgeCaseTests
{
    [TestMethod]
    public void Visit_WithFalseAndExpression_DoesNotEvaluateRightOperand()
    {
        var interpreter = new Interpreter();
        var expression = new LogicalExpressionNode(
            new LiteralNode("false", TokenType.FALSE_LITERAL),
            "&&",
            new IdentifierNode("undefined"));

        var result = interpreter.Visit(expression);

        Assert.AreEqual(false, result);
    }

    [TestMethod]
    public void Analyze_WithForwardFunctionCall_CompletesSuccessfully()
    {
        const string source = """
            func first() -> number { return second(); }
            func second() -> number { return 2; }
            _main_() { print(first()); }
            """;

        var program = new Parser(new Lexer(source).Tokenize()).Parse();

        new SemanticAnalyzer().Analyze(program);
    }

    [TestMethod]
    public void Analyze_WithUndeclaredTopLevelVariable_ThrowsException()
    {
        const string source = "x = 1;";
        var program = new Parser(new Lexer(source).Tokenize()).Parse();

        var exception = Assert.ThrowsException<Exception>(() => new SemanticAnalyzer().Analyze(program));

        StringAssert.Contains(exception.Message, "Undeclared variable 'x'");
    }

    [TestMethod]
    public void Visit_WithIncorrectFunctionArgumentCount_ThrowsArgumentException()
    {
        var interpreter = new Interpreter();
        interpreter.Visit(new FunctionNode(
            "identity",
            [new ParameterNode("number", "value")],
            "number",
            new BlockNode([]),
            new ReturnStatementNode(new IdentifierNode("value"))));

        Assert.ThrowsException<ArgumentException>(() =>
            interpreter.Visit(new FunctionCallNode("identity", [])));
    }
}
