namespace KittenClaws.Api.Tests.Unit;

using Xunit;
using KittenClaws.Api.Functions;

public class HealthFunctionsTests
{
    private readonly HealthFunctions _functions = new();

    [Fact]
    public void Health_ReturnsOk()
    {
        var request = Mocks.CreateApiGatewayRequest();
        request.Resource = "/health";

        var response = _functions.Health(request);

        Assert.Equal(200, response.StatusCode);
        Assert.Equal("{\"status\":\"ok\"}", response.Body);
        Assert.Equal("application/json", response.Headers["Content-Type"]);
    }

    [Fact]
    public void Health_ReturnsMethodNotAllowed_ForAnotherMethod()
    {
        var request = Mocks.CreateApiGatewayRequest("POST");
        request.Path = "/health";

        var response = _functions.Health(request);

        Assert.Equal(405, response.StatusCode);
        Assert.Equal("{\"errorMessage\":\"Method not allowed.\"}", response.Body);
    }

    [Fact]
    public void Health_ReturnsNotFound_ForAnotherPath()
    {
        var request = Mocks.CreateApiGatewayRequest();
        request.Path = "/health/extra";

        var response = _functions.Health(request);

        Assert.Equal(404, response.StatusCode);
        Assert.Equal("{\"errorMessage\":\"Not found.\"}", response.Body);
    }
}
