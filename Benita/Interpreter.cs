
namespace Benita
{
    /// <summary>
    /// The Interpreter class executes the parsed abstract syntax tree (AST) nodes of the Benita language.
    /// It handles expression evaluation, statement execution, function management, and control flow, 
    /// effectively bringing the Benita source code to life through dynamic interpretation.
    /// </summary>
    public class Interpreter
    {
        private Dictionary<string?, object?> _variables = new Dictionary<string?, object?>();
        private readonly Dictionary<string?, FunctionNode> _functions = new Dictionary<string?, FunctionNode>();
        private bool _functionReturnFlag;

        /// <summary>
        /// Visits the given AST node and dispatches to the appropriate visit method.
        /// </summary>
        /// <param name="node">The AST node to visit.</param>
        /// <returns>The result of visiting the node.</returns>
        public object? Visit(ASTNode node)
        {
            switch (node)
            {
                case LiteralNode literalNode:
                    return VisitLiteralNode(literalNode);
                case IdentifierNode identifierNode:
                    return VisitIdentifierNode(identifierNode);
                case BinaryExpressionNode binaryExpressionNode:
                    return VisitBinaryExpressionNode(binaryExpressionNode);
                case UnaryExpressionNode unaryExpressionNode:
                    return VisitUnaryExpressionNode(unaryExpressionNode);
                case LogicalExpressionNode logicalExpressionNode:
                    return VisitLogicalExpressionNode(logicalExpressionNode);
                case FunctionCallNode functionCallNode:
                    return VisitFunctionCallNode(functionCallNode);
                case VariableDeclarationNode variableDeclarationNode:
                    return VisitVariableDeclarationNode(variableDeclarationNode);
                case AssignmentNode assignmentNode:
                    return VisitAssignmentNode(assignmentNode);
                case CompoundAssignmentNode compoundAssignmentNode:
                    return VisitCompoundAssignmentNode(compoundAssignmentNode);
                case IncrementDecrementNode incrementDecrementNode:
                    return VisitIncrementDecrementNode(incrementDecrementNode);
                case ExpressionStatementNode expressionStatementNode:
                    return VisitExpressionStatementNode(expressionStatementNode);
                case BlockNode blockNode:
                    return VisitBlockNode(blockNode);
                case IfStatementNode ifStatementNode:
                    return VisitIfStatementNode(ifStatementNode);
                case WhileStatementNode whileStatementNode:
                    return VisitWhileStatementNode(whileStatementNode);
                case ForStatementNode forStatementNode:
                    return VisitForStatementNode(forStatementNode);
                case FunctionNode functionNode:
                    return VisitFunctionNode(functionNode);
                case ProgramNode programNode:
                    return VisitProgramNode(programNode);
                case ArrayInitializerNode arrayInitializerNode:
                    return VisitArrayInitializerNode(arrayInitializerNode);
                case ArrayAccessNode arrayAccess:
                    return VisitArrayAccessNode(arrayAccess);
                case ArrayAssignmentNode arrayAssignment:
                    return VisitArrayAssignmentNode(arrayAssignment);
                case ReturnStatementNode returnStatementNode:
                    return VisitReturnStatementNode(returnStatementNode);
                default:
                    throw new Exception($"Unknown node type: {node.GetType().Name}");
            }
        }

        /// <summary>
        /// Visits an array access node.
        /// </summary>
        /// <param name="node">The array access node to visit.</param>
        /// <returns>The value at the specified index in the array.</returns>
        private object? VisitArrayAccessNode(ArrayAccessNode node)
        {
            var arrayName = node.Name;
            var index = (int)Visit(node.Index);

            if (_variables.TryGetValue(arrayName, out var value) && value is object[] array)
            {
                if (index >= 0 && index < array.Length)
                {
                    return array[(int)index];
                }
                throw new Exception($"Index out of bounds for array '{arrayName}'");
            }
            throw new Exception($"Variable '{arrayName}' is not an array");
        }

        /// <summary>
        /// Visits an array initializer node.
        /// </summary>
        /// <param name="arrayInitializerNode">The array initializer node to visit.</param>
        /// <returns>An array initialized with the specified elements.</returns>
        private object? VisitArrayInitializerNode(ArrayInitializerNode arrayInitializerNode)
        {
            List<object?> arrayValues = new List<object?>();
            foreach (var element in arrayInitializerNode.Elements)
            {
                arrayValues.Add(Visit(element));
            }
            return arrayValues.ToArray();
        }

        /// <summary>
        /// Visits an array assignment node.
        /// </summary>
        /// <param name="node">The array assignment node to visit.</param>
        /// <returns>The new value assigned to the array at the specified index.</returns>
        private object? VisitArrayAssignmentNode(ArrayAssignmentNode node)
        {
            var arrayName = node.Name;
            var newValue = Visit(node.Value);
            var index = Visit(node.Index);

            if (!_variables.ContainsKey(arrayName))
            {
                throw new Exception($"Array '{arrayName}' not found in variables.");
            }

            object? arrayObj = _variables[arrayName];
            if (arrayObj is object[] array)
            {
                array[Convert.ToInt32(index)] = newValue;
                _variables[arrayName] = array;
                return newValue;
            }
            throw new Exception($"Variable '{arrayName}' is not an array.");
        }

        /// <summary>
        /// Visits a literal node.
        /// </summary>
        /// <param name="node">The literal node to visit.</param>
        /// <returns>The value of the literal.</returns>
        private object? VisitLiteralNode(LiteralNode node)
        {
            switch (node.Type)
            {
                case TokenType.NUMBER:
                case TokenType.NUMBER_LITERAL:
                    return Convert.ToInt32(node.Value);
                case TokenType.STRING:
                case TokenType.STRING_LITERAL:
                    return node.Value;
                case TokenType.TRUE_LITERAL:
                    return true;
                case TokenType.FALSE_LITERAL:
                    return false;
                default:
                    throw new Exception($"Unknown literal type: {node.Type}");
            }
        }

        /// <summary>
        /// Visits an identifier node.
        /// </summary>
        /// <param name="node">The identifier node to visit.</param>
        /// <returns>The value of the variable identified by the node.</returns>
        private object? VisitIdentifierNode(IdentifierNode node)
        {
            if (_variables.TryGetValue(node.Name, out var value))
            {
                return value;
            }
            throw new Exception($"Undefined variable '{node.Name}'");
        }

        /// <summary>
        /// Visits a binary expression node.
        /// </summary>
        /// <param name="node">The binary expression node to visit.</param>
        /// <returns>The result of the binary expression.</returns>
        private object? VisitBinaryExpressionNode(BinaryExpressionNode node)
        {
            var left = Visit(node.Left);
            var right = Visit(node.Right);

            switch (node.Operator)
            {
                case "+":
                    if (left is int && right is int)
                    {
                        return Convert.ToInt32(left) + Convert.ToInt32(right);
                    }
                    if (left is string || right is string)
                    {
                        return left.ToString() + right.ToString();
                    }
                    throw new Exception("Invalid operands for '+' operator");
                case "-":
                    return Convert.ToInt32(left) - Convert.ToInt32(right);
                case "*":
                    return Convert.ToInt32(left) * Convert.ToInt32(right);
                case "/":
                    return Convert.ToInt32(left) / Convert.ToInt32(right);
                case "%":
                    return Convert.ToInt32(left) % Convert.ToInt32(right);
                case "&&":
                    return Convert.ToBoolean(left) && Convert.ToBoolean(right);
                case "||":
                    return Convert.ToBoolean(left) || Convert.ToBoolean(right);
                case "==":
                    return Convert.ToInt32(left) == Convert.ToInt32(right);
                case "!=":
                    return Convert.ToInt32(left) != Convert.ToInt32(right);
                case "<":
                    return Convert.ToInt32(left) < Convert.ToInt32(right);
                case ">":
                    return Convert.ToInt32(left) > Convert.ToInt32(right);
                case "<=":
                    return Convert.ToInt32(left) <= Convert.ToInt32(right);
                case ">=":
                    return Convert.ToInt32(left) >= Convert.ToInt32(right);
                default:
                    throw new Exception($"Unknown operator '{node.Operator}'");
            }
        }

        /// <summary>
        /// Visits a unary expression node.
        /// </summary>
        /// <param name="node">The unary expression node to visit.</param>
        /// <returns>The result of the unary expression.</returns>
        private object? VisitUnaryExpressionNode(UnaryExpressionNode node)
        {
            var operand = Visit(node.Operand);
            switch (node.Operator)
            {
                case "-":
                    return -(int)operand;
                case "!":
                    return !(bool)operand;
                default:
                    throw new Exception($"Unknown operator '{node.Operator}'");
            }
        }

        /// <summary>
        /// Visits a logical expression node.
        /// </summary>
        /// <param name="node">The logical expression node to visit.</param>
        /// <returns>The result of the logical expression.</returns>
        private object? VisitLogicalExpressionNode(LogicalExpressionNode node)
        {
            var left = Visit(node.Left);
            var right = Visit(node.Right);

            switch (node.Operator)
            {
                case "&&":
                    return Convert.ToBoolean(left) && Convert.ToBoolean(right);
                case "||":
                    return Convert.ToBoolean(left) || Convert.ToBoolean(right);
                default:
                    throw new Exception($"Unknown operator '{node.Operator}' for logical expression");
            }
        }

        /// <summary>
        /// Visits a function call node.
        /// </summary>
        /// <param name="node">The function call node to visit.</param>
        /// <returns>The result of the function call.</returns>
        private object? VisitFunctionCallNode(FunctionCallNode node)
        {
            if (_functions.TryGetValue(node.FunctionName, out var function))
            {
                var newScope = new Dictionary<string?, object?>(_variables);

                for (int i = 0; i < function.Parameters.Count; i++)
                {
                    var paramName = function.Parameters[i].Name;
                    var argValue = Visit(node.Arguments[i]);
                    newScope[paramName] = argValue;
                }

                var originalVariables = _variables;
                _variables = newScope;

                try
                {
                    var result = Visit(function.Body);
                    if (_functionReturnFlag)
                    {
                        _functionReturnFlag = false;
                        return result;
                    }
                    else
                    {
                        _functionReturnFlag = false;
                        return function.ReturnStatement != null
                            ? Visit(function.ReturnStatement.ReturnExpression)
                            : null;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
                finally
                {
                    _functionReturnFlag = false;
                    _variables = originalVariables;
                }
            }
            else
            {
                var functionManagementClass = FactoryClass.GetInterpreterClass(node.FunctionName);
                if (functionManagementClass != null)
                {
                    List<object> arguments = (from argument in node.Arguments select Visit(argument)).ToList();
                    return functionManagementClass.HandleFunctionCall(node.FunctionName, arguments);
                }
                else
                {
                    throw new Exception($"Unknown function '{node.FunctionName}'");
                }
            }
        }

        /// <summary>
        /// Visits a return statement node.
        /// </summary>
        /// <param name="returnStatementNode">The return statement node to visit.</param>
        /// <returns>The value to return from the function.</returns>
        private object? VisitReturnStatementNode(ReturnStatementNode returnStatementNode)
        {
            _functionReturnFlag = true;
            return returnStatementNode.ReturnExpression == null ? null : Visit(returnStatementNode.ReturnExpression);
        }

        /// <summary>
        /// Visits a variable declaration node.
        /// </summary>
        /// <param name="node">The variable declaration node to visit.</param>
        /// <returns>Null.</returns>
        private object? VisitVariableDeclarationNode(VariableDeclarationNode node)
        {
            object? value = null;

            if (node.Initializer != null)
            {
                value = Visit(node.Initializer);
            }
            else
            {
                // Initialize to default values based on type
                switch (node.Type)
                {
                    case "number":
                        value = 0.0;
                        break;
                    case "string":
                        value = string.Empty;
                        break;
                    case "bool":
                        value = false;
                        break;
                    case "number[]":
                        value = new int[0];
                        break;
                    case "string[]":
                        value = new string[0];
                        break;
                    case "bool[]":
                        value = new bool[0];
                        break;
                    default:
                        throw new Exception($"Unknown variable type '{node.Type}'");
                }
            }
            _variables[node.Name] = value;
            return null;
        }

        /// <summary>
        /// Visits an assignment node.
        /// </summary>
        /// <param name="node">The assignment node to visit.</param>
        /// <returns>The assigned value.</returns>
        private object? VisitAssignmentNode(AssignmentNode node)
        {
            var value = Visit(node.Expression);
            _variables[node.Name] = value;
            return value;
        }

        /// <summary>
        /// Visits a compound assignment node.
        /// </summary>
        /// <param name="node">The compound assignment node to visit.</param>
        /// <returns>The result of the compound assignment.</returns>
        private object? VisitCompoundAssignmentNode(CompoundAssignmentNode node)
        {
            if (!_variables.TryGetValue(node.Name, out var variable))
            {
                throw new Exception($"Undefined variable '{node.Name}'");
            }

            var oldValue = (int)variable;
            var newValue = (int)Visit(node.Expression);

            switch (node.Operator)
            {
                case "+=":
                    _variables[node.Name] = oldValue + newValue;
                    break;
                case "-=":
                    _variables[node.Name] = oldValue - newValue;
                    break;
                case "*=":
                    _variables[node.Name] = oldValue * newValue;
                    break;
                case "/=":
                    _variables[node.Name] = oldValue / newValue;
                    break;
                default:
                    throw new Exception($"Unknown compound assignment operator '{node.Operator}'");
            }

            return _variables[node.Name];
        }

        /// <summary>
        /// Visits an increment/decrement node.
        /// </summary>
        /// <param name="node">The increment/decrement node to visit.</param>
        /// <returns>The result of the increment or decrement operation.</returns>
        private object? VisitIncrementDecrementNode(IncrementDecrementNode node)
        {
            if (!_variables.TryGetValue(node.Name, out var variable))
            {
                throw new Exception($"Undefined variable '{node.Name}'");
            }
            var oldValue = Convert.ToInt32(variable);
            switch (node.Operator)
            {
                case "++":
                    _variables[node.Name] = oldValue + 1;
                    break;
                case "--":
                    _variables[node.Name] = oldValue - 1;
                    break;
                default:
                    throw new Exception($"Unknown increment/decrement operator '{node.Operator}'");
            }
            return _variables[node.Name];
        }

        /// <summary>
        /// Visits an expression statement node.
        /// </summary>
        /// <param name="node">The expression statement node to visit.</param>
        /// <returns>The result of visiting the expression.</returns>
        private object? VisitExpressionStatementNode(ExpressionStatementNode node)
        {
            return Visit(node.Expression);
        }

        /// <summary>
        /// Visits a block node.
        /// </summary>
        /// <param name="node">The block node to visit.</param>
        /// <returns>The result of visiting the block's statements.</returns>
        private object? VisitBlockNode(BlockNode node)
        {
            foreach (var statement in node.Statements)
            {
                if (statement == null)
                    continue;

                var result = Visit(statement);
                if (_functionReturnFlag)
                    return result;
            }
            return null;
        }

        /// <summary>
        /// Visits an if statement node.
        /// </summary>
        /// <param name="node">The if statement node to visit.</param>
        /// <returns>The result of the if statement execution.</returns>
        private object? VisitIfStatementNode(IfStatementNode node)
        {
            var condition = (bool)Visit(node.Condition);
            if (condition)
            {
                return Visit(node.ThenBranch);
            }

            if (node.ElseBranch != null)
            {
                return Visit(node.ElseBranch);
            }
            return null;
        }

        /// <summary>
        /// Visits a while statement node.
        /// </summary>
        /// <param name="node">The while statement node to visit.</param>
        /// <returns>Null.</returns>
        private object? VisitWhileStatementNode(WhileStatementNode node)
        {
            while ((bool)Visit(node.Condition))
            {
                Visit(node.Body);
            }
            return null;
        }

        /// <summary>
        /// Visits a for statement node.
        /// </summary>
        /// <param name="node">The for statement node to visit.</param>
        /// <returns>Null.</returns>
        private object? VisitForStatementNode(ForStatementNode node)
        {
            Visit(node.Initializer);
            while ((bool)Visit(node.Condition))
            {
                Visit(node.Body);

                Visit(node.Increment);
            }
            return null;
        }

        /// <summary>
        /// Visits a function node.
        /// </summary>
        /// <param name="node">The function node to visit.</param>
        /// <returns>Null.</returns>
        private object? VisitFunctionNode(FunctionNode node)
        {
            _functions[node.Name] = node;
            return null;
        }

        /// <summary>
        /// Visits a program node.
        /// </summary>
        /// <param name="node">The program node to visit.</param>
        /// <returns>Null.</returns>
        private object? VisitProgramNode(ProgramNode node)
        {
            // Initialize global variables
            foreach (var globalVar in node.GlobalVariables)
            {
                Visit(globalVar);
            }

            // Register functions
            foreach (var function in node.Functions)
            {
                Visit(function);
            }

            // Register the main function
            if (node.MainFunction != null)
            {
                _functions[node.MainFunction.Name] = node.MainFunction;
            }

            // Execute the main function
            if (_functions.TryGetValue("_main_", out var mainFunction))
            {
                VisitFunctionCallNode(new FunctionCallNode(mainFunction.Name, new List<ExpressionNode>()));
            }
            else
            {
                throw new Exception("No entry point (_main_) defined.");
            }

            return null;
        }
    }
}
