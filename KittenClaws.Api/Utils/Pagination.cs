namespace KittenClaws.Api.Utils;

using System;
using System.Globalization;
using System.Linq;

public static class Pagination
{
    public const int DefaultListLimit = 100;

    public const int MaxListLimit = 1000;

    public static int ParseLimit(string? raw)
    {
        if (string.IsNullOrEmpty(raw) || !raw.All(char.IsAsciiDigit))
        {
            return DefaultListLimit;
        }
        if (!int.TryParse(raw, NumberStyles.None, CultureInfo.InvariantCulture, out var limit))
        {
            // All digits but too large for an int.
            return MaxListLimit;
        }
        return limit < 1 ? DefaultListLimit : Math.Min(limit, MaxListLimit);
    }
}
