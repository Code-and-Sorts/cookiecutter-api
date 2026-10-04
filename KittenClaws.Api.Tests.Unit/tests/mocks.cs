namespace KittenClaws.Api.Tests.Unit;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Amazon.Lambda.APIGatewayEvents;
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

    public static APIGatewayProxyRequest CreateApiGatewayRequest<T>(T requestBody, string httpMethod = "GET", Dictionary<string, string>? pathParameters = null)
    {
        return new APIGatewayProxyRequest
        {
            HttpMethod = httpMethod,
            Body = Json.Serialize(requestBody!),
            PathParameters = pathParameters ?? new Dictionary<string, string>()
        };
    }

    public static APIGatewayProxyRequest CreateApiGatewayRequest(string httpMethod = "GET", Dictionary<string, string>? pathParameters = null, Dictionary<string, string>? queryStringParameters = null)
    {
        return new APIGatewayProxyRequest
        {
            HttpMethod = httpMethod,
            PathParameters = pathParameters ?? new Dictionary<string, string>(),
            QueryStringParameters = queryStringParameters
        };
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
