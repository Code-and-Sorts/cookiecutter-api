namespace {{cookiecutter.project_class_name}}.Api.Functions;

using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

public class Health
{
    [Function("Health")]
    public Task<IActionResult> Get(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health")] CancellationToken ct = default)
    {
        return Task.FromResult<IActionResult>(new OkObjectResult(new { status = "ok" }));
    }
}
