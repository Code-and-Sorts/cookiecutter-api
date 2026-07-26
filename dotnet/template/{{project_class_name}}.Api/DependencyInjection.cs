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
{%- for resource in resources %}
{%- set r = resource.name %}

        services.AddSingleton<I{{ r }}Controller>(provider =>
        {
            var cosmosClient = provider.GetRequiredService<CosmosClient>();
            string containerName = configuration.GetValue<string>("CosmosDbContainerName_{{ resource.container | to_camel }}") ?? "{{ resource.container }}";
            var repository = new {{ r }}Repository(cosmosClient, databaseName, containerName);
            return new {{ r }}Controller(new {{ r }}Service(repository));
        });
{%- endfor %}

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
{%- for resource in resources %}
{%- set r = resource.name %}

        services.AddSingleton<I{{ r }}Controller>(provider =>
        {
            var firestoreDb = provider.GetRequiredService<FirestoreDb>();
            string collectionName = configuration.GetValue<string>("FIRESTORE_COLLECTION_{{ resource.container | to_camel }}") ?? "{{ resource.container }}";
            var context = new FirestoreContext<Entities.{{ r }}>(firestoreDb, collectionName);
            var repository = new {{ r }}Repository(context);
            return new {{ r }}Controller(new {{ r }}Service(repository));
        });
{%- endfor %}

        return services;
    }
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
    public static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        services.AddSingleton<IAmazonDynamoDB, AmazonDynamoDBClient>();
{%- for resource in resources %}
{%- set r = resource.name %}

        services.AddSingleton<I{{ r }}Controller>(provider =>
        {
            var dynamoClient = provider.GetRequiredService<IAmazonDynamoDB>();
            string tableName = Environment.GetEnvironmentVariable("DYNAMODB_TABLE_NAME_{{ resource.container | upper | replace('-', '_') }}") ?? "{{ resource.container }}";
            var repository = new {{ r }}Repository(dynamoClient, tableName);
            return new {{ r }}Controller(new {{ r }}Service(repository));
        });
{%- endfor %}

        return services;
    }
{%- endif %}
}
