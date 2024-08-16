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
        private readonly Dictionary<string?, PackageNode?> _packages;


        /// <summary>
        /// Dictionary to store global variables and their types.
        /// </summary>
        private readonly Dictionary<string, string?> _globalVariables;

        /// <summary>
        /// Dictionary to store function names and their associated FunctionNode.
        /// </summary>
        private readonly Dictionary<string?, FunctionNode?> _functions;

        /// <summary>
        /// Dictionary to store default functions with their return types and parameter types.
        /// </summary>
        private readonly Dictionary<string, (string, List<string>)> _defaultFunctions;


        /// <summary>
        /// Initializes a new instance of the SemanticAnalyzer class.
        /// </summary>
        public SemanticAnalyzer()
        {
            _globalVariables = new Dictionary<string, string?>();
            _functions = new Dictionary<string?, FunctionNode?>();
            _packages = new Dictionary<string?, PackageNode?>();
            _defaultFunctions = new Dictionary<string, (string, List<string>)>
            {
                { "print", ("void", ["string"]) },
                { "input", ("string", []) },
                { "file_read", ("string", ["string"]) },
                { "file_write", ("void", ["string", "string"]) },
                { "file_exist", ("bool", ["string"]) },
                { "file_delete", ("bool", ["string"]) },
                { "array_len", ("number", ["array"]) },
                { "array_add", ("array", ["array", "string"]) },
                { "array_remove", ("array", ["array", "number"]) },
                { "to_string", ("string", ["number"]) },
                { "to_number", ("number", ["string"]) },
            };
        }

        /// <summary>
        /// Analyzes the provided ProgramNode to perform semantic checks.
        /// </summary>
        /// <param name="program">The ProgramNode to analyze.</param>
        public void Analyze(ProgramNode program)
        {

            foreach (var packageNode in program.Packages)
            {
                DeclarePackage(packageNode);
            }

            // Analyze global variables and ensure they are declared
            foreach (var globalVar in program.GlobalVariables)
            {
                DeclareVariable(globalVar.Name, globalVar.Type, _globalVariables);
            }

            // Analyze function declarations
            foreach (var function in program.Functions)
            {
                DeclareFunction(function);
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
        private void DeclarePackage(PackageNode? packageNode)
        {
            if (_packages.ContainsKey(packageNode.Name))
            {
                throw new Exception($"Function '{packageNode.Name}' is already declared.");
            }
            _packages[packageNode.Name] = packageNode;
            AnalyzePackage(packageNode);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageNode"></param>
        private void AnalyzePackage(PackageNode? packageNode)
        {
            Dictionary<string, string?> variableScope = new Dictionary<string, string?>();
            Dictionary<string?, PackageFunctionNode> packageFunctions = new Dictionary<string?, PackageFunctionNode>();

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

            // Check the function's return statement
            if (function.ReturnStatement != null)
            {
                var returnType = AnalyzeExpression(function.ReturnStatement.ReturnExpression, localVariables);
                if (returnType != function.ReturnType)
                {
                    throw new Exception($"Return type mismatch in function '{function.Name}'. Expected '{function.ReturnType}' but got '{returnType}'.");
                }
            }
            else if (function.ReturnType != "void")
            {
                throw new Exception($"Function '{function.Name}' must return a value of type '{function.ReturnType}'.");
            }
        }

        /// <summary>
        /// Declares a function and performs analysis on it.
        /// </summary>
        /// <param name="function">The function to declare and analyze.</param>
        private void DeclareFunction(FunctionNode? function)
        {
            if (_functions.ContainsKey(function.Name))
            {
                throw new Exception($"Function '{function.Name}' is already declared.");
            }

            _functions[function.Name] = function;
            AnalyzeFunction(function);
        }

        /// <summary>
        /// Analyzes a function's parameters, body, and return type.
        /// </summary>
        /// <param name="function">The function to analyze.</param>
        private void AnalyzeFunction(FunctionNode? function)
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

            // Check the function's return statement
            if (function.ReturnStatement != null)
            {
                var returnType = AnalyzeExpression(function.ReturnStatement.ReturnExpression, localVariables);
                if (returnType != function.ReturnType)
                {
                    throw new Exception($"Return type mismatch in function '{function.Name}'. Expected '{function.ReturnType}' but got '{returnType}'.");
                }
            }
            else if (function.ReturnType != "void")
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
        private void AnalyzeBlock(BlockNode block, Dictionary<string, string?> localVariables, string? functionReturnType = null)
        {
            foreach (var statement in block.Statements)
            {
                AnalyzeStatement(statement, localVariables, functionReturnType);
            }
        }

        /// <summary>
        /// Analyzes a single statement.
        /// </summary>
        /// <param name="statement">The statement to analyze.</param>
        /// <param name="localVariables">The local variables available in the current scope.</param>
        /// <param name="functionReturnType">The return type of the function (if any).</param>
        private void AnalyzeStatement(StatementNode statement, Dictionary<string, string?> localVariables, string? functionReturnType = null)
        {
            switch (statement)
            {
                case VariableDeclarationNode varDecl:
                    var initType = varDecl.Initializer != null ? AnalyzeExpression(varDecl.Initializer, localVariables) : varDecl.Type;
                    DeclareVariable(varDecl.Name, initType, localVariables);
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

                    if (compVariableType != compValueType)
                    {
                        throw new Exception($"Type mismatch in compound assignment to '{compoundAssignment.Name}'. Expected '{compVariableType}' but got '{compValueType}'.");
                    }
                    break;
                case IncrementDecrementNode incDec:
                    if (!localVariables.ContainsKey(incDec.Name))
                    {
                        throw new Exception($"Undeclared variable '{incDec.Name}'.");
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
                    AnalyzeStatement(ifStmt.ThenBranch, localVariables, functionReturnType);
                    if (ifStmt.ElseBranch != null)
                    {
                        AnalyzeStatement(ifStmt.ElseBranch, localVariables, functionReturnType);
                    }
                    break;
                case WhileStatementNode whileStmt:
                    var whileConditionType = AnalyzeExpression(whileStmt.Condition, localVariables);
                    if (whileConditionType != "bool")
                    {
                        throw new Exception("Condition in 'while' statement must be a boolean.");
                    }
                    AnalyzeStatement(whileStmt.Body, localVariables, functionReturnType);
                    break;

                case ForStatementNode forStmt:
                    // TODO: Update this section to handle 'for' loop initialization and iteration
                    AnalyzeStatement(forStmt.Initializer, localVariables, functionReturnType);
                    var forConditionType = AnalyzeExpression(forStmt.Condition, localVariables);
                    if (forConditionType != "bool")
                    {
                        throw new Exception("Condition in 'for' statement must be a boolean.");
                    }
                    AnalyzeStatement(forStmt.Body, localVariables, functionReturnType);
                    break;
                case BlockNode block:
                    AnalyzeBlock(block, localVariables, functionReturnType);
                    break;
                case ArrayAssignmentNode arrayAssignment:
                    AnalyzeArrayAssignment(arrayAssignment, localVariables);
                    break;
                case ReturnStatementNode returnStatementNode:
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
                default:
                    throw new Exception($"Unsupported statement type: {statement.GetType().Name}");
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
                    return HandelMemberAccessNode(memberAccess, localVariables);
                default:
                    throw new Exception($"Unsupported expression type: {expression.GetType().Name}");
            }
        }

        private string? HandelMemberAccessNode(MemberAccessNode memberAccess, Dictionary<string, string?> localVariables)
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
            (string? ReturnType, List<string>) functionInfo = _functions.ContainsKey(functionCall.FunctionName)
                ? (_functions[functionCall.FunctionName].ReturnType, _functions[functionCall.FunctionName].Parameters.ConvertAll(p => p.Type))
                : _defaultFunctions[functionCall.FunctionName];

            if (functionCall.Arguments.Count != functionInfo.Item2.Count)
            {
                throw new Exception($"Argument count mismatch in function call to '{functionCall.FunctionName}'. Expected {functionInfo.Item2.Count} but got {functionCall.Arguments.Count}.");
            }
            for (int i = 0; i < functionCall.Arguments.Count; i++)
            {
                var argType = AnalyzeExpression(functionCall.Arguments[i], localVariables);

                if ((functionCall.FunctionName == "array_len" || functionCall.FunctionName == "array_add" || functionCall.FunctionName == "array_remove") && (argType == "number[]" || argType == "array"))
                {
                    argType = "array";
                }

                if (argType != functionInfo.Item2[i])
                {
                    if (!(argType == "number" && functionInfo.Item2[i] == "string"))
                    {
                        throw new Exception($"Type mismatch in argument {i + 1} of function call to '{functionCall.FunctionName}'. Expected '{functionInfo.Item2[i]}' but got '{argType}'.");
                    }
                }
            }
            return functionInfo.Item1;
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
            return op == "<" || op == ">" || op == "<=" || op == ">=" || op == "==";
        }

        /// <summary>
        /// Checks if two types are compatible.
        /// </summary>
        /// <param name="firstType">The first type to check.</param>
        /// <param name="secondType">The second type to check.</param>
        /// <returns>True if the types are compatible, otherwise false.</returns>
        private bool CheckType(string firstType, string? secondType)
        {
            string[] arrayTypes = { "number[]", "string[]", "bool[]" };
            return (arrayTypes.Contains(firstType) && secondType == "array") ||
                   (arrayTypes.Contains(secondType) && firstType == "array") || firstType == secondType;
        }

        /// <summary>
        /// Converts a token type to its corresponding string representation.
        /// </summary>
        /// <param name="tokenType">The token type to convert.</param>
        /// <returns>The string representation of the token type.</returns>
        private string? ConvertTokenTypeToString(TokenType tokenType)
        {
            switch (tokenType)
            {
                case TokenType.NUMBER_LITERAL:
                case TokenType.NUMBER:
                    return "number";
                case TokenType.STRING_LITERAL:
                case TokenType.STRING:
                    return "string";
                case TokenType.FALSE_LITERAL:
                case TokenType.TRUE_LITERAL:
                case TokenType.BOOL:
                    return "bool";
                default:
                    throw new Exception($"Unsupported token type: {tokenType}");
            }
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
