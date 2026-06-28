namespace {{project_class_name}}.Api.Functions;

using System;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Microsoft.Extensions.Logging;
using {{project_class_name}}.Api.Interfaces;
using {{project_class_name}}.Api.Utils;

public class DeleteOkObjectResult
{
    public required string Message { get; set; }
}

public class Delete{{project_class_name}}(I{{project_class_name}}Controller {{project_lower_camel_name}}Controller, ILogger<Delete{{project_class_name}}> logger)
{
    private readonly I{{project_class_name}}Controller _{{project_lower_camel_name}}Controller = {{project_lower_camel_name}}Controller;
    private readonly ILogger<Delete{{project_class_name}}> _logger = logger;

    public async Task<APIGatewayProxyResponse> Delete(APIGatewayProxyRequest request)
    {
        _logger.LogInformation($"{nameof(Delete{{project_class_name}})} processed a request.");

        try
        {
            var id = request.PathParameters["id"];
            await _{{project_lower_camel_name}}Controller.DeleteAsync(id);

            return ResponseHelper.Ok(new DeleteOkObjectResult
            {
                Message = $"{{project_class_name}} with id {id} was deleted successfully."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName} method.", nameof(Delete{{project_class_name}}), nameof(Delete));

            return ErrorDetector.DetectError(ex);
        }
    }
}
