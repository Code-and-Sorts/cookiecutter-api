namespace KittenClaws.Api.Tests.Unit;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;
using KittenClaws.Api;
using KittenClaws.Api.Interfaces;

public class OpenApiSpecTests
{
    private static readonly Assembly Api = typeof(DependencyInjection).Assembly;
    private static readonly JsonElement Spec = JsonDocument.Parse(File.ReadAllText("openapi.json")).RootElement;
    private const string ItemId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
    private const string NotFoundBody = "{\"errorMessage\":\"Not found.\"}";

    [Fact]
    public async Task Spec_ListsExactlyTheRoutesTheFunctionServes()
    {
        var services = new ServiceCollection();
        services.AddSingleton(Substitute.For<ICatController>());
        services.AddSingleton(Substitute.For<IDogController>());
        services.AddHandlers();
        using var provider = services.BuildServiceProvider();
        var handlers = provider.GetServices<IResourceHandler>().ToList();
        var function = new Function(handlers, NullLogger<Function>.Instance);

        var routes = new List<string>();
        foreach (var endpoint in handlers.Select(handler => handler.Endpoint))
        {
            foreach (var path in new[] { $"/{endpoint}", $"/{endpoint}/{ItemId}" })
            {
                foreach (var method in new[] { "GET", "POST", "PUT", "PATCH", "DELETE" })
                {
                    var httpContext = Mocks.CreateHttpContext(method, path, body: "{\"name\":\"mockName\"}");
                    await function.HandleAsync(httpContext);
                    var status = httpContext.Response.StatusCode;
                    if (status != 405 && !(status == 404 && Mocks.ReadResponseBody(httpContext) == NotFoundBody))
                    {
                        routes.Add($"{method} {path.Replace(ItemId, "{id}")}");
                    }
                }
            }
        }

        Assert.Equal(SpecRoutes(), Sorted(routes));
    }

    [Fact]
    public void Spec_HasARequestSchemaPerRequestType()
    {
        var specRequests = Spec.GetProperty("components").GetProperty("schemas").EnumerateObject()
            .Select(schema => schema.Name)
            .Where(name => name.EndsWith("Request", StringComparison.Ordinal));

        Assert.Equal(Sorted(specRequests), Sorted(RequestTypes().Select(SpecName)));
    }

    [Fact]
    public void RequestSchemas_MatchTheValidators()
    {
        var schemas = Spec.GetProperty("components").GetProperty("schemas");
        foreach (var type in RequestTypes())
        {
            var schema = schemas.GetProperty(SpecName(type));
            var properties = schema.GetProperty("properties");
            var fields = JsonFields(type);
            var required = schema.TryGetProperty("required", out var names) ? names.EnumerateArray().Select(name => name.GetString()!) : [];

            Assert.False(schema.GetProperty("additionalProperties").GetBoolean());
            Assert.Equal(Sorted(properties.EnumerateObject().Select(property => property.Name)), Sorted(fields.Keys));
            Assert.Equal(Sorted(required), Sorted(fields.Where(field => Rejects(type, field.Value, null)).Select(field => field.Key)));
            foreach (var (name, field) in fields)
            {
                var minLength = properties.GetProperty(name).TryGetProperty("minLength", out var length) ? length.GetInt32() : 0;
                Assert.Equal(minLength > 0, Rejects(type, field, string.Empty));
            }
        }
    }

    private static List<string> SpecRoutes() =>
        Sorted(Spec.GetProperty("paths").EnumerateObject().SelectMany(path => path.Value.EnumerateObject()
            .Where(operation => operation.Name != "parameters")
            .Select(operation => $"{operation.Name.ToUpperInvariant()} {path.Name}")));

    private static List<string> Sorted(IEnumerable<string> values) => values.Order(StringComparer.Ordinal).ToList();

    private static IEnumerable<Type> RequestTypes() =>
        Api.GetTypes().Where(type => type.Namespace == "KittenClaws.Api.Requests" && type.Name.EndsWith("Request", StringComparison.Ordinal));

    private static string SpecName(Type type)
    {
        var operation = new[] { "Create", "Update", "Replace" }.Single(prefix => type.Name.StartsWith(prefix, StringComparison.Ordinal));
        return type.Name[operation.Length..^"Request".Length] + operation + "Request";
    }

    private static Dictionary<string, PropertyInfo> JsonFields(Type type) =>
        type.GetProperties()
            .Where(property => property.GetCustomAttribute<JsonIgnoreAttribute>() == null)
            .Select(property => (Property: property, Attribute: property.GetCustomAttribute<JsonPropertyNameAttribute>()))
            .Where(field => field.Attribute != null)
            .ToDictionary(field => field.Attribute!.Name, field => field.Property);

    private static bool Rejects(Type type, PropertyInfo field, string? value)
    {
        var request = Activator.CreateInstance(type)!;
        foreach (var other in JsonFields(type).Values)
        {
            other.SetValue(request, "mockName");
        }
        field.SetValue(request, value);
        var validator = (IValidator)Activator.CreateInstance(Api.GetType($"KittenClaws.Api.Validation.{type.Name}Validator", throwOnError: true)!)!;
        return !validator.Validate(new ValidationContext<object>(request)).IsValid;
    }
}
