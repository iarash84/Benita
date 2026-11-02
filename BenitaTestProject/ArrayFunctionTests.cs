using Benita;

namespace BenitaTestProject;

[TestClass]
/// <summary>نوع‌دهی و رفتار زمان اجرای مجموعهٔ توابع داخلی آرایه را بررسی می‌کند.</summary>
public class ArrayFunctionTests
{
    [TestMethod]
    public void NumericLookupsAndSort_IgnoreClrNumberRepresentation()
    {
        const string source = """
            import Terminal;
            import Array;
            _main_() {
                number[] seed = [10, 20];
                number[] values = [1, Array.length(seed)];
                Terminal.print(Array.contains(values, 2));
                Terminal.print(Array.index_of(values, 2));
                values = Array.sort(values);
                Terminal.print(values[0]);
                Terminal.print(values[1]);
            }
            """;

        foreach (bool optimize in new[] { false, true })
        {
            using var output = new ConsoleOutput();
            new CompilerClass().Exec(source, optimizeAst: optimize);
            Assert.AreEqual(
                $"True{Environment.NewLine}1{Environment.NewLine}1{Environment.NewLine}2{Environment.NewLine}",
                output.GetOutput());
        }
    }

    [DataTestMethod]
    [DataRow("Array.add(source, 3)")]
    [DataRow("Array.remove(source, 0)")]
    [DataRow("Array.reverse(source)")]
    [DataRow("Array.clear(source)")]
    [DataRow("Array.insert(source, 0, 3)")]
    [DataRow("Array.slice(source, 0, 1)")]
    [DataRow("Array.concat(source, [3])")]
    [DataRow("Array.sort(source)")]
    public void Check_LetInfersConcreteArrayTypeFromArrayPreservingBuiltIn(string expression)
    {
        string source = $@"
            import Array;
            _main_() {{
                number[] source = [2, 1];
                let result = {expression};
                for (item in result) {{ number value = item; }}
            }}";

        new CompilerClass().Check(source);
    }

    [TestMethod]
    public void Exec_InferredArrayBuiltInResultSupportsIndexAccess()
    {
        const string source = """
            import Terminal;
            import Array;
            _main_() {
                number[] source = [2, 1];
                let result = Array.reverse(source);
                Terminal.print(result[0]);
            }
            """;
        using var output = new ConsoleOutput();

        new CompilerClass().Exec(source, optimizeAst: true);

        Assert.AreEqual($"1{Environment.NewLine}", output.GetOutput());
    }

    [TestMethod]
    public void Exec_WithAllArrayFunctions_ProducesExpectedOutput()
    {
        const string source = @"import Terminal;
import Array;

_main_() {
    number[] values = [3, 1, 2];
    Terminal.print(Array.contains(values, 1));
    Terminal.print(Array.index_of(values, 2));
    values = Array.insert(values, 1, 4);
    values = Array.reverse(values);
    number[] part = Array.slice(values, 1, 2);
    values = Array.concat(part, [5, 0]);
    values = Array.sort(values);
    Terminal.print(values[0]);
    Terminal.print(values[3]);
    values = Array.clear(values);
    Terminal.print(Array.length(values));
}";
        const string expected = "True\r\n2\r\n0\r\n5\r\n0\r\n";
        using var output = new ConsoleOutput();

        new CompilerClass().Exec(source);

        Assert.AreEqual(expected, output.GetOutput());
    }

    [TestMethod]
    public void Check_WithMismatchedArrayElementType_ThrowsSemanticException()
    {
        const string source = @"import Array;

_main_() {
    number[] values = [1, 2];
    values = Array.insert(values, 1, ""wrong"");
}";

        var exception = Assert.ThrowsException<SemanticException>(() => new CompilerClass().Check(source));
        StringAssert.Contains(exception.Message, "Array.insert");
    }
}
