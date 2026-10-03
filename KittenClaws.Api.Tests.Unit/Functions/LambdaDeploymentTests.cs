namespace KittenClaws.Api.Tests.Unit;

using System;
using System.IO;
using System.Linq;
using Amazon.DynamoDBv2;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;
using KittenClaws.Api;
using KittenClaws.Api.Functions;

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

        Assert.Equal(6, handlers.Count);
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

        Assert.Equal(6, paths.Count);
        Assert.All(paths, path => Assert.DoesNotContain("/api/", path));
        Assert.Contains("/kittenclaws/{id}", paths);
    }

    [Theory]
    [InlineData(typeof(HealthFunctions))]
    [InlineData(typeof(KittenClawsFunctions))]
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
