using System.Text;

namespace Benita
{
    internal interface IInterpreterClass
    {
        object HandleFunctionCall(string? functionName, List<object> arguments);
    }

    internal interface ICodeGeneratorClass
    {
        void HandleFunctionCall(string? functionName, ref StringBuilder code, ref StringBuilder defaultFunction,
            ref StringBuilder codeHeader, ref StringBuilder codeInclude);
    }
}