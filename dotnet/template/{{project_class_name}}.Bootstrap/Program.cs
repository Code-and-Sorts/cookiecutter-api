{%- if cloud_service == 'Azure Function App' %}
using Microsoft.Azure.Cosmos;
{%- elif cloud_service == 'AWS Lambda' %}
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
{%- endif %}
using Microsoft.Extensions.Configuration;
{%- if cloud_service != 'GCP Cloud Function' %}
using Microsoft.Extensions.DependencyInjection;
{%- endif %}
using {{project_class_name}}.Api;

// Prepares the local emulator for the API (make emulator-seed); it is safe to run again.
var timeout = TimeSpan.FromMinutes(2);
var maxDelay = TimeSpan.FromSeconds(8);
var configuration = new ConfigurationBuilder().AddEnvironmentVariables().Build();
var storeNames = DependencyInjection.StoreNames(configuration);
{%- if cloud_service == 'Azure Function App' %}

if (!configuration.GetValue<bool>("CosmosDbEmulator"))
{
    Console.Error.WriteLine("CosmosDbEmulator is not true; refusing to create containers outside the emulator.");
    return 1;
}
using var provider = new ServiceCollection().AddPersistence(configuration).BuildServiceProvider();
var client = provider.GetRequiredService<CosmosClient>();
string databaseName = configuration["CosmosDbDatabaseName"]!;

async Task Bootstrap(CancellationToken ct)
{
    Database database = await client.CreateDatabaseIfNotExistsAsync(databaseName, cancellationToken: ct);
    foreach (var name in storeNames)
    {
        // Every repository reads and writes items by id, so id is the partition key.
        await database.CreateContainerIfNotExistsAsync(name, "/id", cancellationToken: ct);
        Console.WriteLine($"Container {databaseName}/{name} is ready.");
    }
}
{%- elif cloud_service == 'GCP Cloud Function' %}

string? host = configuration["FIRESTORE_EMULATOR_HOST"];
if (string.IsNullOrEmpty(host))
{
    Console.Error.WriteLine("FIRESTORE_EMULATOR_HOST is not set; refusing to run outside the emulator.");
    return 1;
}
using var http = new HttpClient(new HttpClientHandler { UseProxy = false }) { Timeout = TimeSpan.FromSeconds(5) };

// Firestore creates collections on first write, so a reachable emulator is all the API needs.
async Task Bootstrap(CancellationToken ct)
{
    string body = await http.GetStringAsync($"http://{host}/", ct);
    if (body.Trim() != "Ok")
    {
        throw new InvalidOperationException($"{host} is not a Firestore emulator.");
    }
    Console.WriteLine($"Firestore emulator at {host} is ready for project {configuration["GCP_PROJECT_ID"]} (collections: {string.Join(", ", storeNames)}).");
}
{%- else %}

if (string.IsNullOrEmpty(configuration["AWS_ENDPOINT_URL_DYNAMODB"]))
{
    Console.Error.WriteLine("AWS_ENDPOINT_URL_DYNAMODB is not set; refusing to create tables outside the emulator.");
    return 1;
}
using var provider = new ServiceCollection().AddPersistence(configuration).BuildServiceProvider();
var client = provider.GetRequiredService<IAmazonDynamoDB>();

async Task Bootstrap(CancellationToken ct)
{
    foreach (var name in storeNames)
    {
        try
        {
            // Matches the tables in template.yaml.
            await client.CreateTableAsync(new CreateTableRequest
            {
                TableName = name,
                AttributeDefinitions = [new AttributeDefinition("id", ScalarAttributeType.S)],
                KeySchema = [new KeySchemaElement("id", KeyType.HASH)],
                BillingMode = BillingMode.PAY_PER_REQUEST,
            }, ct);
        }
        catch (ResourceInUseException)
        {
        }
        while ((await client.DescribeTableAsync(name, ct)).Table.TableStatus != TableStatus.ACTIVE)
        {
            await Task.Delay(TimeSpan.FromSeconds(1), ct);
        }
        Console.WriteLine($"Table {name} is ready.");
    }
}
{%- endif %}

using var deadline = new CancellationTokenSource(timeout);
for (var delay = TimeSpan.FromSeconds(1); ; delay = TimeSpan.FromTicks(Math.Min(delay.Ticks * 2, maxDelay.Ticks)))
{
    try
    {
        await Bootstrap(deadline.Token);
        return 0;
    }
    catch (Exception error) when (!deadline.IsCancellationRequested)
    {
        Console.Error.WriteLine($"Waiting for the emulator ({error.GetType().Name}); retrying in {delay.TotalSeconds} s.");
        try
        {
            await Task.Delay(delay, deadline.Token);
        }
        catch (OperationCanceledException)
        {
            break;
        }
    }
    catch (Exception error)
    {
        Console.Error.WriteLine($"The emulator was not ready within {timeout.TotalSeconds} s: {error.Message}");
        return 1;
    }
}
Console.Error.WriteLine($"The emulator was not ready within {timeout.TotalSeconds} s.");
return 1;
