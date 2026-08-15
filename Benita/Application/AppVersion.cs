using System.Reflection;

namespace Benita;

/// <summary>نسخهٔ برنامه را از metadata اسمبلی به‌عنوان تنها منبع نسخه استخراج می‌کند.</summary>
internal static class AppVersion
{
    public static string Current { get; } = ReadVersion();

    private static string ReadVersion()
    {
        string? informationalVersion = typeof(AppVersion).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (!string.IsNullOrWhiteSpace(informationalVersion))
            return informationalVersion.Split('+', 2)[0];
        return typeof(AppVersion).Assembly.GetName().Version?.ToString(3) ?? "unknown";
    }
}
