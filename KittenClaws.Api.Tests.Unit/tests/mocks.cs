namespace KittenClaws.Api.Tests.Unit;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Amazon.Lambda.APIGatewayEvents;
using Microsoft.Extensions.Logging;
using KittenClaws.Api.Utils;

public static class Mocks
{
    public static MemoryStream CreateStream(string body) => new(Encoding.UTF8.GetBytes(body));

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
