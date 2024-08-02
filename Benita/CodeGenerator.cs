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
            StringBuilder _finalCode = new StringBuilder();
            _finalCode.Clear();
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
            GenerateMainFunction(program.MainFunction);

            _codeInclude.AppendLine();
            _finalCode.Append(_codeInclude);
            _finalCode.Append(_codeHeader);
            _finalCode.Append(_code);
            _finalCode.Append(_defaultFunction);

            return _finalCode.ToString();
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
        private void GenerateGlobalVariable(VariableDeclarationNode globalVar)
        {
            _code.Append($"{ConvertType(globalVar.Type)} {globalVar.Name}");
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
        private void GenerateFunction(FunctionNode function)
        {
            _code.Append($"{ConvertType(function.ReturnType)} {function.Name}(");
            for (int i = 0; i < function.Parameters.Count; i++)
            {
                _code.Append($"{ConvertType(function.Parameters[i].Type)} {function.Parameters[i].Name}");
                if (i < function.Parameters.Count - 1)
                {
                    _code.Append(", ");
                }
            }

            _code.AppendLine(")");
            _code.AppendLine("{");
            GenerateBlock(function.Body);
            if (function.ReturnStatement != null)
            {
                _code.Append("return ");
                GenerateExpression(function.ReturnStatement.ReturnExpression);
                _code.AppendLine(";");
            }
            _code.AppendLine("}");
            _code.AppendLine();
        }

        /// <summary>
        /// Generates code for a return expression.
        /// </summary>
        /// <param name="expressionNode">The expression node.</param>
        private void GenerateReturnExpression(ExpressionNode expressionNode)
        {
            if (expressionNode != null)
            {
                _code.Append("return ");
                GenerateExpression(expressionNode);
            }
            else
                _code.Append("return");

            _code.AppendLine(";");
        }

        /// <summary>
        /// Generates code for the main function.
        /// </summary>
        /// <param name="mainFunction">The main function node.</param>
        private void GenerateMainFunction(FunctionNode mainFunction)
        {
            _code.AppendLine("int main()");
            _code.AppendLine("{");
            GenerateBlock(mainFunction.Body);
            _code.AppendLine("return 0;");
            _code.AppendLine("}");
        }

        /// <summary>
        /// Generates code for a block of statements.
        /// </summary>
        /// <param name="block">The block node.</param>
        private void GenerateBlock(BlockNode block)
        {
            foreach (var statement in block.Statements)
            {
                GenerateStatement(statement);
            }
        }

        /// <summary>
        /// Generates code for a statement.
        /// </summary>
        /// <param name="statement">The statement node.</param>
        private void GenerateStatement(StatementNode statement)
        {
            switch (statement)
            {
                case VariableDeclarationNode varDecl:
                    GenerateLocalVariable(varDecl);
                    break;
                case AssignmentNode assign:
                    GenerateAssignment(assign);
                    break;
                case CompoundAssignmentNode compAssign:
                    GenerateCompoundAssignment(compAssign);
                    break;
                case IncrementDecrementNode incDec:
                    GenerateIncrementDecrement(incDec);
                    break;
                case IfStatementNode ifStatement:
                    GenerateIfStatement(ifStatement);
                    break;
                case WhileStatementNode whileStatement:
                    GenerateWhileStatement(whileStatement);
                    break;
                case ForStatementNode forStatement:
                    GenerateForStatement(forStatement);
                    break;
                case BlockNode block:
                    GenerateBlock(block);
                    break;
                case ExpressionStatementNode exprStatement:
                    GenerateExpression(exprStatement.Expression);
                    _code.AppendLine(";");
                    break;
                case ArrayAssignmentNode arrayAssignmentNode:
                    GenerateArrayAssignment(arrayAssignmentNode);
                    break;
                case ReturnStatementNode returnAssignmentNode:
                    GenerateReturnExpression(returnAssignmentNode.ReturnExpression);
                    break;
                default:
                    throw new Exception($"Unknown statement type: {statement.GetType().Name}");
            }
        }

        /// <summary>
        /// Generates code for a local variable declaration.
        /// </summary>
        /// <param name="varDecl">The variable declaration node.</param>
        private void GenerateLocalVariable(VariableDeclarationNode varDecl)
        {
            _code.Append($"{ConvertType(varDecl.Type)} {varDecl.Name}");
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
        private void GenerateAssignment(AssignmentNode assign)
        {
            _code.Append($"{assign.Name} = ");
            GenerateExpression(assign.Expression);
            _code.AppendLine(";");
        }

        /// <summary>
        /// Generates code for an array assignment statement.
        /// </summary>
        /// <param name="assign">The array assignment node.</param>
        private void GenerateArrayAssignment(ArrayAssignmentNode assign)
        {
            _code.Append($"{assign.Name}[");
            GenerateExpression(assign.Index);
            _code.Append("] = ");
            GenerateExpression(assign.Value);
            _code.AppendLine(";");
        }

        /// <summary>
        /// Generates code for a compound assignment statement.
        /// </summary>
        /// <param name="compAssign">The compound assignment node.</param>
        private void GenerateCompoundAssignment(CompoundAssignmentNode compAssign)
        {
            _code.Append($"{compAssign.Name} {compAssign.Operator} ");
            GenerateExpression(compAssign.Expression);
            _code.AppendLine(";");
        }

        /// <summary>
        /// Generates code for an increment or decrement statement.
        /// </summary>
        /// <param name="incDec">The increment or decrement node.</param>
        private void GenerateIncrementDecrement(IncrementDecrementNode incDec)
        {
            _code.AppendLine($"{incDec.Name}{incDec.Operator};");
        }

        /// <summary>
        /// Generates code for an if statement.
        /// </summary>
        /// <param name="ifStatement">The if statement node.</param>
        private void GenerateIfStatement(IfStatementNode ifStatement)
        {
            _code.Append("if (");
            GenerateExpression(ifStatement.Condition);
            _code.AppendLine(")");
            _code.AppendLine("{");
            GenerateStatement(ifStatement.ThenBranch);
            _code.AppendLine("}");
            if (ifStatement.ElseBranch != null)
            {
                _code.AppendLine("else");
                _code.AppendLine("{");
                GenerateStatement(ifStatement.ElseBranch);
                _code.AppendLine("}");
            }
        }

        /// <summary>
        /// Generates code for a while statement.
        /// </summary>
        /// <param name="whileStatement">The while statement node.</param>
        private void GenerateWhileStatement(WhileStatementNode whileStatement)
        {
            _code.Append("while (");
            GenerateExpression(whileStatement.Condition);
            _code.AppendLine(")");
            _code.AppendLine("{");
            GenerateStatement(whileStatement.Body);
            _code.AppendLine("}");
        }

        /// <summary>
        /// Generates code for a for statement.
        /// </summary>
        /// <param name="forStatement">The for statement node.</param>
        private void GenerateForStatement(ForStatementNode forStatement)
        {
            _code.Append("for (");
            GenerateStatement(forStatement.Initializer);
            RemoveTrailingCharacters(_code, new char[] { '\n' });
            GenerateExpression(forStatement.Condition);
            _code.Append(';');
            GenerateStatement(forStatement.Increment);
            RemoveTrailingCharacters(_code, new char[] { '\n', ';' });

            _code.AppendLine(")");
            _code.AppendLine("{");
            GenerateStatement(forStatement.Body);
            _code.AppendLine("}");
        }

        /// <summary>
        /// Generates code for an expression.
        /// </summary>
        /// <param name="expression">The expression node.</param>
        /// <param name="isPrintFlag">Indicates if the expression is part of a print function.</param>
        private void GenerateExpression(ExpressionNode expression, bool isPrintFlag = false)
        {
            switch (expression)
            {
                case LiteralNode literal:
                    GenerateLiteral(literal);
                    break;
                case IdentifierNode identifier:
                    _code.Append(identifier.Name);
                    break;
                case ArrayAccessNode arrayAccess:
                    _code.Append($"{arrayAccess.Name}[");
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
                        GeneratePrintFunctionCall(node);
                    }
                    else
                    {
                        var functionManagementClass = FactoryClass.GetCodeGeneratorClass(node.FunctionName);
                        if (functionManagementClass != null)
                        {
                            functionManagementClass.HandleFunctionCall(node.FunctionName, ref _code, ref _defaultFunction, ref _codeHeader, ref _codeInclude);
                        }
                        _code.Append($"{node.FunctionName}(");
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
        private void GeneratePrintFunctionCall(FunctionCallNode funcCall)
        {
            _code.Append("std::cout << ");
            GenerateExpression(funcCall.Arguments[0], true); // Assuming print has exactly one argument
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
