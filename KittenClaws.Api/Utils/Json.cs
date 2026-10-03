namespace KittenClaws.Api.Utils;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

public static class Json
{
    public static readonly JsonSerializerOptions Options = CreateOptions();

    public static string Serialize(object value) => JsonSerializer.Serialize(value, value.GetType(), Options);

    public static bool IsWholeNumber(double value) => Math.Floor(value) == value && Math.Abs(value) < long.MaxValue;

    // Every field the entity's type stores, by name, with no value for one that is unset, so a write can remove it.
    public static IEnumerable<(string Name, JsonElement? Value)> StoredFields<T>(T entity)
    {
        var json = JsonSerializer.SerializeToElement(entity, Options);
        return Options.GetTypeInfo(typeof(T)).Properties
            .Select(property => (property.Name, json.TryGetProperty(property.Name, out var value) ? value : (JsonElement?)null));
    }

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new WholeNumberConverter() },
        };
        options.MakeReadOnly(populateMissingResolver: true);
        return options;
    }

    // JSON clients cannot always tell 1 from 1.0, so an integer may be written with a zero fraction or an exponent.
    private sealed class WholeNumberConverter : JsonConverter<long>
    {
        public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            reader.TryGetInt64(out var value) ? value : (long)reader.GetDouble();

        public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options) => writer.WriteNumberValue(value);
    }
}
