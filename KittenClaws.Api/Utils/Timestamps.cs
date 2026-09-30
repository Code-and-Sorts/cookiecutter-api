namespace KittenClaws.Api.Utils;

using System;
using System.Globalization;

public static class Timestamps
{
    public static string Now() => Format(DateTime.UtcNow);

    public static string Format(DateTime value) =>
        value.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture);
}
