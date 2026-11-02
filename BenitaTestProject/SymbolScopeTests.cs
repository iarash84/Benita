using Benita;

namespace BenitaTestProject;

[TestClass]
public class SymbolScopeTests
{
    [TestMethod]
    public void Lookup_ResolvesThroughParentScopes()
    {
        var global = new Scope(name: "global");
        global.Declare(new VariableSymbol("value", Types.Number));
        var child = new Scope(global, "block");

        Assert.IsTrue(child.TryLookup("value", out Symbol? symbol));
        Assert.AreEqual(Types.Number, symbol!.Type);
    }

    [TestMethod]
    public void ChildDeclaration_ShadowsParentWithoutMutatingIt()
    {
        var global = new Scope(name: "global");
        global.Declare(new VariableSymbol("value", Types.Number));
        var child = new Scope(global, "block");
        child.Declare(new VariableSymbol("value", Types.String));

        Assert.AreEqual(Types.String, child.TryLookup("value", out Symbol? childSymbol) ? childSymbol!.Type : null);
        Assert.AreEqual(Types.Number, global.TryLookup("value", out Symbol? globalSymbol) ? globalSymbol!.Type : null);
    }

    [TestMethod]
    public void DuplicateDeclaration_IsRejectedOnlyInCurrentScope()
    {
        var scope = new Scope(name: "block");
        scope.Declare(new VariableSymbol("value", Types.Number));

        Assert.ThrowsException<InvalidOperationException>(() =>
            scope.Declare(new VariableSymbol("value", Types.String)));
    }

    [TestMethod]
    public void ReplaceLocal_UpdatesExistingInferredSymbol()
    {
        var scope = new Scope(name: "package");
        scope.Declare(new VariableSymbol("value", Types.Unknown));

        scope.ReplaceLocal(new VariableSymbol("value", Types.Number));

        Assert.IsTrue(scope.TryLookupLocal("value", out Symbol? symbol));
        Assert.AreEqual(Types.Number, symbol!.Type);
    }

    [TestMethod]
    public void FunctionSymbol_PreservesSignatureAndDeclaration()
    {
        var function = new FunctionNode("add", [], "number", new BlockNode([]), null);
        var symbol = new FunctionSymbol("add", Types.Number, [Types.Number, Types.Number], function);
        var scope = new Scope(name: "functions");
        scope.Declare(symbol);

        Assert.IsTrue(scope.TryLookup("add", out Symbol? resolved));
        Assert.AreSame(function, ((FunctionSymbol)resolved!).Declaration);
        CollectionAssert.AreEqual(
            new[] { Types.Number, Types.Number },
            ((FunctionSymbol)resolved).ParameterTypes.ToArray());
    }

    [TestMethod]
    public void TypeDeclarationSymbol_IdentifiesPackageAndInterfaceKinds()
    {
        var types = new Scope(name: "types");
        var package = new TypeDeclarationSymbol("User", SymbolKind.Package, new NamedTypeSymbol("User"));
        var contract = new TypeDeclarationSymbol("Printable", SymbolKind.Interface, new NamedTypeSymbol("Printable"));
        types.Declare(package);
        types.Declare(contract);

        Assert.IsTrue(types.TryLookup("User", out Symbol? packageSymbol));
        Assert.AreEqual(SymbolKind.Package, packageSymbol!.Kind);
        Assert.IsTrue(types.TryLookup("Printable", out Symbol? interfaceSymbol));
        Assert.AreEqual(SymbolKind.Interface, interfaceSymbol!.Kind);
    }

    [TestMethod]
    public void RuntimeEnvironment_ResolvesParentAndSupportsShadowing()
    {
        var global = new RuntimeEnvironment(name: "global");
        global.Declare("value", 1d);
        var child = new RuntimeEnvironment(global, "function");
        child.Declare("value", 2d);

        Assert.IsTrue(child.TryGet("value", out object? childValue));
        Assert.AreEqual(2d, childValue);
        Assert.IsTrue(global.TryGet("value", out object? globalValue));
        Assert.AreEqual(1d, globalValue);
    }

    [TestMethod]
    public void RuntimeEnvironment_AssignsNearestExistingParentValue()
    {
        var global = new RuntimeEnvironment(name: "global");
        global.Declare("counter", 1d);
        var child = new RuntimeEnvironment(global, "function");

        Assert.IsTrue(child.TryAssign("counter", 2d));
        Assert.IsTrue(global.TryGet("counter", out object? value));
        Assert.AreEqual(2d, value);
    }

    [TestMethod]
    public void RuntimeEnvironment_RejectsDuplicateLocalDeclaration()
    {
        var environment = new RuntimeEnvironment(name: "block");
        environment.Declare("value", 1d);

        Assert.ThrowsException<InvalidOperationException>(() => environment.Declare("value", 2d));
    }
}
