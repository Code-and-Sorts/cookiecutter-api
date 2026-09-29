namespace KittenClaws.Api.Functions;

using Amazon.Lambda.APIGatewayEvents;
using KittenClaws.Api.Utils;

public class Health
{
    public APIGatewayProxyResponse Get(APIGatewayProxyRequest request)
    {
        return ResponseHelper.Ok(new { status = "ok" });
    }
}
