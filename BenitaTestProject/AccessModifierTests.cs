using Benita;

namespace BenitaTestProject;

[TestClass]
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
        const string source = "pkg Box {} public Box box = new Box(); print(1);";
        ProgramNode program = new Parser(new Lexer(source).Tokenize()).Parse();

        VariableDeclarationNode variable = program.GlobalVariables.Single();
        Assert.AreEqual("Box", variable.Type);
        Assert.AreEqual(AccessModifier.Public, variable.AccessModifier);
    }

    [TestMethod]
    public void PublicPackageMembers_AreAccessibleFromOutside()
    {
        const string source = "pkg Box { public number value = 1; public func read() -> number { return value; } } _main_() { Box box = new Box(); print(box.value); print(box.read()); }";
        new CompilerClass().Check(source);
    }

    [DataTestMethod]
    [DataRow("pkg Box { number value = 1; } _main_() { Box box = new Box(); print(box.value); }")]
    [DataRow("pkg Box { func read() -> number { return 1; } } _main_() { Box box = new Box(); print(box.read()); }")]
    [DataRow("pkg Box { private number value = 1; } _main_() { Box box = new Box(); print(box.value); }")]
    public void PrivatePackageMembers_AreRejectedOutsideTheirPackage(string source)
    {
        SemanticException exception = Assert.ThrowsException<SemanticException>(() => new CompilerClass().Check(source));
        StringAssert.Contains(exception.Message, "private");
    }

    [TestMethod]
    public void PrivateMembers_AreAccessibleThroughThis()
    {
        const string source = "pkg Box { number value = 1; private func hidden() -> number { return this.value; } public func read() -> number { return this.hidden(); } } _main_() { Box box = new Box(); print(box.read()); }";
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
