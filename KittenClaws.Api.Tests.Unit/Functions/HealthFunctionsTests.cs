namespace KittenClaws.Api.Tests.Unit;

using System.Threading.Tasks;
using Xunit;
using KittenClaws.Api.Functions;

public class HealthFunctionsTests
{
    [Fact]
    public async Task Health_ReturnsOk()
    {
        var functions = new HealthFunctions();

        var result = await functions.Health(Mocks.CreateHttpRequestData(), TestContext.Current.CancellationToken);

        Assert.Equal((200, "{\"status\":\"ok\"}"), Mocks.ReadJsonResult(result));
    }
}
