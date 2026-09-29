namespace {{project_class_name}}.Api.Functions;

using Amazon.Lambda.Annotations;
using Amazon.Lambda.APIGatewayEvents;
using {{project_class_name}}.Api.Utils;

public class HealthFunctions
{
    [LambdaFunction(ResourceName = "HealthFunction")]
    public APIGatewayProxyResponse Health(APIGatewayProxyRequest request) => ResponseHelper.Ok(new { status = "ok" });
}
