{%- if cloud_service == 'Azure Function App' %}
namespace {{project_class_name}}.Api.Tests.Unit;

using System;
using System.IO;
using System.Text;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;
using NSubstitute;

public static class Mocks
{
    public static HttpRequestData CreateHttpRequestData<T>(T requestBody, string restMethod = "GET")
    {
        var context = Substitute.For<FunctionContext>();
        var body = JsonConvert.SerializeObject(requestBody);
        var bodyStream = new MemoryStream(Encoding.UTF8.GetBytes(body));

        var request = Substitute.For<HttpRequestData>(context);
        request.Body.Returns(bodyStream);
        request.Method.Returns(restMethod);
        request.Url.Returns(new Uri("http://localhost/api/endpoint"));

        return request;
    }

    public static HttpRequestData CreateHttpRequestData(string restMethod = "GET")
    {
        var context = Substitute.For<FunctionContext>();

        var request = Substitute.For<HttpRequestData>(context);
        request.Body.Returns(new MemoryStream());
        request.Method.Returns(restMethod);
        request.Url.Returns(new Uri("http://localhost/api/endpoint"));

        return request;
    }
}
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
namespace {{project_class_name}}.Api.Tests.Unit;

using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

public static class Mocks
{
    public static HttpContext CreateHttpContext(string method, string path, string? body = null)
    {
        var context = new DefaultHttpContext();
        context.Request.Method = method;
        context.Request.Path = path;
        context.Request.ContentType = "application/json";

        if (body != null)
        {
            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
        }

        context.Response.Body = new MemoryStream();

        return context;
    }

    public static HttpContext CreateHttpContext<T>(T requestBody, string method, string path)
    {
        return CreateHttpContext(method, path, JsonConvert.SerializeObject(requestBody));
    }

    public static string ReadResponseBody(HttpContext context)
    {
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body);
        return reader.ReadToEnd();
    }
}
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
namespace {{project_class_name}}.Api.Tests.Unit;

using System.Collections.Generic;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json;

public static class Mocks
{
    public static APIGatewayProxyRequest CreateApiGatewayRequest<T>(T requestBody, string httpMethod = "GET", Dictionary<string, string>? pathParameters = null)
    {
        return new APIGatewayProxyRequest
        {
            HttpMethod = httpMethod,
            Body = JsonConvert.SerializeObject(requestBody),
            PathParameters = pathParameters ?? new Dictionary<string, string>()
        };
    }

    public static APIGatewayProxyRequest CreateApiGatewayRequest(string httpMethod = "GET", Dictionary<string, string>? pathParameters = null)
    {
        return new APIGatewayProxyRequest
        {
            HttpMethod = httpMethod,
            PathParameters = pathParameters ?? new Dictionary<string, string>()
        };
    }
}
{%- endif %}
