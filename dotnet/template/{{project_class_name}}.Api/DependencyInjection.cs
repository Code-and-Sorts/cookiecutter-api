namespace {{project_class_name}}.Api;

using System;
using {{project_class_name}}.Api.Controllers;
using {{project_class_name}}.Api.Interfaces;
using {{project_class_name}}.Api.Repositories;
using {{project_class_name}}.Api.Services;
{%- if cloud_service == 'Azure Function App' %}
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
using Google.Cloud.Firestore;
using Microsoft.Extensions.Configuration;
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
using Amazon.DynamoDBv2;
{%- endif %}
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<I{{project_class_name}}Service, {{project_class_name}}Service>();
        services.AddScoped<I{{project_class_name}}Controller, {{project_class_name}}Controller>();

        return services;
    }

{%- if cloud_service == 'Azure Function App' %}
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

        services.AddSingleton<I{{project_class_name}}Repository>(provider =>
        {
            var cosmosClient = provider.GetService<CosmosClient>();
            if (cosmosClient == null)
            {
                throw new InvalidOperationException("CosmosDb Client is null.");
            }
            return new {{project_class_name}}Repository(
                cosmosClient,
                databaseName,
                containerName
            );
        });

        return services;
    }
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
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

        services.AddSingleton<IFirestoreContext<Entities.{{project_class_name}}>>(provider =>
        {
            var firestoreDb = provider.GetService<FirestoreDb>();
            if (firestoreDb == null)
            {
                throw new InvalidOperationException("Firestore Client is null.");
            }
            return new FirestoreContext<Entities.{{project_class_name}}>(
                firestoreDb,
                collectionName
            );
        });

        services.AddSingleton<I{{project_class_name}}Repository>(provider =>
        {
            var context = provider.GetService<IFirestoreContext<Entities.{{project_class_name}}>>();
            if (context == null)
            {
                throw new InvalidOperationException("Firestore Context is null.");
            }
            return new {{project_class_name}}Repository(context);
        });

        return services;
    }
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
    public static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        string tableName = Environment.GetEnvironmentVariable("DYNAMODB_TABLE_NAME") ?? string.Empty;

        if (string.IsNullOrEmpty(tableName))
        {
            throw new InvalidOperationException("DynamoDB table name configuration is missing. Set the DYNAMODB_TABLE_NAME environment variable.");
        }

        services.AddSingleton<IAmazonDynamoDB, AmazonDynamoDBClient>();

        services.AddSingleton<I{{project_class_name}}Repository>(provider =>
        {
            var dynamoClient = provider.GetRequiredService<IAmazonDynamoDB>();
            return new {{project_class_name}}Repository(
                dynamoClient,
                tableName
            );
        });

        return services;
    }
{%- endif %}
}
