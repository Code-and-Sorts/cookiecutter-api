{%- if cookiecutter.cloud_service == 'Azure Function App' %}
using {{cookiecutter.project_class_name}}.Api;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();
builder.Configuration
    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);

builder.Services.AddApplication();
builder.Services.AddPersistence(builder.Configuration);

builder.Build().Run();
{%- endif %}
{%- if cookiecutter.cloud_service == 'AWS Lambda' %}
namespace {{cookiecutter.project_class_name}}.Api;

using Amazon.Lambda.Annotations;
using Microsoft.Extensions.DependencyInjection;

[LambdaStartup]
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddApplication();
        services.AddPersistence();
    }
}
{%- endif %}
