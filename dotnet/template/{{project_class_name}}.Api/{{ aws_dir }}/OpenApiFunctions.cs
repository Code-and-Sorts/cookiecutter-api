namespace {{project_class_name}}.Api.Functions;

using System.Collections.Generic;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.APIGatewayEvents;
using {{project_class_name}}.Api.Utils;

public class OpenApiFunctions
{
    private const string Route = "/openapi.json";

    [LambdaFunction(ResourceName = "OpenApiFunction")]
    public APIGatewayProxyResponse OpenApi(APIGatewayProxyRequest request) =>
        RouteGuard.Check(request, "GET", Route) ?? new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
            Body = OpenApiDocument.Json,
        };
}
