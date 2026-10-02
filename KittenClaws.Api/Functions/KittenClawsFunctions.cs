namespace KittenClaws.Api.Functions;

using System.Threading.Tasks;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.APIGatewayEvents;
using Microsoft.Extensions.Logging;
using KittenClaws.Api.Interfaces;
using KittenClaws.Api.Utils;

public class KittenClawsFunctions(IKittenClawsController controller, ILogger<KittenClawsFunctions> logger)
{
    private const string CollectionRoute = "/kittenclaws";
    private const string ItemRoute = "/kittenclaws/{id}";
    private readonly IKittenClawsController _controller = controller;
    private readonly ILogger<KittenClawsFunctions> _logger = logger;

    [LambdaFunction(ResourceName = "GetKittenClawsFunction")]
    public Task<APIGatewayProxyResponse> GetKittenClaws(APIGatewayProxyRequest request) =>
        FunctionRunner.RunAsync(request, "GET", ItemRoute, _logger, ct => _controller.GetAsync(FunctionRunner.Id(request), ct));

    [LambdaFunction(ResourceName = "GetKittenClawsListFunction")]
    public Task<APIGatewayProxyResponse> GetKittenClawsList(APIGatewayProxyRequest request) =>
        FunctionRunner.RunAsync(request, "GET", CollectionRoute, _logger, ct => _controller.GetListAsync(FunctionRunner.Query(request, "limit"), ct));

    [LambdaFunction(ResourceName = "CreateKittenClawsFunction")]
    public Task<APIGatewayProxyResponse> CreateKittenClaws(APIGatewayProxyRequest request) =>
        FunctionRunner.RunAsync(request, "POST", CollectionRoute, _logger, ct => _controller.CreateAsync(FunctionRunner.BodyStream(request), UserIds.From(request), ct), statusCode: 201);

    [LambdaFunction(ResourceName = "UpdateKittenClawsFunction")]
    public Task<APIGatewayProxyResponse> UpdateKittenClaws(APIGatewayProxyRequest request) =>
        FunctionRunner.RunAsync(request, "PATCH", ItemRoute, _logger, ct => _controller.UpdateAsync(FunctionRunner.Id(request), FunctionRunner.BodyStream(request), UserIds.From(request), ct));

    [LambdaFunction(ResourceName = "DeleteKittenClawsFunction")]
    public Task<APIGatewayProxyResponse> DeleteKittenClaws(APIGatewayProxyRequest request) =>
        FunctionRunner.RunAsync(request, "DELETE", ItemRoute, _logger, ct => _controller.DeleteAsync(FunctionRunner.Id(request), UserIds.From(request), ct));
}
