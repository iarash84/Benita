namespace Benita;

/// <summary>ریشهٔ نمایش bound برنامه را نگه می‌دارد، بدون آن‌که AST خام را تغییر دهد.</summary>
public sealed class BoundProgram
{
    internal BoundProgram(ProgramNode syntaxTree, BindingResult bindings)
    {
        SyntaxTree = syntaxTree;
        Bindings = bindings;
    }

    public ProgramNode SyntaxTree { get; }
    public BindingResult Bindings { get; }

    /// <summary>همهٔ referenceهایی را که Binder توانسته به symbol متصل کند برمی‌گرداند.</summary>
    public IReadOnlyDictionary<AstNode, Symbol> ResolvedReferences => Bindings.References;

    public bool TryBindIdentifier(IdentifierNode identifier, out BoundIdentifier? bound)
    {
        if (Bindings.TryGetSymbol(identifier, out Symbol? symbol))
        {
            bound = new BoundIdentifier(identifier, symbol!);
            return true;
        }

        bound = null;
        return false;
    }

    public bool TryBindFunctionCall(FunctionCallNode call, out BoundFunctionCall? bound)
    {
        if (Bindings.TryGetSymbol(call, out Symbol? symbol) && symbol is FunctionSymbol function)
        {
            bound = new BoundFunctionCall(call, function);
            return true;
        }

        bound = null;
        return false;
    }
}

/// <summary>یک identifier را همراه با symbol نهایی آن نمایش می‌دهد.</summary>
public sealed record BoundIdentifier(IdentifierNode Syntax, Symbol Symbol)
{
    public TypeSymbol? Type => Symbol.Type;
}

/// <summary>یک فراخوانی تابع را همراه با امضای resolve‌شدهٔ آن نمایش می‌دهد.</summary>
public sealed record BoundFunctionCall(FunctionCallNode Syntax, FunctionSymbol Function)
{
    public TypeSymbol ReturnType => Function.ReturnType;
    public IReadOnlyList<TypeSymbol> ParameterTypes => Function.ParameterTypes;
}

/// <summary>ساخت نمایش bound از نتیجهٔ Binder را متمرکز می‌کند.</summary>
public static class BoundProgramFactory
{
    public static BoundProgram Create(ProgramNode program, BindingResult? bindings = null)
    {
        ArgumentNullException.ThrowIfNull(program);
        BindingResult result = bindings ?? new Binder().Bind(program);
        return new BoundProgram(program, result);
    }
}
