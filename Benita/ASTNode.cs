namespace Benita
{
    /// <summary>
    /// Base class for all AST nodes.
    /// </summary>
    public abstract class ASTNode
    {
    }

    /// <summary>
    /// Base class for all expression nodes in the AST.
    /// </summary>
    public abstract class ExpressionNode : ASTNode
    {
    }

    /// <summary>
    /// Node representing a literal value in the AST.
    /// </summary>
    public class LiteralNode : ExpressionNode
    {
        /// <summary>
        /// Gets the literal value.
        /// </summary>
        public string? Value { get; }

        /// <summary>
        /// Gets the type of the literal.
        /// </summary>
        public TokenType Type { get; }
        public LiteralNode(string? value, TokenType type)
        {
            Value = value;
            Type = type;
        }
    }

    /// <summary>
    /// Node representing an identifier in the AST.
    /// </summary>
    public class IdentifierNode : ExpressionNode
    {
        /// <summary>
        /// Gets the name of the identifier.
        /// </summary>
        public string? Name { get; }
        public IdentifierNode(string? name)
        {
            Name = name;
        }
    }

    /// <summary>
    /// Node representing a binary expression in the AST.
    /// </summary>
    public class BinaryExpressionNode : ExpressionNode
    {
        /// <summary>
        /// Gets the left-hand side of the expression.
        /// </summary>
        public ExpressionNode Left { get; }

        /// <summary>
        /// Gets the operator (e.g., +, -, *).
        /// </summary>
        public string? Operator { get; }

        /// <summary>
        /// Gets the right-hand side of the expression.
        /// </summary>
        public ExpressionNode Right { get; }
        public BinaryExpressionNode(ExpressionNode left, string? op, ExpressionNode right)
        {
            Left = left;
            Operator = op;
            Right = right;
        }
    }

    /// <summary>
    /// Node representing a logical expression in the AST.
    /// </summary>
    public class LogicalExpressionNode : ExpressionNode
    {
        /// <summary>
        /// Gets the left-hand side of the expression.
        /// </summary>
        public ExpressionNode Left { get; }

        /// <summary>
        /// Gets the logical operator (e.g., &&, ||).
        /// </summary>
        public string? Operator { get; }

        /// <summary>
        /// Gets the right-hand side of the expression.
        /// </summary>
        public ExpressionNode Right { get; }
        public LogicalExpressionNode(ExpressionNode left, string? op, ExpressionNode right)
        {
            Left = left;
            Operator = op;
            Right = right;
        }

        // Implement methods like Accept for visitor pattern, ToString, etc.
    }

    /// <summary>
    /// Node representing a unary expression in the AST.
    /// </summary>
    public class UnaryExpressionNode : ExpressionNode
    {
        /// <summary>
        /// Gets the unary operator (e.g., -, !).
        /// </summary>
        public string? Operator { get; }

        /// <summary>
        /// Gets the operand of the unary expression.
        /// </summary>
        public ExpressionNode Operand { get; }
        public UnaryExpressionNode(string? op, ExpressionNode operand)
        {
            Operator = op;
            Operand = operand;
        }
    }

    /// <summary>
    /// Node representing a function call in the AST.
    /// </summary>
    public class FunctionCallNode : ExpressionNode
    {
        /// <summary>
        /// Gets the name of the function being called.
        /// </summary>
        public string? FunctionName { get; }

        /// <summary>
        /// Gets the list of arguments passed to the function.
        /// </summary>
        public List<ExpressionNode> Arguments { get; }
        public FunctionCallNode(string? functionName, List<ExpressionNode> arguments)
        {
            FunctionName = functionName;
            Arguments = arguments;
        }
    }

    /// <summary>
    /// Base class for all statement nodes in the AST.
    /// </summary>
    public abstract class StatementNode : ASTNode
    {
    }

    /// <summary>
    /// Node representing a variable declaration statement in the AST.
    /// </summary>
    public class VariableDeclarationNode : StatementNode
    {
        /// <summary>
        /// Gets the type of the variable.
        /// </summary>
        public string? Type { get; }

        /// <summary>
        /// Gets the name of the variable.
        /// </summary>
        public string? Name { get; }

        /// <summary>
        /// Gets the initializer expression for the variable.
        /// </summary>
        public ExpressionNode Initializer { get; }
        public VariableDeclarationNode(string? type, string? name, ExpressionNode initializer)
        {
            Type = type;
            Name = name;
            Initializer = initializer;
        }
    }
    /// <summary>
    /// Node representing an assignment statement in the AST.
    /// </summary>
    public class AssignmentNode : StatementNode
    {
        /// <summary>
        /// Gets the name of the variable being assigned.
        /// </summary>
        public string? Name { get; }

        /// <summary>
        /// Gets the expression being assigned to the variable.
        /// </summary>
        public ExpressionNode Expression { get; }
        public AssignmentNode(string? name, ExpressionNode expression)
        {
            Name = name;
            Expression = expression;
        }
    }

    /// <summary>
    /// Node representing an array assignment statement in the AST.
    /// </summary>
    public class ArrayAssignmentNode : StatementNode
    {
        /// <summary>
        /// Gets the name of the array.
        /// </summary>
        public string? Name { get; }

        /// <summary>
        /// Gets the index of the array element.
        /// </summary>
        public ExpressionNode Index { get; }

        /// <summary>
        /// Gets the value being assigned to the array element.
        /// </summary>
        public ExpressionNode Value { get; }
        public ArrayAssignmentNode(string? name, ExpressionNode index, ExpressionNode value)
        {
            Name = name;
            Index = index;
            Value = value;
        }
    }

    /// <summary>
    /// Node representing a compound assignment statement in the AST.
    /// </summary>
    public class CompoundAssignmentNode : StatementNode
    {
        /// <summary>
        /// Gets the name of the variable being assigned.
        /// </summary>
        public string? Name { get; }

        /// <summary>
        /// Gets the compound operator (e.g., +=, -=).
        /// </summary>
        public string? Operator { get; }

        /// <summary>
        /// Gets the expression being assigned to the variable.
        /// </summary>
        public ExpressionNode Expression { get; }
        public CompoundAssignmentNode(string? name, string? op, ExpressionNode expression)
        {
            Name = name;
            Operator = op;
            Expression = expression;
        }
    }

    /// <summary>
    /// Node representing an increment or decrement statement in the AST.
    /// </summary>
    public class IncrementDecrementNode : StatementNode
    {
        /// <summary>
        /// Gets the name of the variable being incremented or decremented.
        /// </summary>
        public string? Name { get; }

        /// <summary>
        /// Gets the increment or decrement operator (e.g., ++, --).
        /// </summary>
        public string? Operator { get; }
        public IncrementDecrementNode(string? name, string? op)
        {
            Name = name;
            Operator = op;
        }
    }

    /// <summary>
    /// Node representing an if statement in the AST.
    /// </summary>
    public class IfStatementNode : StatementNode
    {
        /// <summary>
        /// Gets the condition expression.
        /// </summary>
        public ExpressionNode Condition { get; }

        /// <summary>
        /// Gets the statement to execute if the condition is true.
        /// </summary>
        public StatementNode ThenBranch { get; }

        /// <summary>
        /// Gets the statement to execute if the condition is false.
        /// </summary>
        public StatementNode ElseBranch { get; }
        public IfStatementNode(ExpressionNode condition, StatementNode thenBranch, StatementNode elseBranch)
        {
            Condition = condition;
            ThenBranch = thenBranch;
            ElseBranch = elseBranch;
        }
    }

    /// <summary>
    /// Node representing a while loop statement in the AST.
    /// </summary>
    public class WhileStatementNode : StatementNode
    {
        /// <summary>
        /// Gets the condition expression for the while loop.
        /// </summary>
        public ExpressionNode Condition { get; }

        /// <summary>
        /// Gets the body of the while loop.
        /// </summary>
        public StatementNode Body { get; }
        public WhileStatementNode(ExpressionNode condition, StatementNode body)
        {
            Condition = condition;
            Body = body;
        }
    }

    /// <summary>
    /// Node representing a for loop statement in the AST.
    /// </summary>
    public class ForStatementNode : StatementNode
    {
        /// <summary>
        /// Gets the initializer statement of the for loop.
        /// </summary>
        public StatementNode Initializer { get; }

        /// <summary>
        /// Gets the condition expression of the for loop.
        /// </summary>
        public ExpressionNode Condition { get; }

        /// <summary>
        /// Gets the increment statement of the for loop.
        /// </summary>
        public StatementNode Increment { get; }

        /// <summary>
        /// Gets the body of the for loop.
        /// </summary>
        public StatementNode Body { get; }
        public ForStatementNode(StatementNode initializer, ExpressionNode condition, StatementNode increment, StatementNode body)
        {
            Initializer = initializer;
            Condition = condition;
            Increment = increment;
            Body = body;
        }
    }
    /// <summary>
    /// Node representing a block of statements in the AST.
    /// </summary>
    public class BlockNode : StatementNode
    {
        /// <summary>
        /// Gets the list of statements in the block.
        /// </summary>
        public List<StatementNode> Statements { get; }
        public BlockNode(List<StatementNode> statements)
        {
            Statements = statements;
        }
    }

    /// <summary>
    /// Node representing an expression statement in the AST.
    /// </summary>
    public class ExpressionStatementNode : StatementNode
    {
        /// <summary>
        /// Gets the expression of the statement.
        /// </summary>
        public ExpressionNode Expression { get; }
        public ExpressionStatementNode(ExpressionNode expression)
        {
            Expression = expression;
        }
    }

    /// <summary>
    /// Node representing a function declaration in the AST.
    /// </summary>
    public class FunctionNode : ASTNode
    {
        /// <summary>
        /// Gets the name of the function.
        /// </summary>
        public string? Name { get; }

        /// <summary>
        /// Gets the list of parameters for the function.
        /// </summary>
        public List<ParameterNode> Parameters { get; }

        /// <summary>
        /// Gets the return type of the function.
        /// </summary>
        public string? ReturnType { get; }

        /// <summary>
        /// Gets the body of the function.
        /// </summary>
        public BlockNode Body { get; }

        /// <summary>
        /// Gets the return statement of the function.
        /// </summary>
        public ReturnStatementNode ReturnStatement { get; }
        public FunctionNode(string? name, List<ParameterNode> parameters, string? returnType, BlockNode body, ReturnStatementNode returnStatement)
        {
            Name = name;
            Parameters = parameters;
            ReturnType = returnType;
            Body = body;
            ReturnStatement = returnStatement;
        }
    }

    /// <summary>
    /// Node representing a return statement in the AST.
    /// </summary>
    public class ReturnStatementNode : StatementNode
    {
        /// <summary>
        /// Gets the expression to be returned.
        /// </summary>
        public ExpressionNode ReturnExpression { get; }
        public ReturnStatementNode(ExpressionNode returnExpression)
        {
            ReturnExpression = returnExpression;
        }
    }

    /// <summary>
    /// Node representing a parameter in a function declaration.
    /// </summary>
    public class ParameterNode : ASTNode
    {
        /// <summary>
        /// Gets the type of the parameter.
        /// </summary>
        public string? Type { get; }

        /// <summary>
        /// Gets the name of the parameter.
        /// </summary>
        public string? Name { get; }
        public ParameterNode(string? type, string? name)
        {
            Type = type;
            Name = name;
        }
    }

    /// <summary>
    /// Node representing the entire program in the AST.
    /// </summary>
    public class ProgramNode : ASTNode
    {
        /// <summary>
        /// Gets the list of global variable declarations.
        /// </summary>
        public List<VariableDeclarationNode> GlobalVariables { get; }

        /// <summary>
        /// Gets the list of functions declared in the program.
        /// </summary>
        public List<FunctionNode> Functions { get; }

        /// <summary>
        /// 
        /// </summary>
        public FunctionNode MainFunction { get; }

        public ProgramNode(List<VariableDeclarationNode> globalVariables, List<FunctionNode> functions, FunctionNode mainFunction)
        {
            GlobalVariables = globalVariables;
            MainFunction = mainFunction;
            Functions = functions;
        }
    }

    /// <summary>
    /// Node representing an array access operation in the AST.
    /// </summary>
    public class ArrayAccessNode : ExpressionNode
    {
        /// <summary>
        /// Gets the name of the array being accessed.
        /// </summary>
        public string? Name;

        /// <summary>
        /// Gets the index expression used to access an element in the array.
        /// </summary>
        public ExpressionNode Index;
        public ArrayAccessNode(string? name, ExpressionNode index)
        {
            Name = name;
            Index = index;
        }
    }

    /// <summary>
    /// Node representing an array initializer in the AST.
    /// </summary>
    public class ArrayInitializerNode : ExpressionNode
    {
        /// <summary>
        /// Gets the list of elements used to initialize the array.
        /// </summary>
        public List<ExpressionNode> Elements;
        public ArrayInitializerNode(List<ExpressionNode> elements)
        {
            Elements = elements;
        }
    }



}
