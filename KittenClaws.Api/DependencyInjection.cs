namespace KittenClaws.Api;

using System;
using KittenClaws.Api.Controllers;
using KittenClaws.Api.Entities;
using KittenClaws.Api.Interfaces;
using KittenClaws.Api.Repositories;
using KittenClaws.Api.Services;
using KittenClaws.Api.Utils;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    private const string ContainerSetting = "CosmosDbContainerName_";

    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services;
    }

    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDatabase(configuration);
        services.AddSingleton<ICatController>(provider => new CatController(new CatService(new CatRepository(
            CreateStore<CatEntity>(provider, ContainerName(configuration, "Animals", "animals"))))));
        services.AddSingleton<IDogController>(provider => new DogController(new DogService(new DogRepository(
            CreateStore<DogEntity>(provider, ContainerName(configuration, "Animals", "animals"))))));

        return services;
    }

    private static string ContainerName(IConfiguration configuration, string settingKey, string fallback) =>
        configuration[ContainerSetting + settingKey] ?? fallback;

    private static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        string cosmosConnectionString = configuration.GetConnectionString("CosmosDb") ?? string.Empty;
        string databaseName = configuration.GetValue<string>("CosmosDbDatabaseName") ?? string.Empty;

        if (string.IsNullOrEmpty(cosmosConnectionString) || string.IsNullOrEmpty(databaseName))
        {
            throw new InvalidOperationException("CosmosDb configuration is missing or incomplete.");
        }

        // The Linux Cosmos DB emulator only supports Gateway mode, so local.settings.json sets it.
        string connectionModeSetting = configuration.GetValue<string>("CosmosDbConnectionMode") ?? nameof(ConnectionMode.Direct);
        if (!Enum.TryParse(connectionModeSetting, ignoreCase: true, out ConnectionMode connectionMode) || !Enum.IsDefined(connectionMode))
        {
            throw new InvalidOperationException($"CosmosDbConnectionMode '{connectionModeSetting}' is not valid. Use Direct or Gateway.");
        }

        // Bounded so a failing database answers well inside the platform timeout.
        var cosmosOptions = new CosmosClientOptions
        {
            ConnectionMode = connectionMode,
            RequestTimeout = TimeSpan.FromSeconds(5),
            MaxRetryAttemptsOnRateLimitedRequests = 3,
            MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(3),
            UseSystemTextJsonSerializerWithOptions = Json.Options,
        };
        services.AddSingleton(provider => new CosmosClient(cosmosConnectionString, cosmosOptions));
        services.AddSingleton(provider => provider.GetRequiredService<CosmosClient>().GetDatabase(databaseName));
    }

    private static IDocumentStore<T> CreateStore<T>(IServiceProvider provider, string containerName) where T : BaseEntity, new() =>
        new CosmosDocumentStore<T>(provider.GetRequiredService<Database>().GetContainer(containerName));
}
