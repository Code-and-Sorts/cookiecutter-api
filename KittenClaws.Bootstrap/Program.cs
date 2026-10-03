
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using KittenClaws.Api;

var timeout = TimeSpan.FromMinutes(2);
var maxDelay = TimeSpan.FromSeconds(8);
var configuration = new ConfigurationBuilder().AddEnvironmentVariables().Build();
var storeNames = DependencyInjection.StoreNames(configuration);

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
        await database.CreateContainerIfNotExistsAsync(name, "/id", cancellationToken: ct);
        Console.WriteLine($"Container {databaseName}/{name} is ready.");
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
