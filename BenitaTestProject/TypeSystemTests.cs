using Benita;

namespace BenitaTestProject;

[TestClass]
public class TypeSystemTests
{
    [DataTestMethod]
    [DataRow("number")]
    [DataRow("string")]
    [DataRow("bool")]
    [DataRow("void")]
    [DataRow("number[]")]
    [DataRow("string[][]")]
    [DataRow("Customer")]
    public void FromName_RoundTripsCanonicalTypeName(string name)
    {
        Assert.AreEqual(name, TypeFacts.FromName(name).Name);
    }

    [TestMethod]
    public void FromName_CreatesStructuredArrayType()
    {
        var array = TypeFacts.FromName("number[][]") as ArrayTypeSymbol;

        Assert.IsNotNull(array);
        Assert.IsInstanceOfType<ArrayTypeSymbol>(array.ElementType);
        Assert.AreEqual(Types.Number, ((ArrayTypeSymbol)array.ElementType).ElementType);
    }

    [TestMethod]
    public void IsAssignableTo_AcceptsEqualPrimitiveAndArrayTypes()
    {
        Assert.IsTrue(TypeFacts.IsAssignableTo(Types.Number, Types.Number));
        Assert.IsTrue(TypeFacts.IsAssignableTo(
            Types.ArrayOf(Types.String), Types.ArrayOf(Types.String)));
    }

    [TestMethod]
    public void IsAssignableTo_RejectsDifferentArrayElementTypes()
    {
        Assert.IsFalse(TypeFacts.IsAssignableTo(
            Types.ArrayOf(Types.Number), Types.ArrayOf(Types.String)));
    }

    [TestMethod]
    public void IsAssignableTo_HandlesBuiltInWildcardTypes()
    {
        Assert.IsTrue(TypeFacts.IsAssignableTo(Types.Bool, Types.Any));
        Assert.IsTrue(TypeFacts.IsAssignableTo(Types.ArrayOf(Types.Number), Types.AnyArray));
        Assert.IsTrue(TypeFacts.IsAssignableTo(Types.AnyArray, Types.ArrayOf(Types.Number)));
        Assert.IsFalse(TypeFacts.IsAssignableTo(Types.Number, Types.AnyArray));
    }

    [TestMethod]
    public void NamedTypes_AreComparedByName()
    {
        Assert.IsTrue(TypeFacts.AreEquivalent(
            new NamedTypeSymbol("User"), new NamedTypeSymbol("User")));
        Assert.IsFalse(TypeFacts.AreEquivalent(
            new NamedTypeSymbol("User"), new NamedTypeSymbol("Order")));
    }
}
