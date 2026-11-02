namespace Benita;

/// <summary>
/// A diagnostic produced by a compiler phase. Diagnostics are deliberately independent
/// from the phase that produced them so callers can inspect results without parsing
/// exception messages.
/// </summary>
public sealed record CompilationDiagnostic(
    string Code,
    string Description,
    SourceSpan Span,
    CompilationDiagnosticSeverity Severity = CompilationDiagnosticSeverity.Error);

public enum CompilationDiagnosticSeverity
{
    Info,
    Warning,
    Error
}

/// <summary>Explicit output contract of the syntax phase.</summary>
public sealed record ParseResult(
    ProgramNode? Program,
    IReadOnlyList<CompilationDiagnostic> Diagnostics)
{
    public bool IsSuccess => Program is not null &&
        Diagnostics.All(diagnostic => diagnostic.Severity != CompilationDiagnosticSeverity.Error);
}

/// <summary>Explicit output contract of the semantic phase.</summary>
public sealed record SemanticAnalysisResult(
    ProgramNode Program,
    IReadOnlyList<CompilationDiagnostic> Diagnostics)
{
    public bool IsSuccess => Diagnostics.All(diagnostic =>
        diagnostic.Severity != CompilationDiagnosticSeverity.Error);
}
