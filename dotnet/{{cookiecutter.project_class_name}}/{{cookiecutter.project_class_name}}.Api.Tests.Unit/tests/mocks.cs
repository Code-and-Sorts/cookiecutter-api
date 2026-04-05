namespace {{cookiecutter.project_class_name}}.Api.Tests.Unit;

using System;
using System.IO;
using System.Text;
{%- if cookiecutter.cloud_service == 'Azure Function App' %}
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
{%- endif %}
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
using Microsoft.AspNetCore.Http;
{%- endif %}
using Newtonsoft.Json;
using NSubstitute;

public static class Mocks
{
{%- if cookiecutter.cloud_service == 'Azure Function App' %}
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
{%- endif %}
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
    public static HttpContext CreateHttpContext(string method = "GET", string path = "/{{cookiecutter.project_endpoint}}", string? body = null)
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

    public static HttpContext CreateHttpContext<T>(T requestBody, string method = "GET", string path = "/{{cookiecutter.project_endpoint}}")
    {
        var body = JsonConvert.SerializeObject(requestBody);
        return CreateHttpContext(method, path, body);
    }
{%- endif %}
}
