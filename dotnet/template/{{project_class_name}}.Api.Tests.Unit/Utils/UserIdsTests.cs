namespace {{project_class_name}}.Api.Tests.Unit;
{% if cloud_service == 'AWS Lambda' %}
using System.Collections.Generic;
using Amazon.Lambda.APIGatewayEvents;
{%- elif cloud_service == 'Azure Function App' %}
using Microsoft.Azure.Functions.Worker.Http;
{%- else %}
using Microsoft.AspNetCore.Http;
{%- endif %}
using {{project_class_name}}.Api.Utils;
using Xunit;

public class UserIdsTests
{
{%- if cloud_service == 'AWS Lambda' %}
    private static APIGatewayProxyRequest RequestWith(string? name, string value) => new()
    {
        Headers = name == null ? null : new Dictionary<string, string> { { name, value } },
    };
{%- elif cloud_service == 'Azure Function App' %}
    private static HttpRequestData RequestWith(string? name, string value)
    {
        var request = Mocks.CreateHttpRequestData("POST");
        if (name != null)
        {
            request.Headers.Add(name, value);
        }
        return request;
    }
{%- else %}
    private static HttpRequest RequestWith(string? name, string value)
    {
        var request = new DefaultHttpContext().Request;
        if (name != null)
        {
            request.Headers[name] = value;
        }
        return request;
    }
{%- endif %}

    [Theory]
    [InlineData("X-User-Id", "User1", "User1")]
    [InlineData("x-user-id", "  User1  ", "User1")]
    [InlineData("X-User-Id", "   ", null)]
    [InlineData(null, "", null)]
    public void From_ReturnsTheTrimmedHeaderOrNull(string? name, string value, string? expected)
    {
        Assert.Equal(expected, UserIds.From(RequestWith(name, value)));
    }

    [Fact]
    public void From_ThrowsBadRequest_WhenTooLong()
    {
        UserIds.From(RequestWith("X-User-Id", new string('a', UserIds.MaxLength)));

        var exception = Assert.Throws<BadRequestException>(() => UserIds.From(RequestWith("X-User-Id", new string('a', UserIds.MaxLength + 1))));

        Assert.Equal("X-User-Id must be at most 256 characters.", exception.Message);
    }
}
