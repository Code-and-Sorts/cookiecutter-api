namespace KittenClaws.Api.Tests.Unit;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using KittenClaws.Api.Dtos;
using KittenClaws.Api.Utils;

public static class Mocks
{
    public static MemoryStream CreateStream(string body) => new(Encoding.UTF8.GetBytes(body));

    public static TheoryData<string> ServerOwnedAndUnknownFields => ["id", "isDeleted", "createdTimestamp", "updatedTimestamp", "createdBy", "updatedBy", "unknown"];

    public static T WithAuditFields<T>(T dto, string? userId = "User1") where T : BaseDto
    {
        dto.CreatedTimestamp = "2026-01-01T00:00:00.000Z";
        dto.CreatedBy = userId;
        dto.UpdatedTimestamp = "2026-01-02T00:00:00.000Z";
        dto.UpdatedBy = userId;
        return dto;
    }

    public static string AuditJson(string? userId = "User1") => userId == null
        ? "\"createdTimestamp\":\"2026-01-01T00:00:00.000Z\",\"updatedTimestamp\":\"2026-01-02T00:00:00.000Z\""
        : $"\"createdTimestamp\":\"2026-01-01T00:00:00.000Z\",\"createdBy\":\"{userId}\",\"updatedTimestamp\":\"2026-01-02T00:00:00.000Z\",\"updatedBy\":\"{userId}\"";

    public static MemoryStream CreateBodyWithField(string field) => CreateStream("{\"name\":\"mockName\",\"" + field + "\":\"value\"}");

    public static HttpRequestData CreateHttpRequestData<T>(T requestBody, string restMethod = "GET")
    {
        var context = Substitute.For<FunctionContext>();
        var body = Json.Serialize(requestBody!);

        var request = Substitute.For<HttpRequestData>(context);
        request.Body.Returns(CreateStream(body));
        request.Method.Returns(restMethod);
        request.Headers.Returns(new HttpHeadersCollection());
        request.Url.Returns(new Uri("http://localhost/api/endpoint"));

        return request;
    }

    public static HttpRequestData CreateHttpRequestData(string restMethod = "GET", string query = "")
    {
        var context = Substitute.For<FunctionContext>();

        var request = Substitute.For<HttpRequestData>(context);
        request.Body.Returns(new MemoryStream());
        request.Method.Returns(restMethod);
        request.Headers.Returns(new HttpHeadersCollection());
        request.Url.Returns(new Uri("http://localhost/api/endpoint" + query));

        return request;
    }

    public static (int StatusCode, string Body) ReadJsonResult(IActionResult result)
    {
        var content = Assert.IsType<ContentResult>(result);
        Assert.Equal("application/json", content.ContentType);
        Assert.NotNull(content.StatusCode);
        Assert.NotNull(content.Content);
        return (content.StatusCode.Value, content.Content);
    }
}

public class RecordingLogger<T> : ILogger<T>
{
    public List<(LogLevel Level, Exception? Exception)> Entries { get; } = [];

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
        Entries.Add((logLevel, exception));
}
