namespace Benita
{
    /// <summary>
    /// Represents the base class for all nodes in the Abstract Syntax Tree (AST).
    /// </summary>
    public abstract class AstNode
    {
    }

    /// <summary>
    /// Represents the base class for all expression nodes in the AST.
    /// </summary>
    public abstract class ExpressionNode : AstNode
    {
    }

    /// <summary>
    /// Represents a literal node with a value and a type.
    /// </summary>
    public class LiteralNode(string value, TokenType type) : ExpressionNode
    {
        public string Value { get; } = value;
        public TokenType Type { get; } = type;
    }

    /// <summary>
    /// Represents an identifier node with an optional name.
    /// </summary>
    public class IdentifierNode(string name) : ExpressionNode
    {
        public string Name { get; } = name;
    }

    /// <summary>
    /// Represents a binary expression node with left and right expressions and an operator.
    /// </summary>
    public class BinaryExpressionNode(ExpressionNode? left, string op, ExpressionNode? right) : ExpressionNode
    {
        public ExpressionNode? Left { get; } = left;
        public string Operator { get; } = op;
        public ExpressionNode? Right { get; } = right;
    }

    /// <summary>
    /// Represents a logical expression node with left and right expressions and an operator.
    /// </summary>
    public class LogicalExpressionNode(ExpressionNode? left, string op, ExpressionNode? right) : ExpressionNode
    {
        public ExpressionNode? Left { get; } = left;
        public string Operator { get; } = op;
        public ExpressionNode? Right { get; } = right;
    }

    /// <summary>
    /// Represents a unary expression node with an operator and an operand.
    /// </summary>
    public class UnaryExpressionNode(string? op, ExpressionNode? operand) : ExpressionNode
    {
        public string? Operator { get; } = op;
        public ExpressionNode? Operand { get; } = operand;
    }

    /// <summary>
    /// Represents a function call node with a function name and arguments.
    /// </summary>
    public class FunctionCallNode(string? functionName, List<ExpressionNode?> arguments) : ExpressionNode
    {
        public string? FunctionName { get; } = functionName;
        public List<ExpressionNode?> Arguments { get; } = arguments;
    }

    /// <summary>
    /// Represents the base class for all statement nodes in the AST.
    /// </summary>
    public abstract class StatementNode : AstNode
    {
    }
    /// <summary>
    /// Represents a break statement
    /// </summary>
    public class BreakStatementNode : StatementNode
    {
    }

    /// <summary>
    /// Represents a continue statement
    /// </summary>
    public class ContinueStatementNode : StatementNode
    {
    }

    /// <summary>
    /// Represents a variable declaration node with a type, optional name, and optional initializer.
    /// </summary>
    public class VariableDeclarationNode(string? type, string name, ExpressionNode? initializer) : StatementNode
    {
        public string? Type { get; } = type;
        public string Name { get; } = name;
        public ExpressionNode? Initializer { get; } = initializer;
    }

    /// <summary>
    /// Represents a member access node with an object name and an expression.
    /// </summary>
    public class MemberAccessNode(string objectName, AstNode? expression) : ExpressionNode
    {
        public AstNode? Expression { get; set; } = expression;
        public string ObjectName { get; } = objectName;
    }

    /// <summary>
    /// Represents an object instantiation node with a name, package name, and arguments.
    /// </summary>
    public class ObjectInstantiationNode(string name, string? packageName, List<ExpressionNode?> arguments)
        : StatementNode
    {
        public string Name { get; } = name;
        public string? PackageName { get; } = packageName;
        public List<ExpressionNode?> Arguments { get; } = arguments;
    }

    /// <summary>
    /// Represents an assignment node with a name and an expression.
    /// </summary>
    public class AssignmentNode(string name, ExpressionNode? expression) : StatementNode
    {
        public string Name { get; } = name;
        public ExpressionNode? Expression { get; } = expression;
    }

    /// <summary>
    /// Represents an array assignment node with a name, index, and value.
    /// </summary>
    public class ArrayAssignmentNode(string name, ExpressionNode? index, ExpressionNode? value) : StatementNode
    {
        public string Name { get; } = name;
        public ExpressionNode? Index { get; } = index;
        public ExpressionNode? Value { get; } = value;
    }

    /// <summary>
    /// Represents a compound assignment node with a name, operator, and expression.
    /// </summary>
    public class CompoundAssignmentNode(string name, string op, ExpressionNode? expression) : StatementNode
    {
        public string Name { get; } = name;
        public string Operator { get; } = op;
        public ExpressionNode? Expression { get; } = expression;
    }

    /// <summary>
    /// Represents an increment or decrement node with a name and operator.
    /// </summary>
    public class IncrementDecrementNode(string name, string op) : StatementNode
    {
        public string Name { get; } = name;
        public string Operator { get; } = op;
    }

    /// <summary>
    /// Represents an if statement node with a condition, then branch, and optional else branch.
    /// </summary>
    public class IfStatementNode(ExpressionNode? condition, StatementNode? thenBranch, StatementNode? elseBranch)
        : StatementNode
    {
        public ExpressionNode? Condition { get; } = condition;
        public StatementNode? ThenBranch { get; } = thenBranch;
        public StatementNode? ElseBranch { get; } = elseBranch;
    }

    /// <summary>
    /// Represents a while statement node with a condition and body.
    /// </summary>
    public class WhileStatementNode(ExpressionNode? condition, StatementNode? body) : StatementNode
    {
        public ExpressionNode? Condition { get; } = condition;
        public StatementNode? Body { get; } = body;
    }

    /// <summary>
    /// Represents a for statement node with an initializer, condition, increment, and body.
    /// </summary>
    public class ForStatementNode(
        StatementNode? initializer,
        ExpressionNode? condition,
        StatementNode? increment,
        StatementNode? body) : StatementNode
    {
        public StatementNode? Initializer { get; } = initializer;
        public ExpressionNode? Condition { get; } = condition;
        public StatementNode? Increment { get; } = increment;
        public StatementNode? Body { get; } = body;
    }

    /// <summary>
    /// Represents a block node containing a list of statements.
    /// </summary>
    public class BlockNode(List<StatementNode?> statements) : StatementNode
    {
        public List<StatementNode?> Statements { get; } = statements;
    }

    /// <summary>
    /// Represents an expression statement node containing an expression.
    /// </summary>
    public class ExpressionStatementNode(ExpressionNode? expression) : StatementNode
    {
        public ExpressionNode? Expression { get; } = expression;
    }

    /// <summary>
    /// Represents a function node with a name, parameters, return type, body, and return statement.
    /// </summary>
    public class FunctionNode(
        string? name,
        List<ParameterNode> parameters,
        string? returnType,
        BlockNode? body,
        ReturnStatementNode? returnStatement) : AstNode
    {
        public string? Name { get; } = name;
        public List<ParameterNode> Parameters { get; } = parameters;
        public string? ReturnType { get; } = returnType;
        public BlockNode? Body { get; } = body;
        public ReturnStatementNode? ReturnStatement { get; } = returnStatement;
    }

    /// <summary>
    /// Represents a return statement node containing a return expression.
    /// </summary>
    public class ReturnStatementNode(ExpressionNode? returnExpression) : StatementNode
    {
        public ExpressionNode? ReturnExpression { get; } = returnExpression;
    }

    /// <summary>
    /// Represents a parameter node with a type and optional name.
    /// </summary>
    public class ParameterNode(string? type, string? name) : AstNode
    {
        public string? Type { get; } = type;
        public string? Name { get; } = name;
    }

    /// <summary>
    /// Represents a package node with a name and a list of members.
    /// </summary>
    public class PackageNode(string? name, List<PackageMemberNode> members) : AstNode
    {
        public string? Name { get; } = name;
        public List<PackageMemberNode> Members { get; } = members;
    }

    /// <summary>
    /// Represents the base class for all package member nodes.
    /// </summary>
    public abstract class PackageMemberNode : AstNode
    {
    }

    /// <summary>
    /// Represents a package variable declaration node with a type, optional name, and optional initializer.
    /// </summary>
    public class PackageVariableDeclarationNode(string? type, string name, ExpressionNode? initializer)
        : PackageMemberNode
    {
        public string? Type { get; } = type;
        public string Name { get; } = name;
        public ExpressionNode? Initializer { get; } = initializer;
    }

    /// <summary>
    /// Represents a package function node with a name, parameters, return type, body, and return statement.
    /// </summary>
    public class PackageFunctionNode(
        string? name,
        List<ParameterNode> parameters,
        string? returnType,
        BlockNode? body,
        ReturnStatementNode? returnStatement) : PackageMemberNode
    {
        public string? Name { get; } = name;
        public List<ParameterNode> Parameters { get; } = parameters;
        public string? ReturnType { get; } = returnType;
        public BlockNode? Body { get; } = body;
        public ReturnStatementNode? ReturnStatement { get; } = returnStatement;
    }

    /// <summary>
    /// Represents the program node containing global variables, packages, functions, main function, and statements.
    /// </summary>
    public class ProgramNode(
        List<VariableDeclarationNode?> globalVariables,
        List<PackageNode?> packages,
        List<FunctionNode?> functions,
        FunctionNode? mainFunction,
        List<StatementNode?>? statements) : AstNode
    {
        public List<VariableDeclarationNode?> GlobalVariables { get; } = globalVariables;
        public List<FunctionNode?> Functions { get; } = functions;
        public List<PackageNode?> Packages { get; } = packages;
        public FunctionNode? MainFunction { get; } = mainFunction;
        public List<StatementNode?>? Statements { get; } = statements;
    }

    /// <summary>
    /// Represents an array access node with a name and index.
    /// </summary>
    public class ArrayAccessNode(string name, ExpressionNode? index) : ExpressionNode
    {
        public string Name { get; } = name;
        public ExpressionNode? Index { get; } = index;
    }

    /// <summary>
    /// Represents an array initializer node with a list of elements.
    /// </summary>
    public class ArrayInitializerNode(List<ExpressionNode> elements, ExpressionNode sizeExpression) : ExpressionNode
    {
        /// <summary>
        /// Gets the list of elements used to initialize the array.
        /// </summary>
        public List<ExpressionNode> Elements = elements;

        public ExpressionNode SizeExpression = sizeExpression;
    }
}
