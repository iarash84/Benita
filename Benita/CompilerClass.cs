namespace Benita
{
    public class CompilerClass
    {

        public void Exec(string sourceCode, bool lexerPrint = false, bool parserPrint = false, bool sourcePrint = false)
        {
            // 1. Tokenize the source code
            List<Token> tokens = TokenizeCode(sourceCode, lexerPrint, sourcePrint);

            // 2. Parse the tokens to create an AST
            ProgramNode? programAst = ParseCode(tokens, parserPrint);

            // 3. Analyze the AST using SemanticAnalyzer
            SemanticAnalyzer semanticAnalyzer = new SemanticAnalyzer();
            semanticAnalyzer.Analyze(programAst);

            var interpreter = new Interpreter();
            interpreter.Visit(programAst);
        }

        public string GenerateCppCode(string sourceCode, bool lexerPrint = false, bool parserPrint = false, bool sourcePrint = false)
        {
            // 1. Tokenize the source code
            List<Token> tokens = TokenizeCode(sourceCode, lexerPrint, sourcePrint);

            // 2. Parse the tokens to create an AST
            ProgramNode? programAst = ParseCode(tokens, parserPrint);

            // 3. Analyze the AST using SemanticAnalyzer
            SemanticAnalyzer semanticAnalyzer = new SemanticAnalyzer();
            semanticAnalyzer.Analyze(programAst);

            var codeGenerator = new CodeGenerator();
            var generatedCode = codeGenerator.GenerateCode(programAst);

            return generatedCode;
        }


        private List<Token> TokenizeCode(string sourceCode, bool resultPrint = false, bool sourcePrint = false)
        {
            Lexer lexer = new Lexer(sourceCode, sourcePrint);
            List<Token> tokens = lexer.Tokenize();
            if (resultPrint)
                foreach (Token token in tokens)
                {
                    Console.WriteLine(token.ToString());
                }
            return tokens;
        }

        private ProgramNode? ParseCode(List<Token> tokens, bool resultPrint = false)
        {
            // 2. Parse the tokens to create an AST
            Parser parser = new Parser(tokens);
            ProgramNode programAst = parser.Parse();
            if (resultPrint)
                PrintAst(programAst);
            return programAst;
        }

        private void PrintAst(AstNode? node, string indent = "")
        {
            if (node == null) return;

            Console.WriteLine($"{indent}{node.GetType().Name}");
            switch (node)
            {
                case ProgramNode programNode:
                    {
                        foreach (var globalVariable in programNode.GlobalVariables)
                        {
                            PrintAst(globalVariable, indent + "  ");
                        }
                        foreach (var function in programNode.Functions)
                        {
                            PrintAst(function, indent + "  ");
                        }
                        PrintAst(programNode.MainFunction, indent + "  ");
                        break;
                    }

                case FunctionNode functionNode:
                    {
                        Console.WriteLine($"{indent}  Name: {functionNode.Name}");
                        foreach (var param in functionNode.Parameters)
                        {
                            PrintAst(param, indent + "  ");
                        }
                        PrintAst(functionNode.Body, indent + "  ");
                        Console.WriteLine($"{indent}  ReturnExpression");
                        PrintAst(functionNode.ReturnStatement.ReturnExpression, indent + "  " + "  ");
                        break;
                    }

                case ParameterNode parameterNode:
                    Console.WriteLine($"{indent}  Type: {parameterNode.Type}, Name: {parameterNode.Name}");
                    break;
                case BlockNode blockNode:
                    {
                        foreach (var statement in blockNode.Statements)
                        {
                            PrintAst(statement, indent + "  ");
                        }

                        break;
                    }

                case VariableDeclarationNode varDeclNode:
                    Console.WriteLine($"{indent}  Type: {varDeclNode.Type}, Name: {varDeclNode.Name}");
                    PrintAst(varDeclNode.Initializer, indent + "  ");
                    break;
                case AssignmentNode assignmentNode:
                    Console.WriteLine($"{indent}  Name: {assignmentNode.Name}");
                    PrintAst(assignmentNode.Expression, indent + "  ");
                    break;
                case ArrayAssignmentNode arrayAssignmentNode:
                    Console.WriteLine($"{indent}  Name: {arrayAssignmentNode.Name}");
                    Console.WriteLine($"{indent}  Index => ");
                    PrintAst(arrayAssignmentNode.Index, indent + "  ");
                    Console.WriteLine($"{indent}  Value => ");
                    PrintAst(arrayAssignmentNode.Value, indent + "  ");
                    break;
                case ExpressionStatementNode exprStmtNode:
                    PrintAst(exprStmtNode.Expression, indent + "  ");
                    break;
                case LiteralNode literalNode:
                    Console.WriteLine($"{indent}  Value: {literalNode.Value}");
                    break;
                case IdentifierNode identifierNode:
                    Console.WriteLine($"{indent}  Name: {identifierNode.Name}");
                    break;
                case BinaryExpressionNode binaryExprNode:
                    Console.WriteLine($"{indent}  Operator: {binaryExprNode.Operator}");
                    PrintAst(binaryExprNode.Left, indent + "  ");
                    PrintAst(binaryExprNode.Right, indent + "  ");
                    break;
                case UnaryExpressionNode unaryExprNode:
                    Console.WriteLine($"{indent}  Operator: {unaryExprNode.Operator}");
                    PrintAst(unaryExprNode.Operand, indent + "  ");
                    break;
                case FunctionCallNode functionCallNode:
                    {
                        Console.WriteLine($"{indent}  FunctionName: {functionCallNode.FunctionName}");
                        foreach (var arg in functionCallNode.Arguments)
                        {
                            PrintAst(arg, indent + "  ");
                        }

                        break;
                    }

                case IfStatementNode ifStatementNode:
                    PrintAst(ifStatementNode.Condition, indent + "  ");
                    PrintAst(ifStatementNode.ThenBranch, indent + "  ");
                    PrintAst(ifStatementNode.ElseBranch, indent + "  ");
                    break;
                case WhileStatementNode whileStatementNode:
                    PrintAst(whileStatementNode.Condition, indent + "  ");
                    PrintAst(whileStatementNode.Body, indent + "  ");
                    break;
                case IncrementDecrementNode incDecNode:
                    Console.WriteLine($"{indent}  Name: {incDecNode.Name}");
                    Console.WriteLine($"{indent}  Operator: {incDecNode.Operator}");
                    break;
                case ArrayInitializerNode arrayInitNode:
                    {
                        Console.WriteLine($"{indent}  ArrayInitializer");
                        foreach (var element in arrayInitNode.Elements)
                        {
                            PrintAst(element, indent + "  ");
                        }

                        break;
                    }

                case ArrayAccessNode arrayAccessNode:
                    Console.WriteLine($"{indent}  ArrayName: {arrayAccessNode.Name}");
                    PrintAst(arrayAccessNode.Index, indent + "  ");
                    break;
                case LogicalExpressionNode logicalExpressionNode:
                    PrintAst(logicalExpressionNode.Left, indent + "  ");
                    Console.WriteLine($"{indent}  Operator: {logicalExpressionNode.Operator}");
                    PrintAst(logicalExpressionNode.Right, indent + "  ");
                    break;
                default:
                    Console.WriteLine($"{indent}  Unknown node type: {node.GetType().Name}");
                    break;
            }
        }
    }
}
