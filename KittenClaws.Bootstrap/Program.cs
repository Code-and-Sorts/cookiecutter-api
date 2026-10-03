
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using KittenClaws.Api;

var timeout = TimeSpan.FromMinutes(2);
var maxDelay = TimeSpan.FromSeconds(8);
var configuration = new ConfigurationBuilder().AddEnvironmentVariables().Build();
var storeNames = DependencyInjection.StoreNames(configuration);

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
