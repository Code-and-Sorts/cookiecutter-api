namespace {{project_class_name}}.Api.Tests.Unit;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
{%- if cloud_service == 'Azure Function App' %}
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
using Microsoft.AspNetCore.Http;
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
using Amazon.Lambda.APIGatewayEvents;
{%- endif %}
{%- if cloud_service == 'Azure Function App' %}
using Microsoft.AspNetCore.Mvc;
{%- endif %}
using Microsoft.Extensions.Logging;
{%- if cloud_service == 'Azure Function App' %}
using NSubstitute;
using Xunit;
{%- endif %}
using {{project_class_name}}.Api.Utils;

public static class Mocks
{
    public static MemoryStream CreateStream(string body) => new(Encoding.UTF8.GetBytes(body));
{%- if cloud_service == 'Azure Function App' %}

    public static HttpRequestData CreateHttpRequestData<T>(T requestBody, string restMethod = "GET")
    {
        var context = Substitute.For<FunctionContext>();
        var body = Json.Serialize(requestBody!);

        var request = Substitute.For<HttpRequestData>(context);
        request.Body.Returns(CreateStream(body));
        request.Method.Returns(restMethod);
        request.Url.Returns(new Uri("http://localhost/api/endpoint"));

        return request;
    }

    public static HttpRequestData CreateHttpRequestData(string restMethod = "GET", string query = "")
    {
        var context = Substitute.For<FunctionContext>();

        var request = Substitute.For<HttpRequestData>(context);
        request.Body.Returns(new MemoryStream());
        request.Method.Returns(restMethod);
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
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}

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
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}

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
{%- endif %}
}

public class RecordingLogger<T> : ILogger<T>
{
    public List<(LogLevel Level, Exception? Exception)> Entries { get; } = [];

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
        Entries.Add((logLevel, exception));
}
