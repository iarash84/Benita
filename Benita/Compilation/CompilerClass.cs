namespace Benita;

/// <summary>خط لولهٔ کامپایل بنیتا را از توکن‌سازی تا تحلیل و اجرا هماهنگ می‌کند.</summary>
public sealed class CompilerClass
{
    private readonly BuiltInClassRegistry _builtInClasses;

    public CompilerClass(BuiltInClassRegistry? builtInClasses = null)
    {
        _builtInClasses = builtInClasses ?? BuiltInClassRegistry.CreateDefault();
    }

    /// <summary>کد منبع را تحلیل و با مفسر اجرا می‌کند.</summary>
    public void Exec(string sourceCode, bool lexerPrint = false, bool parserPrint = false,
        bool sourcePrint = false, bool debugModeAvailable = false, string? sourceName = null,
        bool optimizeAst = false)
    {
        ProgramNode program = Compile(sourceCode, lexerPrint, parserPrint, sourcePrint, sourceName, optimizeAst);
        try
        {
            new Interpreter(debugModeAvailable, context: new RuntimeContext(_builtInClasses)).Visit(program);
        }
        catch (BenitaException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new RuntimeException(exception.Message, exception);
        }
    }

    /// <summary>یک قطعه کد را در مفسر پایدار REPL اجرا و کل نشست را تحلیل می‌کند.</summary>
    internal void ExecInSession(string sourceCode, string sessionSource, Interpreter interpreter,
        bool lexerPrint = false, bool parserPrint = false, bool sourcePrint = false,
        bool optimizeAst = false)
    {
        if (!ReferenceEquals(interpreter.BuiltInClasses, _builtInClasses))
            throw new InvalidOperationException("The compiler and REPL interpreter must use the same built-in class registry.");

        const string noOp = "if (false) {}";
        AnalyzeProgram(Parse(Tokenize($"{sessionSource}{Environment.NewLine}{noOp}",
            sourceName: "<repl>")), _builtInClasses);
        ProgramNode program = Parse(Tokenize($"{sourceCode}{Environment.NewLine}{noOp}",
            lexerPrint, sourcePrint, "<repl>"));
        program = OptimizeIfRequested(program, parserPrint, optimizeAst);
        Interpreter.TransactionCheckpoint checkpoint = interpreter.CreateCheckpoint();
        try
        {
            interpreter.Visit(program);
        }
        catch
        {
            interpreter.RestoreCheckpoint(checkpoint);
            throw;
        }
    }

    /// <summary>صحت واژگانی، نحوی و معنایی کد را بدون اجرا بررسی می‌کند.</summary>
    public void Check(string sourceCode, bool lexerPrint = false, bool parserPrint = false,
        bool sourcePrint = false, string? sourceName = null)
    {
        ProgramNode program = Parse(Tokenize(sourceCode, lexerPrint, sourcePrint, sourceName));
        if (parserPrint) AstPrinter.Print(program);
        AnalyzeProgram(program, _builtInClasses);
    }

    private ProgramNode Compile(string sourceCode, bool lexerPrint, bool parserPrint,
        bool sourcePrint, string? sourceName, bool optimizeAst)
    {
        ProgramNode program = Parse(Tokenize(sourceCode, lexerPrint, sourcePrint, sourceName));
        AnalyzeProgram(program, _builtInClasses);
        return OptimizeIfRequested(program, parserPrint, optimizeAst);
    }

    private static List<Token> Tokenize(string sourceCode, bool printTokens = false,
        bool printSource = false, string? sourceName = null)
    {
        List<Token> tokens = new Lexer(sourceCode, printSource, sourceName).Tokenize();
        if (printTokens)
            foreach (Token token in tokens) Console.WriteLine(token);
        return tokens;
    }

    private static ProgramNode Parse(List<Token> tokens) => new Parser(tokens).Parse();

    private static ProgramNode OptimizeIfRequested(ProgramNode program, bool printAst, bool optimizeAst)
    {
        if (optimizeAst) program = new AstOptimizer().Optimize(program);
        if (printAst) AstPrinter.Print(program);
        return program;
    }

    private static void AnalyzeProgram(ProgramNode program, BuiltInClassRegistry builtInClasses)
    {
        var analyzer = new SemanticAnalyzer(builtInClasses);
        try { analyzer.Analyze(program); }
        catch (BenitaException) { throw; }
        catch (Exception exception) { throw new SemanticException(exception.Message, analyzer.CurrentSpan, exception); }
    }
}
