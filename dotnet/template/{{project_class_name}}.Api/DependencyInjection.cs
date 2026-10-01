{%- set key_attr = "container_class" if cloud_service == 'Azure Function App' else "env_key" -%}
namespace {{project_class_name}}.Api;

using System;
using {{project_class_name}}.Api.Controllers;
using {{project_class_name}}.Api.Entities;
{%- if cloud_service == 'GCP Cloud Function' %}
using {{project_class_name}}.Api.Handlers;
{%- endif %}
using {{project_class_name}}.Api.Interfaces;
using {{project_class_name}}.Api.Repositories;
using {{project_class_name}}.Api.Services;
{%- if cloud_service == 'Azure Function App' %}
using System.Data.Common;
using {{project_class_name}}.Api.Utils;
using Microsoft.Azure.Cosmos;
{%- elif cloud_service == 'GCP Cloud Function' %}
using Google.Api.Gax;
using Google.Api.Gax.Grpc;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
{%- else %}
using Amazon.DynamoDBv2;
{%- endif %}
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
{%- if cloud_service == 'Azure Function App' %}
    private const string ContainerSetting = "CosmosDbContainerName_";
{%- elif cloud_service == 'GCP Cloud Function' %}
    private const string ContainerSetting = "FIRESTORE_COLLECTION_";
{%- else %}
    private const string ContainerSetting = "DYNAMODB_TABLE_NAME_";
{%- endif %}

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

    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDatabase(configuration);
{%- for resource in path_resources %}
{%- set r = resource.name %}
        services.AddSingleton<I{{ r }}Controller>(provider => new {{ r }}Controller(new {{ r }}Service(new {{ r }}Repository(
            CreateStore<{{ r }}Entity>(provider, ContainerName(configuration, "{{ resource[key_attr] }}", "{{ resource.container }}"))))));
{%- endfor %}

        return services;
    }

    /// <summary>The distinct store names the resources use, for the emulator bootstrap.</summary>
    public static IReadOnlyList<string> StoreNames(IConfiguration configuration) =>
    [
        ..new SortedSet<string>(StringComparer.Ordinal)
        {
{%- for c in path_resources | unique(attribute='container') %}
            ContainerName(configuration, "{{ c[key_attr] }}", "{{ c.container }}"),
{%- endfor %}
        },
    ];

    private static string ContainerName(IConfiguration configuration, string settingKey, string fallback) =>
        configuration[ContainerSetting + settingKey] ?? fallback;
{%- if cloud_service == 'Azure Function App' %}

    private static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        string cosmosConnectionString = configuration.GetConnectionString("CosmosDb") ?? string.Empty;
        string databaseName = configuration.GetValue<string>("CosmosDbDatabaseName") ?? string.Empty;

        if (string.IsNullOrEmpty(cosmosConnectionString) || string.IsNullOrEmpty(databaseName))
        {
            throw new InvalidOperationException("CosmosDb configuration is missing or incomplete.");
        }

        var cosmosOptions = CreateCosmosClientOptions(configuration, cosmosConnectionString);
        services.AddSingleton(provider => new CosmosClient(cosmosConnectionString, cosmosOptions));
        services.AddSingleton(provider => provider.GetRequiredService<CosmosClient>().GetDatabase(databaseName));
    }

    public static CosmosClientOptions CreateCosmosClientOptions(IConfiguration configuration, string connectionString)
    {
        // The Linux Cosmos DB emulator only supports Gateway mode, so local.settings.json sets it.
        string connectionModeSetting = configuration.GetValue<string>("CosmosDbConnectionMode") ?? nameof(ConnectionMode.Direct);
        if (!Enum.TryParse(connectionModeSetting, ignoreCase: true, out ConnectionMode connectionMode) || !Enum.IsDefined(connectionMode))
        {
            throw new InvalidOperationException($"CosmosDbConnectionMode '{connectionModeSetting}' is not valid. Use Direct or Gateway.");
        }

        // Bounded so a failing database answers well inside the platform timeout.
        var options = new CosmosClientOptions
        {
            ConnectionMode = connectionMode,
            RequestTimeout = TimeSpan.FromSeconds(5),
            MaxRetryAttemptsOnRateLimitedRequests = 3,
            MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(3),
            UseSystemTextJsonSerializerWithOptions = Json.Options,
        };
        if (!configuration.GetValue<bool>("CosmosDbEmulator"))
        {
            return options;
        }

        options.ConnectionMode = ConnectionMode.Gateway;
        // The emulator advertises its own address, which discovery would use instead of the configured one.
        options.LimitToEndpoint = true;
        var connection = new DbConnectionStringBuilder { ConnectionString = connectionString };
        if (connection.TryGetValue("AccountEndpoint", out object? endpoint)
            && endpoint?.ToString()?.StartsWith("https://", StringComparison.OrdinalIgnoreCase) == true)
        {
            // Only an emulator serving HTTPS gets here; its certificate is self-signed.
            options.ServerCertificateCustomValidationCallback = (_, _, _) => true;
        }
        return options;
    }

    private static IDocumentStore<T> CreateStore<T>(IServiceProvider provider, string containerName) where T : BaseEntity, new() =>
        new CosmosDocumentStore<T>(provider.GetRequiredService<Database>().GetContainer(containerName));
{%- elif cloud_service == 'GCP Cloud Function' %}

    private static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        string projectId = configuration.GetValue<string>("GCP_PROJECT_ID") ?? string.Empty;
        string databaseId = configuration.GetValue<string>("FIRESTORE_DATABASE") ?? "(default)";

        if (string.IsNullOrEmpty(projectId))
        {
            throw new InvalidOperationException("Firestore configuration is missing or incomplete.");
        }

        services.AddSingleton(provider => CreateFirestoreDbBuilder(projectId, databaseId).Build());
    }

    public static FirestoreDbBuilder CreateFirestoreDbBuilder(string projectId, string databaseId) => new()
    {
        ProjectId = projectId,
        DatabaseId = databaseId,
        // Uses FIRESTORE_EMULATOR_HOST when it is set; the builder ignores it otherwise.
        EmulatorDetection = EmulatorDetection.EmulatorOrProduction,
        // Bounded so a failing database answers well inside the platform timeout.
        Settings = new FirestoreSettings { CallSettings = CallSettings.FromExpiration(Expiration.FromTimeout(TimeSpan.FromSeconds(5))) },
    };

    private static IDocumentStore<T> CreateStore<T>(IServiceProvider provider, string containerName) where T : BaseEntity, new() =>
        new FirestoreDocumentStore<T>(provider.GetRequiredService<FirestoreDb>(), containerName);
{%- else %}

    private static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var dynamoDbConfig = CreateDynamoDbConfig(configuration);
        services.AddSingleton<IAmazonDynamoDB>(_ => new AmazonDynamoDBClient(dynamoDbConfig));
    }

    /// <summary>The SDK reads a local emulator's endpoint from AWS_ENDPOINT_URL_DYNAMODB itself.</summary>
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
{%- endif %}
}
