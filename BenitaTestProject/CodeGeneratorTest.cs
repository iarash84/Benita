using Benita;

namespace BenitaTestProject
{
    [TestClass]
    public class CodeGeneratorTest
    {

        private CodeGenerator _codeGenerator;

        [TestInitialize]
        public void Setup()
        {
            _codeGenerator = new CodeGenerator();
        }

        [TestMethod]
        public void GenerateCode_SimpleFunction_ReturnsExpectedCode()
        {
            // Arrange
            var program = new ProgramNode(
                new List<VariableDeclarationNode>(), // GlobalVariables
                new List<FunctionNode>
                {
                    new FunctionNode(
                        "Factorial",
                        new List<ParameterNode>
                        {
                            new ParameterNode("number", "n")
                        },
                        "number",
                        new BlockNode(
                            new List<StatementNode>
                            {
                                new VariableDeclarationNode(
                                    "number",
                                    "result",
                                    new LiteralNode("1", TokenType.NUMBER_LITERAL)
                                ),
                                new IfStatementNode(
                                    new BinaryExpressionNode(
                                        new IdentifierNode("n"),
                                        ">",
                                        new LiteralNode("1", TokenType.NUMBER_LITERAL)
                                    ),
                                    new BlockNode(
                                        new List<StatementNode>
                                        {
                                            new AssignmentNode(
                                                "result",
                                                new BinaryExpressionNode(
                                                    new IdentifierNode("n"),
                                                    "*",
                                                    new FunctionCallNode(
                                                        "Factorial",
                                                        new List<ExpressionNode>
                                                        {
                                                            new BinaryExpressionNode(
                                                                new IdentifierNode("n"),
                                                                "-",
                                                                new LiteralNode("1", TokenType.NUMBER_LITERAL)
                                                            )
                                                        }
                                                    )
                                                )
                                            )
                                        }
                                    ),
                                    null
                                )
                            }
                        ),
                        new ReturnStatementNode(new IdentifierNode("result"))
                    )
                },
                new FunctionNode(
                    "_main_",
                    new List<ParameterNode>(),
                    "void",
                    new BlockNode(
                        new List<StatementNode>
                        {
                            new VariableDeclarationNode(
                                "number",
                                "a",
                                new LiteralNode("5", TokenType.NUMBER_LITERAL)
                            ),
                            new ExpressionStatementNode(
                                new FunctionCallNode(
                                    "print",
                                    new List<ExpressionNode>
                                    {
                                        new FunctionCallNode(
                                            "Factorial",
                                            new List<ExpressionNode>
                                            {
                                                new IdentifierNode("a")
                                            }
                                        )
                                    }
                                )
                            )
                        }
                    ),
                    null
                )
            );

            string expectedCode = @"
#include <iostream>
#include <string>
#include <vector>
#include <fstream>


double Factorial(double n)
{
double result = 1;
if (n > 1)
{
result = n * Factorial(n - 1);
}
return result;
}

int main()
{
double a = 5;
std::cout << Factorial(a) << std::endl;
return 0;
}
".Trim();

            // Act
            var generatedCode = _codeGenerator.GenerateCode(program);

            // Assert
            Assert.AreEqual(expectedCode, generatedCode.Trim());
        }

        [TestMethod]
        public void GenerateCode_GlobalVariable_ReturnsExpectedCode()
        {
            // Arrange
            var program = new ProgramNode(
                new List<VariableDeclarationNode>
                {
            new VariableDeclarationNode("number", "globalVar", new LiteralNode("42", TokenType.NUMBER_LITERAL))
                },
                new List<FunctionNode>(),
                new FunctionNode(
                    "_main_",
                    new List<ParameterNode>(),
                    "void",
                    new BlockNode(new List<StatementNode>()),
                    null
                )
            );

            string expectedCode = @"
#include <iostream>
#include <string>
#include <vector>
#include <fstream>


double globalVar = 42;
int main()
{
return 0;
}
".Trim();

            // Act
            var generatedCode = _codeGenerator.GenerateCode(program);

            // Assert
            Assert.AreEqual(expectedCode, generatedCode.Trim());
        }

        [TestMethod]
        public void GenerateCode_FunctionWithMultipleParameters_ReturnsExpectedCode()
        {
            // Arrange
            var program = new ProgramNode(
                new List<VariableDeclarationNode>(),
                new List<FunctionNode>
                {
            new FunctionNode(
                "Add",
                new List<ParameterNode>
                {
                    new ParameterNode("number", "a"),
                    new ParameterNode("number", "b")
                },
                "number",
                new BlockNode(new List<StatementNode>()),
                    new ReturnStatementNode(new BinaryExpressionNode(
                    new IdentifierNode("a"),
                    "+",
                    new IdentifierNode("b"))
                )
            )
                },
                new FunctionNode(
                    "_main_",
                    new List<ParameterNode>(),
                    "void",
                    new BlockNode(new List<StatementNode>()),
                    null
                )
            );

            string expectedCode = @"
#include <iostream>
#include <string>
#include <vector>
#include <fstream>


double Add(double a, double b)
{
return a + b;
}

int main()
{
return 0;
}
".Trim();

            // Act
            var generatedCode = _codeGenerator.GenerateCode(program);

            // Assert
            Assert.AreEqual(expectedCode, generatedCode.Trim());
        }

        [TestMethod]
        public void GenerateCode_IfElseStatement_ReturnsExpectedCode()
        {
            // Arrange
            var program = new ProgramNode(
                new List<VariableDeclarationNode>(), // No global variables
                new List<FunctionNode>
                {
                    new FunctionNode(
                        "CheckNumber",
                        new List<ParameterNode>
                        {
                            new ParameterNode("number", "n")
                        },
                        "string",
                        new BlockNode(new List<StatementNode>
                        {
                            new VariableDeclarationNode(
                                "string",
                                "result",
                                new LiteralNode("", TokenType.STRING_LITERAL)
                            ),
                            new IfStatementNode(
                                new BinaryExpressionNode(
                                    new IdentifierNode("n"),
                                    ">",
                                    new LiteralNode("0", TokenType.NUMBER_LITERAL)
                                ),
                                new BlockNode(new List<StatementNode>
                                {
                                    new AssignmentNode(
                                        "result",
                                        new LiteralNode("Positive", TokenType.STRING_LITERAL)
                                    )
                                }),
                                new BlockNode(new List<StatementNode>
                                {
                                    new AssignmentNode(
                                        "result",
                                        new LiteralNode("Non-Positive", TokenType.STRING_LITERAL)
                                    )
                                })
                            )
                        }),
                        new ReturnStatementNode(new IdentifierNode("result"))
                    )
                },
                new FunctionNode(
                    "_main_",
                    new List<ParameterNode>(),
                    "void",
                    new BlockNode(new List<StatementNode>
                    {
                        new ExpressionStatementNode(
                            new FunctionCallNode("print", new List<ExpressionNode>
                            {
                                new FunctionCallNode("CheckNumber", new List<ExpressionNode>
                                {
                                    new LiteralNode("5", TokenType.NUMBER_LITERAL)
                                })
                            })
                        )
                    }),
                    null // No return expression
                )
            );

            string expectedCode = @"
#include <iostream>
#include <string>
#include <vector>
#include <fstream>


std::string CheckNumber(double n)
{
std::string result = """";
if (n > 0)
{
result = ""Positive"";
}
else
{
result = ""Non-Positive"";
}
return result;
}

int main()
{
std::cout << CheckNumber(5) << std::endl;
return 0;
}
".Trim();

            // Act
            var generatedCode = _codeGenerator.GenerateCode(program);

            // Assert
            Assert.AreEqual(expectedCode, generatedCode.Trim());
        }

        [TestMethod]
        public void GenerateCode_WhileLoop_ReturnsExpectedCode()
        {
            // Arrange
            var program = new ProgramNode(
                new List<VariableDeclarationNode>(),
                new List<FunctionNode>
                {
            new FunctionNode(
                "PrintNumbers",
                new List<ParameterNode>
                {
                    new ParameterNode("number", "n")
                },
                "void",
                new BlockNode(
                    new List<StatementNode>
                    {
                        new VariableDeclarationNode(
                            "number",
                            "i",
                            new LiteralNode("0", TokenType.NUMBER_LITERAL)
                        ),
                        new WhileStatementNode(
                            new BinaryExpressionNode(
                                new IdentifierNode("i"),
                                "<",
                                new IdentifierNode("n")
                            ),
                            new BlockNode(
                                new List<StatementNode>
                                {
                                    new ExpressionStatementNode(
                                        new FunctionCallNode(
                                            "print",
                                            new List<ExpressionNode>
                                            {
                                                new IdentifierNode("i")
                                            }
                                        )
                                    ),
                                    new IncrementDecrementNode(
                                        "i",
                                        "++"
                                    )
                                }
                            )
                        )
                    }
                ),
                null
            )
                },
                new FunctionNode(
                    "_main_",
                    new List<ParameterNode>(),
                    "void",
                    new BlockNode(new List<StatementNode>()),
                    null
                )
            );

            string expectedCode = @"
#include <iostream>
#include <string>
#include <vector>
#include <fstream>


void PrintNumbers(double n)
{
double i = 0;
while (i < n)
{
std::cout << i << std::endl;
i++;
}
}

int main()
{
return 0;
}
".Trim();

            // Act
            var generatedCode = _codeGenerator.GenerateCode(program);

            // Assert
            Assert.AreEqual(expectedCode, generatedCode.Trim());
        }

    }
}
