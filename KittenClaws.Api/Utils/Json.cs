namespace KittenClaws.Api.Utils;

using System.Text.Json;
using System.Text.Json.Serialization;

public static class Json
{
    public static readonly JsonSerializerOptions Options = CreateOptions();

    public static string Serialize(object value) => JsonSerializer.Serialize(value, value.GetType(), Options);

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };
        options.MakeReadOnly(populateMissingResolver: true);
        return options;
    }
}
