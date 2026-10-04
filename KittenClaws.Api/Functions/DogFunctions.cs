namespace KittenClaws.Api.Functions;

using System.Threading.Tasks;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.APIGatewayEvents;
using Microsoft.Extensions.Logging;
using KittenClaws.Api.Interfaces;
using KittenClaws.Api.Utils;

public class DogFunctions(IDogController controller, ILogger<DogFunctions> logger)
{
    private const string CollectionRoute = "/dogs";
    private const string ItemRoute = "/dogs/{id}";
    private readonly IDogController _controller = controller;
    private readonly ILogger<DogFunctions> _logger = logger;

    [LambdaFunction(ResourceName = "GetDogFunction")]
    public Task<APIGatewayProxyResponse> GetDog(APIGatewayProxyRequest request) =>
        FunctionRunner.RunAsync(request, "GET", ItemRoute, _logger, ct => _controller.GetAsync(FunctionRunner.Id(request), ct));

    [LambdaFunction(ResourceName = "GetDogListFunction")]
    public Task<APIGatewayProxyResponse> GetDogList(APIGatewayProxyRequest request) =>
        FunctionRunner.RunAsync(request, "GET", CollectionRoute, _logger, ct => _controller.GetListAsync(FunctionRunner.Query(request, "limit"), ct));

    [LambdaFunction(ResourceName = "CreateDogFunction")]
    public Task<APIGatewayProxyResponse> CreateDog(APIGatewayProxyRequest request) =>
        FunctionRunner.RunAsync(request, "POST", CollectionRoute, _logger, ct => _controller.CreateAsync(FunctionRunner.BodyStream(request), UserIds.From(request), ct), statusCode: 201);

    [LambdaFunction(ResourceName = "ReplaceDogFunction")]
    public Task<APIGatewayProxyResponse> ReplaceDog(APIGatewayProxyRequest request) =>
        FunctionRunner.RunAsync(request, "PUT", ItemRoute, _logger, ct => _controller.ReplaceAsync(FunctionRunner.Id(request), FunctionRunner.BodyStream(request), UserIds.From(request), ct));

    [LambdaFunction(ResourceName = "DeleteDogFunction")]
    public Task<APIGatewayProxyResponse> DeleteDog(APIGatewayProxyRequest request) =>
        FunctionRunner.RunAsync(request, "DELETE", ItemRoute, _logger, ct => _controller.DeleteAsync(FunctionRunner.Id(request), UserIds.From(request), ct));
}
