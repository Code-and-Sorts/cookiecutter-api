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
        services.AddSingleton<IKittenClawsController>(provider => new KittenClawsController(new KittenClawsService(new KittenClawsRepository(
            CreateStore<KittenClawsEntity>(provider, ContainerName(configuration, "KITTENCLAWS", "kittenclaws"))))));

        return services;
    }

    private static string ContainerName(IConfiguration configuration, string settingKey, string fallback) =>
        configuration[ContainerSetting + settingKey] ?? fallback;

    private static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        // Bounded so a failing database answers well inside the Lambda timeout.
        services.AddSingleton<IAmazonDynamoDB>(_ => new AmazonDynamoDBClient(new AmazonDynamoDBConfig
        {
            MaxErrorRetry = 2,
            Timeout = TimeSpan.FromSeconds(3),
        }));
    }

    private static IDocumentStore<T> CreateStore<T>(IServiceProvider provider, string containerName) where T : BaseEntity, new() =>
        new DynamoDocumentStore<T>(provider.GetRequiredService<IAmazonDynamoDB>(), containerName);
}
