using Google.Cloud.Functions.Hosting;
using KittenClaws.Api;

[assembly: FunctionsStartup(typeof(Startup))]

namespace KittenClaws.Api;

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
