namespace {{cookiecutter.project_class_name}}.Api;

using Google.Cloud.Functions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[FunctionsStartup]
public class Startup : FunctionsStartup
{
    public override void ConfigureServices(WebHostBuilderContext context, IServiceCollection services)
    {
        var configuration = context.Configuration;
        services.AddApplication();
        services.AddPersistence(configuration);
    }
}
