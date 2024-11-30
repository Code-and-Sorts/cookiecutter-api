
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
[assembly: FunctionsStartup(typeof({{cookiecutter.project_name}}.Api.Startup))]
namespace {{cookiecutter.project_name}}.Api;

using Microsoft.Extensions.Configuration;
using System;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        var configuration = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

        builder.Services.AddApplication(configuration);
        builder.Services.AddPersistence(configuration);
    }
}
