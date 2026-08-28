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
        return services;
    }
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        string cosmosConnectionString = configuration.GetConnectionString("CosmosDb") ?? string.Empty;
        string databaseName = configuration.GetValue<string>("CosmosDbDatabaseName") ?? string.Empty;

        if (string.IsNullOrEmpty(cosmosConnectionString) || string.IsNullOrEmpty(databaseName))
        {
            throw new InvalidOperationException("CosmosDb configuration is missing or incomplete.");
        }

        services.AddSingleton(provider => new CosmosClient(cosmosConnectionString));

        services.AddSingleton<IKittenClawsController>(provider =>
        {
            var cosmosClient = provider.GetRequiredService<CosmosClient>();
            string containerName = configuration.GetValue<string>("CosmosDbContainerName_Kitties") ?? "kitties";
            var repository = new KittenClawsRepository(cosmosClient, databaseName, containerName);
            return new KittenClawsController(new KittenClawsService(repository));
        });

        return services;
    }
}
