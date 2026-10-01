namespace {{project_class_name}}.Api.Functions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using {{project_class_name}}.Api.Utils;

public class OpenApiFunctions
{
    [Function("OpenApi")]
    public IActionResult OpenApi(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "openapi.json")] HttpRequestData req) =>
        new ContentResult { StatusCode = 200, ContentType = "application/json", Content = OpenApiDocument.Json };
}
