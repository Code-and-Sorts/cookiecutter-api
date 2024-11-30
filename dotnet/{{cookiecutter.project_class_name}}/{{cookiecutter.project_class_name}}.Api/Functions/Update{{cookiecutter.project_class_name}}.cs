namespace {{cookiecutter.project_class_name}}.Api.Functions;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using {{cookiecutter.project_class_name}}.Api.Interfaces;
using {{cookiecutter.project_class_name}}.Api.Requests;
using {{cookiecutter.project_class_name}}.Api.Utils;

public class Update{{cookiecutter.project_class_name}}
{
    private readonly I{{cookiecutter.project_class_name}}Controller _{{cookiecutter.project_lower_camel_name}}Controller;
    private readonly ILogger<Update{{cookiecutter.project_class_name}}> _logger;

    public Update{{cookiecutter.project_class_name}}(I{{cookiecutter.project_class_name}}Controller {{cookiecutter.project_lower_camel_name}}Controller, ILogger<Update{{cookiecutter.project_class_name}}> logger)
    {
        _{{cookiecutter.project_lower_camel_name}}Controller = {{cookiecutter.project_lower_camel_name}}Controller;
        _logger = logger;
    }

    [FunctionName("Update{{cookiecutter.project_class_name}}")]
    public async Task<IActionResult> Patch(
        [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "{{cookiecutter.project_endpoint}}/{id}")] Update{{cookiecutter.project_class_name}}Request update{{cookiecutter.project_class_name}}Request, string id, CancellationToken ct = default)
    {
        _logger.LogInformation($"{nameof(Update{{cookiecutter.project_class_name}})} processed a request.");

        try
        {
            update{{cookiecutter.project_class_name}}Request.Id = id;
            var updated{{cookiecutter.project_class_name}}Dto = await _{{cookiecutter.project_lower_camel_name}}Controller.UpdateAsync(update{{cookiecutter.project_class_name}}Request, ct);

            return new OkObjectResult(updated{{cookiecutter.project_class_name}}Dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName} method.", nameof(Update{{cookiecutter.project_class_name}}), nameof(Patch));

            return ErrorDetector.DetectError(ex);
        }
    }
}
