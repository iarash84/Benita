using Benita;

namespace BenitaTestProject;

[TestClass]
public class IncludeOnceTests
{
    [TestMethod]
    public void IncludeOnce_PrefixInIdentifier_IsTokenizedAsIdentifier()
    {
        const string source = """
            func include_once_helper() -> number { return 1; }
            _main_() { print(include_once_helper()); }
            """;

        new CompilerClass().Check(source);
    }

    [TestMethod]
    public void IncludeOnce_SemanticDiagnosticPointsToIncludedFile()
    {
        string directory = CreateTemporaryDirectory();
        try
        {
            string libraryPath = Path.Combine(directory, "broken.ben");
            File.WriteAllText(libraryPath, "func broken() -> number { return missing; }");
            const string source = "include_once \"broken.ben\";\n_main_() {}";

            SemanticException exception = Assert.ThrowsException<SemanticException>(() =>
                new CompilerClass().Check(source, sourceName: Path.Combine(directory, "main.ben")));

            Assert.AreEqual(Path.GetFullPath(libraryPath), exception.Span.FileName);
            Assert.AreEqual(1, exception.Span.Line);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [TestMethod]
    public void IncludeOnce_CanonicalizesEquivalentPaths()
    {
        string directory = CreateTemporaryDirectory();
        try
        {
            string libraryDirectory = Directory.CreateDirectory(Path.Combine(directory, "lib")).FullName;
            File.WriteAllText(Path.Combine(libraryDirectory, "common.ben"),
                "func value() -> number { return 1; }");
            const string source = """
                include_once "lib/common.ben";
                include_once "lib/../lib/common.ben";
                _main_() { print(value()); }
                """;

            List<Token> tokens = new Lexer(source, sourceName: Path.Combine(directory, "main.ben")).Tokenize();

            Assert.AreEqual(1, tokens.Count(token => token.Type == TokenType.FUNC));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [TestMethod]
    public void IncludeOnce_ResolvesNestedPathRelativeToIncludingFile()
    {
        string directory = CreateTemporaryDirectory();
        try
        {
            string featureDirectory = Directory.CreateDirectory(Path.Combine(directory, "feature")).FullName;
            File.WriteAllText(Path.Combine(directory, "common.ben"),
                "func value() -> number { return 2; }");
            File.WriteAllText(Path.Combine(featureDirectory, "entry.ben"),
                "include_once \"../common.ben\";");
            const string source = """
                include_once "feature/entry.ben";
                _main_() { print(value()); }
                """;

            new CompilerClass().Check(source, sourceName: Path.Combine(directory, "main.ben"));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [TestMethod]
    public void IncludeOnce_CycleBackToRoot_IsRejectedWithDependencyChain()
    {
        string directory = CreateTemporaryDirectory();
        try
        {
            string featureDirectory = Directory.CreateDirectory(Path.Combine(directory, "feature")).FullName;
            string rootPath = Path.Combine(directory, "main.ben");
            const string source = """
                include_once "feature/entry.ben";
                _main_() { print("ok"); }
                """;
            File.WriteAllText(rootPath, source);
            File.WriteAllText(Path.Combine(featureDirectory, "entry.ben"),
                "include_once \"../main.ben\";");

            LexerException exception = Assert.ThrowsException<LexerException>(() =>
                new Lexer(source, sourceName: rootPath).Tokenize());

            Assert.AreEqual("BEN1005", exception.Code);
            StringAssert.Contains(exception.Message, "main.ben -> entry.ben -> main.ben");
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [TestMethod]
    public void IncludeOnce_WithMalformedDirective_ReportsDiagnostic()
    {
        LexerException exception = Assert.ThrowsException<LexerException>(() =>
            new Lexer("include_once common.ben;").Tokenize());

        Assert.AreEqual("BEN1004", exception.Code);
    }

    [TestMethod]
    public void IncludeOnce_InsideBlockComment_IsIgnored()
    {
        const string source = """
            /*
            include_once "definitely-missing.ben";
            */
            _main_() { print("ok"); }
            """;

        new CompilerClass().Check(source);
    }

    [TestMethod]
    public void IncludeOnce_BlockCommentMarkerInsideString_DoesNotHideFollowingDirective()
    {
        string directory = CreateTemporaryDirectory();
        try
        {
            File.WriteAllText(Path.Combine(directory, "library.ben"),
                "func value() -> number { return 3; }");
            const string source = """
                string marker = "/*";
                include_once "library.ben";
                _main_() { print(value()); }
                """;

            new CompilerClass().Check(source, sourceName: Path.Combine(directory, "main.ben"));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [TestMethod]
    public void IncludedSourceDiagnostic_PreservesOriginalFileLineAndText()
    {
        string directory = CreateTemporaryDirectory();
        try
        {
            string libraryPath = Path.Combine(directory, "library.ben");
            File.WriteAllText(libraryPath, "// first line\n_main_() { @ }");
            string rootPath = Path.Combine(directory, "main.ben");

            LexerException exception = Assert.ThrowsException<LexerException>(() =>
                new Lexer("include_once \"library.ben\";", sourceName: rootPath).Tokenize());

            Assert.AreEqual(Path.GetFullPath(libraryPath), exception.Span.FileName);
            Assert.AreEqual(2, exception.Span.Line);
            Assert.AreEqual("_main_() { @ }", exception.Span.LineText);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [TestMethod]
    public void IncludedParserDiagnostic_PreservesOriginalFileLocation()
    {
        string directory = CreateTemporaryDirectory();
        try
        {
            string libraryPath = Path.Combine(directory, "broken.ben");
            File.WriteAllText(libraryPath, "func broken() -> number {\nnumber value = ;\n}");
            string rootPath = Path.Combine(directory, "main.ben");

            ParserException exception = Assert.ThrowsException<ParserException>(() =>
                new CompilerClass().Check("include_once \"broken.ben\";\n_main_() {}", sourceName: rootPath));

            Assert.AreEqual(Path.GetFullPath(libraryPath), exception.Span.FileName);
            Assert.AreEqual(2, exception.Span.Line);
            Assert.AreEqual("number value = ;", exception.Span.LineText);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    /// <summary>برای هر تست یک پوشهٔ موقت مستقل ایجاد می‌کند.</summary>
    private static string CreateTemporaryDirectory()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"BenitaIncludes-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        return directory;
    }
}
