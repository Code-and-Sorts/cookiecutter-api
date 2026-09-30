namespace KittenClaws.Api.Tests.Unit;

using System.Collections.Generic;
using Amazon.Lambda.APIGatewayEvents;
using KittenClaws.Api.Utils;
using Xunit;

public class UserIdsTests
{
    private static APIGatewayProxyRequest RequestWith(string? name, string value) => new()
    {
        Headers = name == null ? null : new Dictionary<string, string> { { name, value } },
    };

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
