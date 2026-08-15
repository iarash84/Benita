using Benita;

namespace BenitaTestProject;

[TestClass]
public class LanguageSyntaxRegressionTests
{
    [TestMethod]
    public void Check_SyntaxErrorAfterReturn_IsNotSkipped()
    {
        const string source = "func value() -> number { return 1; number broken = ; } _main_() {}";
        Assert.ThrowsException<ParserException>(() => new CompilerClass().Check(source));
    }

    [TestMethod]
    public void Check_NonVoidFunctionWithReturningIfElse_IsValid()
    {
        const string source = @"
func sign(number value) -> number {
    if (value >= 0) { return 1; } else { return -1; }
}
_main_() { print(sign(2)); }";
        new CompilerClass().Check(source);
    }

    [TestMethod]
    public void Check_NonVoidFunctionWithMissingReturnPath_IsRejected()
    {
        const string source = "func value(bool flag) -> number { if (flag) { return 1; } } _main_() {}";
        var exception = Assert.ThrowsException<SemanticException>(() => new CompilerClass().Check(source));
        StringAssert.Contains(exception.Message, "must return");
    }

    [DataTestMethod]
    [DataRow("break;")]
    [DataRow("continue;")]
    public void Check_LoopControlOutsideLoop_IsRejected(string statement)
    {
        var exception = Assert.ThrowsException<SemanticException>(() =>
            new CompilerClass().Check($"_main_() {{ {statement} }}"));
        StringAssert.Contains(exception.Message, "inside a loop");
    }

    [TestMethod]
    public void Check_ForIncrement_IsSemanticallyAnalyzed()
    {
        const string source = "_main_() { for (number i = 0; i < 2; missing++) {} }";
        Assert.ThrowsException<SemanticException>(() => new CompilerClass().Check(source));
    }

    [TestMethod]
    public void Exec_ForWithEmptyClauses_CanExitWithBreak()
    {
        const string source = "_main_() { for (;;) { print(1); break; } }";
        using var output = new ConsoleOutput();
        new CompilerClass().Exec(source);
        Assert.AreEqual("1\r\n", output.GetOuput());
    }

    [TestMethod]
    public void Check_MainWithParameters_IsRejected()
    {
        Assert.ThrowsException<ParserException>(() =>
            new CompilerClass().Check("_main_(number value) {}"));
    }

    [DataTestMethod]
    [DataRow("_main_() { void value; }")]
    [DataRow("func value() -> let { return 1; } _main_() {}")]
    [DataRow("func value(let input) -> number { return 1; } _main_() {}")]
    public void Check_InvalidTypePositions_AreRejected(string source)
    {
        Assert.ThrowsException<ParserException>(() => new CompilerClass().Check(source));
    }

    [DataTestMethod]
    [DataRow(".5", "BEN1005")]
    [DataRow("true & false", "BEN1001")]
    [DataRow("true | false", "BEN1001")]
    [DataRow("/* unfinished", "BEN1004")]
    [DataRow("\"bad\\q\"", "BEN1006")]
    public void Lexer_InvalidConstruct_ProducesSpecificDiagnostic(string source, string code)
    {
        var exception = Assert.ThrowsException<LexerException>(() => new Lexer(source).Tokenize());
        Assert.AreEqual(code, exception.Code);
    }

    [TestMethod]
    public void Lexer_StringEscapes_AreDecoded()
    {
        List<Token> tokens = new Lexer("\"line\\n\\t\\\"quote\\\"\\\\\"").Tokenize();
        Assert.AreEqual("line\n\t\"quote\"\\", tokens[0].Lexeme);
    }

    [TestMethod]
    public void Check_ReturnOutsideFunction_IsRejected()
    {
        var exception = Assert.ThrowsException<SemanticException>(() => new CompilerClass().Check("return 1;"));
        StringAssert.Contains(exception.Message, "only be used inside a function");
    }

    [DataTestMethod]
    [DataRow("string value = \"x\"; value++;", "Increment and decrement require a number")]
    [DataRow("string value = \"x\"; value += \"y\";", "Compound assignment requires numeric operands")]
    public void Check_NonNumericMutation_IsRejected(string source, string expectedMessage)
    {
        var exception = Assert.ThrowsException<SemanticException>(() => new CompilerClass().Check(source));
        StringAssert.Contains(exception.Message, expectedMessage);
    }

    [TestMethod]
    public void Exec_ParenthesizedExpression_PreservesGrouping()
    {
        using var output = new ConsoleOutput();
        new CompilerClass().Exec("print((2 + 3) * 4);");

        Assert.AreEqual("20\r\n", output.GetOuput());
    }

    [TestMethod]
    public void Check_ParenthesizedTopLevelExpression_IsAccepted()
    {
        new CompilerClass().Check("(2 + 3) * 4;");
    }

    [TestMethod]
    public void Check_PackageMemberReadAndCall_AreAccepted()
    {
        const string source = """
            pkg Counter {
                number value = 1;
                func get() -> number { return value; }
            }
            Counter counter = new Counter();
            print(counter.value);
            print(counter.get());
            """;

        new CompilerClass().Check(source);
    }

    [TestMethod]
    public void Exec_ElseIf_SelectsFirstMatchingBranch()
    {
        using var output = new ConsoleOutput();
        new CompilerClass().Exec("""
            number x = 7;
            if (x > 10) { print("large"); }
            else if (x > 5) { print("medium"); }
            else { print("small"); }
            """);

        Assert.AreEqual("medium\r\n", output.GetOuput());
    }

    [TestMethod]
    public void Exec_MatchExpression_ReturnsMatchingValue()
    {
        using var output = new ConsoleOutput();
        new CompilerClass().Exec("""
            number score = 4;
            let result = match score {
                0 => "No score",
                1 => "Bad",
                2 => "Average",
                3 => "Good",
                4 => "Excellent",
                _ => "Invalid"
            };
            print(result);
            """);

        Assert.AreEqual("Excellent\r\n", output.GetOuput());
    }

    [TestMethod]
    public void Exec_MatchStatement_ExecutesMatchingBlock()
    {
        using var output = new ConsoleOutput();
        new CompilerClass().Exec("""
            number value = 2;
            match value {
                1 => { print("one"); }
                2 => { print("two"); }
                _ => { print("other"); }
            }
            """);

        Assert.AreEqual("two\r\n", output.GetOuput());
    }

    [TestMethod]
    public void Check_MatchExpressionWithoutDefault_IsRejected()
    {
        Assert.ThrowsException<ParserException>(() =>
            new CompilerClass().Check("number x = match 1 { 1 => 2 };"));
    }

    [TestMethod]
    public void Check_MatchExpressionWithMixedResultTypes_IsRejected()
    {
        var exception = Assert.ThrowsException<SemanticException>(() =>
            new CompilerClass().Check("let x = match 1 { 1 => 2, _ => \"two\" }; print(x);"));

        StringAssert.Contains(exception.Message, "same type");
    }

    [TestMethod]
    public void Check_MatchPatternTypeMustMatchInputType()
    {
        var exception = Assert.ThrowsException<SemanticException>(() =>
            new CompilerClass().Check("let x = match 1 { \"one\" => 1, _ => 0 }; print(x);"));

        StringAssert.Contains(exception.Message, "pattern type mismatch");
    }

    [TestMethod]
    public void Exec_MatchStatement_SupportsMultiplePatternsAndSingleStatementBody()
    {
        using var output = new ConsoleOutput();
        new CompilerClass().Exec("""
            number value = 2;
            match value {
                1, 2, 3 => print("small");
                _ => print("other");
            }
            """);

        Assert.AreEqual("small\r\n", output.GetOuput());
    }

    [TestMethod]
    public void Exec_MatchExpression_SupportsInclusiveRangesAndSemicolonSeparators()
    {
        using var output = new ConsoleOutput();
        new CompilerClass().Exec("""
            number value = 10;
            let result = match value {
                0..10 => "low";
                11..20 => "medium";
                _ => "high"
            };
            print(result);
            """, optimizeAst: true);

        Assert.AreEqual("low\r\n", output.GetOuput());
    }

    [TestMethod]
    public void Lexer_RangeOperator_IsDistinctFromDecimalPoint()
    {
        List<Token> tokens = new Lexer("0..10").Tokenize();

        CollectionAssert.AreEqual(
            new[] { TokenType.NUMBER_LITERAL, TokenType.RANGE, TokenType.NUMBER_LITERAL, TokenType.EOF },
            tokens.Select(token => token.Type).ToArray());
    }

    [TestMethod]
    public void Check_RangePatternRequiresNumbers()
    {
        var exception = Assert.ThrowsException<SemanticException>(() =>
            new CompilerClass().Check("let x = match \"b\" { \"a\"..\"z\" => 1, _ => 0 }; print(x);"));

        StringAssert.Contains(exception.Message, "require a number");
    }

    [TestMethod]
    public void Exec_ForIn_VisitsEveryArrayElement()
    {
        using var output = new ConsoleOutput();
        new CompilerClass().Exec("""
            number[] values = [1, 2, 3];
            for (item in values) {
                print(item);
            }
            """, optimizeAst: true);

        Assert.AreEqual("1\r\n2\r\n3\r\n", output.GetOuput());
    }

    [TestMethod]
    public void Exec_ForIn_SupportsBreakAndContinue()
    {
        using var output = new ConsoleOutput();
        new CompilerClass().Exec("""
            number[] values = [1, 2, 3, 4, 5];
            for (item in values) {
                if (item == 2) { continue; }
                if (item == 4) { break; }
                print(item);
            }
            """);

        Assert.AreEqual("1\r\n3\r\n", output.GetOuput());
    }

    [TestMethod]
    public void Check_ForIn_RejectsNonArrayIterable()
    {
        var exception = Assert.ThrowsException<SemanticException>(() =>
            new CompilerClass().Check("number value = 1; for (item in value) { print(item); }"));

        StringAssert.Contains(exception.Message, "must be an array");
    }

    [TestMethod]
    public void Check_ForIn_VariableDoesNotLeakOutsideLoop()
    {
        var exception = Assert.ThrowsException<SemanticException>(() =>
            new CompilerClass().Check("number[] values = [1]; for (item in values) {} print(item);"));

        StringAssert.Contains(exception.Message, "Undeclared variable 'item'");
    }
}
