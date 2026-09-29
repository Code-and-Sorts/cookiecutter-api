namespace {{project_class_name}}.Api.Utils;

using System;
using System.Globalization;
using System.Linq;

/// <summary>Parses the <c>limit</c> query parameter of list endpoints.</summary>
public static class Pagination
{
    // Default cap on list reads to avoid unbounded queries.
    public const int DefaultListLimit = 100;

    // Largest page a client may request with ?limit=.
    public const int MaxListLimit = 1000;

    /// <summary>
    /// Returns the requested page size. A missing, non-numeric or non-positive value
    /// falls back to <see cref="DefaultListLimit"/>; larger values are capped at
    /// <see cref="MaxListLimit"/>. An invalid limit is never an error.
    /// </summary>
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
