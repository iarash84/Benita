using Benita;

namespace BenitaTestProject;

[TestClass]
/// <summary>قواعد دسترسی پیش‌فرض و صریح اعضا و تابع `_main_` را بررسی می‌کند.</summary>
public class AccessModifierTests
{
    [TestMethod]
    public void Parser_DefaultsVariablesAndFunctionsToPrivate()
    {
        ProgramNode program = new Parser(new Lexer("number value = 1; func read() -> number { return value; } _main_() {}").Tokenize()).Parse();

        Assert.AreEqual(AccessModifier.Private, program.GlobalVariables.Single().AccessModifier);
        Assert.AreEqual(AccessModifier.Private, program.Functions.Single().AccessModifier);
    }

    [TestMethod]
    public void Parser_AcceptsAccessModifierOnNamedTypeVariable()
    {
        const string source = "import Console;\npkg Box {} public Box box = new Box(); Console.print(1);";
        ProgramNode program = new Parser(new Lexer(source).Tokenize()).Parse();

        VariableDeclarationNode variable = program.GlobalVariables.Single();
        Assert.AreEqual("Box", variable.Type);
        Assert.AreEqual(AccessModifier.Public, variable.AccessModifier);
    }

    [TestMethod]
    public void PublicPackageMembers_AreAccessibleFromOutside()
    {
        const string source = "import Console;\npkg Box { public number value = 1; public func read() -> number { return value; } } _main_() { Box box = new Box(); Console.print(box.value); Console.print(box.read()); }";
        new CompilerClass().Check(source);
    }

    [DataTestMethod]
    [DataRow("import Console;\npkg Box { number value = 1; } _main_() { Box box = new Box(); Console.print(box.value); }")]
    [DataRow("import Console;\npkg Box { func read() -> number { return 1; } } _main_() { Box box = new Box(); Console.print(box.read()); }")]
    [DataRow("import Console;\npkg Box { private number value = 1; } _main_() { Box box = new Box(); Console.print(box.value); }")]
    public void PrivatePackageMembers_AreRejectedOutsideTheirPackage(string source)
    {
        SemanticException exception = Assert.ThrowsException<SemanticException>(() => new CompilerClass().Check(source));
        StringAssert.Contains(exception.Message, "private");
    }

    [TestMethod]
    public void PrivateMembers_AreAccessibleThroughThis()
    {
        const string source = "import Console;\npkg Box { number value = 1; private func hidden() -> number { return this.value; } public func read() -> number { return this.hidden(); } } _main_() { Box box = new Box(); Console.print(box.read()); }";
        new CompilerClass().Check(source);
    }

    [DataTestMethod]
    [DataRow("public _main_() {}")]
    [DataRow("private _main_() {}")]
    public void MainFunction_RejectsAccessModifiers(string source)
    {
        ParserException exception = Assert.ThrowsException<ParserException>(() => new CompilerClass().Check(source));
        StringAssert.Contains(exception.Message, "cannot have an access modifier");
    }
}
