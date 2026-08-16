namespace Benita;

/// <summary>عملیات در حال اجرا را تا زمان استفاده توسط await نگهداری می‌کند.</summary>
internal sealed class TaskValue(Task<object> task)
{
    public Task<object> Task { get; } = task;
}
