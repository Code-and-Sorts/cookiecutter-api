namespace KittenClaws.Api;

using System;
using KittenClaws.Api.Controllers;
using KittenClaws.Api.Interfaces;
using KittenClaws.Api.Repositories;
using KittenClaws.Api.Services;
using Google.Cloud.Firestore;
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
        string projectId = configuration.GetValue<string>("GCP_PROJECT_ID") ?? string.Empty;
        string databaseId = configuration.GetValue<string>("FIRESTORE_DATABASE") ?? "(default)";

        if (string.IsNullOrEmpty(projectId))
        {
            throw new InvalidOperationException("Firestore configuration is missing or incomplete.");
        }

        services.AddSingleton(provider =>
            new FirestoreDbBuilder { ProjectId = projectId, DatabaseId = databaseId }.Build()
        );

        services.AddSingleton<IKittenClawsController>(provider =>
        {
            var firestoreDb = provider.GetRequiredService<FirestoreDb>();
            string collectionName = configuration.GetValue<string>("FIRESTORE_COLLECTION_Kitties") ?? "kitties";
            var context = new FirestoreContext<Entities.KittenClaws>(firestoreDb, collectionName);
            var repository = new KittenClawsRepository(context);
            return new KittenClawsController(new KittenClawsService(repository));
        });

        return services;
    }
}
