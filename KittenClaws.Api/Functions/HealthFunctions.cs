namespace KittenClaws.Api.Functions;

using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using KittenClaws.Api.Utils;

public class HealthFunctions
{
    [Function("Health")]
    public Task<IActionResult> Health(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health")] HttpRequestData req, CancellationToken ct = default)
    {
        return Task.FromResult(FunctionRunner.JsonResponse(200, new { status = "ok" }));
    }
}
