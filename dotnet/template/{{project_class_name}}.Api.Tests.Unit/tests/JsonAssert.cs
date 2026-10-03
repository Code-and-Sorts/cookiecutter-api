namespace {{project_class_name}}.Api.Tests.Unit;

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using {{project_class_name}}.Api.Utils;
using Xunit;

// Compares values as the API writes them, so 1 and 1.0 or a list and its collection expression are equal.
public static class JsonAssert
{
    public static void Equal(string expectedJson, object? actual)
    {
        using var expected = JsonDocument.Parse(expectedJson);
        var actualJson = JsonSerializer.SerializeToElement(actual, Json.Options);
        Assert.True(JsonElement.DeepEquals(expected.RootElement, actualJson), $"Expected {expectedJson} but got {actualJson.GetRawText()}.");
    }

    public static void Contains(object expected, object actual)
    {
        var actualProperties = Properties(actual);
        foreach (var property in JsonSerializer.SerializeToElement(expected, Json.Options).EnumerateObject())
        {
            Assert.True(actualProperties.TryGetValue(property.Name, out var value), $"Missing {property.Name}.");
            Assert.True(JsonElement.DeepEquals(property.Value, value), $"{property.Name}: expected {property.Value.GetRawText()} but got {value.GetRawText()}.");
        }
    }

    public static Dictionary<string, JsonElement> Properties(object value) =>
        JsonSerializer.SerializeToElement(value, Json.Options).EnumerateObject().ToDictionary(property => property.Name, property => property.Value);
}
