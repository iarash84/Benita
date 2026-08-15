namespace Benita
{
    /// <summary>
    /// The SemanticAnalyzer class performs semantic analysis on a program to ensure that variables are declared before use,
    /// functions are properly defined, and expressions are correctly typed.
    /// </summary>
    public class SemanticAnalyzer
    {
        /// <summary>
        /// Dictionary to store packages names and their associated PackageNode.
        /// </summary>
        private readonly Dictionary<string, PackageNode> _packages;


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
            _globalVariables.Clear();
            _functions.Clear();

            foreach (var packageNode in program.Packages)
            {
                DeclarePackage(packageNode);
            }

            // Analyze global variables and ensure they are declared
            foreach (var globalVar in program.GlobalVariables)
            {
                var declaredType = globalVar.Type;
                if (globalVar.Initializer != null)
                {
                    var initializerType = AnalyzeExpression(globalVar.Initializer, _globalVariables);
                    if (declaredType == "let")
                    {
                        declaredType = initializerType;
                    }
                    else if (!CheckType(declaredType!, initializerType))
                    {
                        throw new Exception(
                            $"Type mismatch in global variable '{globalVar.Name}'. Expected '{declaredType}' but got '{initializerType}'.");
                    }
                }

                DeclareVariable(globalVar.Name, declaredType, _globalVariables);
            }

            // Register every function before analyzing bodies, allowing forward calls.
            foreach (var function in program.Functions)
            {
                DeclareFunction(function);
            }

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageNode"></param>
        /// <exception cref="Exception"></exception>
        private void DeclarePackage(PackageNode packageNode)
        {
            if (_packages.ContainsKey(packageNode.Name))
            {
                throw new Exception($"Package '{packageNode.Name}' is already declared.");
            }
            _packages[packageNode.Name] = packageNode;
            AnalyzePackage(packageNode);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageNode"></param>
        private void AnalyzePackage(PackageNode packageNode)
        {
            Dictionary<string, string?> variableScope = new Dictionary<string, string?>();
            Dictionary<string, PackageFunctionNode> packageFunctions = new Dictionary<string, PackageFunctionNode>();

            foreach (var packageMember in packageNode.Members)
            {
                switch (packageMember)
                {
                    case PackageVariableDeclarationNode variableDeclaration:
                        if (variableScope.ContainsKey(variableDeclaration.Name))
                        {
                            throw new Exception($"Variable '{variableDeclaration.Name}' is already declared.");
                        }
                        variableScope[variableDeclaration.Name] = variableDeclaration.Type;
                        break;

                    case PackageFunctionNode functionNode:
                        if (packageFunctions.ContainsKey(functionNode.Name))
                        {
                            throw new Exception($"Function '{functionNode.Name}' is already declared.");
                        }

                        packageFunctions[functionNode.Name] = functionNode;
                        AnalyzePackageFunction(functionNode, variableScope);
                        break;
                    default:
                        throw new Exception($"Unsupported statement type: {packageMember.GetType().Name}");
                }
            }
        }

        private void AnalyzePackageFunction(PackageFunctionNode function, Dictionary<string, string?> variableScope)
        {
            // Create a new scope for local variables
            var localVariables = new Dictionary<string, string?>(variableScope);

            // Declare function parameters in the local scope
            foreach (var param in function.Parameters)
            {
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
            // Create a new scope for local variables
            var localVariables = new Dictionary<string, string?>(_globalVariables);

            // Declare function parameters in the local scope
            foreach (var param in function.Parameters)
            {
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
            switch (statement)
            {
                case VariableDeclarationNode varDecl:
                    string? declaredType = varDecl.Type;
                    if (varDecl.Type == "let")
                    {
                        declaredType = varDecl.Initializer != null
                            ? AnalyzeExpression(varDecl.Initializer, localVariables)
                            : varDecl.Type;
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
                    AnalyzeStatement(ifStmt.ThenBranch, localVariables, functionReturnType, loopDepth);
                    if (ifStmt.ElseBranch != null)
                    {
                        AnalyzeStatement(ifStmt.ElseBranch, localVariables, functionReturnType, loopDepth);
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
                    AnalyzeStatement(whileStmt.Body, localVariables, functionReturnType, loopDepth + 1);
                    break;

                case ForStatementNode forStmt:
                    if (forStmt.Initializer != null)
                        AnalyzeStatement(forStmt.Initializer, localVariables, functionReturnType, loopDepth);
                    if (forStmt.Condition != null && AnalyzeExpression(forStmt.Condition, localVariables) != "bool")
                    {
                        throw new Exception("Condition in 'for' statement must be a boolean.");
                    }
                    if (forStmt.Increment != null)
                        AnalyzeStatement(forStmt.Increment, localVariables, functionReturnType, loopDepth + 1);
                    AnalyzeStatement(forStmt.Body, localVariables, functionReturnType, loopDepth + 1);
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
                case ReturnStatementNode returnStatementNode:
                    if (functionReturnType == null)
                    {
                        throw new Exception("'return' can only be used inside a function.");
                    }
                    var resultValueType = returnStatementNode.ReturnExpression == null
                        ? "void"
                        : AnalyzeExpression(returnStatementNode.ReturnExpression, localVariables);
                    if (resultValueType != functionReturnType)
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
                    // TODO: Must check object name in this scope
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
            BlockNode block => block.Statements.Any(AlwaysReturns),
            IfStatementNode branch => branch.ElseBranch != null &&
                                      AlwaysReturns(branch.ThenBranch) && AlwaysReturns(branch.ElseBranch),
            MatchStatementNode match => match.Arms.Any(arm => arm.IsDefault) &&
                                        match.Arms.All(arm => AlwaysReturns(arm.Body as StatementNode)),
            _ => false
        };

        private void AnalyzeMatchStatement(MatchStatementNode match,
            Dictionary<string, string?> localVariables, string? functionReturnType, int loopDepth)
        {
            string? valueType = AnalyzeExpression(match.Value, localVariables);
            foreach (MatchArm arm in match.Arms)
            {
                AnalyzeMatchPatterns(arm, valueType, localVariables);

                AnalyzeStatement((StatementNode)arm.Body, localVariables, functionReturnType, loopDepth);
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
                case ArrayInitializerNode arrayInit:
                    return HandleArrayInitializerNode(arrayInit, localVariables);
                case ArrayAccessNode arrayAccess:
                    return HandleArrayAccessNode(arrayAccess, localVariables);
                case LogicalExpressionNode logical:
                    return HandleLogicalExpressionNode(logical, localVariables);
                case MemberAccessNode memberAccess:
                    return HandleMemberAccessNode(memberAccess, localVariables);
                case MatchExpressionNode matchExpression:
                    return AnalyzeMatchExpression(matchExpression, localVariables);
                default:
                    throw new Exception($"Unsupported expression type: {expression.GetType().Name}");
            }
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
            var packageName = localVariables[memberAccess.ObjectName];
            var outPackage = _packages[packageName];

            switch (memberAccess.Expression)
            {
                case FunctionCallNode expression:
                    foreach (var member in outPackage.Members)
                    {
                        if (!(member is PackageFunctionNode callFunctionNode)) continue;
                        if (callFunctionNode.Name == expression.FunctionName)
                            return callFunctionNode.ReturnType;
                    }

                    // TODO: fix the error
                    throw new Exception($"Error '{outPackage.Name}' does not contain a definition for '{expression.FunctionName}' ");

                case CompoundAssignmentNode compoundAssignment:

                    var compValueType = AnalyzeExpression(compoundAssignment.Expression, localVariables);
                    string? compVariableType = string.Empty;
                    foreach (var member in outPackage.Members)
                    {
                        if (!(member is PackageVariableDeclarationNode variableDeclarationNode)) continue;
                        if (variableDeclarationNode.Name == compoundAssignment.Name)
                        {
                            compVariableType = variableDeclarationNode.Type;
                            break;
                        }
                    }

                    if (string.IsNullOrEmpty(compVariableType))
                    {
                        throw new Exception(
                            $"Variable {compoundAssignment.Name} has not been implemented in package {outPackage.Name}.");
                    }

                    if (compVariableType != compValueType)
                    {
                        throw new Exception(
                            $"Type mismatch in compound assignment to '{compoundAssignment.Name}'. Expected '{compVariableType}' but got '{compValueType}'.");
                    }
                    return compValueType;

                case IdentifierNode identifierNode:
                    foreach (var member in outPackage.Members)
                    {
                        if (!(member is PackageVariableDeclarationNode variableDeclarationNode)) continue;
                        if (variableDeclarationNode.Name == identifierNode.Name)
                        {
                            return variableDeclarationNode.Type;
                        }
                    }
                    throw new Exception($"Undeclared variable '{identifierNode.Name}' in package {outPackage.Name}.");

                case AssignmentNode assignment:
                    //var valueType = AnalyzeExpression(assignment.Expression, localVariables);
                    //if (!localVariables.TryGetValue(assignment.Name, out var variableType))
                    //{
                    //    throw new Exception($"Undeclared variable '{assignment.Name}'.");
                    //}

                    //if (!CheckType(variableType, valueType))
                    //{
                    //    throw new Exception($"Type mismatch in assignment to '{assignment.Name}'. Expected '{variableType}' but got '{valueType}'.");
                    //}
                    //TODO : Not implemented
                    return null;

                default:
                    throw new Exception($"Unsupported expression type: {memberAccess.GetType().Name}");
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

            if (IsComparisonOperator(binary.Operator))
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

        /// <summary>
        /// Handles unary expression nodes and determines their result type.
        /// </summary>
        /// <param name="unary">The unary expression node to handle.</param>
        /// <param name="localVariables">The local variables available in the current scope.</param>
        /// <returns>The type of the unary expression result.</returns>
        private string? HandleUnaryExpressionNode(UnaryExpressionNode unary, Dictionary<string, string?> localVariables)
        {
            return AnalyzeExpression(unary.Operand, localVariables);
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
                if (!TypeFacts.IsAssignableTo(argType, expectedType))
                {
                    throw new Exception($"Type mismatch in argument {i + 1} of function call to '{functionCall.FunctionName}'. Expected '{expectedType}' but got '{argType}'.");
                }
            }
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
        private bool IsComparisonOperator(string op)
        {
            return op == "<" || op == ">" || op == "<=" || op == ">=" || op == "==" || op == "!=";
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
            return TypeFacts.IsAssignableTo(source, target);
        }

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
