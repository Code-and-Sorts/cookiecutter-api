namespace {{cookiecutter.project_class_name}}.Api.Functions;

using System;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Microsoft.Extensions.Logging;
using {{cookiecutter.project_class_name}}.Api.Interfaces;
using {{cookiecutter.project_class_name}}.Api.Utils;

public class Get{{cookiecutter.project_class_name}}(I{{cookiecutter.project_class_name}}Controller {{cookiecutter.project_lower_camel_name}}Controller, ILogger<Get{{cookiecutter.project_class_name}}> logger)
{
    private readonly I{{cookiecutter.project_class_name}}Controller _{{cookiecutter.project_lower_camel_name}}Controller = {{cookiecutter.project_lower_camel_name}}Controller;
    private readonly ILogger<Get{{cookiecutter.project_class_name}}> _logger = logger;

    public async Task<APIGatewayProxyResponse> Get(APIGatewayProxyRequest request)
    {
        _logger.LogInformation($"{nameof(Get{{cookiecutter.project_class_name}})} processed a request.");

        try
        {
            var id = request.PathParameters["id"];
            var {{cookiecutter.project_lower_camel_name}} = await _{{cookiecutter.project_lower_camel_name}}Controller.GetAsync(id);

            return ResponseHelper.Ok({{cookiecutter.project_lower_camel_name}});
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName} method.", nameof(Get{{cookiecutter.project_class_name}}), nameof(Get));

            return ErrorDetector.DetectError(ex);
        }
    }
}
