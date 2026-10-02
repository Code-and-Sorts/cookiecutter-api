namespace {{project_class_name}}.Api.Tests.Unit;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
{%- if cloud_service == 'GCP Cloud Function' %}
using System.Threading.Tasks;
{%- endif %}
using FluentValidation;
{%- if cloud_service == 'Azure Function App' %}
using Microsoft.Azure.Functions.Worker;
{%- elif cloud_service == 'GCP Cloud Function' %}
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
{%- endif %}
using Xunit;
using {{project_class_name}}.Api;
{%- if cloud_service == 'GCP Cloud Function' %}
using {{project_class_name}}.Api.Interfaces;
{%- endif %}

public class OpenApiSpecTests
{
    private static readonly Assembly Api = typeof(DependencyInjection).Assembly;
    private static readonly JsonElement Spec = JsonDocument.Parse(File.ReadAllText("openapi.json")).RootElement;
{%- if cloud_service == 'GCP Cloud Function' %}
    private const string ItemId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
    private const string NotFoundBody = "{\"errorMessage\":\"Not found.\"}";
{%- endif %}
{%- if cloud_service == 'Azure Function App' %}

    [Fact]
    public void Spec_ListsExactlyTheRegisteredRoutes()
    {
        var routes = Api.GetTypes()
            .SelectMany(type => type.GetMethods())
            .Where(method => method.GetCustomAttribute<FunctionAttribute>() != null)
            .SelectMany(method => method.GetParameters())
            .Select(parameter => parameter.GetCustomAttribute<HttpTriggerAttribute>())
            .OfType<HttpTriggerAttribute>()
            .SelectMany(trigger => (trigger.Methods ?? []).Select(method => $"{method.ToUpperInvariant()} /{trigger.Route}"));

        Assert.Equal(SpecRoutes(), Sorted(routes));
    }
{%- elif cloud_service == 'AWS Lambda' %}

    [Fact]
    public void Spec_ListsExactlyTheTemplateRoutes()
    {
        var lines = File.ReadLines("template.yaml").Select(line => line.Trim()).ToList();
        var routes = lines
            .Select((line, index) => (Line: line, Next: index + 1 < lines.Count ? lines[index + 1] : string.Empty))
            .Where(pair => pair.Line.StartsWith("Path: "))
            .Select(pair =>
            {
                Assert.StartsWith("Method: ", pair.Next);
                return $"{pair.Next["Method: ".Length..].ToUpperInvariant()} {pair.Line["Path: ".Length..]}";
            });

        Assert.Equal(SpecRoutes(), Sorted(routes));
    }
{%- else %}

    [Fact]
    public async Task Spec_ListsExactlyTheRoutesTheFunctionServes()
    {
        var services = new ServiceCollection();
{%- for resource in resources %}
        services.AddSingleton(Substitute.For<I{{ resource.name }}Controller>());
{%- endfor %}
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
{%- endif %}

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
        Api.GetTypes().Where(type => type.Namespace == "{{project_class_name}}.Api.Requests" && type.Name.EndsWith("Request", StringComparison.Ordinal));

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
        var validator = (IValidator)Activator.CreateInstance(Api.GetType($"{{project_class_name}}.Api.Validation.{type.Name}Validator", throwOnError: true)!)!;
        return !validator.Validate(new ValidationContext<object>(request)).IsValid;
    }
}
