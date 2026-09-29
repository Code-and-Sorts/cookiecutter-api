
namespace KittenClaws.Api.Tests.Unit;

using System;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using NSubstitute;

public static class Mocks
{
    public static HttpContext CreateHttpContext(string method = "GET", string path = "/kitties", string? body = null)
    {
        var context = new DefaultHttpContext();
        context.Request.Method = method;
        context.Request.Path = path;
        context.Request.ContentType = "application/json";

        if (body != null)
        {
            var bodyStream = new MemoryStream(Encoding.UTF8.GetBytes(body));
            context.Request.Body = bodyStream;
        }

        context.Response.Body = new MemoryStream();

        return context;
    }

    public static HttpContext CreateHttpContext<T>(T requestBody, string method = "GET", string path = "/kitties")
    {
        var body = JsonConvert.SerializeObject(requestBody);
        return CreateHttpContext(method, path, body);
    }
}
