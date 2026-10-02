namespace KittenClaws.Api.Utils;

using System;
using System.Linq;
using Amazon.Lambda.APIGatewayEvents;

public static class RouteGuard
{
    public static APIGatewayProxyResponse? Check(APIGatewayProxyRequest request, string method, string route)
    {
        if (!IsRoute(request, route))
        {
            return ResponseHelper.WithStatus(404, new BaseError { ErrorMessage = ErrorMessages.NotFound });
        }
        if (!string.Equals(request.HttpMethod, method, StringComparison.OrdinalIgnoreCase))
        {
            return ResponseHelper.WithStatus(405, new BaseError { ErrorMessage = ErrorMessages.MethodNotAllowed });
        }
        return null;
    }

    // Direct invocations may lack `resource`, the route template API Gateway matched.
    private static bool IsRoute(APIGatewayProxyRequest request, string route)
    {
        if (!string.IsNullOrEmpty(request.Resource))
        {
            return request.Resource == route;
        }
        var pathSegments = (request.Path ?? string.Empty).Split('/', StringSplitOptions.RemoveEmptyEntries);
        var routeSegments = route.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return pathSegments.Length == routeSegments.Length
            && routeSegments.Zip(pathSegments).All(pair => pair.First.StartsWith('{') || pair.First == pair.Second);
    }
}
