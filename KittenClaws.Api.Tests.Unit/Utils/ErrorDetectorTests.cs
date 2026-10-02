namespace KittenClaws.Api.Tests.Unit;

using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using KittenClaws.Api.Utils;
using Microsoft.Extensions.Logging;
using Xunit;

public class ErrorDetectorTest
{
    private readonly RecordingLogger<ErrorDetectorTest> _logger = new();

    [Fact]
    public void Classify_WithUnexpectedException_Returns500WithGenericMessageAndLogsError()
    {
        var exception = new InvalidOperationException("Secret SDK diagnostics");

        var (statusCode, body) = ErrorDetector.Classify(exception, _logger);

        Assert.Equal(500, statusCode);
        Assert.Equal("An unexpected error occurred.", body.ErrorMessage);
        var entry = Assert.Single(_logger.Entries);
        Assert.Equal(LogLevel.Error, entry.Level);
        Assert.Same(exception, entry.Exception);
    }

    [Fact]
    public void Classify_WithNotFoundException_Returns404AndDoesNotLog()
    {
        var (statusCode, body) = ErrorDetector.Classify(new NotFoundException("Item", "abc"), _logger);

        Assert.Equal(404, statusCode);
        Assert.Equal("Item with id abc was not found.", body.ErrorMessage);
        Assert.Empty(_logger.Entries);
    }

    [Fact]
    public void Classify_WithBadRequestException_Returns400AndDoesNotLog()
    {
        var (statusCode, body) = ErrorDetector.Classify(new BadRequestException("Request body must be valid JSON."), _logger);

        Assert.Equal(400, statusCode);
        Assert.Equal("Request body must be valid JSON.", body.ErrorMessage);
        Assert.Empty(_logger.Entries);
    }

    [Fact]
    public void Classify_WithCancellationFromTheClient_DoesNotLogAnError()
    {
        using var aborted = new CancellationTokenSource();
        aborted.Cancel();

        var (statusCode, _) = ErrorDetector.Classify(new OperationCanceledException(aborted.Token), _logger, aborted.Token);

        Assert.Equal(500, statusCode);
        Assert.DoesNotContain(_logger.Entries, entry => entry.Level >= LogLevel.Warning);
    }

    [Fact]
    public void Classify_WithPassedRequestDeadline_Returns500AndLogsError()
    {
        var (statusCode, body) = ErrorDetector.Classify(new TaskCanceledException("deadline"), _logger, CancellationToken.None);

        Assert.Equal(500, statusCode);
        Assert.Equal("An unexpected error occurred.", body.ErrorMessage);
        Assert.Equal(LogLevel.Error, Assert.Single(_logger.Entries).Level);
    }

    [Fact]
    public void RequestDeadline_CancelsWithinTenSeconds()
    {
        using var deadline = RequestDeadline.Start(CancellationToken.None);

        Assert.True(RequestDeadline.Timeout <= TimeSpan.FromSeconds(10));
        Assert.False(deadline.Token.IsCancellationRequested);
    }

    [Fact]
    public void BaseError_SerializesAsCamelCaseErrorMessage()
    {
        var json = JsonSerializer.Serialize(new BaseError { ErrorMessage = "Not found." });

        Assert.Equal("{\"errorMessage\":\"Not found.\"}", json);
    }

    [Fact]
    public void ItemIds_EnsureValid_ThrowsNotFoundForNonUuid()
    {
        var exception = Assert.Throws<NotFoundException>(() => ItemIds.EnsureValid("Item", "not-a-uuid"));

        Assert.Equal(404, exception.StatusCode);
        Assert.Equal("Item with id not-a-uuid was not found.", exception.Message);
        ItemIds.EnsureValid("Item", "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c");
    }

    [Fact]
    public void ItemIds_New_ReturnsAValidUuid()
    {
        ItemIds.EnsureValid("Item", ItemIds.New());
    }

    [Fact]
    public void DeleteOkObjectResult_For_SerializesTheContractMessage()
    {
        Assert.Equal("{\"message\":\"Item with id abc was deleted successfully.\"}", Json.Serialize(DeleteOkObjectResult.For("Item", "abc")));
    }
}

public class PaginationTests
{
    [Theory]
    [InlineData(null, 100)]
    [InlineData("", 100)]
    [InlineData("abc", 100)]
    [InlineData("-5", 100)]
    [InlineData("0", 100)]
    [InlineData("1.5", 100)]
    [InlineData("1", 1)]
    [InlineData("250", 250)]
    [InlineData("1000", 1000)]
    [InlineData("5000", 1000)]
    [InlineData("99999999999999999999", 1000)]
    public void ParseLimit_ReturnsDefaultOrClampedLimit(string? raw, int expected)
    {
        Assert.Equal(expected, Pagination.ParseLimit(raw));
    }
}

public class TimestampsTests
{
    [Fact]
    public void Format_ReturnsIsoUtcWithMillisecondsAndZ()
    {
        var value = new DateTime(2026, 9, 29, 22, 49, 26, 625, DateTimeKind.Utc).AddTicks(1234);

        Assert.Equal("2026-09-29T22:49:26.625Z", Timestamps.Format(value));
        Assert.Matches(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3}Z$", Timestamps.Now());
    }
}
