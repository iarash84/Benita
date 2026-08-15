namespace Benita
{
    internal interface IInterpreterClass
    {
        object HandleFunctionCall(string? functionName, List<object> arguments);
    }
}
