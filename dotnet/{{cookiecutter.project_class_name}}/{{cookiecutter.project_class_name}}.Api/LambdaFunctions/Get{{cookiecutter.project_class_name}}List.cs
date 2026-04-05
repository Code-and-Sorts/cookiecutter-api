namespace {{cookiecutter.project_class_name}}.Api.Functions;

using System;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Microsoft.Extensions.Logging;
using {{cookiecutter.project_class_name}}.Api.Interfaces;
using {{cookiecutter.project_class_name}}.Api.Utils;

public class Get{{cookiecutter.project_class_name}}List(I{{cookiecutter.project_class_name}}Controller {{cookiecutter.project_lower_camel_name}}Controller, ILogger<Get{{cookiecutter.project_class_name}}List> logger)
{
    private readonly I{{cookiecutter.project_class_name}}Controller _{{cookiecutter.project_lower_camel_name}}Controller = {{cookiecutter.project_lower_camel_name}}Controller;
    private readonly ILogger<Get{{cookiecutter.project_class_name}}List> _logger = logger;

    public async Task<APIGatewayProxyResponse> Get(APIGatewayProxyRequest request)
    {
        _logger.LogInformation($"{nameof(Get{{cookiecutter.project_class_name}}List)} processed a request.");

        try
        {
            var {{cookiecutter.project_lower_camel_name}}List = await _{{cookiecutter.project_lower_camel_name}}Controller.GetListAsync();

            return ResponseHelper.Ok({{cookiecutter.project_lower_camel_name}}List);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName} method.", nameof(Get{{cookiecutter.project_class_name}}List), nameof(Get));

            return ErrorDetector.DetectError(ex);
        }
    }
}
