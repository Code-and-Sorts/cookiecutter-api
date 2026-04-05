using Google.Cloud.Functions.Hosting;
using {{cookiecutter.project_class_name}}.Api;

[assembly: FunctionsStartup(typeof(Startup))]

namespace {{cookiecutter.project_class_name}}.Api;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

public class Startup : FunctionsStartup
{
    public override void ConfigureServices(WebHostBuilderContext context, IServiceCollection services)
    {
        var configuration = context.Configuration;
        services.AddApplication();
        services.AddPersistence(configuration);
    }
}
