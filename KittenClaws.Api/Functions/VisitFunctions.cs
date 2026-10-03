namespace KittenClaws.Api.Functions;

using System.Threading.Tasks;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.APIGatewayEvents;
using Microsoft.Extensions.Logging;
using KittenClaws.Api.Interfaces;
using KittenClaws.Api.Utils;

public class VisitFunctions(IVisitController controller, ILogger<VisitFunctions> logger)
{
    private const string CollectionRoute = "/visits";
    private const string ItemRoute = "/visits/{id}";
    private readonly IVisitController _controller = controller;
    private readonly ILogger<VisitFunctions> _logger = logger;

    [LambdaFunction(ResourceName = "GetVisitFunction")]
    public Task<APIGatewayProxyResponse> GetVisit(APIGatewayProxyRequest request) =>
        FunctionRunner.RunAsync(request, "GET", ItemRoute, _logger, ct => _controller.GetAsync(FunctionRunner.Id(request), ct));

    [LambdaFunction(ResourceName = "CreateVisitFunction")]
    public Task<APIGatewayProxyResponse> CreateVisit(APIGatewayProxyRequest request) =>
        FunctionRunner.RunAsync(request, "POST", CollectionRoute, _logger, ct => _controller.CreateAsync(FunctionRunner.BodyStream(request), UserIds.From(request), ct), statusCode: 201);

    [LambdaFunction(ResourceName = "UpdateVisitFunction")]
    public Task<APIGatewayProxyResponse> UpdateVisit(APIGatewayProxyRequest request) =>
        FunctionRunner.RunAsync(request, "PATCH", ItemRoute, _logger, ct => _controller.UpdateAsync(FunctionRunner.Id(request), FunctionRunner.BodyStream(request), UserIds.From(request), ct));
}
