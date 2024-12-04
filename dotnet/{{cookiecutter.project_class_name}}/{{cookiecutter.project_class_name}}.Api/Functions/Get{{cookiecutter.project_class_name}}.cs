namespace {{cookiecutter.project_class_name}}.Api.Functions;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using {{cookiecutter.project_class_name}}.Api.Interfaces;
using {{cookiecutter.project_class_name}}.Api.Utils;
using Microsoft.Azure.Functions.Worker.Http;

public class Get{{cookiecutter.project_class_name}}(I{{cookiecutter.project_class_name}}Controller {{cookiecutter.project_lower_camel_name}}Controller, ILogger<Get{{cookiecutter.project_class_name}}> logger)
{
    private readonly I{{cookiecutter.project_class_name}}Controller _{{cookiecutter.project_lower_camel_name}}Controller = {{cookiecutter.project_lower_camel_name}}Controller;
    private readonly ILogger<Get{{cookiecutter.project_class_name}}> _logger = logger;

    [Function("Get{{cookiecutter.project_class_name}}")]
    public async Task<IActionResult> Get(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "{{cookiecutter.project_endpoint}}/{id}")] HttpRequestData req, string id, CancellationToken ct = default)
    {
        _logger.LogInformation($"{nameof(Get{{cookiecutter.project_class_name}})} processed a request.");

        try
        {
            var {{cookiecutter.project_lower_camel_name}} = await _{{cookiecutter.project_lower_camel_name}}Controller.GetAsync(id, ct);

            return new OkObjectResult({{cookiecutter.project_lower_camel_name}});
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName} method.", nameof(Get{{cookiecutter.project_class_name}}), nameof(Get));

            return ErrorDetector.DetectError(ex);
        }
    }
}
