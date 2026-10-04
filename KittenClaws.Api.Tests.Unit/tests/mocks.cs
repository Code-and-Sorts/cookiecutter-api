namespace KittenClaws.Api.Tests.Unit;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
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

    public static HttpContext CreateHttpContext(string method, string path, string? body = null, string query = "")
    {
        var context = new DefaultHttpContext();
        context.Request.Method = method;
        context.Request.Path = path;
        context.Request.QueryString = new QueryString(query);
        context.Request.ContentType = "application/json";

        if (body != null)
        {
            context.Request.Body = CreateStream(body);
        }

        context.Response.Body = new MemoryStream();

        return context;
    }

    public static HttpContext CreateHttpContext<T>(T requestBody, string method, string path)
    {
        return CreateHttpContext(method, path, body: Json.Serialize(requestBody!));
    }

    public static string ReadResponseBody(HttpContext context)
    {
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body);
        return reader.ReadToEnd();
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
