namespace {{project_class_name}}.Api.Utils;

using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

internal static class RequestBody
{
    public static async Task<T> DeserializeAsync<T>(Stream body)
    {
        using var reader = new StreamReader(body);
        var bodyString = await reader.ReadToEndAsync();
        return JsonConvert.DeserializeObject<T>(bodyString) ?? throw new JsonSerializationException("Deserialization returned null.");
    }
}
