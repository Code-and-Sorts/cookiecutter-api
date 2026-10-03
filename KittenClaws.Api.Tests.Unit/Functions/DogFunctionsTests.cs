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

public class DogFunctionsTests
{
    private const string ItemId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
    private readonly IDogController _mockDogController;
    private readonly RecordingLogger<DogFunctions> _logger = new();
    private readonly DogFunctions _functions;

    public DogFunctionsTests()
    {
        _mockDogController = Substitute.For<IDogController>();
        _functions = new DogFunctions(_mockDogController, _logger);
    }

    private void AssertUnexpectedError(IActionResult result)
    {
        Assert.Equal((500, "{\"errorMessage\":\"An unexpected error occurred.\"}"), Mocks.ReadJsonResult(result));
        Assert.Contains(_logger.Entries, entry => entry.Level == LogLevel.Error && entry.Exception?.Message == "Mock exception");
    }

    [Fact]
    public async Task GetDog_ReturnsCamelCaseItem_WhenDogIsFound()
    {
        _mockDogController.GetAsync(ItemId, Arg.Any<CancellationToken>()).Returns(DogSamples.Dto());

        var result = await _functions.GetDog(Mocks.CreateHttpRequestData("GET"), ItemId, TestContext.Current.CancellationToken);

        Assert.Equal((200, Json.Serialize(DogSamples.Dto())), Mocks.ReadJsonResult(result));
    }

    [Fact]
    public async Task GetDog_ReturnsNotFound_WithoutLoggingAnError()
    {
        _mockDogController.GetAsync("not-a-uuid", Arg.Any<CancellationToken>()).Throws(new NotFoundException("Dog", "not-a-uuid"));

        var result = await _functions.GetDog(Mocks.CreateHttpRequestData("GET"), "not-a-uuid", TestContext.Current.CancellationToken);

        Assert.Equal((404, "{\"errorMessage\":\"Dog with id not-a-uuid was not found.\"}"), Mocks.ReadJsonResult(result));
        Assert.Empty(_logger.Entries);
    }

    [Fact]
    public async Task GetDog_ReturnsGenericError_WhenExceptionIsThrown()
    {
        _mockDogController.GetAsync(ItemId, Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var result = await _functions.GetDog(Mocks.CreateHttpRequestData("GET"), ItemId, TestContext.Current.CancellationToken);

        AssertUnexpectedError(result);
    }

    [Fact]
    public async Task GetDogList_ReturnsItems()
    {
        _mockDogController.GetListAsync(null, Arg.Any<CancellationToken>()).Returns(new List<DogDto>
        {
            DogSamples.Dto(),
        });

        var result = await _functions.GetDogList(Mocks.CreateHttpRequestData("GET"), TestContext.Current.CancellationToken);

        Assert.Equal((200, $"[{Json.Serialize(DogSamples.Dto())}]"), Mocks.ReadJsonResult(result));
    }

    [Fact]
    public async Task GetDogList_PassesLimitQueryParameter()
    {
        _mockDogController.GetListAsync("5", Arg.Any<CancellationToken>()).Returns(new List<DogDto>());

        var result = await _functions.GetDogList(Mocks.CreateHttpRequestData("GET", "?limit=5"), TestContext.Current.CancellationToken);

        Assert.Equal((200, "[]"), Mocks.ReadJsonResult(result));
        await _mockDogController.Received(1).GetListAsync("5", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetDogList_ReturnsGenericErrorAtTheDeadline_WhenTheDatabaseHangs()
    {
        _mockDogController.GetListAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(new TaskCompletionSource<IEnumerable<DogDto>>().Task);
        var started = DateTime.UtcNow;

        var result = await _functions.GetDogList(Mocks.CreateHttpRequestData("GET"), TestContext.Current.CancellationToken);

        Assert.InRange(DateTime.UtcNow - started, RequestDeadline.Timeout - TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10));
        Assert.Equal(500, Mocks.ReadJsonResult(result).StatusCode);
        Assert.Contains(_logger.Entries, entry => entry.Level == LogLevel.Error);
    }

    [Fact]
    public async Task GetDogList_ReturnsGenericError_WhenExceptionIsThrown()
    {
        _mockDogController.GetListAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var result = await _functions.GetDogList(Mocks.CreateHttpRequestData("GET"), TestContext.Current.CancellationToken);

        AssertUnexpectedError(result);
    }

    [Fact]
    public async Task CreateDog_ReturnsCreated_WhenDogIsCreated()
    {
        _mockDogController.CreateAsync(Arg.Any<Stream>(), Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(DogSamples.Dto());

        var result = await _functions.CreateDog(Mocks.CreateHttpRequestData(new CreateDogRequest(), "POST"), TestContext.Current.CancellationToken);

        Assert.Equal((201, Json.Serialize(DogSamples.Dto())), Mocks.ReadJsonResult(result));
    }

    [Fact]
    public async Task CreateDog_ReturnsBadRequest_WhenBodyIsInvalid()
    {
        _mockDogController.CreateAsync(Arg.Any<Stream>(), Arg.Any<string?>(), Arg.Any<CancellationToken>()).Throws(new BadRequestException("Request body must be valid JSON."));

        var result = await _functions.CreateDog(Mocks.CreateHttpRequestData("POST"), TestContext.Current.CancellationToken);

        Assert.Equal((400, "{\"errorMessage\":\"Request body must be valid JSON.\"}"), Mocks.ReadJsonResult(result));
        Assert.Empty(_logger.Entries);
    }

    [Fact]
    public async Task CreateDog_ReturnsGenericError_WhenExceptionIsThrown()
    {
        _mockDogController.CreateAsync(Arg.Any<Stream>(), Arg.Any<string?>(), Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var result = await _functions.CreateDog(Mocks.CreateHttpRequestData(new CreateDogRequest(), "POST"), TestContext.Current.CancellationToken);

        AssertUnexpectedError(result);
    }

    [Fact]
    public async Task ReplaceDog_ReturnsOk_WhenDogIsReplaced()
    {
        _mockDogController.ReplaceAsync(ItemId, Arg.Any<Stream>(), Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(DogSamples.Dto());

        var result = await _functions.ReplaceDog(Mocks.CreateHttpRequestData(new ReplaceDogRequest(), "PUT"), ItemId, TestContext.Current.CancellationToken);

        Assert.Equal((200, Json.Serialize(DogSamples.Dto())), Mocks.ReadJsonResult(result));
    }

    [Fact]
    public async Task ReplaceDog_ReturnsGenericError_WhenExceptionIsThrown()
    {
        _mockDogController.ReplaceAsync(ItemId, Arg.Any<Stream>(), Arg.Any<string?>(), Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var result = await _functions.ReplaceDog(Mocks.CreateHttpRequestData(new ReplaceDogRequest(), "PUT"), ItemId, TestContext.Current.CancellationToken);

        AssertUnexpectedError(result);
    }

    [Fact]
    public async Task DeleteDog_ReturnsDeleteMessage_WhenDogIsDeleted()
    {
        _mockDogController.DeleteAsync(ItemId, Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(DeleteOkObjectResult.For("Dog", ItemId));

        var result = await _functions.DeleteDog(Mocks.CreateHttpRequestData("DELETE"), ItemId, TestContext.Current.CancellationToken);

        Assert.Equal((200, $"{{\"message\":\"Dog with id {ItemId} was deleted successfully.\"}}"), Mocks.ReadJsonResult(result));
    }

    [Fact]
    public async Task DeleteDog_ReturnsGenericError_WhenExceptionIsThrown()
    {
        _mockDogController.DeleteAsync(ItemId, Arg.Any<string?>(), Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var result = await _functions.DeleteDog(Mocks.CreateHttpRequestData("DELETE"), ItemId, TestContext.Current.CancellationToken);

        AssertUnexpectedError(result);
    }
}
