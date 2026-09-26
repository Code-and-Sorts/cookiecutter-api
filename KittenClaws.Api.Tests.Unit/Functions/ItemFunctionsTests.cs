namespace KittenClaws.Api.Tests.Unit;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using KittenClaws.Api;
using KittenClaws.Api.Dtos;
using KittenClaws.Api.Functions;
using KittenClaws.Api.Interfaces;
using KittenClaws.Api.Requests;
using KittenClaws.Api.Utils;
using Newtonsoft.Json;

public class HealthFunctionTest
{
    [Fact]
    public void Health_ReturnsOk()
    {
        var functions = new ItemFunctions(
            Substitute.For<IKittenClawsController>(),
            Substitute.For<ILogger<ItemFunctions>>());

        var response = functions.Health(Mocks.CreateApiGatewayRequest());

        Assert.Equal(200, response.StatusCode);
        Assert.Contains("ok", response.Body);
    }
}

public class LambdaDeploymentTest
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
            Assert.Equal(typeof(ItemFunctions).Assembly.GetName().Name, handler[0]);
            var handlerType = typeof(ItemFunctions).Assembly.GetType(handler[1]);
            Assert.NotNull(handlerType);
            Assert.NotNull(handlerType.GetConstructor(Type.EmptyTypes));
            Assert.NotNull(handlerType.GetMethod(handler[2]));
        }
    }

    [Fact]
    public void Startup_ResolvesItemFunctions()
    {
        var services = new ServiceCollection();
        new Startup().ConfigureServices(services);
        services.AddSingleton(Substitute.For<IAmazonDynamoDB>());
        services.AddSingleton<ItemFunctions>();
        using var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetRequiredService<ItemFunctions>());
    }
}

public class KittenClawsFunctionsTest
{
    private const string ItemId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
    private readonly IKittenClawsController _mockKittenClawsController;
    private readonly ItemFunctions _functions;

    public KittenClawsFunctionsTest()
    {
        _mockKittenClawsController = Substitute.For<IKittenClawsController>();
        _functions = new ItemFunctions(
            _mockKittenClawsController,
            Substitute.For<ILogger<ItemFunctions>>());
    }

    private static Dictionary<string, string> IdPath() => new() { { "id", ItemId } };

    [Fact]
    public async Task GetKittenClaws_ReturnsOk_WhenKittenClawsIsFound()
    {
        _mockKittenClawsController.GetAsync(ItemId, Arg.Any<CancellationToken>())
            .Returns(new KittenClawsDto { Id = ItemId, Name = "mockKittenClaws" });

        var response = await _functions.GetKittenClaws(Mocks.CreateApiGatewayRequest("GET", IdPath()));
        var responseDto = JsonConvert.DeserializeObject<KittenClawsDto>(response.Body);

        Assert.Equal(200, response.StatusCode);
        Assert.Equal(ItemId, responseDto!.Id);
        Assert.Equal("mockKittenClaws", responseDto.Name);
    }

    [Fact]
    public async Task GetKittenClaws_ReturnsError_WhenExceptionIsThrown()
    {
        _mockKittenClawsController.GetAsync(ItemId, Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var response = await _functions.GetKittenClaws(Mocks.CreateApiGatewayRequest("GET", IdPath()));
        var error = JsonConvert.DeserializeObject<BaseError>(response.Body);

        Assert.Equal(500, response.StatusCode);
        Assert.Equal("Mock exception", error!.ErrorMessage);
    }

    [Fact]
    public async Task GetKittenClawsList_ReturnsOk_WithItems()
    {
        _mockKittenClawsController.GetListAsync(Arg.Any<CancellationToken>())
            .Returns(new List<KittenClawsDto>
            {
                new() { Id = ItemId, Name = "mockKittenClaws1" },
                new() { Id = "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name = "mockKittenClaws2" },
            });

        var response = await _functions.GetKittenClawsList(Mocks.CreateApiGatewayRequest(httpMethod: "GET"));
        var responseDtos = JsonConvert.DeserializeObject<List<KittenClawsDto>>(response.Body);

        Assert.Equal(200, response.StatusCode);
        Assert.Equal(2, responseDtos!.Count);
    }

    [Fact]
    public async Task GetKittenClawsList_ReturnsError_WhenExceptionIsThrown()
    {
        _mockKittenClawsController.GetListAsync(Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var response = await _functions.GetKittenClawsList(Mocks.CreateApiGatewayRequest(httpMethod: "GET"));
        var error = JsonConvert.DeserializeObject<BaseError>(response.Body);

        Assert.Equal(500, response.StatusCode);
        Assert.Equal("Mock exception", error!.ErrorMessage);
    }

    [Fact]
    public async Task CreateKittenClaws_ReturnsCreated_WhenKittenClawsIsCreated()
    {
        _mockKittenClawsController.CreateAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Returns(new KittenClawsDto { Id = ItemId, Name = "mockKittenClaws" });

        var response = await _functions.CreateKittenClaws(Mocks.CreateApiGatewayRequest(new CreateKittenClawsRequest { Name = "mockKittenClaws" }, "POST"));
        var responseDto = JsonConvert.DeserializeObject<KittenClawsDto>(response.Body);

        Assert.Equal(201, response.StatusCode);
        Assert.Equal(ItemId, responseDto!.Id);
        Assert.Equal("mockKittenClaws", responseDto.Name);
    }

    [Fact]
    public async Task CreateKittenClaws_ReturnsError_WhenExceptionIsThrown()
    {
        _mockKittenClawsController.CreateAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var response = await _functions.CreateKittenClaws(Mocks.CreateApiGatewayRequest(new CreateKittenClawsRequest { Name = "mockKittenClaws" }, "POST"));
        var error = JsonConvert.DeserializeObject<BaseError>(response.Body);

        Assert.Equal(500, response.StatusCode);
        Assert.Equal("Mock exception", error!.ErrorMessage);
    }

    [Fact]
    public async Task UpdateKittenClaws_ReturnsOk_WhenKittenClawsIsUpdated()
    {
        _mockKittenClawsController.UpdateAsync(ItemId, Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Returns(new KittenClawsDto { Id = ItemId, Name = "mockUpdatedKittenClaws" });

        var response = await _functions.UpdateKittenClaws(Mocks.CreateApiGatewayRequest(new UpdateKittenClawsRequest { Name = "mockUpdatedKittenClaws" }, "PATCH", IdPath()));
        var responseDto = JsonConvert.DeserializeObject<KittenClawsDto>(response.Body);

        Assert.Equal(200, response.StatusCode);
        Assert.Equal("mockUpdatedKittenClaws", responseDto!.Name);
    }

    [Fact]
    public async Task UpdateKittenClaws_ReturnsError_WhenExceptionIsThrown()
    {
        _mockKittenClawsController.UpdateAsync(ItemId, Arg.Any<Stream>(), Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var response = await _functions.UpdateKittenClaws(Mocks.CreateApiGatewayRequest(new UpdateKittenClawsRequest { Name = "mockUpdatedKittenClaws" }, "PATCH", IdPath()));
        var error = JsonConvert.DeserializeObject<BaseError>(response.Body);

        Assert.Equal(500, response.StatusCode);
        Assert.Equal("Mock exception", error!.ErrorMessage);
    }

    [Fact]
    public async Task DeleteKittenClaws_ReturnsOk_WhenKittenClawsIsDeleted()
    {
        _mockKittenClawsController.DeleteAsync(ItemId, Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var response = await _functions.DeleteKittenClaws(Mocks.CreateApiGatewayRequest("DELETE", IdPath()));
        var result = JsonConvert.DeserializeObject<DeleteOkObjectResult>(response.Body);

        Assert.Equal(200, response.StatusCode);
        Assert.Equal($"KittenClaws with id {ItemId} was deleted successfully.", result!.Message);
    }

    [Fact]
    public async Task DeleteKittenClaws_ReturnsError_WhenExceptionIsThrown()
    {
        _mockKittenClawsController.DeleteAsync(ItemId, Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var response = await _functions.DeleteKittenClaws(Mocks.CreateApiGatewayRequest("DELETE", IdPath()));
        var error = JsonConvert.DeserializeObject<BaseError>(response.Body);

        Assert.Equal(500, response.StatusCode);
        Assert.Equal("Mock exception", error!.ErrorMessage);
    }
}

