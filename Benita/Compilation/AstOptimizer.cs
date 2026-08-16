using System.Globalization;

namespace Benita
{
    /// <summary>
    /// Produces an optimized copy of an AST without changing the source program's semantics.
    /// </summary>
    public sealed class AstOptimizer
    {
        public ProgramNode Optimize(ProgramNode program)
        {
            ArgumentNullException.ThrowIfNull(program);

            return new ProgramNode(
                program.GlobalVariables.Select(OptimizeVariable).ToList(),
                program.Packages.Select(OptimizePackage).ToList(),
                program.Functions.Select(OptimizeFunction).ToList(),
                program.MainFunction is null ? null : OptimizeFunction(program.MainFunction),
                program.Statements?.Select(OptimizeStatement).ToList(),
                program.Interfaces);
        }

        private VariableDeclarationNode OptimizeVariable(VariableDeclarationNode node) =>
            new(node.Type, node.Name, OptimizeExpression(node.Initializer), node.AccessModifier);

        private PackageNode OptimizePackage(PackageNode node) =>
            new(node.Name, node.Members.Select(OptimizePackageMember).ToList(), node.Interfaces);

        private PackageMemberNode OptimizePackageMember(PackageMemberNode node) => node switch
        {
            PackageVariableDeclarationNode variable => new PackageVariableDeclarationNode(
                variable.Type, variable.Name, OptimizeExpression(variable.Initializer), variable.AccessModifier),
            PackageFunctionNode function => new PackageFunctionNode(
                function.Name,
                function.Parameters,
                function.ReturnType,
                OptimizeBlock(function.Body),
                OptimizeReturn(function.ReturnStatement),
                function.AccessModifier),
            _ => throw new InvalidOperationException($"Unsupported package member '{node.GetType().Name}'.")
        };

        private FunctionNode OptimizeFunction(FunctionNode node) =>
            new(
                node.Name,
                node.Parameters,
                node.ReturnType,
                OptimizeBlock(node.Body),
                OptimizeReturn(node.ReturnStatement),
                node.AccessModifier);

        private BlockNode OptimizeBlock(BlockNode node) =>
            new(node.Statements.Select(statement => OptimizeStatement(statement)!).ToList());

        private StatementNode? OptimizeStatement(StatementNode? node)
        {
            return node switch
            {
                null => null,
                VariableDeclarationNode variable => OptimizeVariable(variable),
                AssignmentNode assignment => new AssignmentNode(
                    assignment.Name, OptimizeExpression(assignment.Expression)),
                CompoundAssignmentNode assignment => new CompoundAssignmentNode(
                    assignment.Name, assignment.Operator, OptimizeExpression(assignment.Expression)),
                ArrayAssignmentNode assignment => new ArrayAssignmentNode(
                    assignment.Name,
                    OptimizeExpression(assignment.Index),
                    OptimizeExpression(assignment.Value)),
                IncrementDecrementNode increment => increment,
                ExpressionStatementNode expression => new ExpressionStatementNode(
                    OptimizeExpression(expression.Expression)),
                IfStatementNode ifStatement => OptimizeIf(ifStatement),
                MatchStatementNode match => new MatchStatementNode(
                    OptimizeExpression(match.Value)!, OptimizeMatchArms(match.Arms)),
                WhileStatementNode whileStatement => OptimizeWhile(whileStatement),
                ForStatementNode forStatement => new ForStatementNode(
                    OptimizeStatement(forStatement.Initializer),
                    OptimizeExpression(forStatement.Condition),
                    OptimizeStatement(forStatement.Increment),
                    OptimizeStatement(forStatement.Body)),
                ForEachStatementNode forEach => new ForEachStatementNode(
                    forEach.VariableName,
                    OptimizeExpression(forEach.Iterable)!,
                    OptimizeStatement(forEach.Body)!),
                BlockNode block => OptimizeBlock(block),
                ReturnStatementNode returnStatement => OptimizeReturn(returnStatement),
                ThrowStatementNode throwStatement => new ThrowStatementNode(
                    OptimizeExpression(throwStatement.Error)!),
                TryStatementNode tryStatement => new TryStatementNode(
                    OptimizeBlock(tryStatement.TryBlock),
                    tryStatement.CatchVariable,
                    tryStatement.CatchBlock is null ? null : OptimizeBlock(tryStatement.CatchBlock),
                    tryStatement.FinallyBlock is null ? null : OptimizeBlock(tryStatement.FinallyBlock)),
                ObjectInstantiationNode creation => new ObjectInstantiationNode(
                    creation.Name,
                    creation.PackageName,
                    creation.Arguments.Select(argument => OptimizeExpression(argument)!).ToList()),
                BreakStatementNode or ContinueStatementNode => node,
                _ => throw new InvalidOperationException($"Unsupported statement '{node.GetType().Name}'.")
            };
        }

        private StatementNode OptimizeIf(IfStatementNode node)
        {
            ExpressionNode? condition = OptimizeExpression(node.Condition);
            StatementNode? thenBranch = OptimizeStatement(node.ThenBranch);
            StatementNode? elseBranch = OptimizeStatement(node.ElseBranch);

            if (TryGetBoolean(condition, out bool value))
            {
                return value
                    ? thenBranch ?? new BlockNode(new List<StatementNode>())
                    : elseBranch ?? new BlockNode(new List<StatementNode>());
            }

            return new IfStatementNode(condition, thenBranch, elseBranch);
        }

        private StatementNode OptimizeWhile(WhileStatementNode node)
        {
            ExpressionNode? condition = OptimizeExpression(node.Condition);
            if (TryGetBoolean(condition, out bool value) && !value)
            {
                return new BlockNode(new List<StatementNode>());
            }

            return new WhileStatementNode(condition, OptimizeStatement(node.Body));
        }

        private ReturnStatementNode? OptimizeReturn(ReturnStatementNode? node) =>
            node is null ? null : new ReturnStatementNode(OptimizeExpression(node.ReturnExpression));

        private ExpressionNode? OptimizeExpression(ExpressionNode? node)
        {
            return node switch
            {
                null => null,
                LiteralNode or IdentifierNode => node,
                BinaryExpressionNode binary => OptimizeBinary(binary),
                LogicalExpressionNode logical => OptimizeLogical(logical),
                UnaryExpressionNode unary => OptimizeUnary(unary),
                FunctionCallNode call => new FunctionCallNode(
                    call.FunctionName,
                    call.Arguments.Select(argument => OptimizeExpression(argument)!).ToList()),
                ArrayAccessNode access => new ArrayAccessNode(
                    access.Name, OptimizeExpression(access.Index)),
                ArrayInitializerNode array => new ArrayInitializerNode(
                    array.Elements.Select(element => OptimizeExpression(element)!).ToList(),
                    OptimizeExpression(array.SizeExpression)!),
                MemberAccessNode member => new MemberAccessNode(
                    member.ObjectName, OptimizeMemberExpression(member.Expression)),
                NewExpressionNode creation => new NewExpressionNode(
                    creation.PackageName,
                    creation.Arguments.Select(argument => OptimizeExpression(argument)!).ToList()),
                MatchExpressionNode match => new MatchExpressionNode(
                    OptimizeExpression(match.Value)!, OptimizeMatchArms(match.Arms)),
                _ => throw new InvalidOperationException($"Unsupported expression '{node.GetType().Name}'.")
            };
        }

        private AstNode? OptimizeMemberExpression(AstNode? node) => node switch
        {
            null => null,
            ExpressionNode expression => OptimizeExpression(expression),
            StatementNode statement => OptimizeStatement(statement),
            _ => node
        };

        private List<MatchArm> OptimizeMatchArms(IEnumerable<MatchArm> arms) => arms
            .Select(arm => new MatchArm(
                arm.Patterns.Select(OptimizeMatchPattern).ToList(),
                arm.Body is ExpressionNode expression
                    ? OptimizeExpression(expression)!
                    : OptimizeStatement((StatementNode)arm.Body)!,
                arm.IsDefault))
            .ToList();

        private MatchPatternNode OptimizeMatchPattern(MatchPatternNode pattern) => pattern switch
        {
            ValueMatchPatternNode value => new ValueMatchPatternNode(OptimizeExpression(value.Value)!),
            RangeMatchPatternNode range => new RangeMatchPatternNode(
                OptimizeExpression(range.Start)!, OptimizeExpression(range.End)!),
            _ => throw new InvalidOperationException($"Unsupported match pattern '{pattern.GetType().Name}'.")
        };

        private ExpressionNode OptimizeBinary(BinaryExpressionNode node)
        {
            ExpressionNode left = OptimizeExpression(node.Left)!;
            ExpressionNode right = OptimizeExpression(node.Right)!;

            if (left is not LiteralNode leftLiteral || right is not LiteralNode rightLiteral)
            {
                return new BinaryExpressionNode(left, node.Operator, right);
            }

            if (TryGetNumber(leftLiteral, out double leftNumber) &&
                TryGetNumber(rightLiteral, out double rightNumber))
            {
                return FoldNumbers(leftNumber, node.Operator, rightNumber)
                    ?? new BinaryExpressionNode(left, node.Operator, right);
            }

            if (node.Operator == "+" && IsString(leftLiteral) && IsString(rightLiteral))
            {
                return new LiteralNode(leftLiteral.Value + rightLiteral.Value, TokenType.STRING_LITERAL);
            }

            return new BinaryExpressionNode(left, node.Operator, right);
        }

        private ExpressionNode OptimizeLogical(LogicalExpressionNode node)
        {
            ExpressionNode left = OptimizeExpression(node.Left)!;

            if (TryGetBoolean(left, out bool leftValue))
            {
                if (node.Operator == "&&" && !leftValue || node.Operator == "||" && leftValue)
                {
                    return BooleanLiteral(leftValue);
                }

                if (node.Operator is "&&" or "||")
                {
                    return OptimizeExpression(node.Right)!;
                }
            }

            ExpressionNode right = OptimizeExpression(node.Right)!;
            if (TryGetBoolean(left, out leftValue) && TryGetBoolean(right, out bool rightValue))
            {
                return BooleanLiteral(node.Operator == "&&" ? leftValue && rightValue : leftValue || rightValue);
            }

            return new LogicalExpressionNode(left, node.Operator, right);
        }

        private ExpressionNode OptimizeUnary(UnaryExpressionNode node)
        {
            ExpressionNode operand = OptimizeExpression(node.Operand)!;
            if (node.Operator == "-" && operand is LiteralNode literal && TryGetNumber(literal, out double number))
            {
                return NumberLiteral(-number);
            }

            if (node.Operator == "!" && TryGetBoolean(operand, out bool value))
            {
                return BooleanLiteral(!value);
            }

            return new UnaryExpressionNode(node.Operator, operand);
        }

        private static ExpressionNode? FoldNumbers(double left, string operation, double right)
        {
            return operation switch
            {
                "+" => NumberLiteral(left + right),
                "-" => NumberLiteral(left - right),
                "*" => NumberLiteral(left * right),
                "/" when right != 0 => NumberLiteral(left / right),
                "%" when right != 0 => NumberLiteral(left % right),
                "==" => BooleanLiteral(left == right),
                "!=" => BooleanLiteral(left != right),
                "<" => BooleanLiteral(left < right),
                ">" => BooleanLiteral(left > right),
                "<=" => BooleanLiteral(left <= right),
                ">=" => BooleanLiteral(left >= right),
                _ => null
            };
        }

        private static bool TryGetNumber(LiteralNode node, out double value)
        {
            if (node.Type is TokenType.NUMBER or TokenType.NUMBER_LITERAL)
            {
                return double.TryParse(node.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
            }

            value = default;
            return false;
        }

        private static bool TryGetBoolean(ExpressionNode? node, out bool value)
        {
            if (node is LiteralNode { Type: TokenType.TRUE_LITERAL })
            {
                value = true;
                return true;
            }

            if (node is LiteralNode { Type: TokenType.FALSE_LITERAL })
            {
                value = false;
                return true;
            }

            value = default;
            return false;
        }

        private static bool IsString(LiteralNode node) =>
            node.Type is TokenType.STRING or TokenType.STRING_LITERAL;

        private static LiteralNode NumberLiteral(double value) =>
            new(value.ToString("R", CultureInfo.InvariantCulture), TokenType.NUMBER_LITERAL);

        private static LiteralNode BooleanLiteral(bool value) =>
            new(value ? "true" : "false", value ? TokenType.TRUE_LITERAL : TokenType.FALSE_LITERAL);
    }
}
