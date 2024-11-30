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

public class DeleteOkObjectResult
{
    public string Message { get; set; }
}

public class Delete{{cookiecutter.project_name}}
{
    private readonly I{{cookiecutter.project_name}}Controller _{{cookiecutter.project_lower_camel_name}}Controller;
    private readonly ILogger<Delete{{cookiecutter.project_name}}> _logger;

    public Delete{{cookiecutter.project_name}}(I{{cookiecutter.project_name}}Controller {{cookiecutter.project_lower_camel_name}}Controller, ILogger<Delete{{cookiecutter.project_name}}> logger)
    {
        _{{cookiecutter.project_lower_camel_name}}Controller = {{cookiecutter.project_lower_camel_name}}Controller;
        _logger = logger;
    }

    [FunctionName("Delete{{cookiecutter.project_name}}")]
    public async Task<IActionResult> Delete(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "{{cookiecutter.project_endpoint}}/{id}")] string id, CancellationToken ct = default)
    {
        _logger.LogInformation($"{nameof(Delete{{cookiecutter.project_name}})} processed a request.");

        try
        {
            await _{{cookiecutter.project_lower_camel_name}}Controller.DeleteAsync(id, ct);

            return new OkObjectResult(new DeleteOkObjectResult
            {
                Message = $"{{cookiecutter.project_name}} with id {id} was deleted successfully."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName} method.", nameof(Delete{{cookiecutter.project_name}}), nameof(Delete));

            return ErrorDetector.DetectError(ex);
        }
    }
}
