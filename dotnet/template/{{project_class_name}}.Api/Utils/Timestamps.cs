namespace {{project_class_name}}.Api.Utils;

using System;
using System.Globalization;

public static class Timestamps
{
    /// <summary>The current time as ISO-8601 UTC with millisecond precision, e.g. 2026-09-29T22:49:26.625Z.</summary>
    public static string Now() => Format(DateTime.UtcNow);

    public static string Format(DateTime value) =>
        value.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture);
}
