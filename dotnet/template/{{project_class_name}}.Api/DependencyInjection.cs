namespace {{project_class_name}}.Api;

using System;
using System.Collections.Generic;
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
        // Controllers are wired per storage container in AddPersistence and
        // resolved by the functions via IReadOnlyDictionary<string, IItemController>.
        return services;
    }

{%- if cloud_service == 'Azure Function App' %}
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        string cosmosConnectionString = configuration.GetConnectionString("CosmosDb") ?? string.Empty;
        string databaseName = configuration.GetValue<string>("CosmosDbDatabaseName") ?? string.Empty;

        if (string.IsNullOrEmpty(cosmosConnectionString) || string.IsNullOrEmpty(databaseName))
        {
            throw new InvalidOperationException("CosmosDb configuration is missing or incomplete.");
        }

        services.AddSingleton(provider => new CosmosClient(cosmosConnectionString));

        services.AddSingleton<IReadOnlyDictionary<string, IItemController>>(provider =>
        {
            var cosmosClient = provider.GetRequiredService<CosmosClient>();
            var controllers = new Dictionary<string, IItemController>();
{%- for container in resources | map(attribute='container') | unique %}
            {
                string containerName = configuration.GetValue<string>("CosmosDbContainerName_{{ container | to_camel }}") ?? "{{ container }}";
                var repository = new ItemRepository(cosmosClient, databaseName, containerName);
                controllers["{{ container }}"] = new ItemController(new ItemService(repository));
            }
{%- endfor %}
            return controllers;
        });

        return services;
    }
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
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

        services.AddSingleton<IReadOnlyDictionary<string, IItemController>>(provider =>
        {
            var firestoreDb = provider.GetRequiredService<FirestoreDb>();
            var controllers = new Dictionary<string, IItemController>();
{%- for container in resources | map(attribute='container') | unique %}
            {
                string collectionName = configuration.GetValue<string>("FIRESTORE_COLLECTION_{{ container | to_camel }}") ?? "{{ container }}";
                var context = new FirestoreContext<Entities.Item>(firestoreDb, collectionName);
                var repository = new ItemRepository(context);
                controllers["{{ container }}"] = new ItemController(new ItemService(repository));
            }
{%- endfor %}
            return controllers;
        });

        return services;
    }
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
    public static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        services.AddSingleton<IAmazonDynamoDB, AmazonDynamoDBClient>();

        services.AddSingleton<IReadOnlyDictionary<string, IItemController>>(provider =>
        {
            var dynamoClient = provider.GetRequiredService<IAmazonDynamoDB>();
            var controllers = new Dictionary<string, IItemController>();
{%- for container in resources | map(attribute='container') | unique %}
            {
                string tableName = Environment.GetEnvironmentVariable("DYNAMODB_TABLE_NAME_{{ container | upper | replace('-', '_') }}") ?? "{{ container }}";
                var repository = new ItemRepository(dynamoClient, tableName);
                controllers["{{ container }}"] = new ItemController(new ItemService(repository));
            }
{%- endfor %}
            return controllers;
        });

        return services;
    }
{%- endif %}
}
