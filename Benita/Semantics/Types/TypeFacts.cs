namespace Benita;

/// <summary>تبدیل نام نوع و قواعد سازگاری نوع‌ها را در یک محل نگه می‌دارد.</summary>
public static class TypeFacts
{
    public static TypeSymbol FromName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return Types.Unknown;
        if (name.EndsWith("[]", StringComparison.Ordinal))
            return Types.ArrayOf(FromName(name[..^2]));

        return name switch
        {
            "number" => Types.Number,
            "string" => Types.String,
            "bool" => Types.Bool,
            "void" => Types.Void,
            "let" => Types.Inferred,
            "array" => Types.AnyArray,
            "any" => Types.Any,
            "unknown" => Types.Unknown,
            _ => new NamedTypeSymbol(name)
        };
    }

    public static TypeSymbol FromToken(TokenType tokenType) => tokenType switch
    {
        TokenType.NUMBER_LITERAL or TokenType.NUMBER => Types.Number,
        TokenType.STRING_LITERAL or TokenType.STRING => Types.String,
        TokenType.FALSE_LITERAL or TokenType.TRUE_LITERAL or TokenType.BOOL => Types.Bool,
        TokenType.VOID => Types.Void,
        TokenType.LET => Types.Inferred,
        _ => throw new ArgumentOutOfRangeException(nameof(tokenType), tokenType,
            "The token does not represent a language type.")
    };

    public static bool IsAssignableTo(TypeSymbol source, TypeSymbol target)
    {
        if (source == target || source == Types.Unknown || target == Types.Unknown) return true;
        if (target == Types.Any) return source != Types.Void;
        if (target == Types.AnyArray) return source is ArrayTypeSymbol || source == Types.AnyArray;
        if (source == Types.AnyArray) return target is ArrayTypeSymbol || target == Types.AnyArray;

        return source is ArrayTypeSymbol sourceArray &&
               target is ArrayTypeSymbol targetArray &&
               IsAssignableTo(sourceArray.ElementType, targetArray.ElementType);
    }

    public static bool AreEquivalent(TypeSymbol first, TypeSymbol second) =>
        IsAssignableTo(first, second) && IsAssignableTo(second, first);
}
