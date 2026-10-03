namespace {{project_class_name}}.Api.Tests.Unit;

using System;
using System.Globalization;
using {{project_class_name}}.Api.Utils;
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
    [InlineData(9007199254740991, true)]
    [InlineData(-9007199254740991, true)]
    [InlineData(9007199254740992, false)]
    [InlineData(-9007199254740992, false)]
    public void IsSafeInteger_AcceptsOnlyIntegersEveryJsonParserReadsExactly(long value, bool expected)
    {
        Assert.Equal(expected, Fields.IsSafeInteger(value));
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData("2024-02-29", true)]
    [InlineData("2026-02-29", false)]
    [InlineData("2026-1-31", false)]
    [InlineData("2026-01-31T00:00:00Z", false)]
    [InlineData("2026-01-31\n", false)]
    public void IsDate_AcceptsOnlyCalendarDates(string? value, bool expected)
    {
        Assert.Equal(expected, Fields.IsDate(value));
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData("2026-01-31T09:30:00Z", true)]
    [InlineData("2026-01-31T09:30:00.123456789+02:00", true)]
    [InlineData("2026-01-31T09:30:00", false)]
    [InlineData("2026-01-31 09:30:00Z", false)]
    [InlineData("2026-02-30T09:30:00Z", false)]
    [InlineData("2026-01-31T24:00:00Z", false)]
    [InlineData("2026-01-31T09:30:00Z\n", false)]
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
    [InlineData("2026-01-31T11:30:00+02:00", "2026-01-31T09:30:00.000Z")]
    [InlineData("2026-01-31T09:30:00.1239Z", "2026-01-31T09:30:00.123Z")]
    [InlineData("not a date-time", "not a date-time")]
    [InlineData(null, null)]
    public void UtcDateTime_KeepsTheInstantInUtcWithMilliseconds(string? value, string? expected)
    {
        Assert.Equal(expected, Fields.UtcDateTime(value));
    }

    [Fact]
    public void UtcDateTimes_ConvertsEveryItem()
    {
        Assert.Equal(new[] { "2026-01-31T09:30:00.000Z" }, Fields.UtcDateTimes(["2026-01-31T11:30:00+02:00"]));
        Assert.Null(Fields.UtcDateTimes(null));
    }
}
