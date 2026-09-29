namespace {{project_class_name}}.Api.Tests.Unit;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using {{project_class_name}}.Api.Functions;

public class HealthFunctionsTests
{
    [Fact]
    public async Task Health_ReturnsOk()
    {
        var functions = new HealthFunctions();

        var result = await functions.Health(Mocks.CreateHttpRequestData(), TestContext.Current.CancellationToken);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }
}
