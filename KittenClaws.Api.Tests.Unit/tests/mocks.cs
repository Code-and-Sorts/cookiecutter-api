
namespace KittenClaws.Api.Tests.Unit;

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
