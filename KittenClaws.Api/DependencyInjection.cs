namespace KittenClaws.Api;

using System;
using KittenClaws.Api.Controllers;
using KittenClaws.Api.Entities;
using KittenClaws.Api.Interfaces;
using KittenClaws.Api.Repositories;
using KittenClaws.Api.Services;
using Amazon.DynamoDBv2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    private const string ContainerSetting = "DYNAMODB_TABLE_NAME_";

    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services;
    }

    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDatabase(configuration);
        services.AddSingleton<ICatController>(provider => new CatController(new CatService(new CatRepository(
            CreateStore<CatEntity>(provider, ContainerName(configuration, "ANIMALS", "animals"))))));
        services.AddSingleton<IDogController>(provider => new DogController(new DogService(new DogRepository(
            CreateStore<DogEntity>(provider, ContainerName(configuration, "ANIMALS", "animals"))))));

        return services;
    }

    public static IReadOnlyList<string> StoreNames(IConfiguration configuration) =>
    [
        ..new SortedSet<string>(StringComparer.Ordinal)
        {
            ContainerName(configuration, "ANIMALS", "animals"),
        },
    ];

    private static string ContainerName(IConfiguration configuration, string settingKey, string fallback) =>
        configuration[ContainerSetting + settingKey] ?? fallback;

    private static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var dynamoDbConfig = CreateDynamoDbConfig(configuration);
        services.AddSingleton<IAmazonDynamoDB>(_ => new AmazonDynamoDBClient(dynamoDbConfig));
    }

    public static AmazonDynamoDBConfig CreateDynamoDbConfig(IConfiguration configuration) => new()
    {
        // Bounded so a failing database answers well inside the Lambda timeout.
        MaxErrorRetry = 2,
        Timeout = TimeSpan.FromSeconds(3),
        // sam local sets the variable to "" when no emulator is configured, and the SDK would use "" as the endpoint.
        IgnoreConfiguredEndpointUrls = configuration["AWS_ENDPOINT_URL_DYNAMODB"] is "",
    };

    private static IDocumentStore<T> CreateStore<T>(IServiceProvider provider, string containerName) where T : BaseEntity, new() =>
        new DynamoDocumentStore<T>(provider.GetRequiredService<IAmazonDynamoDB>(), containerName);
}
