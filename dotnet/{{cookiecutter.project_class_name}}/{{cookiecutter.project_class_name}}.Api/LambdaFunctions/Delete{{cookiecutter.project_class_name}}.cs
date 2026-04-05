namespace {{cookiecutter.project_class_name}}.Api.Functions;

using System;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Microsoft.Extensions.Logging;
using {{cookiecutter.project_class_name}}.Api.Interfaces;
using {{cookiecutter.project_class_name}}.Api.Utils;

public class DeleteOkObjectResult
{
    public required string Message { get; set; }
}

public class Delete{{cookiecutter.project_class_name}}(I{{cookiecutter.project_class_name}}Controller {{cookiecutter.project_lower_camel_name}}Controller, ILogger<Delete{{cookiecutter.project_class_name}}> logger)
{
    private readonly I{{cookiecutter.project_class_name}}Controller _{{cookiecutter.project_lower_camel_name}}Controller = {{cookiecutter.project_lower_camel_name}}Controller;
    private readonly ILogger<Delete{{cookiecutter.project_class_name}}> _logger = logger;

    public async Task<APIGatewayProxyResponse> Delete(APIGatewayProxyRequest request)
    {
        _logger.LogInformation($"{nameof(Delete{{cookiecutter.project_class_name}})} processed a request.");

        try
        {
            var id = request.PathParameters["id"];
            await _{{cookiecutter.project_lower_camel_name}}Controller.DeleteAsync(id);

            return ResponseHelper.Ok(new DeleteOkObjectResult
            {
                Message = $"{{cookiecutter.project_class_name}} with id {id} was deleted successfully."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName} method.", nameof(Delete{{cookiecutter.project_class_name}}), nameof(Delete));

            return ErrorDetector.DetectError(ex);
        }
    }
}
