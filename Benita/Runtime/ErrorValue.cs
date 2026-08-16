namespace Benita;

/// <summary>مقدار استاندارد و قابل‌پرتاب خطای زبان را همراه کد و پیام نگهداری می‌کند.</summary>
public sealed record ErrorValue(string Code, string Message)
{
    public override string ToString() => $"{Code}: {Message}";
}

/// <summary>مقدار error پرتاب‌شده را تا نزدیک‌ترین catch در مفسر حمل می‌کند.</summary>
internal sealed class ThrownErrorException(ErrorValue error) : Exception(error.Message)
{
    public ErrorValue Error { get; } = error;
}
