namespace KittenClaws.Api;

using System;
using KittenClaws.Api.Controllers;
using KittenClaws.Api.Entities;
using KittenClaws.Api.Handlers;
using KittenClaws.Api.Interfaces;
using KittenClaws.Api.Repositories;
using KittenClaws.Api.Services;
using Google.Api.Gax;
using Google.Api.Gax.Grpc;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    private const string ContainerSetting = "FIRESTORE_COLLECTION_";

    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services;
    }

    public static IServiceCollection AddHandlers(this IServiceCollection services)
    {
        services.AddSingleton<IResourceHandler, CatHandler>();
        services.AddSingleton<IResourceHandler, DogHandler>();
        services.AddSingleton<IResourceHandler, HealthHandler>();

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
        string projectId = configuration.GetValue<string>("GCP_PROJECT_ID") ?? string.Empty;
        string databaseId = configuration.GetValue<string>("FIRESTORE_DATABASE") ?? "(default)";

        if (string.IsNullOrEmpty(projectId))
        {
            throw new InvalidOperationException("Firestore configuration is missing or incomplete.");
        }

        // Bounded so a failing database answers well inside the platform timeout.
        services.AddSingleton(provider =>
            new FirestoreDbBuilder
            {
                ProjectId = projectId,
                DatabaseId = databaseId,
                EmulatorDetection = EmulatorDetection.EmulatorOrProduction,
                Settings = new FirestoreSettings { CallSettings = CallSettings.FromExpiration(Expiration.FromTimeout(TimeSpan.FromSeconds(5))) },
            }.Build()
        );
    }

    private static IDocumentStore<T> CreateStore<T>(IServiceProvider provider, string containerName) where T : BaseEntity, new() =>
        new FirestoreDocumentStore<T>(provider.GetRequiredService<FirestoreDb>(), containerName);
}
