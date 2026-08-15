using System.Text;

namespace Benita;

/// <summary>
/// محیط تعاملی و stateful خواندن، ارزیابی و نمایش نتیجه را برای زبان بنیتا فراهم می‌کند.
/// </summary>
public sealed class Repl
{
    private readonly TextReader _input;
    private readonly TextWriter _output;
    private readonly List<string> _history = [];
    private readonly StringBuilder _sessionSource = new();
    private CompilerClass _compiler = new();
    private Interpreter _interpreter = new(preserveStateBetweenPrograms: true);

    /// <summary>یک نشست تعاملی با ورودی و خروجی قابل‌جایگزینی ایجاد می‌کند.</summary>
    public Repl(TextReader? input = null, TextWriter? output = null)
    {
        _input = input ?? Console.In;
        _output = output ?? Console.Out;
    }

    /// <summary>حلقهٔ تعاملی را تا دریافت فرمان خروج یا پایان جریان ورودی اجرا می‌کند.</summary>
    public void Run(bool printTokens = false, bool printAst = false, bool printSource = false,
        bool optimizeAst = false)
    {
        _output.WriteLine("Benita REPL 0.5.0");
        _output.WriteLine("Type :help for commands or :exit to quit.");

        while (true)
        {
            string? input = ReadSubmission();
            if (input is null) return;
            if (string.IsNullOrWhiteSpace(input)) continue;
            string command = input.Trim();
            if (command is ":exit" or ":quit") return;
            if (TryHandleCommand(command)) continue;

            string source = PrepareSource(input);
            string candidateSession = _sessionSource + Environment.NewLine + source;
            try
            {
                _compiler.ExecInSession(source, candidateSession, _interpreter,
                    printTokens, printAst, printSource, optimizeAst);
                _sessionSource.AppendLine(source);
                _history.Add(input);
            }
            catch (BenitaException exception)
            {
                _output.WriteLine(exception.Message);
            }
            catch (Exception exception)
            {
                _output.WriteLine(new RuntimeException(exception.Message, exception).Message);
            }
        }
    }

    private string? ReadSubmission()
    {
        var submission = new StringBuilder();
        while (true)
        {
            _output.Write(submission.Length == 0 ? "benita> " : "   ...> ");
            string? line = _input.ReadLine();
            if (line is null) return submission.Length == 0 ? null : submission.ToString();
            submission.AppendLine(line);

            string text = submission.ToString();
            if (text.TrimStart().StartsWith(':') || IsComplete(text))
                return text.TrimEnd();
        }
    }

    private bool TryHandleCommand(string command)
    {
        switch (command.ToLowerInvariant())
        {
            case ":exit":
            case ":quit":
                return true;
            case ":help":
                _output.WriteLine(":help     Show REPL commands");
                _output.WriteLine(":history  Show successful submissions");
                _output.WriteLine(":clear    Clear the console");
                _output.WriteLine(":reset    Clear variables, functions, and history");
                _output.WriteLine(":exit     Exit the REPL");
                return true;
            case ":history":
                for (int index = 0; index < _history.Count; index++)
                    _output.WriteLine($"{index + 1}: {_history[index].ReplaceLineEndings(" ")}");
                return true;
            case ":clear":
                if (ReferenceEquals(_output, Console.Out) && !Console.IsOutputRedirected) Console.Clear();
                return true;
            case ":reset":
                _history.Clear();
                _sessionSource.Clear();
                _compiler = new CompilerClass();
                _interpreter = new Interpreter(preserveStateBetweenPrograms: true);
                _output.WriteLine("Session reset.");
                return true;
            default:
                if (command.StartsWith(':'))
                {
                    _output.WriteLine($"Unknown REPL command '{command}'. Type :help for commands.");
                    return true;
                }
                return false;
        }
    }

    private static string PrepareSource(string input)
    {
        string trimmed = input.Trim();
        if (!trimmed.EndsWith(';') && !trimmed.EndsWith('}'))
            return $"print({trimmed});";
        return input;
    }

    private static bool IsComplete(string source)
    {
        int braces = 0, parentheses = 0, brackets = 0;
        bool inString = false, escaped = false, lineComment = false, blockComment = false;
        for (int index = 0; index < source.Length; index++)
        {
            char current = source[index];
            char next = index + 1 < source.Length ? source[index + 1] : '\0';
            if (lineComment) { if (current == '\n') lineComment = false; continue; }
            if (blockComment) { if (current == '*' && next == '/') { blockComment = false; index++; } continue; }
            if (inString)
            {
                if (escaped) escaped = false;
                else if (current == '\\') escaped = true;
                else if (current == '"') inString = false;
                continue;
            }
            if (current == '/' && next == '/') { lineComment = true; index++; continue; }
            if (current == '/' && next == '*') { blockComment = true; index++; continue; }
            if (current == '"') { inString = true; continue; }
            if (current == '{') braces++;
            else if (current == '}') braces--;
            else if (current == '(') parentheses++;
            else if (current == ')') parentheses--;
            else if (current == '[') brackets++;
            else if (current == ']') brackets--;
        }

        if (inString || blockComment || braces > 0 || parentheses > 0 || brackets > 0) return false;
        string trimmed = source.TrimEnd();
        return trimmed.EndsWith(';') || trimmed.EndsWith('}') ||
               braces == 0 && parentheses == 0 && brackets == 0 && !trimmed.Contains('\n');
    }
}
