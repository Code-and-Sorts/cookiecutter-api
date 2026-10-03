namespace KittenClaws.Api.Functions;

using System.Threading.Tasks;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.APIGatewayEvents;
using Microsoft.Extensions.Logging;
using KittenClaws.Api.Interfaces;
using KittenClaws.Api.Utils;

public class CatFunctions(ICatController controller, ILogger<CatFunctions> logger)
{
    private const string CollectionRoute = "/cats";
    private const string ItemRoute = "/cats/{id}";
    private readonly ICatController _controller = controller;
    private readonly ILogger<CatFunctions> _logger = logger;

    [LambdaFunction(ResourceName = "GetCatFunction")]
    public Task<APIGatewayProxyResponse> GetCat(APIGatewayProxyRequest request) =>
        FunctionRunner.RunAsync(request, "GET", ItemRoute, _logger, ct => _controller.GetAsync(FunctionRunner.Id(request), ct));

    [LambdaFunction(ResourceName = "GetCatListFunction")]
    public Task<APIGatewayProxyResponse> GetCatList(APIGatewayProxyRequest request) =>
        FunctionRunner.RunAsync(request, "GET", CollectionRoute, _logger, ct => _controller.GetListAsync(FunctionRunner.Query(request, "limit"), ct));

    [LambdaFunction(ResourceName = "CreateCatFunction")]
    public Task<APIGatewayProxyResponse> CreateCat(APIGatewayProxyRequest request) =>
        FunctionRunner.RunAsync(request, "POST", CollectionRoute, _logger, ct => _controller.CreateAsync(FunctionRunner.BodyStream(request), UserIds.From(request), ct), statusCode: 201);

    [LambdaFunction(ResourceName = "UpdateCatFunction")]
    public Task<APIGatewayProxyResponse> UpdateCat(APIGatewayProxyRequest request) =>
        FunctionRunner.RunAsync(request, "PATCH", ItemRoute, _logger, ct => _controller.UpdateAsync(FunctionRunner.Id(request), FunctionRunner.BodyStream(request), UserIds.From(request), ct));

    [LambdaFunction(ResourceName = "DeleteCatFunction")]
    public Task<APIGatewayProxyResponse> DeleteCat(APIGatewayProxyRequest request) =>
        FunctionRunner.RunAsync(request, "DELETE", ItemRoute, _logger, ct => _controller.DeleteAsync(FunctionRunner.Id(request), UserIds.From(request), ct));
}
