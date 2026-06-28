namespace {{project_class_name}}.Api.Functions;

using Amazon.Lambda.APIGatewayEvents;
using {{project_class_name}}.Api.Utils;

public class Health
{
    public APIGatewayProxyResponse Get(APIGatewayProxyRequest request)
    {
        return ResponseHelper.Ok(new { status = "ok" });
    }
}
