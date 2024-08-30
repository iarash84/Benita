namespace Benita
{
    public class CompilerClass
    {

        public void Exec(string sourceCode, bool lexerPrint = false, bool parserPrint = false, bool sourcePrint = false)
        {
            // 1. Tokenize the source code
            List<Token> tokens = TokenizeCode(sourceCode: sourceCode, resultPrint: lexerPrint, sourcePrint: sourcePrint);

            // 2. Parse the tokens to create an AST
            ProgramNode? programAst = ParseCode(tokens: tokens, resultPrint: parserPrint);

            // 3. Analyze the AST using SemanticAnalyzer
            SemanticAnalyzer semanticAnalyzer = new SemanticAnalyzer();
            semanticAnalyzer.Analyze(program: programAst);

            var interpreter = new Interpreter();
            interpreter.Visit(node: programAst);
        }

        public string GenerateCppCode(string sourceCode, bool lexerPrint = false, bool parserPrint = false, bool sourcePrint = false)
        {
            // 1. Tokenize the source code
            List<Token> tokens = TokenizeCode(sourceCode: sourceCode, resultPrint: lexerPrint, sourcePrint: sourcePrint);

            // 2. Parse the tokens to create an AST
            ProgramNode? programAst = ParseCode(tokens: tokens, resultPrint: parserPrint);

            // 3. Analyze the AST using SemanticAnalyzer
            SemanticAnalyzer semanticAnalyzer = new SemanticAnalyzer();
            semanticAnalyzer.Analyze(program: programAst);

            var codeGenerator = new CodeGenerator();
            var generatedCode = codeGenerator.GenerateCode(program: programAst);

            return generatedCode;
        }


        private List<Token> TokenizeCode(string sourceCode, bool resultPrint = false, bool sourcePrint = false)
        {
            Lexer lexer = new Lexer(source: sourceCode, sourcePrint: sourcePrint);
            List<Token> tokens = lexer.Tokenize();
            if (resultPrint)
                foreach (Token token in tokens)
                {
                    Console.WriteLine(value: token.ToString());
                }
            return tokens;
        }

        private ProgramNode? ParseCode(List<Token> tokens, bool resultPrint = false)
        {
            // 2. Parse the tokens to create an AST
            Parser parser = new Parser(tokens: tokens);
            ProgramNode programAst = parser.Parse();
            if (resultPrint)
                PrintAst(node: programAst);
            return programAst;
        }

        private void PrintAst(AstNode node, string indent = "")
        {
            if (node == null) return;

            Console.WriteLine(value: $"{indent}{node.GetType().Name}");
            switch (node)
            {
                case ProgramNode programNode:
                    foreach (var globalVariable in programNode.GlobalVariables)
                    {
                        PrintAst(node: globalVariable, indent: indent + "  ");
                    }
                    foreach (var packageNode in programNode.Packages)
                    {
                        PrintAst(node: packageNode, indent: indent + "  ");
                    }
                    foreach (var function in programNode.Functions)
                    {
                        PrintAst(node: function, indent: indent + "  ");
                    }
                    PrintAst(node: programNode.MainFunction, indent: indent + "  ");
                    break;

                case PackageNode packageNode:
                    foreach (var packageMember in packageNode.Members)
                    {
                        PrintAst(node: packageMember, indent: indent + "  ");
                    }
                    break;

                case PackageVariableDeclarationNode packageVariableDeclaration:
                    Console.WriteLine(
                        value: $"{indent}  Type: {packageVariableDeclaration.Type}, Name: {packageVariableDeclaration.Name}");
                    PrintAst(node: packageVariableDeclaration.Initializer, indent: indent + "  ");
                    break;

                case MemberAccessNode memberAccessNode:
                    Console.WriteLine(value: $"{indent}  Object Name: {memberAccessNode.ObjectName}");
                    PrintAst(node: memberAccessNode.Expression, indent: indent + "  ");
                    break;

                case PackageFunctionNode packageFunctionNode:
                    Console.WriteLine(value: $"{indent}  Name: {packageFunctionNode.Name}");
                    foreach (var param in packageFunctionNode.Parameters)
                    {
                        PrintAst(node: param, indent: indent + "  ");
                    }

                    PrintAst(node: packageFunctionNode.Body, indent: indent + "  ");
                    if (packageFunctionNode.ReturnStatement != null)
                    {
                        Console.WriteLine(value: $"{indent}  ReturnExpression");
                        PrintAst(node: packageFunctionNode.ReturnStatement.ReturnExpression, indent: indent + "  " + "  ");
                    }
                    break;

                case FunctionNode functionNode:
                    Console.WriteLine(value: $"{indent}  Name: {functionNode.Name}");
                    foreach (var param in functionNode.Parameters)
                    {
                        PrintAst(node: param, indent: indent + "  ");
                    }

                    PrintAst(node: functionNode.Body, indent: indent + "  ");
                    if (functionNode.ReturnStatement != null)
                    {
                        Console.WriteLine(value: $"{indent}  ReturnExpression");
                        PrintAst(node: functionNode.ReturnStatement.ReturnExpression, indent: indent + "  " + "  ");
                    }
                    break;

                case ParameterNode parameterNode:
                    Console.WriteLine(value: $"{indent}  Type: {parameterNode.Type}, Name: {parameterNode.Name}");
                    break;

                case BlockNode blockNode:
                    foreach (var statement in blockNode.Statements)
                    {
                        PrintAst(node: statement, indent: indent + "  ");
                    }
                    break;

                case VariableDeclarationNode varDeclNode:
                    Console.WriteLine(value: $"{indent}  Type: {varDeclNode.Type}, Name: {varDeclNode.Name}");
                    PrintAst(node: varDeclNode.Initializer, indent: indent + "  ");
                    break;

                case AssignmentNode assignmentNode:
                    Console.WriteLine(value: $"{indent}  Name: {assignmentNode.Name}");
                    PrintAst(node: assignmentNode.Expression, indent: indent + "  ");
                    break;

                case ArrayAssignmentNode arrayAssignmentNode:
                    Console.WriteLine(value: $"{indent}  Name: {arrayAssignmentNode.Name}");
                    Console.WriteLine(value: $"{indent}  Index => ");
                    PrintAst(node: arrayAssignmentNode.Index, indent: indent + "  ");
                    Console.WriteLine(value: $"{indent}  Value => ");
                    PrintAst(node: arrayAssignmentNode.Value, indent: indent + "  ");
                    break;

                case ExpressionStatementNode exprStmtNode:
                    PrintAst(node: exprStmtNode.Expression, indent: indent + "  ");
                    break;

                case LiteralNode literalNode:
                    Console.WriteLine(value: $"{indent}  Value: {literalNode.Value}");
                    break;

                case IdentifierNode identifierNode:
                    Console.WriteLine(value: $"{indent}  Name: {identifierNode.Name}");
                    break;

                case BinaryExpressionNode binaryExprNode:
                    Console.WriteLine(value: $"{indent}  Operator: {binaryExprNode.Operator}");
                    PrintAst(node: binaryExprNode.Left, indent: indent + "  ");
                    PrintAst(node: binaryExprNode.Right, indent: indent + "  ");
                    break;

                case UnaryExpressionNode unaryExprNode:
                    Console.WriteLine(value: $"{indent}  Operator: {unaryExprNode.Operator}");
                    PrintAst(node: unaryExprNode.Operand, indent: indent + "  ");
                    break;

                case FunctionCallNode functionCallNode:
                    Console.WriteLine(value: $"{indent}  FunctionName: {functionCallNode.FunctionName}");
                    foreach (var arg in functionCallNode.Arguments)
                    {
                        PrintAst(node: arg, indent: indent + "  ");
                    }
                    break;

                case IfStatementNode ifStatementNode:
                    PrintAst(node: ifStatementNode.Condition, indent: indent + "  ");
                    PrintAst(node: ifStatementNode.ThenBranch, indent: indent + "  ");
                    PrintAst(node: ifStatementNode.ElseBranch, indent: indent + "  ");
                    break;

                case WhileStatementNode whileStatementNode:
                    PrintAst(node: whileStatementNode.Condition, indent: indent + "  ");
                    PrintAst(node: whileStatementNode.Body, indent: indent + "  ");
                    break;

                case IncrementDecrementNode incDecNode:
                    Console.WriteLine(value: $"{indent}  Name: {incDecNode.Name}");
                    Console.WriteLine(value: $"{indent}  Operator: {incDecNode.Operator}");
                    break;

                case ArrayInitializerNode arrayInitNode:
                    Console.WriteLine(value: $"{indent}  ArrayInitializer");
                    foreach (var element in arrayInitNode.Elements)
                    {
                        PrintAst(node: element, indent: indent + "  ");
                    }
                    break;

                case ArrayAccessNode arrayAccessNode:
                    Console.WriteLine(value: $"{indent}  ArrayName: {arrayAccessNode.Name}");
                    PrintAst(node: arrayAccessNode.Index, indent: indent + "  ");
                    break;

                case LogicalExpressionNode logicalExpressionNode:
                    PrintAst(node: logicalExpressionNode.Left, indent: indent + "  ");
                    Console.WriteLine(value: $"{indent}  Operator: {logicalExpressionNode.Operator}");
                    PrintAst(node: logicalExpressionNode.Right, indent: indent + "  ");
                    break;

                case ObjectInstantiationNode objectInstantiationNode:
                    Console.WriteLine(value: $"{indent}  Package Name: {objectInstantiationNode.PackageName}");
                    Console.WriteLine(value: $"{indent}  Name: {objectInstantiationNode.Name}");
                    foreach (var arg in objectInstantiationNode.Arguments)
                    {
                        PrintAst(node: arg, indent: indent + "  ");
                    }
                    break;
                case BreakStatementNode:
                    Console.WriteLine(value: $"{indent}  BreakStatementNode");
                    break;
                case ContinueStatementNode:
                    Console.WriteLine(value: $"{indent}  ContinueStatementNode");
                    break;
                default:
                    Console.WriteLine(value: $"{indent}  Unknown node type: {node.GetType().Name}");
                    break;
            }
        }
    }
}
