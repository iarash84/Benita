
namespace Benita
{
    /// <summary>
    /// The Interpreter class executes the parsed abstract syntax tree (AST) nodes of the Benita language.
    /// It handles expression evaluation, statement execution, function management, and control flow, 
    /// effectively bringing the Benita source code to life through dynamic interpretation.
    /// </summary>
    public class Interpreter : IAstVisitor<object>
    {
        private readonly Dictionary<string, FunctionNode> _functions = [];
        private Dictionary<string, object> _variables = [];
        private readonly Dictionary<string, object> _outerScopeVariables;
        private readonly HashSet<string> _persistentVariableNames = [];

        private bool _functionReturnFlag;
        private readonly bool _debugMode;
        private readonly string _packageScope;
        private readonly DebugClass _debugClass;
        private readonly bool _preserveStateBetweenPrograms;
        private readonly RuntimeContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="Interpreter"/> class.
        /// </summary>
        /// <param name="packageScope">The scope of the package, default is "_main_".</param>
        public Interpreter(bool debugMode = false, string packageScope = "Program",
            bool preserveStateBetweenPrograms = false, RuntimeContext? context = null)
        {
            _debugMode = debugMode;
            _preserveStateBetweenPrograms = preserveStateBetweenPrograms;
            _context = context ?? new RuntimeContext();
            if (debugMode)
            {
                _debugClass = DebugClass.Instance;
            }
            _packageScope = packageScope;
            _outerScopeVariables = new Dictionary<string, object>();
        }

        /// <summary>از state قابل مشاهدهٔ مفسر snapshot می‌گیرد تا اجرای REPL قابل rollback باشد.</summary>
        internal TransactionCheckpoint CreateCheckpoint()
        {
            var checkpoint = new TransactionCheckpoint();
            CaptureState(checkpoint.RootState, checkpoint);
            foreach (var item in _context.GlobalVariables)
                checkpoint.GlobalVariables[item.Key] = CloneCheckpointValue(item.Value, checkpoint, true);
            foreach (var item in _context.GlobalFunctions)
                checkpoint.GlobalFunctions[item.Key] = item.Value;
            foreach (var item in _context.Packages)
                checkpoint.Packages[item.Key] = item.Value;
            return checkpoint;
        }

        /// <summary>تمام تغییرات runtime پس از snapshot را برای نشست REPL بازمی‌گرداند.</summary>
        internal void RestoreCheckpoint(TransactionCheckpoint checkpoint)
        {
            RestoreState(checkpoint.RootState, checkpoint);
            ReplaceValues(_context.GlobalVariables, checkpoint.GlobalVariables, checkpoint);
            ReplaceItems(_context.GlobalFunctions, checkpoint.GlobalFunctions);
            ReplaceItems(_context.Packages, checkpoint.Packages);
            foreach (var item in checkpoint.PackageStates)
                item.Key.RestoreCheckpoint(item.Value, checkpoint);
        }

        internal void CaptureState(InterpreterState state, TransactionCheckpoint checkpoint)
        {
            foreach (var item in _variables)
                state.Variables[item.Key] = CloneCheckpointValue(item.Value, checkpoint, true);
            foreach (var item in _outerScopeVariables)
                state.OuterScopeVariables[item.Key] = CloneCheckpointValue(item.Value, checkpoint, true);
            foreach (var item in _functions)
                state.Functions[item.Key] = item.Value;
            state.PersistentVariableNames.UnionWith(_persistentVariableNames);
            state.FunctionReturnFlag = _functionReturnFlag;
        }

        internal void RestoreState(InterpreterState state, TransactionCheckpoint checkpoint)
        {
            _variables = state.Variables.ToDictionary(item => item.Key,
                item => CloneCheckpointValue(item.Value, checkpoint, false));
            ReplaceValues(_outerScopeVariables, state.OuterScopeVariables, checkpoint);
            ReplaceItems(_functions, state.Functions);
            _persistentVariableNames.Clear();
            _persistentVariableNames.UnionWith(state.PersistentVariableNames);
            _functionReturnFlag = state.FunctionReturnFlag;
        }

        private static object CloneCheckpointValue(object value, TransactionCheckpoint checkpoint,
            bool capturePackages)
        {
            if (value is PackageInstance package)
            {
                if (capturePackages) checkpoint.CapturePackage(package);
                return package;
            }
            if (value is not Array array) return value;

            Dictionary<object, object> clones = capturePackages
                ? checkpoint.CapturedValues
                : checkpoint.RestoredValues;
            if (clones.TryGetValue(array, out object? existingCopy)) return existingCopy;

            var copy = new object[array.Length];
            clones.Add(array, copy);
            for (int index = 0; index < array.Length; index++)
            {
                object? element = array.GetValue(index);
                copy[index] = element is null ? null! : CloneCheckpointValue(element, checkpoint, capturePackages);
            }
            return copy;
        }

        private static void ReplaceValues(Dictionary<string, object> target,
            Dictionary<string, object> source, TransactionCheckpoint checkpoint)
        {
            target.Clear();
            foreach (var item in source)
                target[item.Key] = CloneCheckpointValue(item.Value, checkpoint, false);
        }

        private static void ReplaceItems<T>(Dictionary<string, T> target, Dictionary<string, T> source)
        {
            target.Clear();
            foreach (var item in source) target[item.Key] = item.Value;
        }

        internal sealed class TransactionCheckpoint
        {
            internal InterpreterState RootState { get; } = new();
            internal Dictionary<string, object> GlobalVariables { get; } = [];
            internal Dictionary<string, FunctionNode> GlobalFunctions { get; } = [];
            internal Dictionary<string, PackageNode> Packages { get; } = [];
            internal Dictionary<PackageInstance, InterpreterState> PackageStates { get; } =
                new(ReferenceEqualityComparer.Instance);
            internal Dictionary<object, object> CapturedValues { get; } =
                new(ReferenceEqualityComparer.Instance);
            internal Dictionary<object, object> RestoredValues { get; } =
                new(ReferenceEqualityComparer.Instance);

            internal void CapturePackage(PackageInstance package)
            {
                if (PackageStates.ContainsKey(package)) return;
                var state = new InterpreterState();
                PackageStates.Add(package, state);
                package.CaptureCheckpoint(state, this);
            }
        }

        internal sealed class InterpreterState
        {
            internal Dictionary<string, object> Variables { get; } = [];
            internal Dictionary<string, object> OuterScopeVariables { get; } = [];
            internal Dictionary<string, FunctionNode> Functions { get; } = [];
            internal HashSet<string> PersistentVariableNames { get; } = [];
            internal bool FunctionReturnFlag { get; set; }
        }

        private void DebugLog(string message, bool pressKeyWait = true)
        {
            if (_debugMode)
            {
                _debugClass.DebugLog(message, _variables, _outerScopeVariables, _functions, _context, pressKeyWait);
            }
        }

        /// <summary>
        /// Sets the outer scope variables for the interpreter.
        /// </summary>
        /// <param name="outerScopeVariables">A dictionary of variables in the outer scope.</param>
        public void SetOuterScopeVariables(Dictionary<string, object> outerScopeVariables)
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
            foreach (var kvp in _context.GlobalVariables)
                _variables.Add(kvp.Key, kvp.Value);
        }

        /// <summary>متغیرهای فعلی را به‌عنوان state پایدار یک instance ثبت می‌کند.</summary>
        internal void MarkCurrentVariablesAsPersistent()
        {
            foreach (string name in _variables.Keys)
                _persistentVariableNames.Add(name);
        }

        /// <summary>
        /// Visits the specified AST node and executes the corresponding logic.
        /// </summary>
        /// <param name="node">The AST node to visit.</param>
        /// <returns>The result of the visit.</returns>
        public object Visit(AstNode? node)
        {
            if (node is null) return null;
            DebugLog($"Visit: {node.GetType().Name}, packageScope = {_packageScope}", false);
            switch (node)
            {
                case ProgramNode programNode:
                    return VisitProgramNode(programNode);
                case BlockNode blockNode:
                    return VisitBlockNode(blockNode);
                case LiteralNode literalNode:
                    return VisitLiteralNode(literalNode);
                case RuntimeValueNode runtimeValueNode:
                    return runtimeValueNode.Value;
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
                case AsyncExpressionNode asyncExpressionNode:
                    return VisitAsyncExpressionNode(asyncExpressionNode);
                case AwaitExpressionNode awaitExpressionNode:
                    return VisitAwaitExpressionNode(awaitExpressionNode);
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
                case MatchExpressionNode matchExpressionNode:
                    return VisitMatchExpressionNode(matchExpressionNode);
                case MatchStatementNode matchStatementNode:
                    return VisitMatchStatementNode(matchStatementNode);
                case WhileStatementNode whileStatementNode:
                    return VisitWhileStatementNode(whileStatementNode);
                case ForStatementNode forStatementNode:
                    return VisitForStatementNode(forStatementNode);
                case ForEachStatementNode forEachStatementNode:
                    return VisitForEachStatementNode(forEachStatementNode);
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
                case ThrowStatementNode throwStatementNode:
                    return VisitThrowStatementNode(throwStatementNode);
                case TryStatementNode tryStatementNode:
                    return VisitTryStatementNode(tryStatementNode);
                case PackageNode packageNode:
                    return VisitPackageNode(packageNode);
                case MemberAccessNode memberAccessNode:
                    return VisitMemberAccessNode(memberAccessNode);
                case NewExpressionNode newExpressionNode:
                    return VisitNewExpressionNode(newExpressionNode);
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
            if (!_context.Packages.TryGetValue(node.PackageName, out var packageNode))
            {
                throw new($"Package '{node.PackageName}' not found.");
            }

            // Create a new package instance
            List<object?> arguments = node.Arguments.Select(Visit).ToList();
            var packageInstance = new PackageInstance(node.Name, packageNode, arguments, _debugMode, _context);

            // Optionally, you might handle constructor arguments here
            // For simplicity, we assume no arguments or default constructor logic.

            // Store the new instance in the variables dictionary
            _variables[node.Name] = packageInstance;

            DebugLog($"Object {node.Name} instantiated.");
            return packageInstance;
        }

        /// <summary>
        /// یک نمونه تازه از package می‌سازد و آن را به‌عنوان مقدار expression برمی‌گرداند.
        /// </summary>
        private object VisitNewExpressionNode(NewExpressionNode node)
        {
            if (!_context.Packages.TryGetValue(node.PackageName, out PackageNode? packageNode))
                throw new Exception($"Package '{node.PackageName}' not found.");

            List<object?> arguments = node.Arguments.Select(Visit).ToList();
            return new PackageInstance(node.PackageName, packageNode, arguments, _debugMode, _context);
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
                TokenType.NUMBER or TokenType.NUMBER_LITERAL => double.Parse(node.Value,
                    System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture),
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
                    return ValuesAreEqual(left, right);

                case "!=":
                    return !ValuesAreEqual(left, right);

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
            return node.Operator switch
            {
                "&&" => Convert.ToBoolean(left) && Convert.ToBoolean(Visit(node.Right)),
                "||" => Convert.ToBoolean(left) || Convert.ToBoolean(Visit(node.Right)),
                _ => throw new($"Unknown operator '{node.Operator}' for logical expression")
            };
        }

        /// <summary>
        /// Attempts to retrieve a function by name.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="value">The retrieved function node.</param>
        /// <returns>True if the function was found; otherwise, false.</returns>
        public bool TryGetFunction(string name, out FunctionNode value)
        {
            DebugLog($"TryGetFunction: Name = {name}", false);

            if (_functions.TryGetValue(name, out value))
                return true;

            if (_context.GlobalFunctions.TryGetValue(name, out value))
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
                if (node.Arguments.Count != function.Parameters.Count)
                {
                    throw new ArgumentException(
                        $"Function '{node.FunctionName}' expects {function.Parameters.Count} argument(s), but received {node.Arguments.Count}.");
                }

                var newScope = new Dictionary<string, object>(_variables);

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
                finally
                {
                    _functionReturnFlag = false;
                    SynchronizeFunctionScope(_variables, originalVariables);
                    _variables = originalVariables;
                    _functionReturnFlag = originalFunctionReturnFlag;
                }
            }

            if (BuiltInRegistry.TryGet(node.FunctionName, out BuiltInDescriptor descriptor))
            {
                List<object> arguments = (from argument in node.Arguments select Visit(argument)).ToList();
                return descriptor.HandlerFactory().HandleFunctionCall(node.FunctionName, arguments);
            }

            throw new($"Unknown function '{node.FunctionName}'");
        }

        /// <summary>
        /// تغییرات متغیرهای قابل مشاهده در scope فراخواننده را پس از پایان تابع منتقل می‌کند.
        /// </summary>
        /// <param name="variables">The current variable dictionary.</param>
        /// <param name="originalVariables">The original variable dictionary.</param>
        private void SynchronizeFunctionScope(Dictionary<string, object> variables,
            Dictionary<string, object> originalVariables)
        {
            DebugLog($"SynchronizeFunctionScope");

            foreach (string key in originalVariables.Keys.ToList())
            {
                bool isPersistent = _persistentVariableNames.Contains(key) ||
                                    _context.GlobalVariables.ContainsKey(key);
                if (isPersistent && variables.TryGetValue(key, out object? value))
                {
                    originalVariables[key] = value;
                    if (_context.GlobalVariables.ContainsKey(key))
                        _context.GlobalVariables[key] = value;
                }
            }
        }

        /// <summary>برابری runtime را مطابق قرارداد scalar زبان محاسبه می‌کند.</summary>
        private static bool ValuesAreEqual(object left, object right)
            => RuntimeValueComparer.AreEqual(left, right);

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

        /// <summary>آرگومان‌ها و scope فعلی را snapshot می‌گیرد و فراخوانی را روی thread pool اجرا می‌کند.</summary>
        private object VisitAsyncExpressionNode(AsyncExpressionNode node)
        {
            var context = new RuntimeContext();
            foreach (var item in _context.GlobalFunctions)
                context.GlobalFunctions[item.Key] = item.Value;
            foreach (var item in _functions)
                context.GlobalFunctions[item.Key] = item.Value;
            foreach (var item in _context.Packages)
                context.Packages[item.Key] = item.Value;

            var cloneContext = new TaskCloneContext(context);
            foreach (var item in _context.GlobalVariables)
                context.GlobalVariables[item.Key] = CloneTaskValue(item.Value, cloneContext);

            List<ExpressionNode> arguments = node.Call.Arguments
                .Select(argument => (ExpressionNode)new RuntimeValueNode(
                    CloneTaskValue(Visit(argument), cloneContext)))
                .ToList();
            var call = new FunctionCallNode(node.Call.FunctionName, arguments);
            Dictionary<string, object> variables = _variables.ToDictionary(
                item => item.Key, item => CloneTaskValue(item.Value, cloneContext));

            return new TaskValue(System.Threading.Tasks.Task.Run(() =>
            {
                var interpreter = new Interpreter(_debugMode, _packageScope, true, context)
                {
                    _variables = variables
                };
                return interpreter.VisitFunctionCallNode(call);
            }));
        }

        /// <summary>نتیجهٔ task را بدون پوشاندن exception اصلی برمی‌گرداند.</summary>
        private object VisitAwaitExpressionNode(AwaitExpressionNode node)
        {
            if (Visit(node.Task) is not TaskValue task)
                throw new RuntimeException("'await' requires a task value.");
            return task.Task.GetAwaiter().GetResult();
        }

        /// <summary>یک مقدار mutable را برای task با حفظ aliasها و جلوگیری از cycle clone می‌کند.</summary>
        private static object CloneTaskValue(object value, TaskCloneContext context)
        {
            if (value is PackageInstance package)
                return package.CloneForTask(context);
            if (value is not Array array) return value;
            if (context.Values.TryGetValue(array, out object? existing)) return existing;

            var copy = new object[array.Length];
            context.Values.Add(array, copy);
            for (int index = 0; index < array.Length; index++)
            {
                object? item = array.GetValue(index);
                copy[index] = item is null ? null! : CloneTaskValue(item, context);
            }
            return copy;
        }

        /// <summary>یک shell خالی با تنظیمات همین مفسر برای clone داخلی package می‌سازد.</summary>
        internal Interpreter CreateTaskCloneShell(RuntimeContext context) =>
            new(_debugMode, _packageScope, true, context);

        /// <summary>state داخلی مفسر package را بدون اجرای initializer به shell مقصد منتقل می‌کند.</summary>
        internal void CopyTaskStateTo(Interpreter target, TaskCloneContext context)
        {
            target._variables = _variables.ToDictionary(item => item.Key,
                item => CloneTaskValue(item.Value, context));
            foreach (var item in _outerScopeVariables)
                target._outerScopeVariables[item.Key] = CloneTaskValue(item.Value, context);
            foreach (var item in _functions)
                target._functions[item.Key] = item.Value;
            target._persistentVariableNames.UnionWith(_persistentVariableNames);
            target._functionReturnFlag = _functionReturnFlag;
        }

        internal sealed class TaskCloneContext(RuntimeContext runtimeContext)
        {
            internal RuntimeContext RuntimeContext { get; } = runtimeContext;
            internal Dictionary<object, object> Values { get; } =
                new(ReferenceEqualityComparer.Instance);
            internal Dictionary<PackageInstance, PackageInstance> Packages { get; } =
                new(ReferenceEqualityComparer.Instance);
        }

        /// <summary>مقدار error را ارزیابی و برای انتقال به نزدیک‌ترین catch پرتاب می‌کند.</summary>
        private object VisitThrowStatementNode(ThrowStatementNode node)
        {
            if (Visit(node.Error) is not ErrorValue error)
                throw new RuntimeException("A throw statement requires an error value.");
            throw new ThrownErrorException(error);
        }

        /// <summary>شاخه‌های try، catch و finally را با حفظ درست return و خطای در حال انتشار اجرا می‌کند.</summary>
        private object VisitTryStatementNode(TryStatementNode node)
        {
            object? result = null;
            try
            {
                try
                {
                    result = Visit(node.TryBlock);
                }
                catch (Exception exception) when (exception is not BreakException and not ContinueException)
                {
                    if (node.CatchBlock is null)
                        throw;

                    ErrorValue error = exception switch
                    {
                        ThrownErrorException thrown => thrown.Error,
                        BenitaException benita => new ErrorValue(benita.Code, benita.Description),
                        _ => new ErrorValue("BEN5000", exception.Message)
                    };

                    bool hadPrevious = _variables.TryGetValue(node.CatchVariable!, out object? previous);
                    _variables[node.CatchVariable!] = error;
                    try
                    {
                        result = Visit(node.CatchBlock);
                    }
                    finally
                    {
                        if (hadPrevious)
                            _variables[node.CatchVariable!] = previous!;
                        else
                            _variables.Remove(node.CatchVariable!);
                    }
                }
            }
            finally
            {
                if (node.FinallyBlock is not null)
                {
                    bool pendingReturn = _functionReturnFlag;
                    _functionReturnFlag = false;
                    Visit(node.FinallyBlock);
                    if (_functionReturnFlag)
                        throw new RuntimeException("Control cannot leave a finally block with return.");
                    _functionReturnFlag = pendingReturn;
                }
            }

            return result;
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

            while (node.Condition is null || (bool)Visit(node.Condition))
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
            while (node.Condition is null || (bool)Visit(node.Condition))
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
                    Visit(node.Increment);
                    continue; // Move to the next iteration
                }

                Visit(node.Increment);
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
            try
            {
                return ExecuteProgramNode(node);
            }
            catch (ThrownErrorException exception)
            {
                throw new UnhandledErrorException(exception.Error, exception);
            }
        }

        /// <summary>گره برنامه را اجرا می‌کند؛ تبدیل خطای مدیریت‌نشده در wrapper عمومی انجام می‌شود.</summary>
        private object ExecuteProgramNode(ProgramNode node)
        {
            DebugLog($"VisitProgramNode:", false);

            if (!_preserveStateBetweenPrograms)
            {
                _context.Clear();
                _variables.Clear();
                _functions.Clear();
                _outerScopeVariables.Clear();
                _persistentVariableNames.Clear();
            }

            foreach (var packageNode in node.Packages)
            {
                Visit(packageNode);
            }

            // تابع‌ها باید هنگام ارزیابی initializerهای سراسری قابل فراخوانی باشند.
            foreach (var function in node.Functions)
            {
                Visit(function);
            }

            foreach (var globalVar in node.GlobalVariables)
            {
                Visit(globalVar);
                _persistentVariableNames.Add(globalVar.Name);
            }

            Synchronize(_context.GlobalFunctions, _functions);
            Synchronize(_context.GlobalVariables, _variables);

            if (node.MainFunction != null)
            {
                _functions[node.MainFunction.Name] = node.MainFunction;
            }

            if (node.MainFunction != null && _functions.TryGetValue("_main_", out var mainFunction))
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

            Synchronize(_context.GlobalFunctions, _functions);
            Synchronize(_context.GlobalVariables, _variables);
            return null;
        }

        /// <summary>تمام عناصر آرایه را به‌ترتیب در متغیر iteration قرار می‌دهد.</summary>
        private object VisitForEachStatementNode(ForEachStatementNode node)
        {
            object iterable = Visit(node.Iterable);
            if (iterable is not Array values)
                throw new Exception("The expression after 'in' must evaluate to an array.");

            bool hadPreviousValue = _variables.TryGetValue(node.VariableName, out object previousValue);
            try
            {
                foreach (object? value in values)
                {
                    _variables[node.VariableName] = value;
                    try
                    {
                        object result = Visit(node.Body);
                        if (_functionReturnFlag)
                            return result;
                    }
                    catch (BreakException)
                    {
                        break;
                    }
                    catch (ContinueException)
                    {
                        continue;
                    }
                }
            }
            finally
            {
                if (hadPreviousValue)
                    _variables[node.VariableName] = previousValue;
                else
                    _variables.Remove(node.VariableName);
            }

            return null;
        }

        /// <summary>اولین شاخه منطبق match مقدارساز را ارزیابی می‌کند.</summary>
        private object VisitMatchExpressionNode(MatchExpressionNode node)
        {
            object value = Visit(node.Value);
            MatchArm? defaultArm = null;
            foreach (MatchArm arm in node.Arms)
            {
                if (arm.IsDefault)
                {
                    defaultArm = arm;
                    continue;
                }
                if (MatchArmAccepts(arm, value))
                    return Visit(arm.Body);
            }
            return Visit(defaultArm!.Body);
        }

        /// <summary>اولین شاخه منطبق match دستوری را اجرا می‌کند.</summary>
        private object VisitMatchStatementNode(MatchStatementNode node)
        {
            object value = Visit(node.Value);
            MatchArm? defaultArm = null;
            foreach (MatchArm arm in node.Arms)
            {
                if (arm.IsDefault)
                {
                    defaultArm = arm;
                    continue;
                }
                if (MatchArmAccepts(arm, value))
                    return Visit(arm.Body);
            }
            return defaultArm is null ? null : Visit(defaultArm.Body);
        }

        private bool MatchArmAccepts(MatchArm arm, object value)
        {
            foreach (MatchPatternNode pattern in arm.Patterns)
            {
                if (pattern is ValueMatchPatternNode valuePattern &&
                    ValuesAreEqual(value, Visit(valuePattern.Value)))
                    return true;
                if (pattern is RangeMatchPatternNode range)
                {
                    double candidate = Convert.ToDouble(value);
                    double start = Convert.ToDouble(Visit(range.Start));
                    double end = Convert.ToDouble(Visit(range.End));
                    if (candidate >= start && candidate <= end)
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Visits a package node and registers the package.
        /// </summary>
        /// <param name="packageNode">The package node.</param>
        /// <returns>Null.</returns>
        private object VisitPackageNode(PackageNode packageNode)
        {
            DebugLog($"VisitPackageNode: Name = {packageNode.Name}");

            _context.Packages[packageNode.Name] = packageNode;
            return null;
        }

        private static void Synchronize<TKey, TValue>(Dictionary<TKey, TValue> target,
            Dictionary<TKey, TValue> source) where TKey : notnull
        {
            target.Clear();
            foreach (var item in source) target[item.Key] = item.Value;
        }

        /// <summary>
        /// Visits a member access node and evaluates the member access.
        /// </summary>
        /// <param name="node">The member access node.</param>
        /// <returns>The result of the member access.</returns>
        private object VisitMemberAccessNode(MemberAccessNode node)
        {
            DebugLog($"VisitMemberAccessNode: ObjectName = {node.ObjectName}");

            if (node.ObjectName == "this" || node.ObjectName == _packageScope)
                return Visit(node.Expression);

            if (TryGetVariableValue(node.ObjectName, out var errorValue) && errorValue is ErrorValue error)
            {
                if (node.Expression is IdentifierNode member)
                    return member.Name switch
                    {
                        "code" => error.Code,
                        "message" => error.Message,
                        _ => throw new RuntimeException($"Error has no member named '{member.Name}'.")
                    };
                throw new RuntimeException("Error members are read-only.");
            }

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
        public bool TryGetVariableValue(string name, out object value)
        {
            DebugLog($"TryGetVariableValue: Name = {name}", false);

            if (_variables.TryGetValue(name, out value))
                return true;

            if (_outerScopeVariables.TryGetValue(name, out value))
                return true;

            value = null!;
            return false;
        }

        /// <summary>
        /// Visits an array initializer node and initializes the array.
        /// </summary>
        /// <param name="arrayInitializerNode">The array initializer node.</param>
        /// <returns>The initialized array.</returns>
        private object VisitArrayInitializerNode(ArrayInitializerNode arrayInitializerNode)
        {
            DebugLog($"VisitArrayInitializerNode", false);

            if (arrayInitializerNode.ElementType is null)
                return arrayInitializerNode.Elements.Select(Visit).ToArray();

            double requestedSize = Convert.ToDouble(Visit(arrayInitializerNode.SizeExpression));
            if (!double.IsFinite(requestedSize) || requestedSize < 0 || requestedSize != Math.Truncate(requestedSize) ||
                requestedSize > int.MaxValue)
            {
                throw new RuntimeException("Array length must be a non-negative whole number within the supported range.");
            }

            int size = (int)requestedSize;
            object defaultValue = GetDefaultValue(arrayInitializerNode.ElementType);
            DebugLog($"VisitArrayInitializerNode: ArraySize = {size}, ElementType = {arrayInitializerNode.ElementType}");
            return Enumerable.Repeat(defaultValue, size).ToArray();
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
                "number" => 0d,
                "string" => string.Empty,
                "bool" => false,
                "number[]" or "string[]" or "bool[]" => Array.Empty<object>(),
                _ => throw new($"Unknown type '{type}'")
            };
        }
    }

}
