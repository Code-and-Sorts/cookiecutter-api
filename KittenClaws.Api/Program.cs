
namespace KittenClaws.Api;

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
