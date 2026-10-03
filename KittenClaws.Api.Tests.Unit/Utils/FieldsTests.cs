namespace KittenClaws.Api.Tests.Unit;

using System;
using System.Globalization;
using KittenClaws.Api.Utils;
using Xunit;

public class FieldsTests
{
    [Fact]
    public void Now_IsAUtcTimestampWithMilliseconds()
    {
        Assert.Matches(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3}Z$", Fields.Now());
    }

    [Fact]
    public void Today_IsTheUtcDate()
    {
        Assert.Equal(DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), Fields.Today());
    }

    [Fact]
    public void NewUuid_IsANewUuidEachTime()
    {
        Assert.True(Fields.IsUuid(Fields.NewUuid()));
        Assert.NotEqual(Fields.NewUuid(), Fields.NewUuid());
    }

    [Theory]
    [InlineData(Fields.MaxSafeInteger, true)]
    [InlineData(-Fields.MaxSafeInteger, true)]
    [InlineData(Fields.MaxSafeInteger + 1, false)]
    [InlineData(-Fields.MaxSafeInteger - 1, false)]
    public void IsSafeInteger_AcceptsOnlyIntegersEveryJsonParserReadsExactly(long value, bool expected)
    {
        Assert.Equal(expected, Fields.IsSafeInteger(value));
    }

    [Fact]
    public void Length_CountsCodePoints()
    {
        Assert.Equal(3, Fields.Length("a😺é"));
    }

    [Theory]
    [InlineData("😺", @"^[^\n]\z", true)]
    [InlineData("a😺", @"^[^b]{2}\z", true)]
    [InlineData("😺", @"^[^\n][^\n]\z", false)]
    [InlineData("a\n", @"^a\z", false)]
    public void Matches_ReadsEachCodePointAsOneCharacter(string value, string pattern, bool expected)
    {
        Assert.Equal(expected, Fields.Matches(value, pattern));
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData("2024-02-29", true)]
    [InlineData("0001-01-01", true)]
    [InlineData("2026-1-31", false)]
    [InlineData("2026-01-31T00:00:00Z", false)]
    [InlineData("2026-01-31\n", false)]
    [InlineData("2026-02-30", false)]
    [InlineData("2026-13-01", false)]
    [InlineData("0000-01-01", false)]
    public void IsDate_AcceptsOnlyCalendarDates(string? value, bool expected)
    {
        Assert.Equal(expected, Fields.IsDate(value));
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData("2026-01-31 09:30:00Z", false)]
    [InlineData("2026-02-30T09:30:00Z", false)]
    [InlineData("2026-01-31T09:30:00Z\n", false)]
    [InlineData("2026-01-31T11:30:00.1239+02:00", true)]
    [InlineData("2026-01-31T00:30:00.123456789+23:59", true)]
    [InlineData("2026-01-31T23:30:00-23:59", true)]
    [InlineData("0001-01-01T00:00:00-00:00", true)]
    [InlineData("9999-12-31T23:59:59.999Z", true)]
    [InlineData("2026-01-31T09:30:00", false)]
    [InlineData("2026-01-31T24:00:00Z", false)]
    [InlineData("2026-01-31T23:59:60Z", false)]
    [InlineData("2026-01-31T09:30:00+14:60", false)]
    [InlineData("0000-12-31T23:00:00-01:00", false)]
    [InlineData("0001-01-01T00:00:00+01:00", false)]
    [InlineData("9999-12-31T23:59:59-01:00", false)]
    public void IsDateTime_AcceptsOnlyDateTimesWithAnOffset(string? value, bool expected)
    {
        Assert.Equal(expected, Fields.IsDateTime(value));
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData("0F3A7FF7-a601-4d23-b33c-7f8f18b57a4c", true)]
    [InlineData("0f3a7ff7a6014d23b33c7f8f18b57a4c", false)]
    [InlineData("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c\n", false)]
    public void IsUuid_AcceptsOnlyHyphenatedUuids(string? value, bool expected)
    {
        Assert.Equal(expected, Fields.IsUuid(value));
    }

    [Theory]
    [InlineData("2026-01-31T11:30:00.1239+02:00", "2026-01-31T09:30:00.123Z")]
    [InlineData("2026-01-31T00:30:00.123456789+23:59", "2026-01-30T00:31:00.123Z")]
    [InlineData("2026-01-31T23:30:00-23:59", "2026-02-01T23:29:00.000Z")]
    [InlineData("0001-01-01T00:00:00-00:00", "0001-01-01T00:00:00.000Z")]
    [InlineData("9999-12-31T23:59:59.999Z", "9999-12-31T23:59:59.999Z")]
    [InlineData("not a date-time", "not a date-time")]
    [InlineData(null, null)]
    public void UtcDateTime_KeepsTheInstantInUtcWithMilliseconds(string? value, string? expected)
    {
        Assert.Equal(expected, Fields.UtcDateTime(value));
    }

    [Fact]
    public void UtcDateTimes_ConvertsEveryItem()
    {
        Assert.Equal(new[] { "2026-01-31T09:30:00.123Z" }, Fields.UtcDateTimes(["2026-01-31T11:30:00.1239+02:00"]));
        Assert.Null(Fields.UtcDateTimes(null));
    }
}
