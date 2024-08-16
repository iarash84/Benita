using System.Text;

namespace Benita
{
    /// <summary>
    /// The CodeGenerator class generates C++ code from the parsed abstract syntax tree(AST) of the Benita language.
    /// It handles the conversion of program structures, expressions, and statements into their C++ equivalents,
    /// facilitating the translation of Benita source code into executable C++ code.
    /// </summary>
    public class CodeGenerator
    {
        private StringBuilder _code;
        private StringBuilder _codeHeader;
        private StringBuilder _codeInclude;
        private StringBuilder _defaultFunction;

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeGenerator"/> class.
        /// </summary>
        public CodeGenerator()
        {
            _code = new StringBuilder();
            _codeHeader = new StringBuilder();
            _codeInclude = new StringBuilder();
            _defaultFunction = new StringBuilder();
        }

        /// <summary>
        /// Generates C++ code from the given program node.
        /// </summary>
        /// <param name="program">The program node.</param>
        /// <returns>The generated C++ code as a string.</returns>
        public string GenerateCode(ProgramNode program)
        {
            StringBuilder finalCode = new StringBuilder();
            finalCode.Clear();
            _code.Clear();
            _codeHeader.Clear();
            _defaultFunction.Clear();

            // Include necessary headers for C++ output
            AppendToSubstring("#include <iostream>", ref _codeInclude);
            AppendToSubstring("#include <string>", ref _codeInclude);
            AppendToSubstring("#include <vector>", ref _codeInclude);
            AppendToSubstring("#include <fstream>", ref _codeInclude);

            _code.AppendLine();
            _defaultFunction.AppendLine();

            //generate packages
            foreach (var packageNode in program.Packages)
            {
                GeneratePackage(packageNode);
            }


            // Generate global variable declarations
            foreach (var globalVar in program.GlobalVariables)
            {
                GenerateGlobalVariable(globalVar);
            }

            // Generate function definitions
            foreach (var function in program.Functions)
            {
                GenerateFunction(function);
            }

            // Generate main function
            GenerateMainFunction(program.MainFunction, program.Statements);

            _codeInclude.AppendLine();
            finalCode.Append(_codeInclude);
            finalCode.Append(_codeHeader);
            finalCode.Append(_code);
            finalCode.Append(_defaultFunction);

            return finalCode.ToString();
        }

        private void GeneratePackage(PackageNode? packageNode, string indent = "")
        {
            _code.Append($"class {packageNode.Name}");
            _code.AppendLine("{");

            foreach (var member in packageNode.Members)
            {
                if (member is PackageVariableDeclarationNode field)
                {
                    var packageVariableDeclarationNode =
                        new VariableDeclarationNode(field.Type, field.Name, field.Initializer);
                    GenerateGlobalVariable(packageVariableDeclarationNode, indent + "  ");
                }
                else if (member is PackageFunctionNode method)
                {
                    var functionNode = new FunctionNode(method.Name, method.Parameters, method.ReturnType, method.Body,
                        method.ReturnStatement);
                    GenerateFunction(functionNode, indent + "  ");
                }
            }
            _code.AppendLine("}");
            _code.AppendLine();
        }

        /// <summary>
        /// Appends a value to a StringBuilder if it does not already contain it.
        /// </summary>
        /// <param name="value">The value to append.</param>
        /// <param name="stringBuilder">The StringBuilder to append to.</param>
        /// <returns>True if the value was appended; otherwise, false.</returns>
        private bool AppendToSubstring(string value, ref StringBuilder stringBuilder)
        {
            string sbContent = stringBuilder.ToString();
            if (!sbContent.Contains(value))
            {
                stringBuilder.AppendLine(value);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Generates code for a global variable declaration.
        /// </summary>
        /// <param name="globalVar">The global variable node.</param>
        /// <param name="indent"></param>
        private void GenerateGlobalVariable(VariableDeclarationNode? globalVar, string indent = "")
        {
            _code.Append(indent + $"{ConvertType(globalVar.Type)} {globalVar.Name}");
            if (globalVar.Initializer != null)
            {
                _code.Append(" = ");
                GenerateExpression(globalVar.Initializer);
            }
            _code.AppendLine(";");
        }

        /// <summary>
        /// Generates code for a function definition.
        /// </summary>
        /// <param name="function">The function node.</param>
        /// <param name="indent"></param>
        private void GenerateFunction(FunctionNode? function, string indent = "")
        {
            _code.Append(indent + $"{ConvertType(function.ReturnType)} {function.Name}(");
            for (int i = 0; i < function.Parameters.Count; i++)
            {
                _code.Append($"{ConvertType(function.Parameters[i].Type)} {function.Parameters[i].Name}");
                if (i < function.Parameters.Count - 1)
                {
                    _code.Append(", ");
                }
            }

            _code.AppendLine(")");
            _code.AppendLine(indent + "{");
            GenerateBlock(function.Body, indent + "  ");
            if (function.ReturnStatement != null)
            {
                _code.Append(indent + "  " + "return ");
                GenerateExpression(function.ReturnStatement.ReturnExpression);
                _code.AppendLine(";");
            }
            _code.AppendLine(indent + "}");
            _code.AppendLine();
        }

        /// <summary>
        /// Generates code for a return expression.
        /// </summary>
        /// <param name="expressionNode">The expression node.</param>
        /// <param name="indent"></param>
        private void GenerateReturnExpression(ExpressionNode expressionNode, string indent = "")
        {
            if (expressionNode != null)
            {
                _code.Append(indent + "return ");
                GenerateExpression(expressionNode);
            }
            else
                _code.Append(indent + "return");

            _code.AppendLine(";");
        }

        /// <summary>
        /// Generates code for the main function.
        /// </summary>
        /// <param name="mainFunction">The main function node.</param>
        /// <param name="statements"></param>
        /// <param name="indent"></param>
        private void GenerateMainFunction(FunctionNode? mainFunction, List<StatementNode?>? statements, string indent = "")
        {
            _code.AppendLine("int main()");
            _code.AppendLine("{");
            if (mainFunction == null)
            {
                foreach (var statement in statements)
                {
                    GenerateStatement(statement, indent + "  ");
                }
            }
            else
            {
                GenerateBlock(mainFunction.Body, indent + "  ");
            }
            _code.AppendLine(indent + "  return 0;");
            _code.AppendLine("}");
        }

        /// <summary>
        /// Generates code for a block of statements.
        /// </summary>
        /// <param name="block">The block node.</param>
        /// <param name="indent"></param>
        private void GenerateBlock(BlockNode block, string indent = "")
        {
            foreach (var statement in block.Statements)
            {
                GenerateStatement(statement, indent);
            }
        }

        /// <summary>
        /// Generates code for a statement.
        /// </summary>
        /// <param name="statement">The statement node.</param>
        /// <param name="indent"></param>
        private void GenerateStatement(StatementNode? statement, string indent = "")
        {
            switch (statement)
            {
                case VariableDeclarationNode varDecl:
                    GenerateLocalVariable(varDecl, indent);
                    break;
                case AssignmentNode assign:
                    GenerateAssignment(assign, indent);
                    break;
                case CompoundAssignmentNode compAssign:
                    GenerateCompoundAssignment(compAssign, indent);
                    break;
                case IncrementDecrementNode incDec:
                    GenerateIncrementDecrement(incDec, indent);
                    break;
                case IfStatementNode ifStatement:
                    GenerateIfStatement(ifStatement, indent);
                    break;
                case WhileStatementNode whileStatement:
                    GenerateWhileStatement(whileStatement, indent);
                    break;
                case ForStatementNode forStatement:
                    GenerateForStatement(forStatement, indent);
                    break;
                case BlockNode block:
                    GenerateBlock(block, indent);
                    break;
                case ExpressionStatementNode exprStatement:
                    if (exprStatement.Expression is MemberAccessNode memberAccess)
                    {
                        GenerateMemberAccess(memberAccess, indent, true);
                    }
                    else
                    {
                        GenerateExpression(exprStatement.Expression, indent);
                        _code.AppendLine(";");
                    }
                    break;
                case ArrayAssignmentNode arrayAssignmentNode:
                    GenerateArrayAssignment(arrayAssignmentNode, indent);
                    break;
                case ReturnStatementNode returnAssignmentNode:
                    GenerateReturnExpression(returnAssignmentNode.ReturnExpression, indent);
                    break;
                case ObjectInstantiationNode objectInstantiationNode:
                    GenerateObjectInstantiation(objectInstantiationNode, indent);
                    break;
                default:
                    throw new Exception($"Unknown statement type: {statement.GetType().Name}");
            }
        }

        private void GenerateMemberAccess(MemberAccessNode memberAccess, string indent = "", bool addSemicolon = false)
        {
            _code.Append(indent + $"{memberAccess.ObjectName}.");
            if (memberAccess.Expression is FunctionCallNode functionCallNode)
            {
                GenerateExpression(functionCallNode);
                if (addSemicolon)
                    _code.AppendLine(";");
            }
            else if (memberAccess.Expression is IdentifierNode identifierNode)
                GenerateExpression(identifierNode);
            else if (memberAccess.Expression is CompoundAssignmentNode compoundAssignment)
                GenerateStatement(compoundAssignment);
            else if (memberAccess.Expression is AssignmentNode assignment)
                GenerateStatement(assignment);
            else if (memberAccess.Expression is BinaryExpressionNode binaryExpression)
                GenerateExpression(binaryExpression);
            else if (memberAccess.Expression is ExpressionStatementNode expressionStatement)
                GenerateStatement(expressionStatement);
            else
                _code.Append($"{memberAccess.Expression.GetType().Name}");


        }

        private void GenerateObjectInstantiation(ObjectInstantiationNode objectInstantiation, string indent = "")
        {
            _code.Append(indent + $"{objectInstantiation.PackageName} {objectInstantiation.Name} = new {objectInstantiation.PackageName}(");
            for (int i = 0; i < objectInstantiation.Arguments.Count; i++)
            {
                GenerateExpression(objectInstantiation.Arguments[i]);
                if (i < objectInstantiation.Arguments.Count - 1)
                {
                    _code.Append(", ");
                }
            }
            _code.Append(")");
            _code.AppendLine(";");
        }

        /// <summary>
        /// Generates code for a local variable declaration.
        /// </summary>
        /// <param name="varDecl">The variable declaration node.</param>
        /// <param name="indent"></param>
        private void GenerateLocalVariable(VariableDeclarationNode varDecl, string indent = "")
        {
            _code.Append(indent + $"{ConvertType(varDecl.Type)} {varDecl.Name}");
            if (varDecl.Initializer != null)
            {
                _code.Append(" = ");
                GenerateExpression(varDecl.Initializer);
            }
            _code.AppendLine(";");
        }

        /// <summary>
        /// Generates code for an assignment statement.
        /// </summary>
        /// <param name="assign">The assignment node.</param>
        /// <param name="indent"></param>
        private void GenerateAssignment(AssignmentNode assign, string indent = "")
        {
            _code.Append(indent + $"{assign.Name} = ");
            GenerateExpression(assign.Expression);

            // If the last character is not semicolon  add it
            TrimEndNewline(ref _code);
            if (_code.Length > 0 && _code[_code.Length - 1] != ';')
                _code.AppendLine(";");
            else
                _code.AppendLine();

            //_code.AppendLine(";");
        }

        private static void TrimEndNewline(ref StringBuilder sb)
        {
            if (sb.Length >= 2)
            {
                // Check if the last two characters are \r\n
                if (sb[sb.Length - 2] == '\r' && sb[sb.Length - 1] == '\n')
                {
                    // Remove the last two characters
                    sb.Length -= 2;
                }
            }
        }

        /// <summary>
        /// Generates code for an array assignment statement.
        /// </summary>
        /// <param name="assign">The array assignment node.</param>
        /// <param name="indent"></param>
        private void GenerateArrayAssignment(ArrayAssignmentNode assign, string indent = "")
        {
            _code.Append(indent + $"{assign.Name}[");
            GenerateExpression(assign.Index);
            _code.Append("] = ");
            GenerateExpression(assign.Value);
            _code.AppendLine(";");
        }

        /// <summary>
        /// Generates code for a compound assignment statement.
        /// </summary>
        /// <param name="compAssign">The compound assignment node.</param>
        /// <param name="indent"></param>
        private void GenerateCompoundAssignment(CompoundAssignmentNode compAssign, string indent = "")
        {
            _code.Append(indent + $"{compAssign.Name} {compAssign.Operator} ");
            GenerateExpression(compAssign.Expression);
            _code.AppendLine(";");
        }

        /// <summary>
        /// Generates code for an increment or decrement statement.
        /// </summary>
        /// <param name="incDec">The increment or decrement node.</param>
        /// <param name="indent"></param>
        private void GenerateIncrementDecrement(IncrementDecrementNode incDec, string indent = "")
        {
            _code.AppendLine(indent + $"{incDec.Name}{incDec.Operator};");
        }

        /// <summary>
        /// Generates code for an if statement.
        /// </summary>
        /// <param name="ifStatement">The if statement node.</param>
        /// <param name="indent"></param>
        private void GenerateIfStatement(IfStatementNode ifStatement, string indent = "")
        {
            _code.Append(indent + "if (");
            GenerateExpression(ifStatement.Condition);
            _code.AppendLine(")");
            _code.AppendLine(indent + "{");
            GenerateStatement(ifStatement.ThenBranch, indent + "  ");
            _code.AppendLine(indent + "}");
            if (ifStatement.ElseBranch != null)
            {
                _code.AppendLine(indent + "else");
                _code.AppendLine(indent + "{");
                GenerateStatement(ifStatement.ElseBranch, indent + "  ");
                _code.AppendLine(indent + "}");
            }
        }

        /// <summary>
        /// Generates code for a while statement.
        /// </summary>
        /// <param name="whileStatement">The while statement node.</param>
        /// <param name="indent"></param>
        private void GenerateWhileStatement(WhileStatementNode whileStatement, string indent = "")
        {
            _code.Append(indent + "while (");
            GenerateExpression(whileStatement.Condition);
            _code.AppendLine(")");
            _code.AppendLine(indent + "{");
            GenerateStatement(whileStatement.Body, indent + "  ");
            _code.AppendLine(indent + "}");
        }

        /// <summary>
        /// Generates code for a for statement.
        /// </summary>
        /// <param name="forStatement">The for statement node.</param>
        /// <param name="indent"></param>
        private void GenerateForStatement(ForStatementNode forStatement, string indent = "")
        {
            _code.Append(indent + "for (");
            GenerateStatement(forStatement.Initializer);
            RemoveTrailingCharacters(_code, new char[] { '\n' });
            GenerateExpression(forStatement.Condition);
            _code.Append(';');
            GenerateStatement(forStatement.Increment);
            RemoveTrailingCharacters(_code, new char[] { '\n', ';' });

            _code.AppendLine(")");
            _code.AppendLine(indent + "{");
            GenerateStatement(forStatement.Body, indent + "  ");
            _code.AppendLine(indent + "}");
        }

        /// <summary>
        /// Generates code for an expression.
        /// </summary>
        /// <param name="expression">The expression node.</param>
        /// <param name="isPrintFlag">Indicates if the expression is part of a print function.</param>
        /// <param name="indent"></param>
        private void GenerateExpression(ExpressionNode expression, string indent = "", bool isPrintFlag = false)
        {
            switch (expression)
            {
                case LiteralNode literal:
                    GenerateLiteral(literal);
                    break;
                case IdentifierNode identifier:
                    _code.Append(indent + identifier.Name);
                    break;
                case ArrayAccessNode arrayAccess:
                    _code.Append(indent + $"{arrayAccess.Name}[");
                    GenerateExpression(arrayAccess.Index);
                    _code.Append("]");
                    break;
                case ArrayInitializerNode arrayInitializerNode:
                    _code.Append("{");
                    for (int i = 0; i < arrayInitializerNode.Elements.Count; i++)
                    {
                        _code.Append(((LiteralNode)arrayInitializerNode.Elements[i]).Value);
                        if (i < arrayInitializerNode.Elements.Count - 1)
                        {
                            _code.Append(", ");
                        }
                    }
                    _code.Append("}");
                    break;
                case FunctionCallNode node:
                    if (node.FunctionName == "print")
                    {
                        GeneratePrintFunctionCall(node, indent);
                    }
                    else
                    {
                        var functionManagementClass = FactoryClass.GetCodeGeneratorClass(node.FunctionName);
                        if (functionManagementClass != null)
                        {
                            functionManagementClass.HandleFunctionCall(node.FunctionName, ref _code, ref _defaultFunction, ref _codeHeader, ref _codeInclude);
                        }
                        _code.Append(indent + $"{node.FunctionName}(");
                        GenerateFunctionCallArguments(node.Arguments);
                        _code.Append(")");
                    }
                    break;
                case BinaryExpressionNode binaryExpr:
                    // Check if we are in a print function to replace + with <<
                    if (isPrintFlag && binaryExpr.Operator == "+")
                    {
                        GenerateExpression(binaryExpr.Left);
                        _code.Append(" << ");
                        GenerateExpression(binaryExpr.Right);
                    }
                    else
                    {
                        GenerateExpression(binaryExpr.Left);
                        _code.Append($" {binaryExpr.Operator} ");
                        GenerateExpression(binaryExpr.Right);
                    }
                    break;
                case UnaryExpressionNode unaryExpr:
                    _code.Append($"{unaryExpr.Operator}");
                    GenerateExpression(unaryExpr.Operand);
                    break;
                case LogicalExpressionNode logicalExpr:
                    GenerateExpression(logicalExpr.Left);
                    _code.Append($" {logicalExpr.Operator} ");
                    GenerateExpression(logicalExpr.Right);
                    break;
                case MemberAccessNode memberAccessExpr:
                    GenerateMemberAccess(memberAccessExpr);
                    break;
                default:
                    throw new Exception($"Unknown expression type: {expression.GetType().Name}");
            }
        }

        /// <summary>
        /// Generates code for function call arguments.
        /// </summary>
        /// <param name="arguments">The list of expression nodes representing the arguments.</param>
        private void GenerateFunctionCallArguments(List<ExpressionNode> arguments)
        {
            for (int i = 0; i < arguments.Count; i++)
            {
                GenerateExpression(arguments[i]);
                if (i < arguments.Count - 1)
                {
                    _code.Append(", ");
                }
            }
        }

        /// <summary>
        /// Generates code for a literal node.
        /// </summary>
        /// <param name="literal">The literal node.</param>
        private void GenerateLiteral(LiteralNode literal)
        {
            if (literal.Type == TokenType.STRING_LITERAL)
            {
                _code.Append($"\"{literal.Value}\"");
            }
            else
            {
                _code.Append(literal.Value);
            }
        }

        /// <summary>
        /// Generates code for a print function call.
        /// </summary>
        /// <param name="funcCall">The function call node.</param>
        /// <param name="indent"></param>
        private void GeneratePrintFunctionCall(FunctionCallNode funcCall, string indent = "")
        {
            _code.Append(indent + "std::cout << ");
            GenerateExpression(funcCall.Arguments[0], "", true); // Assuming print has exactly one argument
            _code.Append(" << std::endl");
        }

        /// <summary>
        /// Converts a custom type to a C++ type.
        /// </summary>
        /// <param name="type">The custom type.</param>
        /// <returns>The C++ type as a string.</returns>
        private string ConvertType(string? type)
        {
            switch (type)
            {
                case "number":
                    return "double";
                case "string":
                    return "std::string";
                case "bool":
                    return "bool";
                case "number[]":
                    return "std::vector<double>";
                case "string[]":
                    return "std::vector<string>";
                case "bool[]":
                    return "std::vector<bool>";
                case "void":
                    return "void";
                case "let":
                    return "auto";
                default:
                    throw new Exception($"Unknown type: {type}");
            }
        }

        /// <summary>
        /// Removes trailing characters from a StringBuilder.
        /// </summary>
        /// <param name="sb">The StringBuilder to modify.</param>
        /// <param name="characters">The characters to remove.</param>
        static void RemoveTrailingCharacters(StringBuilder sb, char[] characters)
        {
            if (sb.Length == 0) return;
            bool removed;
            do
            {
                removed = false;
                foreach (char character in characters)
                {
                    string trailingString = character == '\n' ? "\r\n" : character.ToString();
                    if (sb.ToString().EndsWith(trailingString))
                    {
                        sb.Length -= trailingString.Length; // Remove the specified character(s)
                        removed = true;
                    }
                    else if (sb.ToString().EndsWith(character.ToString()))
                    {
                        sb.Length--; // Remove the specified character
                        removed = true;
                    }
                }
            } while (removed && sb.Length > 0);
        }
    }
}