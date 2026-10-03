namespace KittenClaws.Api.Tests.Unit;

using System.Threading.Tasks;
using Xunit;
using KittenClaws.Api.Handlers;

public class HealthHandlerTests
{
    private readonly HealthHandler _handler = new();

    [Fact]
    public void Endpoint_IsHealthEndpoint()
    {
        Assert.Equal("health", _handler.Endpoint);
    }

    [Fact]
    public async Task HandleAsync_Get_ReturnsOk()
    {
        var httpContext = Mocks.CreateHttpContext("GET", "/health");

        await _handler.HandleAsync(httpContext, null);

        Assert.Equal(200, httpContext.Response.StatusCode);
        Assert.Equal("{\"status\":\"ok\"}", Mocks.ReadResponseBody(httpContext));
    }

    [Fact]
    public async Task HandleAsync_OtherMethod_ReturnsMethodNotAllowed()
    {
        var httpContext = Mocks.CreateHttpContext("POST", "/health");

        await _handler.HandleAsync(httpContext, null);

        Assert.Equal(405, httpContext.Response.StatusCode);
        Assert.Equal("{\"errorMessage\":\"Method not allowed.\"}", Mocks.ReadResponseBody(httpContext));
    }

    [Fact]
    public async Task HandleAsync_WithId_ReturnsNotFound()
    {
        var httpContext = Mocks.CreateHttpContext("GET", "/health/extra");

        await _handler.HandleAsync(httpContext, "extra");

        Assert.Equal(404, httpContext.Response.StatusCode);
        Assert.Equal("{\"errorMessage\":\"Not found.\"}", Mocks.ReadResponseBody(httpContext));
    }
}
