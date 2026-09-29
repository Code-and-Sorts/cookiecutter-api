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
            Substitute.For<ICatController>(),
            Substitute.For<IDogController>(),
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

        Assert.Equal(11, handlers.Count);
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

public class CatFunctionsTest
{
    private const string ItemId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
    private readonly ICatController _mockCatController;
    private readonly ItemFunctions _functions;

    public CatFunctionsTest()
    {
        _mockCatController = Substitute.For<ICatController>();
        _functions = new ItemFunctions(
            _mockCatController,
            Substitute.For<IDogController>(),
            Substitute.For<ILogger<ItemFunctions>>());
    }

    private static Dictionary<string, string> IdPath() => new() { { "id", ItemId } };

    [Fact]
    public async Task GetCat_ReturnsOk_WhenCatIsFound()
    {
        _mockCatController.GetAsync(ItemId, Arg.Any<CancellationToken>())
            .Returns(new CatDto { Id = ItemId, Name = "mockCat" });

        var response = await _functions.GetCat(Mocks.CreateApiGatewayRequest("GET", IdPath()));
        var responseDto = JsonConvert.DeserializeObject<CatDto>(response.Body);

        Assert.Equal(200, response.StatusCode);
        Assert.Equal(ItemId, responseDto!.Id);
        Assert.Equal("mockCat", responseDto.Name);
    }

    [Fact]
    public async Task GetCat_ReturnsError_WhenExceptionIsThrown()
    {
        _mockCatController.GetAsync(ItemId, Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var response = await _functions.GetCat(Mocks.CreateApiGatewayRequest("GET", IdPath()));
        var error = JsonConvert.DeserializeObject<BaseError>(response.Body);

        Assert.Equal(500, response.StatusCode);
        Assert.Equal("Mock exception", error!.ErrorMessage);
    }

    [Fact]
    public async Task GetCatList_ReturnsOk_WithItems()
    {
        _mockCatController.GetListAsync(Arg.Any<CancellationToken>())
            .Returns(new List<CatDto>
            {
                new() { Id = ItemId, Name = "mockCat1" },
                new() { Id = "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name = "mockCat2" },
            });

        var response = await _functions.GetCatList(Mocks.CreateApiGatewayRequest(httpMethod: "GET"));
        var responseDtos = JsonConvert.DeserializeObject<List<CatDto>>(response.Body);

        Assert.Equal(200, response.StatusCode);
        Assert.Equal(2, responseDtos!.Count);
    }

    [Fact]
    public async Task GetCatList_ReturnsError_WhenExceptionIsThrown()
    {
        _mockCatController.GetListAsync(Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var response = await _functions.GetCatList(Mocks.CreateApiGatewayRequest(httpMethod: "GET"));
        var error = JsonConvert.DeserializeObject<BaseError>(response.Body);

        Assert.Equal(500, response.StatusCode);
        Assert.Equal("Mock exception", error!.ErrorMessage);
    }

    [Fact]
    public async Task CreateCat_ReturnsCreated_WhenCatIsCreated()
    {
        _mockCatController.CreateAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Returns(new CatDto { Id = ItemId, Name = "mockCat" });

        var response = await _functions.CreateCat(Mocks.CreateApiGatewayRequest(new CreateCatRequest { Name = "mockCat" }, "POST"));
        var responseDto = JsonConvert.DeserializeObject<CatDto>(response.Body);

        Assert.Equal(201, response.StatusCode);
        Assert.Equal(ItemId, responseDto!.Id);
        Assert.Equal("mockCat", responseDto.Name);
    }

    [Fact]
    public async Task CreateCat_ReturnsError_WhenExceptionIsThrown()
    {
        _mockCatController.CreateAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var response = await _functions.CreateCat(Mocks.CreateApiGatewayRequest(new CreateCatRequest { Name = "mockCat" }, "POST"));
        var error = JsonConvert.DeserializeObject<BaseError>(response.Body);

        Assert.Equal(500, response.StatusCode);
        Assert.Equal("Mock exception", error!.ErrorMessage);
    }

    [Fact]
    public async Task UpdateCat_ReturnsOk_WhenCatIsUpdated()
    {
        _mockCatController.UpdateAsync(ItemId, Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Returns(new CatDto { Id = ItemId, Name = "mockUpdatedCat" });

        var response = await _functions.UpdateCat(Mocks.CreateApiGatewayRequest(new UpdateCatRequest { Name = "mockUpdatedCat" }, "PATCH", IdPath()));
        var responseDto = JsonConvert.DeserializeObject<CatDto>(response.Body);

        Assert.Equal(200, response.StatusCode);
        Assert.Equal("mockUpdatedCat", responseDto!.Name);
    }

    [Fact]
    public async Task UpdateCat_ReturnsError_WhenExceptionIsThrown()
    {
        _mockCatController.UpdateAsync(ItemId, Arg.Any<Stream>(), Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var response = await _functions.UpdateCat(Mocks.CreateApiGatewayRequest(new UpdateCatRequest { Name = "mockUpdatedCat" }, "PATCH", IdPath()));
        var error = JsonConvert.DeserializeObject<BaseError>(response.Body);

        Assert.Equal(500, response.StatusCode);
        Assert.Equal("Mock exception", error!.ErrorMessage);
    }

    [Fact]
    public async Task DeleteCat_ReturnsOk_WhenCatIsDeleted()
    {
        _mockCatController.DeleteAsync(ItemId, Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var response = await _functions.DeleteCat(Mocks.CreateApiGatewayRequest("DELETE", IdPath()));
        var result = JsonConvert.DeserializeObject<DeleteOkObjectResult>(response.Body);

        Assert.Equal(200, response.StatusCode);
        Assert.Equal($"Cat with id {ItemId} was deleted successfully.", result!.Message);
    }

    [Fact]
    public async Task DeleteCat_ReturnsError_WhenExceptionIsThrown()
    {
        _mockCatController.DeleteAsync(ItemId, Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var response = await _functions.DeleteCat(Mocks.CreateApiGatewayRequest("DELETE", IdPath()));
        var error = JsonConvert.DeserializeObject<BaseError>(response.Body);

        Assert.Equal(500, response.StatusCode);
        Assert.Equal("Mock exception", error!.ErrorMessage);
    }
}

public class DogFunctionsTest
{
    private const string ItemId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
    private readonly IDogController _mockDogController;
    private readonly ItemFunctions _functions;

    public DogFunctionsTest()
    {
        _mockDogController = Substitute.For<IDogController>();
        _functions = new ItemFunctions(
            Substitute.For<ICatController>(),
            _mockDogController,
            Substitute.For<ILogger<ItemFunctions>>());
    }

    private static Dictionary<string, string> IdPath() => new() { { "id", ItemId } };

    [Fact]
    public async Task GetDog_ReturnsOk_WhenDogIsFound()
    {
        _mockDogController.GetAsync(ItemId, Arg.Any<CancellationToken>())
            .Returns(new DogDto { Id = ItemId, Name = "mockDog" });

        var response = await _functions.GetDog(Mocks.CreateApiGatewayRequest("GET", IdPath()));
        var responseDto = JsonConvert.DeserializeObject<DogDto>(response.Body);

        Assert.Equal(200, response.StatusCode);
        Assert.Equal(ItemId, responseDto!.Id);
        Assert.Equal("mockDog", responseDto.Name);
    }

    [Fact]
    public async Task GetDog_ReturnsError_WhenExceptionIsThrown()
    {
        _mockDogController.GetAsync(ItemId, Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var response = await _functions.GetDog(Mocks.CreateApiGatewayRequest("GET", IdPath()));
        var error = JsonConvert.DeserializeObject<BaseError>(response.Body);

        Assert.Equal(500, response.StatusCode);
        Assert.Equal("Mock exception", error!.ErrorMessage);
    }

    [Fact]
    public async Task GetDogList_ReturnsOk_WithItems()
    {
        _mockDogController.GetListAsync(Arg.Any<CancellationToken>())
            .Returns(new List<DogDto>
            {
                new() { Id = ItemId, Name = "mockDog1" },
                new() { Id = "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name = "mockDog2" },
            });

        var response = await _functions.GetDogList(Mocks.CreateApiGatewayRequest(httpMethod: "GET"));
        var responseDtos = JsonConvert.DeserializeObject<List<DogDto>>(response.Body);

        Assert.Equal(200, response.StatusCode);
        Assert.Equal(2, responseDtos!.Count);
    }

    [Fact]
    public async Task GetDogList_ReturnsError_WhenExceptionIsThrown()
    {
        _mockDogController.GetListAsync(Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var response = await _functions.GetDogList(Mocks.CreateApiGatewayRequest(httpMethod: "GET"));
        var error = JsonConvert.DeserializeObject<BaseError>(response.Body);

        Assert.Equal(500, response.StatusCode);
        Assert.Equal("Mock exception", error!.ErrorMessage);
    }

    [Fact]
    public async Task CreateDog_ReturnsCreated_WhenDogIsCreated()
    {
        _mockDogController.CreateAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Returns(new DogDto { Id = ItemId, Name = "mockDog" });

        var response = await _functions.CreateDog(Mocks.CreateApiGatewayRequest(new CreateDogRequest { Name = "mockDog" }, "POST"));
        var responseDto = JsonConvert.DeserializeObject<DogDto>(response.Body);

        Assert.Equal(201, response.StatusCode);
        Assert.Equal(ItemId, responseDto!.Id);
        Assert.Equal("mockDog", responseDto.Name);
    }

    [Fact]
    public async Task CreateDog_ReturnsError_WhenExceptionIsThrown()
    {
        _mockDogController.CreateAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var response = await _functions.CreateDog(Mocks.CreateApiGatewayRequest(new CreateDogRequest { Name = "mockDog" }, "POST"));
        var error = JsonConvert.DeserializeObject<BaseError>(response.Body);

        Assert.Equal(500, response.StatusCode);
        Assert.Equal("Mock exception", error!.ErrorMessage);
    }

    [Fact]
    public async Task ReplaceDog_ReturnsOk_WhenDogIsReplaced()
    {
        _mockDogController.ReplaceAsync(ItemId, Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Returns(new DogDto { Id = ItemId, Name = "mockReplacedDog" });

        var response = await _functions.ReplaceDog(Mocks.CreateApiGatewayRequest(new ReplaceDogRequest { Name = "mockReplacedDog" }, "PUT", IdPath()));
        var responseDto = JsonConvert.DeserializeObject<DogDto>(response.Body);

        Assert.Equal(200, response.StatusCode);
        Assert.Equal("mockReplacedDog", responseDto!.Name);
    }

    [Fact]
    public async Task ReplaceDog_ReturnsError_WhenExceptionIsThrown()
    {
        _mockDogController.ReplaceAsync(ItemId, Arg.Any<Stream>(), Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var response = await _functions.ReplaceDog(Mocks.CreateApiGatewayRequest(new ReplaceDogRequest { Name = "mockReplacedDog" }, "PUT", IdPath()));
        var error = JsonConvert.DeserializeObject<BaseError>(response.Body);

        Assert.Equal(500, response.StatusCode);
        Assert.Equal("Mock exception", error!.ErrorMessage);
    }

    [Fact]
    public async Task DeleteDog_ReturnsOk_WhenDogIsDeleted()
    {
        _mockDogController.DeleteAsync(ItemId, Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var response = await _functions.DeleteDog(Mocks.CreateApiGatewayRequest("DELETE", IdPath()));
        var result = JsonConvert.DeserializeObject<DeleteOkObjectResult>(response.Body);

        Assert.Equal(200, response.StatusCode);
        Assert.Equal($"Dog with id {ItemId} was deleted successfully.", result!.Message);
    }

    [Fact]
    public async Task DeleteDog_ReturnsError_WhenExceptionIsThrown()
    {
        _mockDogController.DeleteAsync(ItemId, Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var response = await _functions.DeleteDog(Mocks.CreateApiGatewayRequest("DELETE", IdPath()));
        var error = JsonConvert.DeserializeObject<BaseError>(response.Body);

        Assert.Equal(500, response.StatusCode);
        Assert.Equal("Mock exception", error!.ErrorMessage);
    }
}

