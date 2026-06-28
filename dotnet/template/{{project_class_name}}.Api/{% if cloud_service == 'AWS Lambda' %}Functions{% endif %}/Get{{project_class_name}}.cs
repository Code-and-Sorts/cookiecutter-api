namespace {{project_class_name}}.Api.Functions;

using System;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Microsoft.Extensions.Logging;
using {{project_class_name}}.Api.Interfaces;
using {{project_class_name}}.Api.Utils;

public class Get{{project_class_name}}(I{{project_class_name}}Controller {{project_lower_camel_name}}Controller, ILogger<Get{{project_class_name}}> logger)
{
    private readonly I{{project_class_name}}Controller _{{project_lower_camel_name}}Controller = {{project_lower_camel_name}}Controller;
    private readonly ILogger<Get{{project_class_name}}> _logger = logger;

    public async Task<APIGatewayProxyResponse> Get(APIGatewayProxyRequest request)
    {
        _logger.LogInformation($"{nameof(Get{{project_class_name}})} processed a request.");

        try
        {
            var id = request.PathParameters["id"];
            var {{project_lower_camel_name}} = await _{{project_lower_camel_name}}Controller.GetAsync(id);

            return ResponseHelper.Ok({{project_lower_camel_name}});
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName} method.", nameof(Get{{project_class_name}}), nameof(Get));

            return ErrorDetector.DetectError(ex);
        }
    }
}
