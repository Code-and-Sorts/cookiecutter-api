
namespace KittenClaws.Api.Tests.Unit;

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
