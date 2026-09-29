namespace {{project_class_name}}.Api.Tests.Unit;

using Xunit;
using {{project_class_name}}.Api.Functions;

public class HealthFunctionsTests
{
    [Fact]
    public void Health_ReturnsOk()
    {
        var functions = new HealthFunctions();

        var response = functions.Health(Mocks.CreateApiGatewayRequest());

        Assert.Equal(200, response.StatusCode);
        Assert.Contains("ok", response.Body);
    }
}
