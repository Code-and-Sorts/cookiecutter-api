
using Microsoft.Extensions.Configuration;
using KittenClaws.Api;

var timeout = TimeSpan.FromMinutes(2);
var maxDelay = TimeSpan.FromSeconds(8);
var configuration = new ConfigurationBuilder().AddEnvironmentVariables().Build();
var storeNames = DependencyInjection.StoreNames(configuration);

string? host = configuration["FIRESTORE_EMULATOR_HOST"];
if (string.IsNullOrEmpty(host))
{
    Console.Error.WriteLine("FIRESTORE_EMULATOR_HOST is not set; refusing to run outside the emulator.");
    return 1;
}
using var http = new HttpClient(new HttpClientHandler { UseProxy = false }) { Timeout = TimeSpan.FromSeconds(5) };

async Task Bootstrap(CancellationToken ct)
{
    string body = await http.GetStringAsync($"http://{host}/", ct);
    if (body.Trim() != "Ok")
    {
        throw new InvalidOperationException($"{host} is not a Firestore emulator.");
    }
    Console.WriteLine($"Firestore emulator at {host} is ready for project {configuration["GCP_PROJECT_ID"]} (collections: {string.Join(", ", storeNames)}).");
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
