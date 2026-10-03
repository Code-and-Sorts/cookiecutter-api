{%- from 'dotnet/_model.jinja' import field_regex -%}
{%- from 'shared/_fields.jinja' import DATE_TIME_CASES, INVALID_DATES, INVALID_DATE_TIMES, WIDE_CHARACTER -%}
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
        Assert.Equal(3, Fields.Length("a{{ WIDE_CHARACTER }}é"));
    }

    [Theory]
    [InlineData("{{ WIDE_CHARACTER }}", {{ field_regex("^.$") }}, true)]
    [InlineData("a{{ WIDE_CHARACTER }}", {{ field_regex("^[^b]{2}$") }}, true)]
    [InlineData("{{ WIDE_CHARACTER }}", {{ field_regex("^..$") }}, false)]
    [InlineData("a\n", {{ field_regex("^a$") }}, false)]
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
{%- for value in INVALID_DATES %}
    [InlineData("{{ value }}", false)]
{%- endfor %}
    public void IsDate_AcceptsOnlyCalendarDates(string? value, bool expected)
    {
        Assert.Equal(expected, Fields.IsDate(value));
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData("2026-01-31 09:30:00Z", false)]
    [InlineData("2026-02-30T09:30:00Z", false)]
    [InlineData("2026-01-31T09:30:00Z\n", false)]
{%- for case in DATE_TIME_CASES %}
    [InlineData("{{ case.sent }}", true)]
{%- endfor %}
{%- for value in INVALID_DATE_TIMES %}
    [InlineData("{{ value }}", false)]
{%- endfor %}
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
{%- for case in DATE_TIME_CASES %}
    [InlineData("{{ case.sent }}", "{{ case.stored }}")]
{%- endfor %}
    [InlineData("not a date-time", "not a date-time")]
    [InlineData(null, null)]
    public void UtcDateTime_KeepsTheInstantInUtcWithMilliseconds(string? value, string? expected)
    {
        Assert.Equal(expected, Fields.UtcDateTime(value));
    }

    [Fact]
    public void UtcDateTimes_ConvertsEveryItem()
    {
        Assert.Equal(new[] { "{{ DATE_TIME_CASES[0].stored }}" }, Fields.UtcDateTimes(["{{ DATE_TIME_CASES[0].sent }}"]));
        Assert.Null(Fields.UtcDateTimes(null));
    }
}
