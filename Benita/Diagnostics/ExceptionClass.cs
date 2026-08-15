namespace Benita;

public readonly record struct SourceSpan(
    string? FileName,
    int Line,
    int Column,
    int Length,
    string? LineText)
{
    public static SourceSpan Unknown => new(null, 0, 0, 0, null);
}

public abstract class BenitaException : Exception
{
    protected BenitaException(string code, string description, SourceSpan span = default, Exception? innerException = null)
        : base(FormatMessage(code, description, span), innerException)
    {
        Code = code;
        Description = description;
        Span = span;
    }

    public string Code { get; }
    public string Description { get; }
    public SourceSpan Span { get; }

    private static string FormatMessage(string code, string description, SourceSpan span)
    {
        if (span.Line <= 0)
            return $"{code}: {description}";

        var location = string.IsNullOrWhiteSpace(span.FileName)
            ? $"line {span.Line}, column {span.Column}"
            : $"{span.FileName}:{span.Line}:{span.Column}";
        if (span.LineText == null)
            return $"{code}: {description}{Environment.NewLine}  --> {location}";

        var gutter = span.Line.ToString();
        var markerLength = Math.Max(1, span.Length);
        var marker = new string(' ', Math.Max(0, span.Column - 1)) + new string('^', markerLength);
        return $"{code}: {description}{Environment.NewLine}" +
               $"  --> {location}{Environment.NewLine}" +
               $"   |{Environment.NewLine}" +
               $"{gutter,3} | {span.LineText}{Environment.NewLine}" +
               $"   | {marker}";
    }
}

public sealed class LexerException(string code, string description, SourceSpan span, Exception? innerException = null)
    : BenitaException(code, description, span, innerException);

public sealed class ParserException(string code, string description, SourceSpan span)
    : BenitaException(code, description, span);

public sealed class SemanticException(string description, Exception? innerException = null)
    : BenitaException("BEN3001", description, innerException: innerException);

public sealed class RuntimeException(string description, Exception? innerException = null)
    : BenitaException("BEN4001", description, innerException: innerException);

/// <summary>خطای پایدار مربوط به اعتبارسنجی یا اجرای توابع داخلی زبان.</summary>
public sealed class BuiltInException(string description, Exception? innerException = null)
    : BenitaException("BEN4101", description, innerException: innerException);

public class BreakException() : Exception("Break statement encountered.");

public class ContinueException() : Exception("Continue statement encountered.");
