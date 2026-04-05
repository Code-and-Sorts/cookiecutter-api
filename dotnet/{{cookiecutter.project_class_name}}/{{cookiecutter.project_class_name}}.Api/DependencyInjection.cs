namespace {{cookiecutter.project_class_name}}.Api;

using System;
using {{cookiecutter.project_class_name}}.Api.Controllers;
using {{cookiecutter.project_class_name}}.Api.Interfaces;
using {{cookiecutter.project_class_name}}.Api.Repositories;
using {{cookiecutter.project_class_name}}.Api.Services;
{%- if cookiecutter.cloud_service == 'Azure Function App' %}
using Microsoft.Azure.Cosmos;
{%- endif %}
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
using Google.Cloud.Firestore;
{%- endif %}
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<I{{cookiecutter.project_class_name}}Service, {{cookiecutter.project_class_name}}Service>();
        services.AddScoped<I{{cookiecutter.project_class_name}}Controller, {{cookiecutter.project_class_name}}Controller>();

        return services;
    }

    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
{%- if cookiecutter.cloud_service == 'Azure Function App' %}
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

        services.AddSingleton<I{{cookiecutter.project_class_name}}Repository>(provider =>
        {
            var cosmosClient = provider.GetService<CosmosClient>();
            if (cosmosClient == null)
            {
                throw new InvalidOperationException("CosmosDb Client is null.");
            }
            return new {{cookiecutter.project_class_name}}Repository(
                cosmosClient,
                databaseName,
                containerName
            );
        });
{%- endif %}
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
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

        services.AddSingleton<I{{cookiecutter.project_class_name}}Repository>(provider =>
        {
            var firestoreDb = provider.GetService<FirestoreDb>();
            if (firestoreDb == null)
            {
                throw new InvalidOperationException("Firestore Client is null.");
            }
            return new {{cookiecutter.project_class_name}}Repository(
                firestoreDb,
                collectionName
            );
        });
{%- endif %}

        return services;
    }
}
