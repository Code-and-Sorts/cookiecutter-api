namespace {{project_class_name}}.Api.Tests.Unit;

using System;
using System.IO;
using System.Linq;
using Amazon.DynamoDBv2;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;
using {{project_class_name}}.Api;
using {{project_class_name}}.Api.Functions;

public class LambdaDeploymentTests
{
    [Fact]
    public void TemplateHandlers_PointAtGeneratedLambdaHandlers()
    {
        var handlers = File.ReadLines("template.yaml")
            .Select(line => line.Trim())
            .Where(line => line.StartsWith("Handler: "))
            .Select(line => line["Handler: ".Length..].Split("::"))
            .ToList();

        Assert.Equal({{ (resources | map(attribute='operations') | map('length') | sum) + (2 if health_endpoint else 1) }}, handlers.Count);
        foreach (var handler in handlers)
        {
            Assert.Equal(typeof(Startup).Assembly.GetName().Name, handler[0]);
            var handlerType = typeof(Startup).Assembly.GetType(handler[1]);
            Assert.NotNull(handlerType);
            Assert.NotNull(handlerType.GetConstructor(Type.EmptyTypes));
            Assert.NotNull(handlerType.GetMethod(handler[2]));
        }
    }

    [Fact]
    public void TemplatePaths_HaveNoPrefixAndUseIdParameter()
    {
        var paths = File.ReadLines("template.yaml")
            .Select(line => line.Trim())
            .Where(line => line.StartsWith("Path: "))
            .Select(line => line["Path: ".Length..])
            .ToList();

        Assert.Equal({{ (resources | map(attribute='operations') | map('length') | sum) + (2 if health_endpoint else 1) }}, paths.Count);
        Assert.All(paths, path => Assert.DoesNotContain("/api/", path));
        Assert.Contains("/{{ resources[0].endpoint }}{{ '/{id}' if (resources[0].operations | reject('in', ['list', 'create']) | list) else '' }}", paths);
    }

    [Theory]
{%- if health_endpoint %}
    [InlineData(typeof(HealthFunctions))]
{%- endif %}
    [InlineData(typeof(OpenApiFunctions))]
{%- for resource in resources %}
    [InlineData(typeof({{ resource.name }}Functions))]
{%- endfor %}
    public void Startup_ResolvesFunctions(Type functionsType)
    {
        var services = new ServiceCollection();
        new Startup().ConfigureServices(services);
        services.AddSingleton(Substitute.For<IAmazonDynamoDB>());
        services.AddSingleton(functionsType);
        using var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetRequiredService(functionsType));
    }
}
