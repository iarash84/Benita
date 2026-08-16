using Benita.itpr_df;

using Benita;

namespace BenitaTestProject.Runtime.BuiltIns;

[TestClass]
/// <summary>پیاده‌سازی زمان اجرای عملیات رشته‌ای و حالت‌های مرزی آن را بررسی می‌کند.</summary>
public class StringManagementTests
{
    private readonly StringManagement _strings = new();

    [TestMethod]
    public void HandleFunctionCall_WithStringFunctions_ReturnsExpectedValues()
    {
        Assert.AreEqual(6, _strings.HandleFunctionCall("string_len", new List<object> { "Benita" }));
        Assert.AreEqual("e", _strings.HandleFunctionCall("string_char_at", new List<object> { "Benita", 1 }));
        Assert.AreEqual("nit", _strings.HandleFunctionCall("string_substring", new List<object> { "Benita", 2, 3 }));
        Assert.AreEqual(true, _strings.HandleFunctionCall("string_contains", new List<object> { "Benita", "nit" }));
        Assert.AreEqual(2, _strings.HandleFunctionCall("string_index_of", new List<object> { "Benita", "nit" }));
        Assert.AreEqual(-1, _strings.HandleFunctionCall("string_index_of", new List<object> { "Benita", "xyz" }));
        Assert.AreEqual("Bonita", _strings.HandleFunctionCall("string_replace", new List<object> { "Benita", "e", "o" }));
        Assert.AreEqual("Benita", _strings.HandleFunctionCall("string_trim", new List<object> { "  Benita  " }));
        Assert.AreEqual("benita", _strings.HandleFunctionCall("string_to_lower", new List<object> { "BENITA" }));
        Assert.AreEqual("BENITA", _strings.HandleFunctionCall("string_to_upper", new List<object> { "benita" }));
    }

    [TestMethod]
    public void HandleFunctionCall_WithStringSplit_ReturnsAllPartsIncludingEmptyParts()
    {
        var result = (object[])_strings.HandleFunctionCall(
            "string_split", new List<object> { "one,,three", "," });

        CollectionAssert.AreEqual(new object[] { "one", string.Empty, "three" }, result);
    }

    [TestMethod]
    public void HandleFunctionCall_WithEmptySplitSeparator_ReturnsOriginalString()
    {
        var result = (object[])_strings.HandleFunctionCall(
            "string_split", new List<object> { "Benita", string.Empty });

        CollectionAssert.AreEqual(new object[] { "Benita" }, result);
    }

    [TestMethod]
    public void HandleFunctionCall_WithOutOfRangeCharacterIndex_ThrowsArgumentOutOfRangeException()
    {
        Assert.ThrowsException<BuiltInException>(() =>
            _strings.HandleFunctionCall("string_char_at", new List<object> { "Benita", 10 }));
    }
}
