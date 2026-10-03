namespace {{project_class_name}}.Api.Utils;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

// Model code calls these through Fields, a name no field can take, so a property never hides them.
public static partial class Fields
{
    // The largest integer every JSON parser reads exactly (2^53 - 1).
    public const long MaxSafeInteger = 9007199254740991;

    private const string DateFormat = "yyyy-MM-dd";

    public static string Now() => Timestamps.Now();

    public static string Today() => DateTime.UtcNow.ToString(DateFormat, CultureInfo.InvariantCulture);

    public static string NewUuid() => ItemIds.New();

    public static bool IsSafeInteger(long? value) => value is null or (>= -MaxSafeInteger and <= MaxSafeInteger);

    public static bool IsDate(string? value) =>
        value == null || DateOnly.TryParseExact(value, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out _);

    public static bool IsDateTime(string? value) =>
        value == null || (DateTimePattern().IsMatch(value) && DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out _));

    public static bool IsUuid(string? value) => value == null || UuidPattern().IsMatch(value);

    // Validation runs first, so a value that does not parse is kept as sent.
    public static string? UtcDateTime(string? value) =>
        value != null && DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
            ? Timestamps.Format(parsed.UtcDateTime)
            : value;

    public static List<string>? UtcDateTimes(List<string>? values) => values?.Select(value => UtcDateTime(value)!).ToList();

    [GeneratedRegex(@"^[0-9]{4}-[0-9]{2}-[0-9]{2}T[0-9]{2}:[0-9]{2}:[0-9]{2}(\.[0-9]{1,9})?(Z|[+-][0-9]{2}:[0-9]{2})\z")]
    private static partial Regex DateTimePattern();

    [GeneratedRegex(@"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}\z")]
    private static partial Regex UuidPattern();
}
