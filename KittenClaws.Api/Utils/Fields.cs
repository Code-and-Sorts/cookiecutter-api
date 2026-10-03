namespace KittenClaws.Api.Utils;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

// Model code calls these through Fields, a name the template refuses for fields, so a property never hides them.
public static partial class Fields
{
    // The largest integer every JSON parser reads exactly.
    public const long MaxSafeInteger = 9007199254740991;

    private const string DateFormat = "yyyy-MM-dd";

    // Patterns name no character beyond U+FFFE, so this one stands for any character .NET would see as two.
    private const char WideCharacter = '￿';

    public static string Now() => Timestamps.Now();

    public static string Today() => DateTime.UtcNow.ToString(DateFormat, CultureInfo.InvariantCulture);

    public static string NewUuid() => ItemIds.New();

    public static bool IsSafeInteger(long? value) => value is null or (>= -MaxSafeInteger and <= MaxSafeInteger);

    // Lengths count code points, as every other language does, not UTF-16 units.
    public static int Length(string value) => value.EnumerateRunes().Count();

    // Matches as other languages do, one character per code point: each character beyond the BMP becomes one.
    // NonBacktracking runs in linear time, so a pattern cannot hang a request.
    public static bool Matches(string value, string pattern) =>
        Regex.IsMatch(string.Concat(value.EnumerateRunes().Select(rune => rune.IsBmp ? rune.ToString() : WideCharacter.ToString())), pattern, RegexOptions.NonBacktracking);

    public static bool IsDate(string? value) =>
        value == null || (DatePattern().IsMatch(value) && DateOnly.TryParseExact(value, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out _));

    public static bool IsDateTime(string? value) => value == null || ToUtc(value) != null;

    public static bool IsUuid(string? value) => value == null || UuidPattern().IsMatch(value);

    // Validation runs first, so a value that does not parse is kept as sent.
    public static string? UtcDateTime(string? value) => value != null && ToUtc(value) is { } utc ? Timestamps.Format(utc) : value;

    public static List<string>? UtcDateTimes(List<string>? values) => values?.Select(value => UtcDateTime(value)!).ToList();

    // RFC 3339 allows offsets up to ±23:59, beyond DateTimeOffset's ±14:00, so the offset is applied by hand.
    private static DateTime? ToUtc(string value)
    {
        if (!DateTimePattern().IsMatch(value) || !DateTime.TryParseExact(value[..19], "yyyy-MM-dd'T'HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var local))
        {
            return null;
        }
        var offset = value.EndsWith('Z') ? "Z" : value[^6..];
        var fraction = value[19..^offset.Length].TrimStart('.');
        var ticks = local.Ticks + (fraction.Length > 0 ? long.Parse(fraction.PadRight(7, '0')[..7], CultureInfo.InvariantCulture) : 0);
        if (offset != "Z")
        {
            var minutes = int.Parse(offset[1..3], CultureInfo.InvariantCulture) * 60 + int.Parse(offset[4..], CultureInfo.InvariantCulture);
            ticks -= (offset[0] == '-' ? -minutes : minutes) * TimeSpan.TicksPerMinute;
        }
        return ticks >= DateTime.MinValue.Ticks && ticks <= DateTime.MaxValue.Ticks ? new DateTime(ticks, DateTimeKind.Utc) : null;
    }

    [GeneratedRegex(@"^(000[1-9]|00[1-9][0-9]|0[1-9][0-9]{2}|[1-9][0-9]{3})-(0[1-9]|1[0-2])-(0[1-9]|[12][0-9]|3[01])\z")]
    private static partial Regex DatePattern();

    [GeneratedRegex(@"^(000[1-9]|00[1-9][0-9]|0[1-9][0-9]{2}|[1-9][0-9]{3})-(0[1-9]|1[0-2])-(0[1-9]|[12][0-9]|3[01])T([01][0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9](\.[0-9]{1,9})?(Z|[+-]([01][0-9]|2[0-3]):[0-5][0-9])\z")]
    private static partial Regex DateTimePattern();

    [GeneratedRegex(@"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}\z")]
    private static partial Regex UuidPattern();
}
