namespace {{cookiecutter.project_class_name}}.Api.Functions;

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Microsoft.Extensions.Logging;
using {{cookiecutter.project_class_name}}.Api.Interfaces;
using {{cookiecutter.project_class_name}}.Api.Utils;

public class Update{{cookiecutter.project_class_name}}(I{{cookiecutter.project_class_name}}Controller {{cookiecutter.project_lower_camel_name}}Controller, ILogger<Update{{cookiecutter.project_class_name}}> logger)
{
    private readonly I{{cookiecutter.project_class_name}}Controller _{{cookiecutter.project_lower_camel_name}}Controller = {{cookiecutter.project_lower_camel_name}}Controller;
    private readonly ILogger<Update{{cookiecutter.project_class_name}}> _logger = logger;

    public async Task<APIGatewayProxyResponse> Patch(APIGatewayProxyRequest request)
    {
        _logger.LogInformation($"{nameof(Update{{cookiecutter.project_class_name}})} processed a request.");

        try
        {
            var id = request.PathParameters["id"];
            var bodyStream = new MemoryStream(Encoding.UTF8.GetBytes(request.Body ?? ""));
            var updated{{cookiecutter.project_class_name}}Dto = await _{{cookiecutter.project_lower_camel_name}}Controller.UpdateAsync(id, bodyStream);

            return ResponseHelper.Ok(updated{{cookiecutter.project_class_name}}Dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName} method.", nameof(Update{{cookiecutter.project_class_name}}), nameof(Patch));

            return ErrorDetector.DetectError(ex);
        }
    }
}
