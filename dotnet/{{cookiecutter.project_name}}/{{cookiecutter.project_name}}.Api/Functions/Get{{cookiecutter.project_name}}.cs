namespace {{cookiecutter.project_name}}.Api.Functions;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using {{cookiecutter.project_name}}.Api.Interfaces;
using {{cookiecutter.project_name}}.Api.Utils;

public class Get{{cookiecutter.project_name}}
{
    private readonly I{{cookiecutter.project_name}}Controller _{{cookiecutter.project_lower_camel_name}}Controller;
    private readonly ILogger<Get{{cookiecutter.project_name}}> _logger;

    public Get{{cookiecutter.project_name}}(I{{cookiecutter.project_name}}Controller {{cookiecutter.project_lower_camel_name}}Controller, ILogger<Get{{cookiecutter.project_name}}> logger)
    {
        _{{cookiecutter.project_lower_camel_name}}Controller = {{cookiecutter.project_lower_camel_name}}Controller;
        _logger = logger;
    }

    [FunctionName("Get{{cookiecutter.project_name}}")]
    public async Task<IActionResult> Get(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "{{cookiecutter.project_endpoint}}/{id}")] string id, CancellationToken ct = default)
    {
        _logger.LogInformation($"{nameof(Get{{cookiecutter.project_name}})} processed a request.");

        try
        {
            var {{cookiecutter.project_lower_camel_name}} = await _{{cookiecutter.project_lower_camel_name}}Controller.GetAsync(id, ct);

            return new OkObjectResult({{cookiecutter.project_lower_camel_name}});
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName} method.", nameof(Get{{cookiecutter.project_name}}), nameof(Get));

            return ErrorDetector.DetectError(ex);
        }
    }
}
