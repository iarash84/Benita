using Benita;

namespace BenitaTestProject;

[TestClass]
public class ArrayFunctionTests
{
    [TestMethod]
    public void NumericLookupsAndSort_IgnoreClrNumberRepresentation()
    {
        const string source = """
            _main_() {
                number[] seed = [10, 20];
                number[] values = [1, array_len(seed)];
                print(array_contains(values, 2));
                print(array_index_of(values, 2));
                values = array_sort(values);
                print(values[0]);
                print(values[1]);
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
    [DataRow("array_add(source, 3)")]
    [DataRow("array_remove(source, 0)")]
    [DataRow("array_reverse(source)")]
    [DataRow("array_clear(source)")]
    [DataRow("array_insert(source, 0, 3)")]
    [DataRow("array_slice(source, 0, 1)")]
    [DataRow("array_concat(source, [3])")]
    [DataRow("array_sort(source)")]
    public void Check_LetInfersConcreteArrayTypeFromArrayPreservingBuiltIn(string expression)
    {
        string source = $@"
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
            _main_() {
                number[] source = [2, 1];
                let result = array_reverse(source);
                print(result[0]);
            }
            """;
        using var output = new ConsoleOutput();

        new CompilerClass().Exec(source, optimizeAst: true);

        Assert.AreEqual($"1{Environment.NewLine}", output.GetOutput());
    }

    [TestMethod]
    public void Exec_WithAllArrayFunctions_ProducesExpectedOutput()
    {
        const string source = @"
_main_() {
    number[] values = [3, 1, 2];
    print(array_contains(values, 1));
    print(array_index_of(values, 2));
    values = array_insert(values, 1, 4);
    values = array_reverse(values);
    number[] part = array_slice(values, 1, 2);
    values = array_concat(part, [5, 0]);
    values = array_sort(values);
    print(values[0]);
    print(values[3]);
    values = array_clear(values);
    print(array_len(values));
}";
        const string expected = "True\r\n2\r\n0\r\n5\r\n0\r\n";
        using var output = new ConsoleOutput();

        new CompilerClass().Exec(source);

        Assert.AreEqual(expected, output.GetOutput());
    }

    [TestMethod]
    public void Check_WithMismatchedArrayElementType_ThrowsSemanticException()
    {
        const string source = @"
_main_() {
    number[] values = [1, 2];
    values = array_insert(values, 1, ""wrong"");
}";

        var exception = Assert.ThrowsException<SemanticException>(() => new CompilerClass().Check(source));
        StringAssert.Contains(exception.Message, "array_insert");
    }
}
