namespace {{cookiecutter.project_class_name}}.Api.Tests.Unit;

using System;
using System.IO;
using System.Text;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;
using NSubstitute;

public static class Mocks
{
    public static HttpRequestData CreateHttpRequestData<T>(T {{cookiecutter.project_lower_camel_name}}Request, string restMethod = "GET")
    {
        var context = Substitute.For<FunctionContext>();
        var body = JsonConvert.SerializeObject({{cookiecutter.project_lower_camel_name}}Request);
        var bodyStream = new MemoryStream(Encoding.UTF8.GetBytes(body));

        var request = Substitute.For<HttpRequestData>(context);
        request.Body.Returns(bodyStream);
        request.Method.Returns(restMethod);
        request.Url.Returns(new Uri("http://localhost/api/endpoint"));

        return request;
    }
}