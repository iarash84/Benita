using Benita;

namespace BenitaTestProject;

[TestClass]
public class AstOptimizerTests
{
    private readonly AstOptimizer _optimizer = new();

    [TestMethod]
    public void Optimize_WithNestedConstantArithmetic_FoldsExpression()
    {
        var expression = new BinaryExpressionNode(
            new LiteralNode("2", TokenType.NUMBER_LITERAL),
            "+",
            new BinaryExpressionNode(
                new LiteralNode("3", TokenType.NUMBER_LITERAL),
                "*",
                new LiteralNode("4", TokenType.NUMBER_LITERAL)));

        ProgramNode optimized = _optimizer.Optimize(CreateProgram(
            new VariableDeclarationNode("number", "result", expression)));

        var declaration = (VariableDeclarationNode)optimized.MainFunction!.Body.Statements[0];
        var literal = (LiteralNode)declaration.Initializer!;
        Assert.AreEqual("14", literal.Value);
        Assert.AreEqual(TokenType.NUMBER_LITERAL, literal.Type);
    }

    [TestMethod]
    public void Optimize_WithConstantComparison_FoldsToBooleanLiteral()
    {
        var expression = new BinaryExpressionNode(
            new LiteralNode("10", TokenType.NUMBER_LITERAL),
            ">",
            new LiteralNode("3", TokenType.NUMBER_LITERAL));

        ProgramNode optimized = _optimizer.Optimize(CreateProgram(
            new VariableDeclarationNode("bool", "result", expression)));

        var declaration = (VariableDeclarationNode)optimized.MainFunction!.Body.Statements[0];
        var literal = (LiteralNode)declaration.Initializer!;
        Assert.AreEqual(TokenType.TRUE_LITERAL, literal.Type);
    }

    [TestMethod]
    public void Optimize_WithShortCircuitedLogicalExpression_DoesNotEvaluateRightOperand()
    {
        var divisionByZero = new BinaryExpressionNode(
            new LiteralNode("1", TokenType.NUMBER_LITERAL),
            "/",
            new LiteralNode("0", TokenType.NUMBER_LITERAL));
        var expression = new LogicalExpressionNode(
            new LiteralNode("false", TokenType.FALSE_LITERAL),
            "&&",
            new BinaryExpressionNode(
                divisionByZero,
                ">",
                new LiteralNode("0", TokenType.NUMBER_LITERAL)));

        ProgramNode optimized = _optimizer.Optimize(CreateProgram(
            new VariableDeclarationNode("bool", "result", expression)));

        var declaration = (VariableDeclarationNode)optimized.MainFunction!.Body.Statements[0];
        var literal = (LiteralNode)declaration.Initializer!;
        Assert.AreEqual(TokenType.FALSE_LITERAL, literal.Type);
    }

    [TestMethod]
    public void Optimize_WithDivisionByZero_PreservesRuntimeExpression()
    {
        var expression = new BinaryExpressionNode(
            new LiteralNode("1", TokenType.NUMBER_LITERAL),
            "/",
            new LiteralNode("0", TokenType.NUMBER_LITERAL));

        ProgramNode optimized = _optimizer.Optimize(CreateProgram(
            new VariableDeclarationNode("number", "result", expression)));

        var declaration = (VariableDeclarationNode)optimized.MainFunction!.Body.Statements[0];
        Assert.IsInstanceOfType<BinaryExpressionNode>(declaration.Initializer);
    }

    [TestMethod]
    public void Optimize_WithConstantIfCondition_KeepsOnlyReachableBranch()
    {
        var thenBranch = new ExpressionStatementNode(
            new FunctionCallNode("print", new List<ExpressionNode>
            {
                new LiteralNode("reachable", TokenType.STRING_LITERAL)
            }));
        var elseBranch = new ExpressionStatementNode(
            new FunctionCallNode("print", new List<ExpressionNode>
            {
                new LiteralNode("unreachable", TokenType.STRING_LITERAL)
            }));

        ProgramNode optimized = _optimizer.Optimize(CreateProgram(
            new IfStatementNode(
                new LiteralNode("true", TokenType.TRUE_LITERAL),
                thenBranch,
                elseBranch)));

        Assert.IsInstanceOfType<ExpressionStatementNode>(optimized.MainFunction!.Body.Statements[0]);
    }

    [TestMethod]
    public void GenerateCppCode_WithOptimizationEnabled_EmitsFoldedConstant()
    {
        const string source = @"
_main_() {
    number result = 2 + 3 * 4;
    print(result);
}";

        string generatedCode = new CompilerClass().GenerateCppCode(source, optimizeAst: true);

        StringAssert.Contains(generatedCode, "double result = 14;");
        Assert.IsFalse(generatedCode.Contains("2 + 3 * 4", StringComparison.Ordinal));
    }

    private static ProgramNode CreateProgram(StatementNode statement)
    {
        return new ProgramNode(
            new List<VariableDeclarationNode>(),
            new List<PackageNode>(),
            new List<FunctionNode>(),
            new FunctionNode(
                "_main_",
                new List<ParameterNode>(),
                "void",
                new BlockNode(new List<StatementNode> { statement }),
                null),
            new List<StatementNode?>());
    }
}
