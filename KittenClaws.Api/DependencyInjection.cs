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
        services.AddScoped<IKittenClawsService, KittenClawsService>();
        services.AddScoped<IKittenClawsController, KittenClawsController>();

        return services;
    }
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        string projectId = configuration.GetValue<string>("GCP_PROJECT_ID") ?? string.Empty;
        string databaseId = configuration.GetValue<string>("FIRESTORE_DATABASE") ?? "(default)";
        string collectionName = configuration.GetValue<string>("FIRESTORE_COLLECTION") ?? string.Empty;

        if (string.IsNullOrEmpty(projectId) || string.IsNullOrEmpty(collectionName))
        {
            throw new InvalidOperationException("Firestore configuration is missing or incomplete.");
        }

        services.AddSingleton(provider =>
            new FirestoreDbBuilder { ProjectId = projectId, DatabaseId = databaseId }.Build()
        );

        services.AddSingleton<IFirestoreContext<Entities.KittenClaws>>(provider =>
        {
            var firestoreDb = provider.GetService<FirestoreDb>();
            if (firestoreDb == null)
            {
                throw new InvalidOperationException("Firestore Client is null.");
            }
            return new FirestoreContext<Entities.KittenClaws>(
                firestoreDb,
                collectionName
            );
        });

        services.AddSingleton<IKittenClawsRepository>(provider =>
        {
            var context = provider.GetService<IFirestoreContext<Entities.KittenClaws>>();
            if (context == null)
            {
                throw new InvalidOperationException("Firestore Context is null.");
            }
            return new KittenClawsRepository(context);
        });

        return services;
    }
}
