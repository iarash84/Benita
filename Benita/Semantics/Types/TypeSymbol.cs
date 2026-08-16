namespace Benita;

/// <summary>نمایش معنایی یک نوع در زبان است و به متن خام source وابسته نیست.</summary>
public abstract record TypeSymbol(string Name)
{
    public sealed override string ToString() => Name;
}

/// <summary>یکی از نوع‌های پایه و از پیش تعریف‌شدهٔ زبان را نشان می‌دهد.</summary>
public sealed record PrimitiveTypeSymbol(string TypeName) : TypeSymbol(TypeName);

/// <summary>نوع یک آرایه را همراه نوع عناصر آن نگه می‌دارد.</summary>
public sealed record ArrayTypeSymbol(TypeSymbol ElementType) : TypeSymbol($"{ElementType.Name}[]");

/// <summary>نوع نام‌دار تعریف‌شده توسط کاربر، مانند package، را نشان می‌دهد.</summary>
public sealed record NamedTypeSymbol(string TypeName) : TypeSymbol(TypeName);

/// <summary>نوع‌های داخلی موردنیاز تحلیل‌گر را که مستقیماً در source نوشته نمی‌شوند نشان می‌دهد.</summary>
public sealed record SpecialTypeSymbol(string TypeName) : TypeSymbol(TypeName);

/// <summary>نمونه‌های مشترک نوع‌ها را در اختیار بخش‌های مختلف تحلیل معنایی قرار می‌دهد.</summary>
public static class Types
{
    public static readonly TypeSymbol Number = new PrimitiveTypeSymbol("number");
    public static readonly TypeSymbol String = new PrimitiveTypeSymbol("string");
    public static readonly TypeSymbol Bool = new PrimitiveTypeSymbol("bool");
    public static readonly TypeSymbol Void = new PrimitiveTypeSymbol("void");
    public static readonly TypeSymbol Error = new PrimitiveTypeSymbol("error");

    public static readonly TypeSymbol Unknown = new SpecialTypeSymbol("unknown");
    public static readonly TypeSymbol Inferred = new SpecialTypeSymbol("let");
    public static readonly TypeSymbol Any = new SpecialTypeSymbol("any");
    public static readonly TypeSymbol AnyArray = new SpecialTypeSymbol("array");

    public static ArrayTypeSymbol ArrayOf(TypeSymbol elementType) => new(elementType);
}
