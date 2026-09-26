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
            Substitute.For<IKittenClawsController>(),
            Substitute.For<ILogger<ItemFunctions>>());

        var result = await functions.Health(Mocks.CreateHttpRequestData(), TestContext.Current.CancellationToken);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
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

    [Fact]
    public async Task GetKittenClaws_ReturnsOkResult_WhenKittenClawsIsFound()
    {
        var dto = new KittenClawsDto { Id = ItemId, Name = "mockKittenClaws" };
        _mockKittenClawsController.GetAsync(ItemId, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await _functions.GetKittenClaws(Mocks.CreateHttpRequestData("GET"), ItemId, TestContext.Current.CancellationToken);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(dto, okResult.Value);
    }

    [Fact]
    public async Task GetKittenClaws_ReturnsErrorResult_WhenExceptionIsThrown()
    {
        _mockKittenClawsController.GetAsync(ItemId, Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var result = await _functions.GetKittenClaws(Mocks.CreateHttpRequestData("GET"), ItemId, TestContext.Current.CancellationToken);

        var errorResult = Assert.IsType<HttpResponseInit>(result);
        var error = Assert.IsType<BaseError>(errorResult.Value);
        Assert.Equal(500, errorResult.StatusCode);
        Assert.Equal("Mock exception", error.ErrorMessage);
    }

    [Fact]
    public async Task GetKittenClawsList_ReturnsOkResult_WithItems()
    {
        var dtos = new List<KittenClawsDto>
        {
            new() { Id = ItemId, Name = "mockKittenClaws1" },
            new() { Id = "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name = "mockKittenClaws2" },
        };
        _mockKittenClawsController.GetListAsync(Arg.Any<CancellationToken>()).Returns(dtos);

        var result = await _functions.GetKittenClawsList(Mocks.CreateHttpRequestData("GET"), TestContext.Current.CancellationToken);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(dtos, okResult.Value);
    }

    [Fact]
    public async Task GetKittenClawsList_ReturnsErrorResult_WhenExceptionIsThrown()
    {
        _mockKittenClawsController.GetListAsync(Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var result = await _functions.GetKittenClawsList(Mocks.CreateHttpRequestData("GET"), TestContext.Current.CancellationToken);

        var errorResult = Assert.IsType<HttpResponseInit>(result);
        var error = Assert.IsType<BaseError>(errorResult.Value);
        Assert.Equal(500, errorResult.StatusCode);
        Assert.Equal("Mock exception", error.ErrorMessage);
    }

    [Fact]
    public async Task CreateKittenClaws_ReturnsCreatedResult_WhenKittenClawsIsCreated()
    {
        var request = new CreateKittenClawsRequest { Name = "mockKittenClaws" };
        var dto = new KittenClawsDto { Id = ItemId, Name = "mockKittenClaws" };
        _mockKittenClawsController.CreateAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>()).Returns(dto);

        var result = await _functions.CreateKittenClaws(Mocks.CreateHttpRequestData(request, "POST"), TestContext.Current.CancellationToken);

        var createdResult = Assert.IsType<CreatedResult>(result);
        var responseDto = JsonConvert.DeserializeObject<KittenClawsDto>(JsonConvert.SerializeObject(createdResult.Value));
        Assert.Equal(201, createdResult.StatusCode);
        Assert.Equal("/api/kitties", createdResult.Location);
        Assert.Equal(dto.Id, responseDto!.Id);
        Assert.Equal(dto.Name, responseDto.Name);
    }

    [Fact]
    public async Task CreateKittenClaws_ReturnsErrorResult_WhenExceptionIsThrown()
    {
        var request = new CreateKittenClawsRequest { Name = "mockKittenClaws" };
        _mockKittenClawsController.CreateAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var result = await _functions.CreateKittenClaws(Mocks.CreateHttpRequestData(request, "POST"), TestContext.Current.CancellationToken);

        var errorResult = Assert.IsType<HttpResponseInit>(result);
        var error = Assert.IsType<BaseError>(errorResult.Value);
        Assert.Equal(500, errorResult.StatusCode);
        Assert.Equal("Mock exception", error.ErrorMessage);
    }

    [Fact]
    public async Task UpdateKittenClaws_ReturnsOkResult_WhenKittenClawsIsUpdated()
    {
        var request = new UpdateKittenClawsRequest { Name = "mockUpdatedKittenClaws" };
        var dto = new KittenClawsDto { Id = ItemId, Name = "mockUpdatedKittenClaws" };
        _mockKittenClawsController.UpdateAsync(ItemId, Arg.Any<Stream>(), Arg.Any<CancellationToken>()).Returns(dto);

        var result = await _functions.UpdateKittenClaws(Mocks.CreateHttpRequestData(request, "PATCH"), ItemId, TestContext.Current.CancellationToken);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(dto, okResult.Value);
    }

    [Fact]
    public async Task UpdateKittenClaws_ReturnsErrorResult_WhenExceptionIsThrown()
    {
        var request = new UpdateKittenClawsRequest { Name = "mockUpdatedKittenClaws" };
        _mockKittenClawsController.UpdateAsync(ItemId, Arg.Any<Stream>(), Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var result = await _functions.UpdateKittenClaws(Mocks.CreateHttpRequestData(request, "PATCH"), ItemId, TestContext.Current.CancellationToken);

        var errorResult = Assert.IsType<HttpResponseInit>(result);
        var error = Assert.IsType<BaseError>(errorResult.Value);
        Assert.Equal(500, errorResult.StatusCode);
        Assert.Equal("Mock exception", error.ErrorMessage);
    }

    [Fact]
    public async Task DeleteKittenClaws_ReturnsOkResult_WhenKittenClawsIsDeleted()
    {
        _mockKittenClawsController.DeleteAsync(ItemId, Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var result = await _functions.DeleteKittenClaws(Mocks.CreateHttpRequestData("DELETE"), ItemId, TestContext.Current.CancellationToken);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var message = Assert.IsType<DeleteOkObjectResult>(okResult.Value);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal($"KittenClaws with id {ItemId} was deleted successfully.", message.Message);
    }

    [Fact]
    public async Task DeleteKittenClaws_ReturnsErrorResult_WhenExceptionIsThrown()
    {
        _mockKittenClawsController.DeleteAsync(ItemId, Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var result = await _functions.DeleteKittenClaws(Mocks.CreateHttpRequestData("DELETE"), ItemId, TestContext.Current.CancellationToken);

        var errorResult = Assert.IsType<HttpResponseInit>(result);
        var error = Assert.IsType<BaseError>(errorResult.Value);
        Assert.Equal(500, errorResult.StatusCode);
        Assert.Equal("Mock exception", error.ErrorMessage);
    }
}

