namespace Benita
{
    /// <summary>
    /// The SemanticAnalyzer class performs semantic analysis on a program to ensure that variables are declared before use,
    /// functions are properly defined, and expressions are correctly typed.
    /// </summary>
    public class SemanticAnalyzer
    {
        /// <summary>آخرین موقعیت AST در حال تحلیل را برای تبدیل خطای داخلی به diagnostic نگه می‌دارد.</summary>
        internal SourceSpan CurrentSpan { get; private set; } = SourceSpan.Unknown;
        /// <summary>
        /// Dictionary to store packages names and their associated PackageNode.
        /// </summary>
        private readonly Dictionary<string, PackageNode> _packages;
        /// <summary>قراردادهای interface ثبت‌شده را برای تحلیل نوع و member access نگه می‌دارد.</summary>
        private readonly Dictionary<string, InterfaceNode> _interfaces;


        /// <summary>
        /// Dictionary to store global variables and their types.
        /// </summary>
        private readonly Dictionary<string, string?> _globalVariables;

        /// <summary>
        /// Dictionary to store function names and their associated FunctionNode.
        /// </summary>
        private readonly Dictionary<string, FunctionNode> _functions;

        /// <summary>
        /// Dictionary to store default functions with their return types and parameter types.
        /// </summary>
        private readonly Dictionary<string, (TypeSymbol ReturnType, List<TypeSymbol> ParameterTypes)> _defaultFunctions;


        /// <summary>
        /// Initializes a new instance of the SemanticAnalyzer class.
        /// </summary>
        public SemanticAnalyzer()
        {
            _globalVariables = new Dictionary<string, string?>();
            _functions = new Dictionary<string, FunctionNode>();
            _packages = new Dictionary<string, PackageNode>();
            _interfaces = new Dictionary<string, InterfaceNode>();
            _defaultFunctions = BuiltInRegistry.Descriptors.Values.ToDictionary(
                descriptor => descriptor.Name,
                descriptor => (descriptor.ReturnType, descriptor.ParameterTypes.ToList()),
                StringComparer.Ordinal);
        }

        /// <summary>
        /// Analyzes the provided ProgramNode to perform semantic checks.
        /// </summary>
        /// <param name="program">The ProgramNode to analyze.</param>
        public void Analyze(ProgramNode program)
        {
            _packages.Clear();
            _interfaces.Clear();
            _globalVariables.Clear();
            _functions.Clear();

            foreach (InterfaceNode interfaceNode in program.Interfaces)
                DeclareInterface(interfaceNode);
            foreach (var packageNode in program.Packages)
                DeclarePackage(packageNode);
            // امضاها پیش از تحلیل هر call site ثبت می‌شوند تا forward call در initializer نیز معتبر باشد.
            foreach (var function in program.Functions)
                DeclareFunction(function);
            foreach (InterfaceNode interfaceNode in program.Interfaces)
                AnalyzeInterface(interfaceNode);

            // globalها پیش از body بسته‌ها تحلیل می‌شوند تا همان outer scope قابل مشاهده در runtime
            // هنگام تحلیل متدها و initializerهای package نیز در دسترس باشد.
            foreach (var globalVar in program.GlobalVariables)
            {
                var declaredType = globalVar.Type;
                EnsureKnownType(declaredType);
                RequireInitializerForNamedType(declaredType, globalVar.Initializer, globalVar.Name);
                if (globalVar.Initializer != null)
                {
                    var initializerType = AnalyzeExpression(globalVar.Initializer, _globalVariables);
                    if (declaredType == "let")
                    {
                        declaredType = initializerType;
                        RequireConcreteInferredType(declaredType, globalVar.Name);
                    }
                    else if (!CheckType(declaredType!, initializerType))
                    {
                        throw new Exception(
                            $"Type mismatch in global variable '{globalVar.Name}'. Expected '{declaredType}' but got '{initializerType}'.");
                    }
                }
                else if (declaredType == "let")
                {
                    RequireConcreteInferredType(declaredType, globalVar.Name);
                }

                DeclareVariable(globalVar.Name, declaredType, _globalVariables);
            }

            foreach (var packageNode in program.Packages)
                AnalyzePackage(packageNode);

            foreach (var function in program.Functions)
            {
                AnalyzeFunction(function);
            }

            if (program.Statements != null)
            {
                var topLevelVariables = new Dictionary<string, string?>(_globalVariables);
                foreach (var statement in program.Statements)
                {
                    if (statement != null)
                        AnalyzeStatement(statement, topLevelVariables);
                }
            }

            // Analyze the main function
            if (program.MainFunction != null)
                AnalyzeFunction(program.MainFunction);
        }

        /// <summary>نام package را ثبت و از برخورد نام آن با سایر نوع‌ها جلوگیری می‌کند.</summary>
        private void DeclarePackage(PackageNode packageNode)
        {
            if (packageNode.Name == Types.Error.Name)
                throw new Exception("The name 'error' is reserved by the language error type.");
            if (_packages.ContainsKey(packageNode.Name) || _interfaces.ContainsKey(packageNode.Name))
            {
                throw new Exception($"Package '{packageNode.Name}' is already declared.");
            }
            _packages[packageNode.Name] = packageNode;
        }

        /// <summary>نام interface را پیش از تحلیل بدنه‌ها ثبت و تکراری‌بودن نوع را بررسی می‌کند.</summary>
        private void DeclareInterface(InterfaceNode interfaceNode)
        {
            if (interfaceNode.Name == Types.Error.Name)
                throw new Exception("The name 'error' is reserved by the language error type.");
            if (_interfaces.ContainsKey(interfaceNode.Name) || _packages.ContainsKey(interfaceNode.Name))
                throw new Exception($"Type '{interfaceNode.Name}' is already declared.");
            _interfaces[interfaceNode.Name] = interfaceNode;
        }

        /// <summary>نوع‌های امضا و یکتایی متدها و پارامترهای یک interface را اعتبارسنجی می‌کند.</summary>
        private void AnalyzeInterface(InterfaceNode interfaceNode)
        {
            HashSet<string> methodNames = [];
            foreach (InterfaceMethodNode method in interfaceNode.Methods)
            {
                if (!methodNames.Add(method.Name))
                    throw new Exception($"Interface '{interfaceNode.Name}' declares method '{method.Name}' more than once.");
                EnsureKnownType(method.ReturnType, allowVoid: true);
                HashSet<string> parameterNames = [];
                foreach (ParameterNode parameter in method.Parameters)
                {
                    EnsureKnownType(parameter.Type);
                    if (!parameterNames.Add(parameter.Name))
                        throw new Exception($"Parameter '{parameter.Name}' is already declared in '{method.Name}'.");
                }
            }
        }

        /// <summary>فیلدها، مقداردهی اولیه و متدهای یک package را تحلیل معنایی می‌کند.</summary>
        private void AnalyzePackage(PackageNode packageNode)
        {
            ValidateImplementedInterfaces(packageNode);
            Dictionary<string, string?> allFieldScope = new(_globalVariables)
            {
                ["this"] = packageNode.Name
            };
            Dictionary<string, PackageFunctionNode> packageFunctions = new Dictionary<string, PackageFunctionNode>();
            HashSet<string> fieldNames = [];

            foreach (var packageMember in packageNode.Members)
            {
                if (packageMember is PackageVariableDeclarationNode variableDeclaration)
                {
                    if (!fieldNames.Add(variableDeclaration.Name))
                        throw new Exception($"Variable '{variableDeclaration.Name}' is already declared.");
                    TypeSymbol fieldType = TypeFacts.FromName(variableDeclaration.Type);
                    EnsureKnownType(fieldType.Name, allowLet: true);
                    RequireInitializerForNamedType(fieldType.Name, variableDeclaration.Initializer,
                        variableDeclaration.Name);
                    allFieldScope[variableDeclaration.Name] = fieldType.Name;
                }
            }

            Dictionary<string, string?> initializedFieldScope = new(_globalVariables)
            {
                ["this"] = packageNode.Name
            };
            foreach (var field in packageNode.Members.OfType<PackageVariableDeclarationNode>())
            {
                if (field.Initializer is null)
                {
                    if (field.Type == "let") RequireConcreteInferredType(field.Type, field.Name);
                    initializedFieldScope[field.Name] = field.Type;
                    continue;
                }
                string? initializerType = AnalyzeExpression(field.Initializer, initializedFieldScope);
                if (field.Type == "let")
                {
                    RequireConcreteInferredType(initializerType, field.Name);
                    field.Type = initializerType;
                    allFieldScope[field.Name] = initializerType;
                }
                else if (!CheckType(field.Type, initializerType))
                    throw new Exception($"Type mismatch in field '{field.Name}'. Expected '{field.Type}' but got '{initializerType}'.");
                initializedFieldScope[field.Name] = field.Type;
            }

            foreach (var packageMember in packageNode.Members)
            {
                if (packageMember is not PackageFunctionNode functionNode) continue;
                if (!packageFunctions.TryAdd(functionNode.Name, functionNode))
                    throw new Exception($"Function '{functionNode.Name}' is already declared.");
                AnalyzePackageFunction(functionNode, allFieldScope);
            }
        }

        /// <summary>کامل‌بودن، public بودن و تطابق دقیق امضای پیاده‌سازی interfaceها را بررسی می‌کند.</summary>
        private void ValidateImplementedInterfaces(PackageNode package)
        {
            HashSet<string> implemented = [];
            foreach (string interfaceName in package.Interfaces)
            {
                if (!implemented.Add(interfaceName))
                    throw new Exception($"Package '{package.Name}' lists interface '{interfaceName}' more than once.");
                if (!_interfaces.TryGetValue(interfaceName, out InterfaceNode? contract))
                    throw new Exception($"Unknown interface '{interfaceName}'.");
                foreach (InterfaceMethodNode required in contract.Methods)
                {
                    PackageFunctionNode? implementation = package.Members.OfType<PackageFunctionNode>()
                        .FirstOrDefault(method => method.Name == required.Name);
                    if (implementation is null)
                        throw new Exception($"Package '{package.Name}' does not implement '{interfaceName}.{required.Name}'.");
                    if (implementation.AccessModifier != AccessModifier.Public)
                        throw new Exception($"Implementation '{package.Name}.{required.Name}' must be public.");
                    if (implementation.ReturnType != required.ReturnType ||
                        implementation.Parameters.Count != required.Parameters.Count ||
                        implementation.Parameters.Where((parameter, index) =>
                            parameter.Type != required.Parameters[index].Type).Any())
                        throw new Exception($"Method '{package.Name}.{required.Name}' does not match interface '{interfaceName}'.");
                }
            }
        }

        private void AnalyzePackageFunction(PackageFunctionNode function, Dictionary<string, string?> variableScope)
        {
            EnsureKnownType(function.ReturnType, allowVoid: true);
            // Create a new scope for local variables
            var localVariables = new Dictionary<string, string?>(variableScope);

            // Declare function parameters in the local scope
            foreach (var param in function.Parameters)
            {
                TypeSymbol parameterType = TypeFacts.FromName(param.Type);
                EnsureKnownType(parameterType.Name);
                DeclareVariable(param.Name, param.Type, localVariables);
            }

            // Analyze the function body
            AnalyzeBlock(function.Body, localVariables, function.ReturnType);
            if (function.ReturnType != "void" && !AlwaysReturns(function.Body))
            {
                throw new Exception($"Function '{function.Name}' must return a value of type '{function.ReturnType}'.");
            }
        }

        /// <summary>
        /// Declares a function and performs analysis on it.
        /// </summary>
        /// <param name="function">The function to declare and analyze.</param>
        private void DeclareFunction(FunctionNode function)
        {
            if (function.Name == Types.Error.Name)
                throw new Exception("The function name 'error' is reserved by the language runtime.");
            if (_functions.ContainsKey(function.Name))
            {
                throw new Exception($"Function '{function.Name}' is already declared.");
            }

            _functions[function.Name] = function;
        }

        /// <summary>
        /// Analyzes a function's parameters, body, and return type.
        /// </summary>
        /// <param name="function">The function to analyze.</param>
        private void AnalyzeFunction(FunctionNode function)
        {
            EnsureKnownType(function.ReturnType, allowVoid: true);
            // Create a new scope for local variables
            var localVariables = new Dictionary<string, string?>(_globalVariables);

            // Declare function parameters in the local scope
            foreach (var param in function.Parameters)
            {
                EnsureKnownType(param.Type);
                DeclareVariable(param.Name, param.Type, localVariables);
            }

            // Analyze the function body
            AnalyzeBlock(function.Body, localVariables, function.ReturnType);
            if (function.ReturnType != "void" && !AlwaysReturns(function.Body))
            {
                throw new Exception($"Function '{function.Name}' must return a value of type '{function.ReturnType}'.");
            }
        }

        /// <summary>
        /// Analyzes a block of statements.
        /// </summary>
        /// <param name="block">The block of statements to analyze.</param>
        /// <param name="localVariables">The local variables available in the block.</param>
        /// <param name="functionReturnType">The return type of the function (if any).</param>
        private void AnalyzeBlock(BlockNode block, Dictionary<string, string?> localVariables,
            string? functionReturnType = null, int loopDepth = 0)
        {
            foreach (var statement in block.Statements)
            {
                AnalyzeStatement(statement, localVariables, functionReturnType, loopDepth);
            }
        }

        /// <summary>
        /// Analyzes a single statement.
        /// </summary>
        /// <param name="statement">The statement to analyze.</param>
        /// <param name="localVariables">The local variables available in the current scope.</param>
        /// <param name="functionReturnType">The return type of the function (if any).</param>
        private void AnalyzeStatement(StatementNode statement, Dictionary<string, string?> localVariables,
            string? functionReturnType = null, int loopDepth = 0)
        {
            if (statement.Span.Line > 0) CurrentSpan = statement.Span;
            switch (statement)
            {
                case VariableDeclarationNode varDecl:
                    string? declaredType = varDecl.Type;
                    EnsureKnownType(declaredType, allowLet: true);
                    RequireInitializerForNamedType(declaredType, varDecl.Initializer, varDecl.Name);
                    if (varDecl.Type == "let")
                    {
                        declaredType = varDecl.Initializer != null
                            ? AnalyzeExpression(varDecl.Initializer, localVariables)
                            : varDecl.Type;
                        RequireConcreteInferredType(declaredType, varDecl.Name);
                    }
                    else if (varDecl.Initializer != null)
                    {
                        string? initializerType = AnalyzeExpression(varDecl.Initializer, localVariables);
                        if (!CheckType(varDecl.Type!, initializerType))
                        {
                            throw new Exception(
                                $"Type mismatch in variable '{varDecl.Name}'. Expected '{varDecl.Type}' but got '{initializerType}'.");
                        }
                    }
                    DeclareVariable(varDecl.Name, declaredType, localVariables);
                    break;
                case AssignmentNode assignment:
                    var valueType = AnalyzeExpression(assignment.Expression, localVariables);
                    if (!localVariables.TryGetValue(assignment.Name, out var variableType))
                    {
                        throw new Exception($"Undeclared variable '{assignment.Name}'.");
                    }

                    if (!CheckType(variableType, valueType))
                    {
                        throw new Exception($"Type mismatch in assignment to '{assignment.Name}'. Expected '{variableType}' but got '{valueType}'.");
                    }
                    break;
                case CompoundAssignmentNode compoundAssignment:
                    var compValueType = AnalyzeExpression(compoundAssignment.Expression, localVariables);
                    if (!localVariables.TryGetValue(compoundAssignment.Name, out var compVariableType))
                    {
                        throw new Exception($"Undeclared variable '{compoundAssignment.Name}'.");
                    }

                    if (compVariableType != "number" || compValueType != "number")
                    {
                        throw new Exception($"Compound assignment requires numeric operands, but '{compoundAssignment.Name}' is '{compVariableType}' and the value is '{compValueType}'.");
                    }
                    break;
                case IncrementDecrementNode incDec:
                    if (!localVariables.TryGetValue(incDec.Name, out var incrementType))
                    {
                        throw new Exception($"Undeclared variable '{incDec.Name}'.");
                    }
                    if (incrementType != "number")
                    {
                        throw new Exception($"Increment and decrement require a number, but '{incDec.Name}' is '{incrementType}'.");
                    }
                    break;
                case ExpressionStatementNode exprStmt:
                    AnalyzeExpression(exprStmt.Expression, localVariables);
                    break;
                case IfStatementNode ifStmt:
                    var conditionType = AnalyzeExpression(ifStmt.Condition, localVariables);
                    if (conditionType != "bool")
                    {
                        throw new Exception("Condition in 'if' statement must be a boolean.");
                    }
                    AnalyzeStatement(ifStmt.ThenBranch, new(localVariables), functionReturnType, loopDepth);
                    if (ifStmt.ElseBranch != null)
                    {
                        AnalyzeStatement(ifStmt.ElseBranch, new(localVariables), functionReturnType, loopDepth);
                    }
                    break;
                case MatchStatementNode matchStatement:
                    AnalyzeMatchStatement(matchStatement, localVariables, functionReturnType, loopDepth);
                    break;
                case WhileStatementNode whileStmt:
                    var whileConditionType = AnalyzeExpression(whileStmt.Condition, localVariables);
                    if (whileConditionType != "bool")
                    {
                        throw new Exception("Condition in 'while' statement must be a boolean.");
                    }
                    AnalyzeStatement(whileStmt.Body, new(localVariables), functionReturnType, loopDepth + 1);
                    break;

                case ForStatementNode forStmt:
                    var forScope = new Dictionary<string, string?>(localVariables);
                    if (forStmt.Initializer != null)
                        AnalyzeStatement(forStmt.Initializer, forScope, functionReturnType, loopDepth);
                    if (forStmt.Condition != null && AnalyzeExpression(forStmt.Condition, forScope) != "bool")
                    {
                        throw new Exception("Condition in 'for' statement must be a boolean.");
                    }
                    if (forStmt.Increment != null)
                        AnalyzeStatement(forStmt.Increment, forScope, functionReturnType, loopDepth + 1);
                    AnalyzeStatement(forStmt.Body, forScope, functionReturnType, loopDepth + 1);
                    break;
                case ForEachStatementNode forEach:
                    string? iterableType = AnalyzeExpression(forEach.Iterable, localVariables);
                    if (iterableType?.EndsWith("[]", StringComparison.Ordinal) != true)
                        throw new Exception($"The expression after 'in' must be an array, but got '{iterableType}'.");
                    var iterationScope = new Dictionary<string, string?>(localVariables);
                    DeclareVariable(forEach.VariableName, iterableType[..^2], iterationScope);
                    AnalyzeStatement(forEach.Body, iterationScope, functionReturnType, loopDepth + 1);
                    break;
                case BlockNode block:
                    AnalyzeBlock(block, localVariables, functionReturnType, loopDepth);
                    break;
                case ArrayAssignmentNode arrayAssignment:
                    AnalyzeArrayAssignment(arrayAssignment, localVariables);
                    break;
                case ThrowStatementNode throwStatement:
                    string? thrownType = AnalyzeExpression(throwStatement.Error, localVariables);
                    if (thrownType != Types.Error.Name)
                        throw new Exception($"A throw statement requires an error value, but got '{thrownType}'.");
                    break;
                case TryStatementNode tryStatement:
                    AnalyzeStatement(tryStatement.TryBlock, new(localVariables), functionReturnType, loopDepth);
                    if (tryStatement.CatchBlock is not null)
                    {
                        var catchScope = new Dictionary<string, string?>(localVariables);
                        catchScope[tryStatement.CatchVariable!] = Types.Error.Name;
                        AnalyzeStatement(tryStatement.CatchBlock, catchScope, functionReturnType, loopDepth);
                    }
                    if (tryStatement.FinallyBlock is not null)
                        AnalyzeStatement(tryStatement.FinallyBlock, new(localVariables), functionReturnType: null, loopDepth: 0);
                    break;
                case ReturnStatementNode returnStatementNode:
                    if (functionReturnType == null)
                    {
                        throw new Exception("'return' can only be used inside a function.");
                    }
                    var resultValueType = returnStatementNode.ReturnExpression == null
                        ? "void"
                        : AnalyzeExpression(returnStatementNode.ReturnExpression, localVariables);
                    if (!CheckType(functionReturnType, resultValueType))
                    {
                        throw new Exception($"Return type mismatch in function. Expected '{functionReturnType}' but got '{resultValueType}'.");
                    }
                    break;
                case ObjectInstantiationNode objectInstantiationNode:

                    if (!_packages.ContainsKey(objectInstantiationNode.PackageName))
                    {
                        throw new Exception(
                            $"A package with this name : {objectInstantiationNode.PackageName} has not been implemented ");
                    }
                    if (localVariables.ContainsKey(objectInstantiationNode.Name))
                    {
                        throw new Exception($"Variable '{objectInstantiationNode.Name}' is already declared.");
                    }

                    localVariables[objectInstantiationNode.Name] = objectInstantiationNode.PackageName;
                    break;
                case BreakStatementNode:
                case ContinueStatementNode:
                    if (loopDepth == 0)
                        throw new Exception($"'{(statement is BreakStatementNode ? "break" : "continue")}' can only be used inside a loop.");
                    break;
                default:
                    throw new Exception($"Unsupported statement type: {statement.GetType().Name}");
            }
        }

        private static bool AlwaysReturns(StatementNode? statement) => statement switch
        {
            ReturnStatementNode => true,
            ThrowStatementNode => true,
            BlockNode block => block.Statements.Any(AlwaysReturns),
            IfStatementNode branch => branch.ElseBranch != null &&
                                      AlwaysReturns(branch.ThenBranch) && AlwaysReturns(branch.ElseBranch),
            MatchStatementNode match => match.Arms.Any(arm => arm.IsDefault) &&
                                        match.Arms.All(arm => AlwaysReturns(arm.Body as StatementNode)),
            TryStatementNode tryStatement =>
                tryStatement.FinallyBlock is not null && AlwaysReturns(tryStatement.FinallyBlock) ||
                AlwaysReturns(tryStatement.TryBlock) &&
                (tryStatement.CatchBlock is null || AlwaysReturns(tryStatement.CatchBlock)),
            WhileStatementNode loop => IsAlwaysTrue(loop.Condition) &&
                                       AlwaysReturns(loop.Body) &&
                                       !ContainsBreakForCurrentLoop(loop.Body),
            ForStatementNode loop => (loop.Condition is null || IsAlwaysTrue(loop.Condition)) &&
                                     AlwaysReturns(loop.Body) &&
                                     !ContainsBreakForCurrentLoop(loop.Body),
            _ => false
        };

        /// <summary>تشخیص می‌دهد شرط حلقه literal صحیح و در نتیجه ورود به آن قطعی است.</summary>
        private static bool IsAlwaysTrue(ExpressionNode? expression) =>
            expression is LiteralNode { Type: TokenType.TRUE_LITERAL };

        /// <summary>تشخیص می‌دهد شرط literal قطعاً نادرست است و شاخهٔ then قابل‌دسترسی نیست.</summary>
        private static bool IsAlwaysFalse(ExpressionNode? expression) =>
            expression is LiteralNode { Type: TokenType.FALSE_LITERAL };

        /// <summary>وجود break متعلق به حلقهٔ جاری را بدون شمردن break حلقه‌های تو در تو بررسی می‌کند.</summary>
        private static bool ContainsBreakForCurrentLoop(StatementNode? statement) => statement switch
        {
            BreakStatementNode => true,
            BlockNode block => block.Statements.Any(ContainsBreakForCurrentLoop),
            IfStatementNode branch when IsAlwaysTrue(branch.Condition) =>
                ContainsBreakForCurrentLoop(branch.ThenBranch),
            IfStatementNode branch when IsAlwaysFalse(branch.Condition) =>
                ContainsBreakForCurrentLoop(branch.ElseBranch),
            IfStatementNode branch => ContainsBreakForCurrentLoop(branch.ThenBranch) ||
                                      ContainsBreakForCurrentLoop(branch.ElseBranch),
            MatchStatementNode match => match.Arms.Any(arm =>
                ContainsBreakForCurrentLoop(arm.Body as StatementNode)),
            TryStatementNode tryStatement => ContainsBreakForCurrentLoop(tryStatement.TryBlock) ||
                                             ContainsBreakForCurrentLoop(tryStatement.CatchBlock) ||
                                             ContainsBreakForCurrentLoop(tryStatement.FinallyBlock),
            WhileStatementNode or ForStatementNode or ForEachStatementNode => false,
            _ => false
        };

        private void AnalyzeMatchStatement(MatchStatementNode match,
            Dictionary<string, string?> localVariables, string? functionReturnType, int loopDepth)
        {
            string? valueType = AnalyzeExpression(match.Value, localVariables);
            foreach (MatchArm arm in match.Arms)
            {
                AnalyzeMatchPatterns(arm, valueType, localVariables);

                AnalyzeStatement((StatementNode)arm.Body, new(localVariables), functionReturnType, loopDepth);
            }
        }

        private void AnalyzeMatchPatterns(MatchArm arm, string? valueType,
            Dictionary<string, string?> localVariables)
        {
            foreach (MatchPatternNode pattern in arm.Patterns)
            {
                switch (pattern)
                {
                    case ValueMatchPatternNode valuePattern:
                        string? patternType = AnalyzeExpression(valuePattern.Value, localVariables);
                        if (!CheckType(valueType, patternType))
                            throw new Exception($"Match pattern type mismatch. Expected '{valueType}' but got '{patternType}'.");
                        break;
                    case RangeMatchPatternNode range:
                        string? startType = AnalyzeExpression(range.Start, localVariables);
                        string? endType = AnalyzeExpression(range.End, localVariables);
                        if (valueType != "number" || startType != "number" || endType != "number")
                            throw new Exception("Match range patterns require a number value and numeric boundaries.");
                        break;
                }
            }
        }

        /// <summary>
        /// Analyzes an array assignment statement.
        /// </summary>
        /// <param name="arrayAssignment">The array assignment statement to analyze.</param>
        /// <param name="localVariables">The local variables available in the current scope.</param>
        private void AnalyzeArrayAssignment(ArrayAssignmentNode arrayAssignment, Dictionary<string, string?> localVariables)
        {
            // Check if the array variable is declared
            if (!localVariables.TryGetValue(arrayAssignment.Name, out var arrayType))
            {
                throw new Exception($"Undeclared array variable '{arrayAssignment.Name}'.");
            }

            // Ensure the variable is indeed an array
            if (!arrayType.EndsWith("[]"))
            {
                throw new Exception($"Variable '{arrayAssignment.Name}' is not an array.");
            }

            // Analyze the index expression
            var indexType = AnalyzeExpression(arrayAssignment.Index, localVariables);

            // Ensure the index expression type is 'number'
            if (indexType != "number")
            {
                throw new Exception("Array index must be of type 'number'.");
            }

            // Analyze the value expression
            var valueType = AnalyzeExpression(arrayAssignment.Value, localVariables);

            // Remove the '[]' from arrayType to get the element type
            var elementType = arrayType.Substring(0, arrayType.Length - 2);

            // Ensure the value expression type matches the element type of the array
            if (elementType != valueType)
            {
                throw new Exception($"Type mismatch in array assignment to '{arrayAssignment.Name}[{arrayAssignment.Index}]'. Expected '{elementType}' but got '{valueType}'.");
            }
        }

        /// <summary>
        /// Analyzes an expression and determines its type.
        /// </summary>
        /// <param name="expression">The expression to analyze.</param>
        /// <param name="localVariables">The local variables available in the current scope.</param>
        /// <returns>The type of the expression.</returns>
        private string? AnalyzeExpression(ExpressionNode expression, Dictionary<string, string?> localVariables)
        {
            if (expression.Span.Line > 0) CurrentSpan = expression.Span;
            switch (expression)
            {
                case LiteralNode literal:
                    return ConvertTokenTypeToString(literal.Type);
                case IdentifierNode identifier:
                    return HandleIdentifierNode(identifier, localVariables);
                case BinaryExpressionNode binary:
                    return HandleBinaryExpressionNode(binary, localVariables);
                case UnaryExpressionNode unary:
                    return HandleUnaryExpressionNode(unary, localVariables);
                case FunctionCallNode functionCall:
                    return HandleFunctionCallNode(functionCall, localVariables);
                case AsyncExpressionNode asyncExpression:
                    return Types.TaskOf(TypeFacts.FromName(
                        HandleFunctionCallNode(asyncExpression.Call, localVariables))).Name;
                case AwaitExpressionNode awaitExpression:
                    TypeSymbol awaitedType = TypeFacts.FromName(
                        AnalyzeExpression(awaitExpression.Task, localVariables));
                    if (awaitedType is not TaskTypeSymbol taskType)
                        throw new Exception("'await' requires a task value.");
                    return taskType.ResultType.Name;
                case ArrayInitializerNode arrayInit:
                    return HandleArrayInitializerNode(arrayInit, localVariables);
                case ArrayAccessNode arrayAccess:
                    return HandleArrayAccessNode(arrayAccess, localVariables);
                case LogicalExpressionNode logical:
                    return HandleLogicalExpressionNode(logical, localVariables);
                case MemberAccessNode memberAccess:
                    return HandleMemberAccessNode(memberAccess, localVariables);
                case NewExpressionNode creation:
                    return AnalyzeNewExpression(creation, localVariables);
                case MatchExpressionNode matchExpression:
                    return AnalyzeMatchExpression(matchExpression, localVariables);
                default:
                    throw new Exception($"Unsupported expression type: {expression.GetType().Name}");
            }
        }

        private string AnalyzeNewExpression(NewExpressionNode creation,
            Dictionary<string, string?> localVariables)
        {
            if (!_packages.TryGetValue(creation.PackageName, out PackageNode? package))
                throw new Exception($"Unknown package '{creation.PackageName}'.");

            PackageFunctionNode? initializer = package.Members.OfType<PackageFunctionNode>()
                .FirstOrDefault(member => member.Name == "init");
            if (initializer is null)
            {
                if (creation.Arguments.Count != 0)
                    throw new Exception($"Package '{package.Name}' has no init constructor and accepts no arguments.");
                return package.Name;
            }

            ValidateArguments("init", creation.Arguments, initializer.Parameters, localVariables);
            return package.Name;
        }

        private string? AnalyzeMatchExpression(MatchExpressionNode match,
            Dictionary<string, string?> localVariables)
        {
            string? valueType = AnalyzeExpression(match.Value, localVariables);
            string? resultType = null;
            foreach (MatchArm arm in match.Arms)
            {
                AnalyzeMatchPatterns(arm, valueType, localVariables);

                string? armType = AnalyzeExpression((ExpressionNode)arm.Body, localVariables);
                resultType ??= armType;
                if (!CheckType(resultType, armType))
                    throw new Exception($"All match expression arms must return the same type. Expected '{resultType}' but got '{armType}'.");
            }
            return resultType;
        }

        private string? HandleMemberAccessNode(MemberAccessNode memberAccess, Dictionary<string, string?> localVariables)
        {
            if (!localVariables.TryGetValue(memberAccess.ObjectName, out string? packageName) || packageName is null)
                throw new Exception($"Member access requires a package instance, but '{memberAccess.ObjectName}' is not one.");

            if (packageName == Types.Error.Name)
            {
                if (memberAccess.Expression is IdentifierNode errorMember &&
                    errorMember.Name is "code" or "message")
                    return "string";
                throw new Exception("An error value exposes only the read-only members 'code' and 'message'.");
            }

            if (_interfaces.TryGetValue(packageName, out InterfaceNode? outInterface))
            {
                if (memberAccess.Expression is not FunctionCallNode interfaceCall)
                    throw new Exception($"Interface '{outInterface.Name}' exposes methods only.");
                InterfaceMethodNode? contractMethod = outInterface.Methods
                    .FirstOrDefault(method => method.Name == interfaceCall.FunctionName);
                if (contractMethod is null)
                    throw new Exception($"Interface '{outInterface.Name}' has no method named '{interfaceCall.FunctionName}'.");
                ValidateArguments(interfaceCall.FunctionName, interfaceCall.Arguments,
                    contractMethod.Parameters, localVariables);
                return contractMethod.ReturnType;
            }

            if (!_packages.TryGetValue(packageName, out PackageNode? outPackage))
                throw new Exception($"Member access requires a package instance, but '{memberAccess.ObjectName}' is not one.");

            bool requirePublic = memberAccess.ObjectName != "this";
            switch (memberAccess.Expression)
            {
                case FunctionCallNode expression:
                    PackageFunctionNode? method = outPackage.Members.OfType<PackageFunctionNode>()
                        .FirstOrDefault(member => member.Name == expression.FunctionName);
                    if (method is null)
                        throw new Exception($"Package '{outPackage.Name}' has no method named '{expression.FunctionName}'.");
                    if (requirePublic)
                        EnsurePublic(method.AccessModifier, outPackage.Name, expression.FunctionName);
                    ValidateArguments(expression.FunctionName, expression.Arguments, method.Parameters, localVariables);
                    return method.ReturnType;

                case CompoundAssignmentNode compoundAssignment:

                    var compValueType = AnalyzeExpression(compoundAssignment.Expression, localVariables);
                    string? compVariableType = FindPackageFieldType(outPackage, compoundAssignment.Name, requirePublic);
                    if (compVariableType != "number" || compValueType != "number")
                        throw new Exception($"Compound assignment requires numeric operands for member '{compoundAssignment.Name}'.");
                    return compValueType;

                case IdentifierNode identifierNode:
                    return FindPackageFieldType(outPackage, identifierNode.Name, requirePublic);

                case AssignmentNode assignment:
                    string? fieldType = FindPackageFieldType(outPackage, assignment.Name, requirePublic);
                    string? assignedType = AnalyzeExpression(assignment.Expression, localVariables);
                    if (!CheckType(fieldType!, assignedType))
                        throw new Exception($"Type mismatch in assignment to member '{assignment.Name}'. Expected '{fieldType}' but got '{assignedType}'.");
                    return fieldType;

                case IncrementDecrementNode increment:
                    string incrementType = FindPackageFieldType(outPackage, increment.Name, requirePublic);
                    if (incrementType != "number")
                        throw new Exception($"Increment and decrement require a numeric member, but '{increment.Name}' is '{incrementType}'.");
                    return incrementType;

                default:
                    throw new Exception($"Unsupported expression type: {memberAccess.GetType().Name}");
            }

        }

        private static string FindPackageFieldType(PackageNode package, string fieldName, bool requirePublic = false)
        {
            PackageVariableDeclarationNode? field = package.Members.OfType<PackageVariableDeclarationNode>()
                .FirstOrDefault(member => member.Name == fieldName);
            if (field is null)
                throw new Exception($"Package '{package.Name}' has no field named '{fieldName}'.");
            if (requirePublic)
                EnsurePublic(field.AccessModifier, package.Name, fieldName);
            return field.Type!;
        }

        private static void EnsurePublic(AccessModifier access, string packageName, string memberName)
        {
            if (access == AccessModifier.Private)
                throw new Exception($"Member '{memberName}' is private in package '{packageName}'.");
        }

        private void ValidateArguments(string callableName, IReadOnlyList<ExpressionNode> arguments,
            IReadOnlyList<ParameterNode> parameters, Dictionary<string, string?> localVariables)
        {
            if (arguments.Count != parameters.Count)
                throw new Exception($"'{callableName}' expects {parameters.Count} argument(s), but got {arguments.Count}.");
            for (int index = 0; index < arguments.Count; index++)
            {
                string? actualType = AnalyzeExpression(arguments[index], localVariables);
                string? expectedType = parameters[index].Type;
                if (!CheckType(expectedType!, actualType))
                    throw new Exception($"Type mismatch in argument {index + 1} of '{callableName}'. Expected '{expectedType}' but got '{actualType}'.");
            }
        }

        /// <summary>
        /// Handles identifier nodes and retrieves their type from local variables.
        /// </summary>
        /// <param name="identifier">The identifier node to handle.</param>
        /// <param name="localVariables">The local variables available in the current scope.</param>
        /// <returns>The type of the identifier.</returns>
        private string? HandleIdentifierNode(IdentifierNode identifier, Dictionary<string, string?> localVariables)
        {
            if (!localVariables.TryGetValue(identifier.Name, out var node))
            {
                throw new Exception($"Undeclared variable '{identifier.Name}'.");
            }
            return node;
        }

        /// <summary>
        /// Handles binary expression nodes and determines their result type.
        /// </summary>
        /// <param name="binary">The binary expression node to handle.</param>
        /// <param name="localVariables">The local variables available in the current scope.</param>
        /// <returns>The type of the binary expression result.</returns>
        private string? HandleBinaryExpressionNode(BinaryExpressionNode binary, Dictionary<string, string?> localVariables)
        {
            var leftType = AnalyzeExpression(binary.Left, localVariables);
            var rightType = AnalyzeExpression(binary.Right, localVariables);

            if (IsArithmeticOperator(binary.Operator))
            {
                return HandleArithmeticOperator(binary, leftType, rightType);
            }

            if (binary.Operator is "==" or "!=")
            {
                return HandleEqualityOperator(leftType, rightType);
            }

            if (IsOrderedComparisonOperator(binary.Operator))
            {
                return HandleComparisonOperator(leftType, rightType);
            }

            return leftType; // Assuming both sides have the same type for non-arithmetic operations
        }

        /// <summary>
        /// Handles arithmetic operators in binary expressions and determines the result type.
        /// </summary>
        /// <param name="binary">The binary expression node to handle.</param>
        /// <param name="leftType">The type of the left operand.</param>
        /// <param name="rightType">The type of the right operand.</param>
        /// <returns>The type of the result of the arithmetic operation.</returns>
        private string? HandleArithmeticOperator(BinaryExpressionNode binary, string? leftType, string? rightType)
        {
            if (binary.Operator == "+" && (leftType == "string" || rightType == "string"))
            {
                if (leftType == "string" && rightType == "string")
                    return "string";
                if (leftType == "string" && rightType == "number")
                    return "string";
                if (leftType == "number" && rightType == "string")
                    return "string";

                throw new Exception("Operands of arithmetic operations must be of type 'number'.");
            }

            if (leftType != "number" || rightType != "number")
            {
                throw new Exception("Operands of arithmetic operations must be of type 'number'.");
            }
            return "number";
        }

        /// <summary>
        /// Handles comparison operators in binary expressions and determines the result type.
        /// </summary>
        /// <param name="leftType">The type of the left operand.</param>
        /// <param name="rightType">The type of the right operand.</param>
        /// <returns>The type of the result of the comparison operation.</returns>
        private string? HandleComparisonOperator(string? leftType, string? rightType)
        {
            if (leftType != "number" || rightType != "number")
            {
                throw new Exception("Operands of comparison operations must be of type 'number'.");
            }
            return "bool"; // Comparison operators result in boolean type
        }

        /// <summary>برابری را فقط برای دو مقدار scalar هم‌نوع معتبر می‌داند.</summary>
        private static string HandleEqualityOperator(string? leftType, string? rightType)
        {
            bool supportedType = leftType is "number" or "string" or "bool";
            if (!supportedType || leftType != rightType)
                throw new Exception("Equality operands must have the same scalar type.");
            return "bool";
        }

        /// <summary>
        /// Handles unary expression nodes and determines their result type.
        /// </summary>
        /// <param name="unary">The unary expression node to handle.</param>
        /// <param name="localVariables">The local variables available in the current scope.</param>
        /// <returns>The type of the unary expression result.</returns>
        private string? HandleUnaryExpressionNode(UnaryExpressionNode unary, Dictionary<string, string?> localVariables)
        {
            string? operandType = AnalyzeExpression(unary.Operand, localVariables);
            return unary.Operator switch
            {
                "-" when operandType == Types.Number.Name => Types.Number.Name,
                "!" when operandType == Types.Bool.Name => Types.Bool.Name,
                "-" => throw new Exception("Unary '-' requires an operand of type 'number'."),
                "!" => throw new Exception("Unary '!' requires an operand of type 'bool'."),
                _ => throw new Exception($"Unknown unary operator '{unary.Operator}'.")
            };
        }

        /// <summary>
        /// Handles function call nodes and determines the return type of the function call.
        /// </summary>
        /// <param name="functionCall">The function call node to handle.</param>
        /// <param name="localVariables">The local variables available in the current scope.</param>
        /// <returns>The return type of the function call.</returns>
        private string? HandleFunctionCallNode(FunctionCallNode functionCall, Dictionary<string, string?> localVariables)
        {
            if (!_functions.ContainsKey(functionCall.FunctionName) && !_defaultFunctions.ContainsKey(functionCall.FunctionName))
            {
                throw new Exception($"Undeclared function '{functionCall.FunctionName}'.");
            }
            (TypeSymbol ReturnType, List<TypeSymbol> ParameterTypes) functionInfo = _functions.ContainsKey(functionCall.FunctionName)
                ? (TypeFacts.FromName(_functions[functionCall.FunctionName].ReturnType),
                    _functions[functionCall.FunctionName].Parameters
                        .Select(parameter => TypeFacts.FromName(parameter.Type)).ToList())
                : _defaultFunctions[functionCall.FunctionName];

            if (functionCall.Arguments.Count != functionInfo.ParameterTypes.Count)
            {
                throw new Exception($"Argument count mismatch in function call to '{functionCall.FunctionName}'. Expected {functionInfo.ParameterTypes.Count} but got {functionCall.Arguments.Count}.");
            }
            TypeSymbol? arrayElementType = null;
            for (int i = 0; i < functionCall.Arguments.Count; i++)
            {
                TypeSymbol argType = TypeFacts.FromName(
                    AnalyzeExpression(functionCall.Arguments[i], localVariables));

                bool isArrayFunction = functionCall.FunctionName.StartsWith("array_", StringComparison.Ordinal);
                if (isArrayFunction && i == 0 && argType is ArrayTypeSymbol arrayType)
                    arrayElementType = arrayType.ElementType;

                bool isElementArgument =
                    functionCall.FunctionName is "array_add" or "array_contains" or "array_index_of" && i == 1 ||
                    functionCall.FunctionName == "array_insert" && i == 2;
                if (isElementArgument && arrayElementType is not null && arrayElementType != Types.Unknown &&
                    !TypeFacts.IsAssignableTo(argType, arrayElementType))
                    throw new Exception($"Type mismatch in argument {i + 1} of function call to '{functionCall.FunctionName}'. Expected '{arrayElementType}' but got '{argType}'.");

                if (functionCall.FunctionName == "array_concat" && i == 1 &&
                    arrayElementType is not null && arrayElementType != Types.Unknown &&
                    !TypeFacts.IsAssignableTo(argType, Types.ArrayOf(arrayElementType)))
                    throw new Exception($"Type mismatch in argument 2 of function call to 'array_concat'. Expected '{arrayElementType}[]' but got '{argType}'.");

                TypeSymbol expectedType = functionInfo.ParameterTypes[i];
                if (!CheckType(expectedType.Name, argType.Name))
                {
                    throw new Exception($"Type mismatch in argument {i + 1} of function call to '{functionCall.FunctionName}'. Expected '{expectedType}' but got '{argType}'.");
                }
            }
            bool preservesArrayType = functionCall.FunctionName is
                "array_add" or "array_remove" or "array_reverse" or "array_clear" or
                "array_insert" or "array_slice" or "array_concat" or "array_sort";
            if (preservesArrayType && arrayElementType is not null)
                return Types.ArrayOf(arrayElementType).Name;

            return functionInfo.ReturnType.Name;
        }

        /// <summary>
        /// Handles array initializer nodes and determines the type of the array.
        /// </summary>
        /// <param name="arrayInit">The array initializer node to handle.</param>
        /// <param name="localVariables">The local variables available in the current scope.</param>
        /// <returns>The type of the array.</returns>
        private string? HandleArrayInitializerNode(ArrayInitializerNode arrayInit, Dictionary<string, string?> localVariables)
        {
            if (arrayInit.ElementType is not null)
            {
                string? sizeType = AnalyzeExpression(arrayInit.SizeExpression, localVariables);
                if (sizeType != Types.Number.Name)
                    throw new Exception("Sized array length must be of type 'number'.");
                return $"{arrayInit.ElementType}[]";
            }

            var elementType = "unknown"; // Placeholder for the element type of the array
            foreach (var element in arrayInit.Elements)
            {
                var elementExprType = AnalyzeExpression(element, localVariables);
                if (elementType == "unknown")
                {
                    elementType = elementExprType; // Set the element type on first iteration
                }
                else if (elementType != elementExprType)
                {
                    throw new Exception("Array elements must have consistent types.");
                }
            }
            return $"{elementType}[]"; // Return the array type
        }

        /// <summary>
        /// Handles array access nodes and determines the type of the array element.
        /// </summary>
        /// <param name="arrayAccess">The array access node to handle.</param>
        /// <param name="localVariables">The local variables available in the current scope.</param>
        /// <returns>The type of the array element.</returns>
        private string? HandleArrayAccessNode(ArrayAccessNode arrayAccess, Dictionary<string, string?> localVariables)
        {
            if (!localVariables.TryGetValue(arrayAccess.Name, out var arrayType))
            {
                throw new Exception($"Undeclared array variable '{arrayAccess.Name}'.");
            }

            var indexType = AnalyzeExpression(arrayAccess.Index, localVariables);
            if (indexType != Types.Number.Name)
            {
                throw new Exception("Array index must be of type 'number'.");
            }
            if (!arrayType.EndsWith("[]"))
            {
                throw new Exception($"Cannot index into non-array type '{arrayType}'.");
            }
            var elementType = arrayType.Substring(0, arrayType.Length - 2);
            return elementType;
        }

        /// <summary>
        /// Handles logical expression nodes and determines their result type.
        /// </summary>
        /// <param name="logical">The logical expression node to handle.</param>
        /// <param name="localVariables">The local variables available in the current scope.</param>
        /// <returns>The type of the result of the logical operation.</returns>
        private string? HandleLogicalExpressionNode(LogicalExpressionNode logical, Dictionary<string, string?> localVariables)
        {
            var leftType = AnalyzeExpression(logical.Left, localVariables);
            var rightType = AnalyzeExpression(logical.Right, localVariables);
            if (leftType != "bool" || rightType != "bool")
            {
                throw new Exception("Operands of logical operations must be of type 'bool'.");
            }
            if (logical.Operator != "&&" && logical.Operator != "||")
            {
                throw new Exception("Operator of logical operations must be || or &&.");
            }
            return "bool";
        }

        /// <summary>
        /// Determines if a given operator is an arithmetic operator.
        /// </summary>
        /// <param name="op">The operator to check.</param>
        /// <returns>True if the operator is an arithmetic operator, otherwise false.</returns>
        private bool IsArithmeticOperator(string op)
        {
            return op == "+" || op == "-" || op == "*" || op == "/" || op == "%";
        }

        /// <summary>
        /// Determines if a given operator is a comparison operator.
        /// </summary>
        /// <param name="op">The operator to check.</param>
        /// <returns>True if the operator is a comparison operator, otherwise false.</returns>
        private bool IsOrderedComparisonOperator(string op)
        {
            return op == "<" || op == ">" || op == "<=" || op == ">=";
        }

        /// <summary>
        /// Checks if two types are compatible.
        /// </summary>
        /// <param name="firstType">The first type to check.</param>
        /// <param name="secondType">The second type to check.</param>
        /// <returns>True if the types are compatible, otherwise false.</returns>
        private bool CheckType(string firstType, string? secondType)
        {
            TypeSymbol target = TypeFacts.FromName(firstType);
            TypeSymbol source = TypeFacts.FromName(secondType);
            if (target is NamedTypeSymbol targetNamed && source is NamedTypeSymbol sourceNamed &&
                _interfaces.ContainsKey(targetNamed.Name) &&
                _packages.TryGetValue(sourceNamed.Name, out PackageNode? sourcePackage))
                return sourcePackage.Interfaces.Contains(targetNamed.Name);
            return TypeFacts.IsAssignableTo(source, target);
        }

        private void EnsureKnownType(string? typeName, bool allowVoid = false, bool allowLet = false)
        {
            if (typeName is null) return;
            if (allowVoid && typeName == "void" || allowLet && typeName == "let") return;
            TypeSymbol type = TypeFacts.FromName(typeName);
            if (type is NamedTypeSymbol named &&
                !_packages.ContainsKey(named.Name) && !_interfaces.ContainsKey(named.Name))
                throw new Exception($"Unknown package or interface type '{named.Name}'.");
        }

        /// <summary>Benita مقدار null ندارد؛ بنابراین reference نام‌دار باید هنگام declaration مقدار بگیرد.</summary>
        private static void RequireInitializerForNamedType(string? typeName, ExpressionNode? initializer,
            string variableName)
        {
            if (TypeFacts.FromName(typeName) is NamedTypeSymbol && initializer is null)
                throw new Exception($"Variable '{variableName}' of named type '{typeName}' requires an initializer.");
        }

        /// <summary>از ورود نوع‌های placeholder به symbol table پس از استنتاج let جلوگیری می‌کند.</summary>
        private static void RequireConcreteInferredType(string? typeName, string variableName)
        {
            TypeSymbol type = TypeFacts.FromName(typeName);
            if (!IsConcreteInferredType(type))
                throw new Exception(
                    $"Cannot infer a concrete type for let variable '{variableName}'. Use an explicit type or a typed initializer.");
        }

        private static bool IsConcreteInferredType(TypeSymbol type) => type switch
        {
            PrimitiveTypeSymbol primitive => primitive != Types.Void,
            NamedTypeSymbol => true,
            ArrayTypeSymbol array => IsConcreteInferredType(array.ElementType),
            TaskTypeSymbol task => task.ResultType == Types.Void || IsConcreteInferredType(task.ResultType),
            _ => false
        };

        /// <summary>
        /// Converts a token type to its corresponding string representation.
        /// </summary>
        /// <param name="tokenType">The token type to convert.</param>
        /// <returns>The string representation of the token type.</returns>
        private string? ConvertTokenTypeToString(TokenType tokenType)
        {
            return TypeFacts.FromToken(tokenType).Name;
        }

        /// <summary>
        /// Declares a variable in the given scope.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="type">The type of the variable.</param>
        /// <param name="variableScope">The scope in which to declare the variable.</param>
        private void DeclareVariable(string name, string? type, Dictionary<string, string?> variableScope)
        {
            if (variableScope.ContainsKey(name))
            {
                throw new Exception($"Variable '{name}' is already declared.");
            }
            variableScope[name] = type;
        }
    }
}
