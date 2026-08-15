using Benita;

namespace BenitaTestProject;

[TestClass]
public class ArrayFunctionTests
{
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

        Assert.AreEqual(expected, output.GetOuput());
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
