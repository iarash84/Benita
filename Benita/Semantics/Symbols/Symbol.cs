namespace Benita;

/// <summary>دسته‌بندی نمادهای معنایی زبان را مشخص می‌کند.</summary>
public enum SymbolKind
{
    Variable,
    Parameter,
    Function,
    Package,
    Interface,
    BuiltInType
}

/// <summary>قرارداد پایهٔ همهٔ نمادهای قابل resolve در تحلیل معنایی.</summary>
public abstract record Symbol(string Name, SymbolKind Kind)
{
    public abstract TypeSymbol? Type { get; }
}

/// <summary>نماد متغیر محلی، سراسری، field یا پارامتر را نمایش می‌دهد.</summary>
public sealed record VariableSymbol(string Name, TypeSymbol VariableType, bool IsMutable = true)
    : Symbol(Name, SymbolKind.Variable)
{
    public override TypeSymbol Type => VariableType;
}

/// <summary>نماد پارامتر تابع را از متغیر عادی متمایز می‌کند.</summary>
public sealed record ParameterSymbol(string Name, TypeSymbol ParameterType)
    : Symbol(Name, SymbolKind.Parameter)
{
    public override TypeSymbol Type => ParameterType;
}

/// <summary>نماد یک تابع را همراه با نوع خروجی و پارامترهای معنایی آن نمایش می‌دهد.</summary>
public sealed record FunctionSymbol(
    string Name,
    TypeSymbol ReturnType,
    IReadOnlyList<TypeSymbol> ParameterTypes,
    FunctionNode? Declaration = null)
    : Symbol(Name, SymbolKind.Function)
{
    public override TypeSymbol Type => ReturnType;
}

/// <summary>نماد یک package یا interface را در namespace نوع‌ها نمایش می‌دهد.</summary>
public sealed record TypeDeclarationSymbol(
    string Name,
    SymbolKind DeclarationKind,
    TypeSymbol DeclaredType,
    AstNode? Declaration = null)
    : Symbol(Name, DeclarationKind)
{
    public override TypeSymbol Type => DeclaredType;
}

/// <summary>نماد یک built-in type را از typeهای تعریف‌شده توسط کاربر متمایز می‌کند.</summary>
public sealed record BuiltInTypeSymbol(
    string Name,
    TypeSymbol DeclaredType,
    BuiltInClassDefinition? Definition = null)
    : Symbol(Name, SymbolKind.BuiltInType)
{
    public override TypeSymbol Type => DeclaredType;
}

/// <summary>دامنهٔ زنجیره‌ای نام‌ها را نگه می‌دارد و shadowing را پشتیبانی می‌کند.</summary>
public sealed class Scope
{
    private readonly Dictionary<string, Symbol> _symbols = new(StringComparer.Ordinal);

    public Scope(Scope? parent = null, string? name = null)
    {
        Parent = parent;
        Name = name ?? "scope";
    }

    public Scope? Parent { get; }
    public string Name { get; }
    public IReadOnlyDictionary<string, Symbol> Symbols => _symbols;

    /// <summary>دسترسی سازگار به نوع نماد برای مسیرهای تحلیل قدیمی و جدید.</summary>
    public string? this[string name]
    {
        get => TryGetValue(name, out string? type) ? type : null;
        set
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));
            Declare(new VariableSymbol(name, TypeFacts.FromName(value)));
        }
    }

    /// <summary>نام را در زنجیرهٔ scopeها پیدا و نوع معنایی آن را برمی‌گرداند.</summary>
    public bool TryGetValue(string name, out string? type)
    {
        if (TryLookup(name, out Symbol? symbol))
        {
            type = symbol.Type?.Name;
            return true;
        }
        type = null;
        return false;
    }

    /// <summary>همهٔ declarationهای scope فعلی را پاک می‌کند.</summary>
    public void Clear() => _symbols.Clear();

    /// <summary>نماد را فقط در scope فعلی ثبت می‌کند؛ declaration تکراری مجاز نیست.</summary>
    public void Declare(Symbol symbol)
    {
        ArgumentNullException.ThrowIfNull(symbol);
        if (!_symbols.TryAdd(symbol.Name, symbol))
            throw new InvalidOperationException($"Symbol '{symbol.Name}' is already declared in scope '{Name}'.");
    }

    /// <summary>نماد موجود در scope فعلی را پس از inference یا تکمیل declaration جایگزین می‌کند.</summary>
    public void ReplaceLocal(Symbol symbol)
    {
        ArgumentNullException.ThrowIfNull(symbol);
        if (!_symbols.ContainsKey(symbol.Name))
            throw new InvalidOperationException($"Symbol '{symbol.Name}' is not declared in scope '{Name}'.");
        _symbols[symbol.Name] = symbol;
    }

    /// <summary>ابتدا scope فعلی و سپس parentها را برای نام جست‌وجو می‌کند.</summary>
    public bool TryLookup(string name, out Symbol? symbol)
    {
        if (_symbols.TryGetValue(name, out symbol))
            return true;
        if (Parent is not null)
            return Parent.TryLookup(name, out symbol);
        symbol = null;
        return false;
    }

    /// <summary>فقط declarationهای scope فعلی را جست‌وجو می‌کند.</summary>
    public bool TryLookupLocal(string name, out Symbol? symbol) => _symbols.TryGetValue(name, out symbol);
}
