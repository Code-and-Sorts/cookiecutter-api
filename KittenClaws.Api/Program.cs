
namespace KittenClaws.Api;

using Amazon.Lambda.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

[LambdaStartup]
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging(logging => logging.AddConsole());
        services.AddApplication();
        services.AddPersistence();
    }
}
