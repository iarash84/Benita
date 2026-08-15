namespace Benita;

/// <summary>قرارداد پایه برای مؤلفه‌هایی که یک گره AST را بازدید و نتیجه تولید می‌کنند.</summary>
/// <typeparam name="TResult">نوع نتیجهٔ بازدید.</typeparam>
public interface IAstVisitor<out TResult>
{
    TResult Visit(AstNode? node);
}
