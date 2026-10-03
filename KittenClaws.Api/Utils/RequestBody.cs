namespace KittenClaws.Api.Utils;

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

public static class RequestBody
{
    private static readonly ConcurrentDictionary<Type, IReadOnlyDictionary<string, Type>> FieldsByType = new();

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
                if (fieldType == typeof(string) && property.Value.ValueKind != JsonValueKind.String)
                {
                    throw new BadRequestException($"{property.Name} must be a string.");
                }
            }

            try
            {
                return root.Deserialize<T>(Json.Options)
                    ?? throw new BadRequestException("Request body must be a JSON object.");
            }
            catch (JsonException ex)
            {
                throw new BadRequestException($"{ex.Path?.TrimStart('$', '.')} has an invalid value.");
            }
        }
    }

    private static IReadOnlyDictionary<string, Type> GetFields(Type type) =>
        type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(property => (Property: property, Attribute: property.GetCustomAttribute<JsonPropertyNameAttribute>()))
            .Where(field => field.Attribute != null && field.Property.GetCustomAttribute<JsonIgnoreAttribute>() == null)
            .ToDictionary(field => field.Attribute!.Name, field => field.Property.PropertyType, StringComparer.Ordinal);
}
