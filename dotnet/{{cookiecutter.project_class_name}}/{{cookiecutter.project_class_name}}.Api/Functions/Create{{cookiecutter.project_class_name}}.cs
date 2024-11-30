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
using {{cookiecutter.project_class_name}}.Api.Controllers;

public class Create{{cookiecutter.project_class_name}}
{
    private readonly I{{cookiecutter.project_class_name}}Controller _{{cookiecutter.project_lower_camel_name}}Controller;
    private readonly ILogger<Create{{cookiecutter.project_class_name}}> _logger;

    public Create{{cookiecutter.project_class_name}}(I{{cookiecutter.project_class_name}}Controller {{cookiecutter.project_lower_camel_name}}Controller, ILogger<Create{{cookiecutter.project_class_name}}> logger)
    {
        _{{cookiecutter.project_lower_camel_name}}Controller = {{cookiecutter.project_lower_camel_name}}Controller;
        _logger = logger;
    }

    [FunctionName("Create{{cookiecutter.project_class_name}}")]
    public async Task<IActionResult> Post(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "{{cookiecutter.project_endpoint}}")] Create{{cookiecutter.project_class_name}}Request create{{cookiecutter.project_class_name}}Request, CancellationToken ct = default)
    {
        _logger.LogInformation($"{nameof(Create{{cookiecutter.project_class_name}})} processed a request.");

        try
        {
            var new{{cookiecutter.project_class_name}}Dto = await _{{cookiecutter.project_lower_camel_name}}Controller.CreateAsync(create{{cookiecutter.project_class_name}}Request, ct);

            return new CreatedResult($"/api/{{cookiecutter.project_endpoint}}", new{{cookiecutter.project_class_name}}Dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in {ClassName} -> {MethodName} method.", nameof(Create{{cookiecutter.project_class_name}}), nameof(Post));

            return ErrorDetector.DetectError(ex);
        }
    }
}
