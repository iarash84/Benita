namespace Benita;

/// <summary>درخت نحو انتزاعی را به‌شکل سلسله‌مراتبی در خروجی استاندارد نمایش می‌دهد.</summary>
internal static class AstPrinter
{
    /// <summary>یک گره و تمام فرزندان قابل‌نمایش آن را چاپ می‌کند.</summary>
    public static void Print(AstNode? node, string indent = "")
    {
        if (node is null) return;
        Console.WriteLine($"{indent}{node.GetType().Name}");
        string child = indent + "  ";
        switch (node)
        {
            case ProgramNode value: PrintAll(value.Interfaces, child); PrintAll(value.GlobalVariables, child); PrintAll(value.Packages, child); PrintAll(value.Functions, child); Print(value.MainFunction, child); PrintAll(value.Statements, child); break;
            case InterfaceNode value: Detail(child, "Name", value.Name); PrintAll(value.Methods, child); break;
            case InterfaceMethodNode value: Detail(child, "Method", $"{value.Name} -> {value.ReturnType}"); PrintAll(value.Parameters, child); break;
            case PackageNode value: Detail(child, "Name", value.Interfaces.Count == 0 ? value.Name : $"{value.Name} : {string.Join(", ", value.Interfaces)}"); PrintAll(value.Members, child); break;
            case FunctionNode value: Detail(child, "Name", $"{value.AccessModifier.ToString().ToLowerInvariant()} {value.Name}"); PrintAll(value.Parameters, child); Print(value.Body, child); Print(value.ReturnStatement, child); break;
            case PackageFunctionNode value: Detail(child, "Name", $"{value.AccessModifier.ToString().ToLowerInvariant()} {value.Name}"); PrintAll(value.Parameters, child); Print(value.Body, child); Print(value.ReturnStatement, child); break;
            case BlockNode value: PrintAll(value.Statements, child); break;
            case VariableDeclarationNode value: Detail(child, "Variable", $"{value.AccessModifier.ToString().ToLowerInvariant()} {value.Type} {value.Name}"); Print(value.Initializer, child); break;
            case PackageVariableDeclarationNode value: Detail(child, "Variable", $"{value.AccessModifier.ToString().ToLowerInvariant()} {value.Type} {value.Name}"); Print(value.Initializer, child); break;
            case ParameterNode value: Detail(child, "Parameter", $"{value.Type} {value.Name}"); break;
            case LiteralNode value: Detail(child, "Value", value.Value); break;
            case IdentifierNode value: Detail(child, "Name", value.Name); break;
            case AssignmentNode value: Detail(child, "Name", value.Name); Print(value.Expression, child); break;
            case CompoundAssignmentNode value: Detail(child, "Assignment", $"{value.Name} {value.Operator}"); Print(value.Expression, child); break;
            case IncrementDecrementNode value: Detail(child, "Expression", $"{value.Name}{value.Operator}"); break;
            case BinaryExpressionNode value: Detail(child, "Operator", value.Operator); Print(value.Left, child); Print(value.Right, child); break;
            case LogicalExpressionNode value: Detail(child, "Operator", value.Operator); Print(value.Left, child); Print(value.Right, child); break;
            case UnaryExpressionNode value: Detail(child, "Operator", value.Operator); Print(value.Operand, child); break;
            case ExpressionStatementNode value: Print(value.Expression, child); break;
            case FunctionCallNode value: Detail(child, "Function", value.FunctionName); PrintAll(value.Arguments, child); break;
            case ReturnStatementNode value: Print(value.ReturnExpression, child); break;
            case IfStatementNode value: Print(value.Condition, child); Print(value.ThenBranch, child); Print(value.ElseBranch, child); break;
            case MatchExpressionNode value: Print(value.Value, child); PrintMatchArms(value.Arms, child); break;
            case MatchStatementNode value: Print(value.Value, child); PrintMatchArms(value.Arms, child); break;
            case WhileStatementNode value: Print(value.Condition, child); Print(value.Body, child); break;
            case ForStatementNode value: Print(value.Initializer, child); Print(value.Condition, child); Print(value.Increment, child); Print(value.Body, child); break;
            case ForEachStatementNode value: Detail(child, "Variable", value.VariableName); Print(value.Iterable, child); Print(value.Body, child); break;
            case ArrayInitializerNode value: PrintAll(value.Elements, child); break;
            case ArrayAccessNode value: Detail(child, "Array", value.Name); Print(value.Index, child); break;
            case ArrayAssignmentNode value: Detail(child, "Array", value.Name); Print(value.Index, child); Print(value.Value, child); break;
            case MemberAccessNode value: Detail(child, "Object", value.ObjectName); Print(value.Expression, child); break;
            case ObjectInstantiationNode value: Detail(child, "Instance", $"{value.Name}: {value.PackageName}"); PrintAll(value.Arguments, child); break;
            case NewExpressionNode value: Detail(child, "New", value.PackageName); PrintAll(value.Arguments, child); break;
        }
    }

    private static void PrintAll<T>(IEnumerable<T?> nodes, string indent) where T : AstNode
    {
        foreach (T? node in nodes) Print(node, indent);
    }

    private static void PrintMatchArms(IEnumerable<MatchArm> arms, string indent)
    {
        foreach (MatchArm arm in arms)
        {
            Console.WriteLine($"{indent}MatchArm{(arm.IsDefault ? " (default)" : string.Empty)}");
            foreach (MatchPatternNode pattern in arm.Patterns)
            {
                switch (pattern)
                {
                    case ValueMatchPatternNode value: Print(value.Value, indent + "  "); break;
                    case RangeMatchPatternNode range: Print(range.Start, indent + "  "); Print(range.End, indent + "  "); break;
                }
            }
            Print(arm.Body, indent + "  ");
        }
    }

    private static void Detail(string indent, string label, string? value) =>
        Console.WriteLine($"{indent}{label}: {value}");
}
