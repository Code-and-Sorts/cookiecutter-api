namespace {{project_class_name}}.Api.Utils;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;

// Binding gives a property the body left out and one it set to null the same value, so requests record what was sent.
public interface ISentFields
{
    HashSet<string> Sent { get; }
}

public static class RequestBody
{
    private static readonly ConcurrentDictionary<Type, IReadOnlyDictionary<string, Type>> FieldsByType = new();

    // Values are never converted, so "1" is not an integer and 1 is not a string; 1.0 is an integer, as JavaScript sees it.
    private static readonly Dictionary<Type, (Func<JsonElement, bool> Accepts, string Single, string Plural)> ValueTypes = new()
    {
        { typeof(string), (value => value.ValueKind == JsonValueKind.String, "a string", "strings") },
        { typeof(long), (value => value.ValueKind == JsonValueKind.Number && (value.TryGetInt64(out _) || (value.TryGetDouble(out var number) && Json.IsWholeNumber(number))), "an integer", "integers") },
        { typeof(double), (value => value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out var number) && double.IsFinite(number), "a number", "numbers") },
        { typeof(bool), (value => value.ValueKind is JsonValueKind.True or JsonValueKind.False, "true or false", "booleans") },
    };

    public static async Task<TRequest> ReadValidAsync<TRequest, TValidator>(Stream body, CancellationToken ct = default)
        where TValidator : IValidator<TRequest>, new()
    {
        var request = await DeserializeAsync<TRequest>(body, ct);
        var result = await new TValidator().ValidateAsync(request, ct);
        if (!result.IsValid)
        {
            throw new BadRequestException(string.Join(" ", result.Errors.Select(failure => failure.ErrorMessage).Distinct()));
        }
        return request;
    }

    public static async Task<T> DeserializeAsync<T>(Stream body, CancellationToken ct = default)
    {
        JsonDocument document;
        try
        {
            document = await JsonDocument.ParseAsync(body, default, ct);
        }
        catch (JsonException)
        {
            throw new BadRequestException("Request body must be valid JSON.");
        }

        using (document)
        {
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                throw new BadRequestException("Request body must be a JSON object.");
            }

            var fields = FieldsByType.GetOrAdd(typeof(T), GetFields);
            foreach (var property in root.EnumerateObject())
            {
                if (!fields.TryGetValue(property.Name, out var fieldType))
                {
                    throw new BadRequestException($"Unknown field: {property.Name}.");
                }
                // Validators decide whether null is allowed.
                if (property.Value.ValueKind != JsonValueKind.Null && !Matches(fieldType, property.Value))
                {
                    throw new BadRequestException($"{property.Name} must be {Describe(fieldType)}.");
                }
            }

            T request;
            try
            {
                request = root.Deserialize<T>(Json.Options)
                    ?? throw new BadRequestException("Request body must be a JSON object.");
            }
            catch (JsonException ex)
            {
                throw new BadRequestException($"{ex.Path?.TrimStart('$', '.')} has an invalid value.");
            }

            if (request is ISentFields sent)
            {
                sent.Sent.UnionWith(root.EnumerateObject().Select(property => property.Name));
            }
            return request;
        }
    }

    private static bool Matches(Type type, JsonElement value)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        return IsList(type)
            ? value.ValueKind == JsonValueKind.Array && value.EnumerateArray().All(item => Matches(type.GetGenericArguments()[0], item))
            : ValueTypes[type].Accepts(value);
    }

    private static string Describe(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        return IsList(type) ? $"a list of {ValueTypes[type.GetGenericArguments()[0]].Plural}" : ValueTypes[type].Single;
    }

    private static bool IsList(Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);

    private static IReadOnlyDictionary<string, Type> GetFields(Type type) =>
        type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(property => (Property: property, Attribute: property.GetCustomAttribute<JsonPropertyNameAttribute>()))
            .Where(field => field.Attribute != null && field.Property.GetCustomAttribute<JsonIgnoreAttribute>() == null)
            .ToDictionary(field => field.Attribute!.Name, field => field.Property.PropertyType, StringComparer.Ordinal);
}
