namespace KittenClaws.Api.Functions;

using Amazon.Lambda.Annotations;
using Amazon.Lambda.APIGatewayEvents;
using KittenClaws.Api.Utils;

public class HealthFunctions
{
    private const string Route = "/health";

    [LambdaFunction(ResourceName = "HealthFunction")]
    public APIGatewayProxyResponse Health(APIGatewayProxyRequest request) =>
        RouteGuard.Check(request, "GET", Route) ?? ResponseHelper.Ok(new { status = "ok" });
}
