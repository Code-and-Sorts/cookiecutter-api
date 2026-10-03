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

public class KittenClawsFunctionsTests
{
    private const string ItemId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
    private readonly IKittenClawsController _mockKittenClawsController;
    private readonly RecordingLogger<KittenClawsFunctions> _logger = new();
    private readonly KittenClawsFunctions _functions;

    public KittenClawsFunctionsTests()
    {
        _mockKittenClawsController = Substitute.For<IKittenClawsController>();
        _functions = new KittenClawsFunctions(_mockKittenClawsController, _logger);
    }

    private void AssertUnexpectedError(IActionResult result)
    {
        Assert.Equal((500, "{\"errorMessage\":\"An unexpected error occurred.\"}"), Mocks.ReadJsonResult(result));
        Assert.Contains(_logger.Entries, entry => entry.Level == LogLevel.Error && entry.Exception?.Message == "Mock exception");
    }

    [Fact]
    public async Task GetKittenClaws_ReturnsCamelCaseItem_WhenKittenClawsIsFound()
    {
        _mockKittenClawsController.GetAsync(ItemId, Arg.Any<CancellationToken>()).Returns(new KittenClawsDto { Id = ItemId, Name = "mockKittenClaws" });

        var result = await _functions.GetKittenClaws(Mocks.CreateHttpRequestData("GET"), ItemId, TestContext.Current.CancellationToken);

        Assert.Equal((200, $"{{\"id\":\"{ItemId}\",\"name\":\"mockKittenClaws\"}}"), Mocks.ReadJsonResult(result));
    }

    [Fact]
    public async Task GetKittenClaws_ReturnsNotFound_WithoutLoggingAnError()
    {
        _mockKittenClawsController.GetAsync("not-a-uuid", Arg.Any<CancellationToken>()).Throws(new NotFoundException("KittenClaws", "not-a-uuid"));

        var result = await _functions.GetKittenClaws(Mocks.CreateHttpRequestData("GET"), "not-a-uuid", TestContext.Current.CancellationToken);

        Assert.Equal((404, "{\"errorMessage\":\"KittenClaws with id not-a-uuid was not found.\"}"), Mocks.ReadJsonResult(result));
        Assert.Empty(_logger.Entries);
    }

    [Fact]
    public async Task GetKittenClaws_ReturnsGenericError_WhenExceptionIsThrown()
    {
        _mockKittenClawsController.GetAsync(ItemId, Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var result = await _functions.GetKittenClaws(Mocks.CreateHttpRequestData("GET"), ItemId, TestContext.Current.CancellationToken);

        AssertUnexpectedError(result);
    }

    [Fact]
    public async Task GetKittenClawsList_ReturnsItems()
    {
        _mockKittenClawsController.GetListAsync(null, Arg.Any<CancellationToken>()).Returns(new List<KittenClawsDto>
        {
            new() { Id = ItemId, Name = "mockKittenClaws1" },
        });

        var result = await _functions.GetKittenClawsList(Mocks.CreateHttpRequestData("GET"), TestContext.Current.CancellationToken);

        Assert.Equal((200, $"[{{\"id\":\"{ItemId}\",\"name\":\"mockKittenClaws1\"}}]"), Mocks.ReadJsonResult(result));
    }

    [Fact]
    public async Task GetKittenClawsList_PassesLimitQueryParameter()
    {
        _mockKittenClawsController.GetListAsync("5", Arg.Any<CancellationToken>()).Returns(new List<KittenClawsDto>());

        var result = await _functions.GetKittenClawsList(Mocks.CreateHttpRequestData("GET", "?limit=5"), TestContext.Current.CancellationToken);

        Assert.Equal((200, "[]"), Mocks.ReadJsonResult(result));
        await _mockKittenClawsController.Received(1).GetListAsync("5", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetKittenClawsList_ReturnsGenericErrorAtTheDeadline_WhenTheDatabaseHangs()
    {
        _mockKittenClawsController.GetListAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(new TaskCompletionSource<IEnumerable<KittenClawsDto>>().Task);
        var started = DateTime.UtcNow;

        var result = await _functions.GetKittenClawsList(Mocks.CreateHttpRequestData("GET"), TestContext.Current.CancellationToken);

        Assert.InRange(DateTime.UtcNow - started, RequestDeadline.Timeout - TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10));
        Assert.Equal(500, Mocks.ReadJsonResult(result).StatusCode);
        Assert.Contains(_logger.Entries, entry => entry.Level == LogLevel.Error);
    }

    [Fact]
    public async Task GetKittenClawsList_ReturnsGenericError_WhenExceptionIsThrown()
    {
        _mockKittenClawsController.GetListAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var result = await _functions.GetKittenClawsList(Mocks.CreateHttpRequestData("GET"), TestContext.Current.CancellationToken);

        AssertUnexpectedError(result);
    }

    [Fact]
    public async Task CreateKittenClaws_ReturnsCreated_WhenKittenClawsIsCreated()
    {
        _mockKittenClawsController.CreateAsync(Arg.Any<Stream>(), Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(new KittenClawsDto { Id = ItemId, Name = "mockKittenClaws" });

        var result = await _functions.CreateKittenClaws(Mocks.CreateHttpRequestData(new CreateKittenClawsRequest { Name = "mockKittenClaws" }, "POST"), TestContext.Current.CancellationToken);

        Assert.Equal((201, $"{{\"id\":\"{ItemId}\",\"name\":\"mockKittenClaws\"}}"), Mocks.ReadJsonResult(result));
    }

    [Fact]
    public async Task CreateKittenClaws_ReturnsBadRequest_WhenBodyIsInvalid()
    {
        _mockKittenClawsController.CreateAsync(Arg.Any<Stream>(), Arg.Any<string?>(), Arg.Any<CancellationToken>()).Throws(new BadRequestException("name must be a string."));

        var result = await _functions.CreateKittenClaws(Mocks.CreateHttpRequestData("POST"), TestContext.Current.CancellationToken);

        Assert.Equal((400, "{\"errorMessage\":\"name must be a string.\"}"), Mocks.ReadJsonResult(result));
        Assert.Empty(_logger.Entries);
    }

    [Fact]
    public async Task CreateKittenClaws_ReturnsGenericError_WhenExceptionIsThrown()
    {
        _mockKittenClawsController.CreateAsync(Arg.Any<Stream>(), Arg.Any<string?>(), Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var result = await _functions.CreateKittenClaws(Mocks.CreateHttpRequestData(new CreateKittenClawsRequest { Name = "mockKittenClaws" }, "POST"), TestContext.Current.CancellationToken);

        AssertUnexpectedError(result);
    }

    [Fact]
    public async Task UpdateKittenClaws_ReturnsOk_WhenKittenClawsIsUpdated()
    {
        _mockKittenClawsController.UpdateAsync(ItemId, Arg.Any<Stream>(), Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(new KittenClawsDto { Id = ItemId, Name = "mockUpdatedKittenClaws" });

        var result = await _functions.UpdateKittenClaws(Mocks.CreateHttpRequestData(new UpdateKittenClawsRequest { Name = "mockUpdatedKittenClaws" }, "PATCH"), ItemId, TestContext.Current.CancellationToken);

        Assert.Equal((200, $"{{\"id\":\"{ItemId}\",\"name\":\"mockUpdatedKittenClaws\"}}"), Mocks.ReadJsonResult(result));
    }

    [Fact]
    public async Task UpdateKittenClaws_ReturnsGenericError_WhenExceptionIsThrown()
    {
        _mockKittenClawsController.UpdateAsync(ItemId, Arg.Any<Stream>(), Arg.Any<string?>(), Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var result = await _functions.UpdateKittenClaws(Mocks.CreateHttpRequestData(new UpdateKittenClawsRequest { Name = "mockUpdatedKittenClaws" }, "PATCH"), ItemId, TestContext.Current.CancellationToken);

        AssertUnexpectedError(result);
    }

    [Fact]
    public async Task DeleteKittenClaws_ReturnsDeleteMessage_WhenKittenClawsIsDeleted()
    {
        _mockKittenClawsController.DeleteAsync(ItemId, Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(DeleteOkObjectResult.For("KittenClaws", ItemId));

        var result = await _functions.DeleteKittenClaws(Mocks.CreateHttpRequestData("DELETE"), ItemId, TestContext.Current.CancellationToken);

        Assert.Equal((200, $"{{\"message\":\"KittenClaws with id {ItemId} was deleted successfully.\"}}"), Mocks.ReadJsonResult(result));
    }

    [Fact]
    public async Task DeleteKittenClaws_ReturnsGenericError_WhenExceptionIsThrown()
    {
        _mockKittenClawsController.DeleteAsync(ItemId, Arg.Any<string?>(), Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var result = await _functions.DeleteKittenClaws(Mocks.CreateHttpRequestData("DELETE"), ItemId, TestContext.Current.CancellationToken);

        AssertUnexpectedError(result);
    }
}
