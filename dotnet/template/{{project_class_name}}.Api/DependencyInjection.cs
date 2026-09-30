namespace {{project_class_name}}.Api;

using System;
using {{project_class_name}}.Api.Controllers;
{%- if cloud_service == 'GCP Cloud Function' %}
using {{project_class_name}}.Api.Handlers;
{%- endif %}
using {{project_class_name}}.Api.Interfaces;
using {{project_class_name}}.Api.Repositories;
using {{project_class_name}}.Api.Services;
{%- if cloud_service == 'Azure Function App' %}
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
using Google.Api.Gax;
using Google.Api.Gax.Grpc;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
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
{%- if cloud_service == 'GCP Cloud Function' %}

    public static IServiceCollection AddHandlers(this IServiceCollection services)
    {
{%- for resource in resources %}
        services.AddSingleton<IResourceHandler, {{ resource.name }}Handler>();
{%- endfor %}
{%- if health_endpoint %}
        services.AddSingleton<IResourceHandler, HealthHandler>();
{%- endif %}

        return services;
    }
{%- endif %}

{%- if cloud_service == 'Azure Function App' %}

    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        string cosmosConnectionString = configuration.GetConnectionString("CosmosDb") ?? string.Empty;
        string databaseName = configuration.GetValue<string>("CosmosDbDatabaseName") ?? string.Empty;

        if (string.IsNullOrEmpty(cosmosConnectionString) || string.IsNullOrEmpty(databaseName))
        {
            throw new InvalidOperationException("CosmosDb configuration is missing or incomplete.");
        }

        // Direct mode (the SDK default) suits Azure; the Linux Cosmos DB emulator only
        // supports Gateway mode, so local.settings.json sets CosmosDbConnectionMode=Gateway.
        string connectionModeSetting = configuration.GetValue<string>("CosmosDbConnectionMode") ?? nameof(ConnectionMode.Direct);
        if (!Enum.TryParse(connectionModeSetting, ignoreCase: true, out ConnectionMode connectionMode) || !Enum.IsDefined(connectionMode))
        {
            throw new InvalidOperationException($"CosmosDbConnectionMode '{connectionModeSetting}' is not valid. Use Direct or Gateway.");
        }

        // Bound each call so a failing database answers well inside the platform timeout
        // (each request also has a RequestDeadline).
        var cosmosOptions = new CosmosClientOptions
        {
            ConnectionMode = connectionMode,
            RequestTimeout = TimeSpan.FromSeconds(5),
            MaxRetryAttemptsOnRateLimitedRequests = 3,
            MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(3),
        };
        services.AddSingleton(provider => new CosmosClient(cosmosConnectionString, cosmosOptions));
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

        // Bound each call so a failing database answers well inside the platform timeout
        // (each request also has a RequestDeadline).
        services.AddSingleton(provider =>
            new FirestoreDbBuilder
            {
                ProjectId = projectId,
                DatabaseId = databaseId,
                EmulatorDetection = EmulatorDetection.EmulatorOrProduction,
                Settings = new FirestoreSettings { CallSettings = CallSettings.FromExpiration(Expiration.FromTimeout(TimeSpan.FromSeconds(5))) },
            }.Build()
        );
{%- for resource in resources %}
{%- set r = resource.name %}

        services.AddSingleton<I{{ r }}Controller>(provider =>
        {
            var firestoreDb = provider.GetRequiredService<FirestoreDb>();
            string collectionName = configuration.GetValue<string>("FIRESTORE_COLLECTION_{{ resource.container | upper | replace('-', '_') }}") ?? "{{ resource.container }}";
            var context = new FirestoreContext<Entities.{{ r }}Entity>(firestoreDb, collectionName);
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
        // Bound each call and its retries so a failing database answers well inside the
        // Lambda timeout (each request also has a RequestDeadline).
        services.AddSingleton<IAmazonDynamoDB>(_ => new AmazonDynamoDBClient(new AmazonDynamoDBConfig
        {
            MaxErrorRetry = 2,
            Timeout = TimeSpan.FromSeconds(3),
        }));
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
