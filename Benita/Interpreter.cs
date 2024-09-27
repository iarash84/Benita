
namespace Benita
{
    /// <summary>
    /// The Interpreter class executes the parsed abstract syntax tree (AST) nodes of the Benita language.
    /// It handles expression evaluation, statement execution, function management, and control flow, 
    /// effectively bringing the Benita source code to life through dynamic interpretation.
    /// </summary>
    public class Interpreter
    {
        private readonly Dictionary<string?, FunctionNode> _functions = [];
        private Dictionary<string?, object> _variables = [];
        private readonly Dictionary<string?, object> _outerScopeVariables;

        private bool _functionReturnFlag;
        private readonly bool _debugMode;
        private readonly string _packageScope;
        private readonly DebugClass _debugClass;

        /// <summary>
        /// Initializes a new instance of the <see cref="Interpreter"/> class.
        /// </summary>
        /// <param name="packageScope">The scope of the package, default is "_main_".</param>
        public Interpreter(bool debugMode = false, string packageScope = "Program")
        {
            if (debugMode)
            {
                _debugClass = DebugClass.Instance;
            }
            _packageScope = packageScope;
            _outerScopeVariables = new Dictionary<string?, object>();
        }

        private void DebugLog(string message, bool pressKeyWait = true)
        {
            if (_debugMode)
            {
                _debugClass.DebugLog(message, _variables, _outerScopeVariables, _functions, pressKeyWait);
            }
        }

        /// <summary>
        /// Sets the outer scope variables for the interpreter.
        /// </summary>
        /// <param name="outerScopeVariables">A dictionary of variables in the outer scope.</param>
        public void SetOuterScopeVariables(Dictionary<string?, object> outerScopeVariables)
        {
            DebugLog($"SetOuterScopeVariables: {string.Join(", ", outerScopeVariables.Select(kvp => $"{kvp.Key} = {kvp.Value}"))}");
            _outerScopeVariables.Clear();
            foreach (var kvp in outerScopeVariables)
                _outerScopeVariables.Add(kvp.Key, kvp.Value);
        }

        /// <summary>
        /// Sets the global variables for the interpreter.
        /// </summary>
        public void SetGlobalVariable()
        {
            DebugLog("SetGlobalVariable");
            foreach (var kvp in Globals.GlobalVariable)
                _variables.Add(kvp.Key, kvp.Value);
        }

        /// <summary>
        /// Visits the specified AST node and executes the corresponding logic.
        /// </summary>
        /// <param name="node">The AST node to visit.</param>
        /// <returns>The result of the visit.</returns>
        public object Visit(AstNode? node)
        {
            DebugLog($"Visit: {node.GetType().Name}, packageScope = {_packageScope}", false);
            switch (node)
            {
                case ProgramNode programNode:
                    return VisitProgramNode(programNode);
                case BlockNode blockNode:
                    return VisitBlockNode(blockNode);
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
                case IfStatementNode ifStatementNode:
                    return VisitIfStatementNode(ifStatementNode);
                case WhileStatementNode whileStatementNode:
                    return VisitWhileStatementNode(whileStatementNode);
                case ForStatementNode forStatementNode:
                    return VisitForStatementNode(forStatementNode);
                case FunctionNode functionNode:
                    return VisitFunctionNode(functionNode);
                case ArrayInitializerNode arrayInitializerNode:
                    return VisitArrayInitializerNode(arrayInitializerNode);
                case ArrayAccessNode arrayAccessNode:
                    return VisitArrayAccessNode(arrayAccessNode);
                case ArrayAssignmentNode arrayAssignmentNode:
                    return VisitArrayAssignmentNode(arrayAssignmentNode);
                case ReturnStatementNode returnStatementNode:
                    return VisitReturnStatementNode(returnStatementNode);
                case BreakStatementNode:
                    DebugLog($"BreakStatementNode", false);
                    throw new BreakException();
                case ContinueStatementNode:
                    DebugLog($"ContinueStatementNode", false);
                    throw new ContinueException();
                case PackageNode packageNode:
                    return VisitPackageNode(packageNode);
                case MemberAccessNode memberAccessNode:
                    return VisitMemberAccessNode(memberAccessNode);
                case ObjectInstantiationNode objectInstantiationNode:
                    return VisitObjectInstantiationNode(objectInstantiationNode);
                default:
                    throw new Exception($"Unknown node type: {node.GetType().Name}");
            }
        }

        /// <summary>
        /// Visits an object instantiation node and creates a new instance.
        /// </summary>
        /// <param name="node">The object instantiation node.</param>
        /// <returns>The created package instance.</returns>
        private object VisitObjectInstantiationNode(ObjectInstantiationNode node)
        {
            DebugLog($"Instantiating object {node.Name} from package {node.PackageName}", false);
            // Get the package definition from the list of packages
            if (!Globals.PackageList.TryGetValue(node.PackageName, out var packageNode))
            {
                throw new($"Package '{node.PackageName}' not found.");
            }

            // Create a new package instance
            var packageInstance = new PackageInstance(node.Name, packageNode, node.Arguments, _debugMode);

            // Optionally, you might handle constructor arguments here
            // For simplicity, we assume no arguments or default constructor logic.

            // Store the new instance in the variables dictionary
            _variables[node.Name] = packageInstance;

            DebugLog($"Object {node.Name} instantiated.");
            return packageInstance;
        }

        /// <summary>
        /// Visits a literal node and returns its value.
        /// </summary>
        /// <param name="node">The literal node.</param>
        /// <returns>The value of the literal node.</returns>
        private object VisitLiteralNode(LiteralNode node)
        {
            DebugLog($"VisitLiteralNode: Type = {node.Type}, Value = {node.Value}");
            return node.Type switch
            {
                TokenType.NUMBER or TokenType.NUMBER_LITERAL => Convert.ToDouble(node.Value),
                TokenType.STRING or TokenType.STRING_LITERAL => node.Value,
                TokenType.TRUE_LITERAL => true,
                TokenType.FALSE_LITERAL => false,
                _ => throw new Exception($"Unknown literal type: {node.Type}")
            };
        }

        /// <summary>
        /// Visits an identifier node and returns its value.
        /// </summary>
        /// <param name="node">The identifier node.</param>
        /// <returns>The value of the identifier node.</returns>
        private object VisitIdentifierNode(IdentifierNode node)
        {
            DebugLog($"VisitIdentifierNode: Name = {node.Name}", false);

            if (TryGetVariableValue(node.Name, out var value))
            {
                DebugLog($"VisitIdentifierNode: value = {value}");
                return value;
            }

            throw new($"Undefined variable '{node.Name}'");
        }

        /// <summary>
        /// Visits a binary expression node and evaluates the expression.
        /// </summary>
        /// <param name="node">The binary expression node.</param>
        /// <returns>The result of the binary expression.</returns>
        private object VisitBinaryExpressionNode(BinaryExpressionNode node)
        {
            DebugLog($"VisitBinaryExpressionNode binary expression: {node.Operator}", false);

            var left = Visit(node.Left);
            var right = Visit(node.Right);

            DebugLog($"VisitBinaryExpressionNode binary expression result: {left} {node.Operator} {right}");

            switch (node.Operator)
            {
                case "+":
                    if (left is string || right is string)
                    {
                        return left.ToString() + right.ToString();
                    }
                    return Convert.ToDouble(left) + Convert.ToDouble(right);

                case "-":
                    return Convert.ToDouble(left) - Convert.ToDouble(right);

                case "*":
                    return Convert.ToDouble(left) * Convert.ToDouble(right);

                case "/":
                    if (Convert.ToDouble(right) != 0)
                    {
                        return Convert.ToDouble(left) / Convert.ToDouble(right);
                    }

                    throw new DivideByZeroException("Division by zero");

                case "%":
                    if (Convert.ToDouble(right) != 0)
                    {
                        return Convert.ToDouble(left) % Convert.ToDouble(right);
                    }
                    throw new DivideByZeroException("Division by zero");

                case "&&":
                    return Convert.ToBoolean(left) && Convert.ToBoolean(right);

                case "||":
                    return Convert.ToBoolean(left) || Convert.ToBoolean(right);

                case "==":
                    if (left is string || right is string)
                    {
                        return left.Equals(right);
                    }
                    return Convert.ToDouble(left) == Convert.ToDouble(right);

                case "!=":
                    if (left is string || right is string)
                    {
                        return !left.Equals(right);
                    }
                    return Convert.ToDouble(left) != Convert.ToDouble(right);

                case "<":
                    return Convert.ToDouble(left) < Convert.ToDouble(right);

                case ">":
                    return Convert.ToDouble(left) > Convert.ToDouble(right);

                case "<=":
                    return Convert.ToDouble(left) <= Convert.ToDouble(right);

                case ">=":
                    return Convert.ToDouble(left) >= Convert.ToDouble(right);

                default:
                    throw new Exception($"Unknown operator '{node.Operator}'");
            }
        }

        /// <summary>
        /// Visits a unary expression node and evaluates the expression.
        /// </summary>
        /// <param name="node">The unary expression node.</param>
        /// <returns>The result of the unary expression.</returns>
        private object VisitUnaryExpressionNode(UnaryExpressionNode node)
        {
            DebugLog($"VisitUnaryExpressionNode: Operator = {node.Operator}");

            var operand = Visit(node.Operand);
            return node.Operator switch
            {
                "-" => -Convert.ToDouble(operand),
                "!" => !Convert.ToBoolean(operand),
                _ => throw new($"Unknown operator '{node.Operator}'")
            };
        }

        /// <summary>
        /// Visits a logical expression node and evaluates the expression.
        /// </summary>
        /// <param name="node">The logical expression node.</param>
        /// <returns>The result of the logical expression.</returns>
        private object VisitLogicalExpressionNode(LogicalExpressionNode node)
        {
            DebugLog($"VisitLogicalExpressionNode: Operator = {node.Operator}");

            var left = Visit(node.Left);
            var right = Visit(node.Right);

            return node.Operator switch
            {
                "&&" => Convert.ToBoolean(left) && Convert.ToBoolean(right),
                "||" => Convert.ToBoolean(left) || Convert.ToBoolean(right),
                _ => throw new($"Unknown operator '{node.Operator}' for logical expression")
            };
        }

        /// <summary>
        /// Attempts to retrieve a function by name.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="value">The retrieved function node.</param>
        /// <returns>True if the function was found; otherwise, false.</returns>
        public bool TryGetFunction(string? name, out FunctionNode value)
        {
            DebugLog($"TryGetFunction: Name = {name}", false);

            if (_functions.TryGetValue(name, out value))
                return true;

            if (Globals.GlobalFunctions.TryGetValue(name, out value))
                return true;

            return false;
        }

        /// <summary>
        /// Visits a function call node and executes the function.
        /// </summary>
        /// <param name="node">The function call node.</param>
        /// <returns>The result of the function call.</returns>
        private object VisitFunctionCallNode(FunctionCallNode node)
        {
            DebugLog($"VisitFunctionCallNode: FunctionName = {node.FunctionName}", false);

            if (TryGetFunction(node.FunctionName, out var function))
            {
                var newScope = new Dictionary<string?, object>(_variables);

                for (int i = 0; i < function.Parameters.Count; i++)
                {
                    var paramName = function.Parameters[i].Name;
                    var argValue = Visit(node.Arguments[i]);
                    newScope[paramName] = argValue;
                    DebugLog($"Parameter: {paramName} = {argValue}");
                }

                var originalVariables = _variables;
                var originalFunctionReturnFlag = _functionReturnFlag;
                _functionReturnFlag = false;
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
                    throw new(ex.Message);
                }
                finally
                {
                    _functionReturnFlag = false;
                    originalVariables = SyncDictionaryValues(_variables, originalVariables);
                    _variables = originalVariables;
                    _functionReturnFlag = originalFunctionReturnFlag;
                }
            }

            var functionManagementClass = FactoryClass.GetInterpreterClass(node.FunctionName);
            if (functionManagementClass != null)
            {
                List<object> arguments = (from argument in node.Arguments select Visit(argument)).ToList();
                return functionManagementClass.HandleFunctionCall(node.FunctionName, arguments);
            }

            throw new($"Unknown function '{node.FunctionName}'");
        }

        /// <summary>
        /// Synchronizes values between two dictionaries.
        /// </summary>
        /// <param name="variables">The current variable dictionary.</param>
        /// <param name="originalVariables">The original variable dictionary.</param>
        /// <returns>The synchronized dictionary.</returns>
        private Dictionary<string?, object> SyncDictionaryValues(Dictionary<string?, object> variables, Dictionary<string?, object> originalVariables)
        {
            DebugLog($"SyncDictionaryValues");

            foreach (var key in variables.Keys.ToList())
            {
                if (originalVariables.ContainsKey(key) && _packageScope != "Program")
                {
                    originalVariables[key] = variables[key];
                }
                else if (Globals.GlobalVariable.ContainsKey(key))
                {
                    Globals.GlobalVariable[key] = variables[key];
                }
            }

            return originalVariables;
        }

        /// <summary>
        /// Visits a return statement node and sets the function return flag.
        /// </summary>
        /// <param name="returnStatementNode">The return statement node.</param>
        /// <returns>The result of the return expression.</returns>
        private object VisitReturnStatementNode(ReturnStatementNode returnStatementNode)
        {
            DebugLog($"VisitReturnStatementNode", false);

            _functionReturnFlag = true;
            return returnStatementNode.ReturnExpression == null ? null : Visit(returnStatementNode.ReturnExpression);
        }

        /// <summary>
        /// Visits a variable declaration node and declares the variable.
        /// </summary>
        /// <param name="node">The variable declaration node.</param>
        /// <returns>Null.</returns>
        private object VisitVariableDeclarationNode(VariableDeclarationNode node)
        {
            DebugLog($"VisitVariableDeclarationNode: Name = {node.Name}, Type = {node.Type}");

            object value = node.Initializer != null ? Visit(node.Initializer) : GetDefaultValue(node.Type);
            _variables[node.Name] = value;
            return null;
        }

        /// <summary>
        /// Visits an assignment node and assigns the value.
        /// </summary>
        /// <param name="node">The assignment node.</param>
        /// <returns>The assigned value.</returns>
        private object VisitAssignmentNode(AssignmentNode node)
        {
            DebugLog($"VisitAssignmentNode: Name = {node.Name}");

            var value = Visit(node.Expression);
            _variables[node.Name] = value;
            return value;
        }

        /// <summary>
        /// Visits a compound assignment node and performs the compound assignment.
        /// </summary>
        /// <param name="node">The compound assignment node.</param>
        /// <returns>The result of the compound assignment.</returns>
        private object VisitCompoundAssignmentNode(CompoundAssignmentNode node)
        {
            DebugLog($"VisitCompoundAssignmentNode: Name = {node.Name}, Operator = {node.Operator}", false);

            if (!TryGetVariableValue(node.Name, out var variable))
            {
                throw new($"Undefined variable '{node.Name}'");
            }

            var oldValue = Convert.ToDouble(variable);
            var newValue = Convert.ToDouble(Visit(node.Expression));

            _variables[node.Name] = node.Operator switch
            {
                "+=" => oldValue + newValue,
                "-=" => oldValue - newValue,
                "*=" => oldValue * newValue,
                "/=" => oldValue / newValue,
                _ => throw new($"Unknown compound assignment operator '{node.Operator}'")
            };
            DebugLog($"VisitCompoundAssignmentNode: oldValue = {oldValue}, Operator = {node.Operator} ,newValue = {newValue}, result = {_variables[node.Name]}");
            return _variables[node.Name];
        }

        /// <summary>
        /// Visits an increment/decrement node and performs the operation.
        /// </summary>
        /// <param name="node">The increment/decrement node.</param>
        /// <returns>The result of the increment/decrement operation.</returns>
        private object VisitIncrementDecrementNode(IncrementDecrementNode node)
        {
            DebugLog($"VisitIncrementDecrementNode: Name = {node.Name}, Operator = {node.Operator}", false);

            if (!TryGetVariableValue(node.Name, out var variable))
            {
                throw new($"Undefined variable '{node.Name}'");
            }

            var oldValue = Convert.ToDouble(variable);
            _variables[node.Name] = node.Operator switch
            {
                "++" => oldValue + 1,
                "--" => oldValue - 1,
                _ => throw new($"Unknown increment/decrement operator '{node.Operator}'")
            };
            DebugLog($"VisitIncrementDecrementNode: result = {_variables[node.Name]}");
            return _variables[node.Name];
        }

        /// <summary>
        /// Visits an expression statement node and evaluates the expression.
        /// </summary>
        /// <param name="node">The expression statement node.</param>
        /// <returns>The result of the expression.</returns>
        private object VisitExpressionStatementNode(ExpressionStatementNode node)
        {
            DebugLog($"VisitExpressionStatementNode", false);
            return Visit(node.Expression);
        }

        /// <summary>
        /// Visits a block node and evaluates the statements within the block.
        /// </summary>
        /// <param name="node">The block node.</param>
        /// <returns>The result of the block execution.</returns>
        private object VisitBlockNode(BlockNode node)
        {
            DebugLog($"VisitBlockNode");
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
        /// Visits an if statement node and evaluates the branches.
        /// </summary>
        /// <param name="node">The if statement node.</param>
        /// <returns>The result of the if statement execution.</returns>
        private object VisitIfStatementNode(IfStatementNode node)
        {
            DebugLog($"VisitIfStatementNode");

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
        /// Visits a while statement node and evaluates the loop.
        /// </summary>
        /// <param name="node">The while statement node.</param>
        /// <returns>Null.</returns>
        private object VisitWhileStatementNode(WhileStatementNode node)
        {
            DebugLog($"VisitWhileStatementNode");

            while ((bool)Visit(node.Condition))
            {
                try
                {
                    var result = Visit(node.Body);
                    if (_functionReturnFlag)
                        return result;
                }
                catch (BreakException)
                {
                    break; // Exit the loop
                }
                catch (ContinueException)
                {
                    continue; // Move to the next iteration
                }
            }

            return null;
        }

        /// <summary>
        /// Visits a for statement node and evaluates the loop.
        /// </summary>
        /// <param name="node">The for statement node.</param>
        /// <returns>Null.</returns>
        private object VisitForStatementNode(ForStatementNode node)
        {
            DebugLog($"VisitForStatementNode");

            Visit(node.Initializer);
            while ((bool)Visit(node.Condition))
            {
                try
                {
                    var result = Visit(node.Body);
                    if (_functionReturnFlag)
                        return result;
                }
                catch (BreakException)
                {
                    break; // Exit the loop
                }
                catch (ContinueException)
                {
                    continue; // Move to the next iteration
                }
                finally
                {
                    Visit(node.Increment);
                }
            }

            return null;
        }

        /// <summary>
        /// Visits a function node and registers the function.
        /// </summary>
        /// <param name="node">The function node.</param>
        /// <returns>Null.</returns>
        private object VisitFunctionNode(FunctionNode node)
        {
            DebugLog($"VisitFunctionNode: Name = {node.Name}", false);

            _functions[node.Name] = node;
            return null;
        }

        /// <summary>
        /// Visits a program node and evaluates the program.
        /// </summary>
        /// <param name="node">The program node.</param>
        /// <returns>Null.</returns>
        private object VisitProgramNode(ProgramNode node)
        {
            DebugLog($"VisitProgramNode:", false);

            foreach (var packageNode in node.Packages)
            {
                Visit(packageNode);
            }

            foreach (var globalVar in node.GlobalVariables)
            {
                Visit(globalVar);
            }

            foreach (var function in node.Functions)
            {
                Visit(function);
            }

            Globals.GlobalFunctions = _functions;
            Globals.GlobalVariable = _variables;

            if (node.MainFunction != null)
            {
                _functions[node.MainFunction.Name] = node.MainFunction;
            }

            if (_functions.TryGetValue("_main_", out var mainFunction))
            {
                VisitFunctionCallNode(new(mainFunction.Name, new()));
            }
            else if (node.Statements.Count > 0)
            {
                foreach (var statement in node.Statements)
                {
                    if (statement == null)
                        continue;

                    var result = Visit(statement);
                }
            }
            else
            {
                throw new("No entry point (_main_) defined.");
            }

            return null;
        }

        /// <summary>
        /// Visits a package node and registers the package.
        /// </summary>
        /// <param name="packageNode">The package node.</param>
        /// <returns>Null.</returns>
        private object VisitPackageNode(PackageNode packageNode)
        {
            DebugLog($"VisitPackageNode: Name = {packageNode.Name}");

            Globals.PackageList[packageNode.Name] = packageNode;
            return null;
        }

        /// <summary>
        /// Visits a member access node and evaluates the member access.
        /// </summary>
        /// <param name="node">The member access node.</param>
        /// <returns>The result of the member access.</returns>
        private object VisitMemberAccessNode(MemberAccessNode node)
        {
            DebugLog($"VisitMemberAccessNode: ObjectName = {node.ObjectName}");

            if (node.ObjectName == _packageScope)
                return Visit(node.Expression);

            if (TryGetVariableValue(node.ObjectName, out var instance) && instance is PackageInstance packageInstance)
            {
                return packageInstance.Visit(node.Expression, _variables);
            }

            throw new Exception($"Member access on non-package instance '{instance}'");
        }

        /// <summary>
        /// Tries to get the value of a variable by its name.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="value">The value of the variable.</param>
        /// <returns>True if the variable is found, false otherwise.</returns>
        public bool TryGetVariableValue(string? name, out object value)
        {
            DebugLog($"TryGetVariableValue: Name = {name}", false);

            if (_variables.TryGetValue(name, out value))
                return true;

            if (_outerScopeVariables.TryGetValue(name, out value))
                return true;

            throw new Exception($"Undefined variable '{name}'");
        }

        /// <summary>
        /// Visits an array initializer node and initializes the array.
        /// </summary>
        /// <param name="arrayInitializerNode">The array initializer node.</param>
        /// <returns>The initialized array.</returns>
        private object VisitArrayInitializerNode(ArrayInitializerNode arrayInitializerNode)
        {
            DebugLog($"VisitArrayInitializerNode", false);

            List<object> arrayValues = new();

            var sizeValue = Convert.ToInt32(Visit(arrayInitializerNode.SizeExpression));
            DebugLog($"VisitArrayInitializerNode: ArraySize = {sizeValue}, ElementsCount = {arrayInitializerNode.Elements.Count}");

            if (sizeValue == arrayInitializerNode.Elements.Count)
            {
                foreach (var element in arrayInitializerNode.Elements)
                {
                    arrayValues.Add(Visit(element));
                }
            }
            else
            {
                for (int i = 0; i < sizeValue; i++)
                {
                    arrayValues.Add(0);
                }
            }

            return arrayValues.ToArray();
        }

        /// <summary>
        /// Visits an array access node and retrieves the array element.
        /// </summary>
        /// <param name="node">The array access node.</param>
        /// <returns>The array element.</returns>
        private object VisitArrayAccessNode(ArrayAccessNode node)
        {
            DebugLog($"VisitArrayAccessNode: ArrayName = {node.Name}", false);

            var arrayName = node.Name;
            var index = Convert.ToInt32(Visit(node.Index));

            DebugLog($"VisitArrayAccessNode: index = {index}", false);

            //if (_variables.TryGetValue(arrayName, out var value) && value is object[] array)
            if (TryGetVariableValue(arrayName, out var value) && value is object[] array)
            {
                if (index < 0)
                    index = array.Length + index;

                if (index < 0 || index >= array.Length)
                    throw new($"Index out of bounds for array '{arrayName}'");

                DebugLog($"VisitArrayAccessNode: value = {array[index]}", false);
                return array[index];
            }

            throw new($"Variable '{arrayName}' is not an array");
        }

        /// <summary>
        /// Visits an array assignment node and assigns the value to the array element.
        /// </summary>
        /// <param name="node">The array assignment node.</param>
        /// <returns>The assigned value.</returns>
        private object VisitArrayAssignmentNode(ArrayAssignmentNode node)
        {
            DebugLog($"VisitArrayAssignmentNode: ArrayName = {node.Name}", false);

            var arrayName = node.Name;
            var newValue = Visit(node.Value);
            var index = Visit(node.Index);

            DebugLog($"VisitArrayAssignmentNode: index = {index}, newValue = {newValue}");

            if (!_variables.ContainsKey(arrayName))
            {
                throw new($"Array '{arrayName}' not found in variables.");
            }

            object arrayObj = _variables[arrayName];
            if (arrayObj is object[] array)
            {
                array[Convert.ToInt32(index)] = newValue;
                _variables[arrayName] = array;
                return newValue;
            }

            throw new($"Variable '{arrayName}' is not an array.");
        }

        /// <summary>
        /// Gets the default value for a given type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The default value.</returns>
        private object GetDefaultValue(string? type)
        {
            DebugLog($"GetDefaultValue: Type = {type}");

            return type switch
            {
                "number" => 0,
                "string" => string.Empty,
                "bool" => false,
                "number[]" => new List<object>(),
                "string[]" => new List<object>(),
                "bool[]" => new List<object>(),
                _ => throw new($"Unknown type '{type}'")
            };
        }
    }

}
