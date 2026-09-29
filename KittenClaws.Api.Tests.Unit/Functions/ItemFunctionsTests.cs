namespace KittenClaws.Api.Tests.Unit;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using KittenClaws.Api.Dtos;
using KittenClaws.Api.Functions;
using KittenClaws.Api.Interfaces;
using KittenClaws.Api.Requests;
using KittenClaws.Api.Utils;
using Newtonsoft.Json;

public class HealthFunctionTest
{
    [Fact]
    public async Task Health_ReturnsOk()
    {
        var functions = new ItemFunctions(
            Substitute.For<ICatController>(),
            Substitute.For<IDogController>(),
            Substitute.For<ILogger<ItemFunctions>>());

        var result = await functions.Health(Mocks.CreateHttpRequestData(), TestContext.Current.CancellationToken);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
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

    [Fact]
    public async Task GetCat_ReturnsOkResult_WhenCatIsFound()
    {
        var dto = new CatDto { Id = ItemId, Name = "mockCat" };
        _mockCatController.GetAsync(ItemId, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await _functions.GetCat(Mocks.CreateHttpRequestData("GET"), ItemId, TestContext.Current.CancellationToken);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(dto, okResult.Value);
    }

    [Fact]
    public async Task GetCat_ReturnsErrorResult_WhenExceptionIsThrown()
    {
        _mockCatController.GetAsync(ItemId, Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var result = await _functions.GetCat(Mocks.CreateHttpRequestData("GET"), ItemId, TestContext.Current.CancellationToken);

        var errorResult = Assert.IsType<HttpResponseInit>(result);
        var error = Assert.IsType<BaseError>(errorResult.Value);
        Assert.Equal(500, errorResult.StatusCode);
        Assert.Equal("Mock exception", error.ErrorMessage);
    }

    [Fact]
    public async Task GetCatList_ReturnsOkResult_WithItems()
    {
        var dtos = new List<CatDto>
        {
            new() { Id = ItemId, Name = "mockCat1" },
            new() { Id = "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name = "mockCat2" },
        };
        _mockCatController.GetListAsync(Arg.Any<CancellationToken>()).Returns(dtos);

        var result = await _functions.GetCatList(Mocks.CreateHttpRequestData("GET"), TestContext.Current.CancellationToken);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(dtos, okResult.Value);
    }

    [Fact]
    public async Task GetCatList_ReturnsErrorResult_WhenExceptionIsThrown()
    {
        _mockCatController.GetListAsync(Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var result = await _functions.GetCatList(Mocks.CreateHttpRequestData("GET"), TestContext.Current.CancellationToken);

        var errorResult = Assert.IsType<HttpResponseInit>(result);
        var error = Assert.IsType<BaseError>(errorResult.Value);
        Assert.Equal(500, errorResult.StatusCode);
        Assert.Equal("Mock exception", error.ErrorMessage);
    }

    [Fact]
    public async Task CreateCat_ReturnsCreatedResult_WhenCatIsCreated()
    {
        var request = new CreateCatRequest { Name = "mockCat" };
        var dto = new CatDto { Id = ItemId, Name = "mockCat" };
        _mockCatController.CreateAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>()).Returns(dto);

        var result = await _functions.CreateCat(Mocks.CreateHttpRequestData(request, "POST"), TestContext.Current.CancellationToken);

        var createdResult = Assert.IsType<CreatedResult>(result);
        var responseDto = JsonConvert.DeserializeObject<CatDto>(JsonConvert.SerializeObject(createdResult.Value));
        Assert.Equal(201, createdResult.StatusCode);
        Assert.Equal("/api/cats", createdResult.Location);
        Assert.Equal(dto.Id, responseDto!.Id);
        Assert.Equal(dto.Name, responseDto.Name);
    }

    [Fact]
    public async Task CreateCat_ReturnsErrorResult_WhenExceptionIsThrown()
    {
        var request = new CreateCatRequest { Name = "mockCat" };
        _mockCatController.CreateAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var result = await _functions.CreateCat(Mocks.CreateHttpRequestData(request, "POST"), TestContext.Current.CancellationToken);

        var errorResult = Assert.IsType<HttpResponseInit>(result);
        var error = Assert.IsType<BaseError>(errorResult.Value);
        Assert.Equal(500, errorResult.StatusCode);
        Assert.Equal("Mock exception", error.ErrorMessage);
    }

    [Fact]
    public async Task UpdateCat_ReturnsOkResult_WhenCatIsUpdated()
    {
        var request = new UpdateCatRequest { Name = "mockUpdatedCat" };
        var dto = new CatDto { Id = ItemId, Name = "mockUpdatedCat" };
        _mockCatController.UpdateAsync(ItemId, Arg.Any<Stream>(), Arg.Any<CancellationToken>()).Returns(dto);

        var result = await _functions.UpdateCat(Mocks.CreateHttpRequestData(request, "PATCH"), ItemId, TestContext.Current.CancellationToken);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(dto, okResult.Value);
    }

    [Fact]
    public async Task UpdateCat_ReturnsErrorResult_WhenExceptionIsThrown()
    {
        var request = new UpdateCatRequest { Name = "mockUpdatedCat" };
        _mockCatController.UpdateAsync(ItemId, Arg.Any<Stream>(), Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var result = await _functions.UpdateCat(Mocks.CreateHttpRequestData(request, "PATCH"), ItemId, TestContext.Current.CancellationToken);

        var errorResult = Assert.IsType<HttpResponseInit>(result);
        var error = Assert.IsType<BaseError>(errorResult.Value);
        Assert.Equal(500, errorResult.StatusCode);
        Assert.Equal("Mock exception", error.ErrorMessage);
    }

    [Fact]
    public async Task DeleteCat_ReturnsOkResult_WhenCatIsDeleted()
    {
        _mockCatController.DeleteAsync(ItemId, Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var result = await _functions.DeleteCat(Mocks.CreateHttpRequestData("DELETE"), ItemId, TestContext.Current.CancellationToken);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var message = Assert.IsType<DeleteOkObjectResult>(okResult.Value);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal($"Cat with id {ItemId} was deleted successfully.", message.Message);
    }

    [Fact]
    public async Task DeleteCat_ReturnsErrorResult_WhenExceptionIsThrown()
    {
        _mockCatController.DeleteAsync(ItemId, Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var result = await _functions.DeleteCat(Mocks.CreateHttpRequestData("DELETE"), ItemId, TestContext.Current.CancellationToken);

        var errorResult = Assert.IsType<HttpResponseInit>(result);
        var error = Assert.IsType<BaseError>(errorResult.Value);
        Assert.Equal(500, errorResult.StatusCode);
        Assert.Equal("Mock exception", error.ErrorMessage);
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

    [Fact]
    public async Task GetDog_ReturnsOkResult_WhenDogIsFound()
    {
        var dto = new DogDto { Id = ItemId, Name = "mockDog" };
        _mockDogController.GetAsync(ItemId, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await _functions.GetDog(Mocks.CreateHttpRequestData("GET"), ItemId, TestContext.Current.CancellationToken);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(dto, okResult.Value);
    }

    [Fact]
    public async Task GetDog_ReturnsErrorResult_WhenExceptionIsThrown()
    {
        _mockDogController.GetAsync(ItemId, Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var result = await _functions.GetDog(Mocks.CreateHttpRequestData("GET"), ItemId, TestContext.Current.CancellationToken);

        var errorResult = Assert.IsType<HttpResponseInit>(result);
        var error = Assert.IsType<BaseError>(errorResult.Value);
        Assert.Equal(500, errorResult.StatusCode);
        Assert.Equal("Mock exception", error.ErrorMessage);
    }

    [Fact]
    public async Task GetDogList_ReturnsOkResult_WithItems()
    {
        var dtos = new List<DogDto>
        {
            new() { Id = ItemId, Name = "mockDog1" },
            new() { Id = "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name = "mockDog2" },
        };
        _mockDogController.GetListAsync(Arg.Any<CancellationToken>()).Returns(dtos);

        var result = await _functions.GetDogList(Mocks.CreateHttpRequestData("GET"), TestContext.Current.CancellationToken);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(dtos, okResult.Value);
    }

    [Fact]
    public async Task GetDogList_ReturnsErrorResult_WhenExceptionIsThrown()
    {
        _mockDogController.GetListAsync(Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var result = await _functions.GetDogList(Mocks.CreateHttpRequestData("GET"), TestContext.Current.CancellationToken);

        var errorResult = Assert.IsType<HttpResponseInit>(result);
        var error = Assert.IsType<BaseError>(errorResult.Value);
        Assert.Equal(500, errorResult.StatusCode);
        Assert.Equal("Mock exception", error.ErrorMessage);
    }

    [Fact]
    public async Task CreateDog_ReturnsCreatedResult_WhenDogIsCreated()
    {
        var request = new CreateDogRequest { Name = "mockDog" };
        var dto = new DogDto { Id = ItemId, Name = "mockDog" };
        _mockDogController.CreateAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>()).Returns(dto);

        var result = await _functions.CreateDog(Mocks.CreateHttpRequestData(request, "POST"), TestContext.Current.CancellationToken);

        var createdResult = Assert.IsType<CreatedResult>(result);
        var responseDto = JsonConvert.DeserializeObject<DogDto>(JsonConvert.SerializeObject(createdResult.Value));
        Assert.Equal(201, createdResult.StatusCode);
        Assert.Equal("/api/dogs", createdResult.Location);
        Assert.Equal(dto.Id, responseDto!.Id);
        Assert.Equal(dto.Name, responseDto.Name);
    }

    [Fact]
    public async Task CreateDog_ReturnsErrorResult_WhenExceptionIsThrown()
    {
        var request = new CreateDogRequest { Name = "mockDog" };
        _mockDogController.CreateAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var result = await _functions.CreateDog(Mocks.CreateHttpRequestData(request, "POST"), TestContext.Current.CancellationToken);

        var errorResult = Assert.IsType<HttpResponseInit>(result);
        var error = Assert.IsType<BaseError>(errorResult.Value);
        Assert.Equal(500, errorResult.StatusCode);
        Assert.Equal("Mock exception", error.ErrorMessage);
    }

    [Fact]
    public async Task ReplaceDog_ReturnsOkResult_WhenDogIsReplaced()
    {
        var request = new ReplaceDogRequest { Name = "mockReplacedDog" };
        var dto = new DogDto { Id = ItemId, Name = "mockReplacedDog" };
        _mockDogController.ReplaceAsync(ItemId, Arg.Any<Stream>(), Arg.Any<CancellationToken>()).Returns(dto);

        var result = await _functions.ReplaceDog(Mocks.CreateHttpRequestData(request, "PUT"), ItemId, TestContext.Current.CancellationToken);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(dto, okResult.Value);
    }

    [Fact]
    public async Task ReplaceDog_ReturnsErrorResult_WhenExceptionIsThrown()
    {
        var request = new ReplaceDogRequest { Name = "mockReplacedDog" };
        _mockDogController.ReplaceAsync(ItemId, Arg.Any<Stream>(), Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var result = await _functions.ReplaceDog(Mocks.CreateHttpRequestData(request, "PUT"), ItemId, TestContext.Current.CancellationToken);

        var errorResult = Assert.IsType<HttpResponseInit>(result);
        var error = Assert.IsType<BaseError>(errorResult.Value);
        Assert.Equal(500, errorResult.StatusCode);
        Assert.Equal("Mock exception", error.ErrorMessage);
    }

    [Fact]
    public async Task DeleteDog_ReturnsOkResult_WhenDogIsDeleted()
    {
        _mockDogController.DeleteAsync(ItemId, Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var result = await _functions.DeleteDog(Mocks.CreateHttpRequestData("DELETE"), ItemId, TestContext.Current.CancellationToken);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var message = Assert.IsType<DeleteOkObjectResult>(okResult.Value);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal($"Dog with id {ItemId} was deleted successfully.", message.Message);
    }

    [Fact]
    public async Task DeleteDog_ReturnsErrorResult_WhenExceptionIsThrown()
    {
        _mockDogController.DeleteAsync(ItemId, Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var result = await _functions.DeleteDog(Mocks.CreateHttpRequestData("DELETE"), ItemId, TestContext.Current.CancellationToken);

        var errorResult = Assert.IsType<HttpResponseInit>(result);
        var error = Assert.IsType<BaseError>(errorResult.Value);
        Assert.Equal(500, errorResult.StatusCode);
        Assert.Equal("Mock exception", error.ErrorMessage);
    }
}

