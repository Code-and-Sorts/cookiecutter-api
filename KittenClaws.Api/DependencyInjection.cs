namespace KittenClaws.Api;

using System;
using KittenClaws.Api.Controllers;
using KittenClaws.Api.Interfaces;
using KittenClaws.Api.Repositories;
using KittenClaws.Api.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IKittenClawsService, KittenClawsService>();
        services.AddScoped<IKittenClawsController, KittenClawsController>();

        return services;
    }
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        string cosmosConnectionString = configuration.GetConnectionString("CosmosDb") ?? string.Empty;
        string databaseName = configuration.GetValue<string>("CosmosDbDatabaseName") ?? string.Empty;
        string containerName = configuration.GetValue<string>("CosmosDbContainerName") ?? string.Empty;

        if (string.IsNullOrEmpty(cosmosConnectionString) || string.IsNullOrEmpty(databaseName) || string.IsNullOrEmpty(containerName))
        {
            throw new InvalidOperationException("CosmosDb configuration is missing or incomplete.");
        }

        services.AddSingleton(provider =>
            new CosmosClient(cosmosConnectionString)
        );

        services.AddSingleton<IKittenClawsRepository>(provider =>
        {
            var cosmosClient = provider.GetService<CosmosClient>();
            if (cosmosClient == null)
            {
                throw new InvalidOperationException("CosmosDb Client is null.");
            }
            return new KittenClawsRepository(
                cosmosClient,
                databaseName,
                containerName
            );
        });

        return services;
    }
}
