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

public class CatFunctionsTests
{
    private const string ItemId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
    private readonly ICatController _mockCatController;
    private readonly RecordingLogger<CatFunctions> _logger = new();
    private readonly CatFunctions _functions;

    public CatFunctionsTests()
    {
        _mockCatController = Substitute.For<ICatController>();
        _functions = new CatFunctions(_mockCatController, _logger);
    }

    private void AssertUnexpectedError(IActionResult result)
    {
        Assert.Equal((500, "{\"errorMessage\":\"An unexpected error occurred.\"}"), Mocks.ReadJsonResult(result));
        Assert.Contains(_logger.Entries, entry => entry.Level == LogLevel.Error && entry.Exception?.Message == "Mock exception");
    }

    [Fact]
    public async Task GetCat_ReturnsCamelCaseItem_WhenCatIsFound()
    {
        _mockCatController.GetAsync(ItemId, Arg.Any<CancellationToken>()).Returns(Mocks.WithAuditFields(new CatDto { Id = ItemId, Name = "mockCat" }));

        var result = await _functions.GetCat(Mocks.CreateHttpRequestData("GET"), ItemId, TestContext.Current.CancellationToken);

        Assert.Equal((200, $"{{\"id\":\"{ItemId}\",\"name\":\"mockCat\",{Mocks.AuditJson()}}}"), Mocks.ReadJsonResult(result));
    }

    [Fact]
    public async Task GetCat_ReturnsNotFound_WithoutLoggingAnError()
    {
        _mockCatController.GetAsync("not-a-uuid", Arg.Any<CancellationToken>()).Throws(new NotFoundException("Cat", "not-a-uuid"));

        var result = await _functions.GetCat(Mocks.CreateHttpRequestData("GET"), "not-a-uuid", TestContext.Current.CancellationToken);

        Assert.Equal((404, "{\"errorMessage\":\"Cat with id not-a-uuid was not found.\"}"), Mocks.ReadJsonResult(result));
        Assert.Empty(_logger.Entries);
    }

    [Fact]
    public async Task GetCat_ReturnsGenericError_WhenExceptionIsThrown()
    {
        _mockCatController.GetAsync(ItemId, Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var result = await _functions.GetCat(Mocks.CreateHttpRequestData("GET"), ItemId, TestContext.Current.CancellationToken);

        AssertUnexpectedError(result);
    }

    [Fact]
    public async Task GetCatList_ReturnsItems()
    {
        _mockCatController.GetListAsync(null, Arg.Any<CancellationToken>()).Returns(new List<CatDto>
        {
            Mocks.WithAuditFields(new CatDto { Id = ItemId, Name = "mockCat1" }),
            Mocks.WithAuditFields(new CatDto { Id = "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name = "mockCat2" }, userId: null),
        });

        var result = await _functions.GetCatList(Mocks.CreateHttpRequestData("GET"), TestContext.Current.CancellationToken);

        Assert.Equal((200, $"[{{\"id\":\"{ItemId}\",\"name\":\"mockCat1\",{Mocks.AuditJson()}}},"
            + $"{{\"id\":\"5615ff05-3032-4459-88ad-b6a4c3e51ca0\",\"name\":\"mockCat2\",{Mocks.AuditJson(userId: null)}}}]"), Mocks.ReadJsonResult(result));
    }

    [Fact]
    public async Task GetCatList_PassesLimitQueryParameter()
    {
        _mockCatController.GetListAsync("5", Arg.Any<CancellationToken>()).Returns(new List<CatDto>());

        var result = await _functions.GetCatList(Mocks.CreateHttpRequestData("GET", "?limit=5"), TestContext.Current.CancellationToken);

        Assert.Equal((200, "[]"), Mocks.ReadJsonResult(result));
        await _mockCatController.Received(1).GetListAsync("5", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetCatList_ReturnsGenericErrorAtTheDeadline_WhenTheDatabaseHangs()
    {
        _mockCatController.GetListAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(new TaskCompletionSource<IEnumerable<CatDto>>().Task);
        var started = DateTime.UtcNow;

        var result = await _functions.GetCatList(Mocks.CreateHttpRequestData("GET"), TestContext.Current.CancellationToken);

        Assert.InRange(DateTime.UtcNow - started, RequestDeadline.Timeout - TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10));
        Assert.Equal(500, Mocks.ReadJsonResult(result).StatusCode);
        Assert.Contains(_logger.Entries, entry => entry.Level == LogLevel.Error);
    }

    [Fact]
    public async Task GetCatList_ReturnsGenericError_WhenExceptionIsThrown()
    {
        _mockCatController.GetListAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var result = await _functions.GetCatList(Mocks.CreateHttpRequestData("GET"), TestContext.Current.CancellationToken);

        AssertUnexpectedError(result);
    }

    [Fact]
    public async Task CreateCat_ReturnsCreated_WhenCatIsCreated()
    {
        _mockCatController.CreateAsync(Arg.Any<Stream>(), Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(Mocks.WithAuditFields(new CatDto { Id = ItemId, Name = "mockCat" }));

        var result = await _functions.CreateCat(Mocks.CreateHttpRequestData(new CreateCatRequest { Name = "mockCat" }, "POST"), TestContext.Current.CancellationToken);

        Assert.Equal((201, $"{{\"id\":\"{ItemId}\",\"name\":\"mockCat\",{Mocks.AuditJson()}}}"), Mocks.ReadJsonResult(result));
    }

    [Fact]
    public async Task CreateCat_ReturnsBadRequest_WhenBodyIsInvalid()
    {
        _mockCatController.CreateAsync(Arg.Any<Stream>(), Arg.Any<string?>(), Arg.Any<CancellationToken>()).Throws(new BadRequestException("name must be a string."));

        var result = await _functions.CreateCat(Mocks.CreateHttpRequestData("POST"), TestContext.Current.CancellationToken);

        Assert.Equal((400, "{\"errorMessage\":\"name must be a string.\"}"), Mocks.ReadJsonResult(result));
        Assert.Empty(_logger.Entries);
    }

    [Fact]
    public async Task CreateCat_ReturnsGenericError_WhenExceptionIsThrown()
    {
        _mockCatController.CreateAsync(Arg.Any<Stream>(), Arg.Any<string?>(), Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var result = await _functions.CreateCat(Mocks.CreateHttpRequestData(new CreateCatRequest { Name = "mockCat" }, "POST"), TestContext.Current.CancellationToken);

        AssertUnexpectedError(result);
    }

    [Fact]
    public async Task UpdateCat_ReturnsOk_WhenCatIsUpdated()
    {
        _mockCatController.UpdateAsync(ItemId, Arg.Any<Stream>(), Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(Mocks.WithAuditFields(new CatDto { Id = ItemId, Name = "mockUpdatedCat" }));

        var result = await _functions.UpdateCat(Mocks.CreateHttpRequestData(new UpdateCatRequest { Name = "mockUpdatedCat" }, "PATCH"), ItemId, TestContext.Current.CancellationToken);

        Assert.Equal((200, $"{{\"id\":\"{ItemId}\",\"name\":\"mockUpdatedCat\",{Mocks.AuditJson()}}}"), Mocks.ReadJsonResult(result));
    }

    [Fact]
    public async Task UpdateCat_ReturnsGenericError_WhenExceptionIsThrown()
    {
        _mockCatController.UpdateAsync(ItemId, Arg.Any<Stream>(), Arg.Any<string?>(), Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var result = await _functions.UpdateCat(Mocks.CreateHttpRequestData(new UpdateCatRequest { Name = "mockUpdatedCat" }, "PATCH"), ItemId, TestContext.Current.CancellationToken);

        AssertUnexpectedError(result);
    }

    [Fact]
    public async Task DeleteCat_ReturnsDeleteMessage_WhenCatIsDeleted()
    {
        _mockCatController.DeleteAsync(ItemId, Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(DeleteOkObjectResult.For("Cat", ItemId));

        var result = await _functions.DeleteCat(Mocks.CreateHttpRequestData("DELETE"), ItemId, TestContext.Current.CancellationToken);

        Assert.Equal((200, $"{{\"message\":\"Cat with id {ItemId} was deleted successfully.\"}}"), Mocks.ReadJsonResult(result));
    }

    [Fact]
    public async Task DeleteCat_ReturnsGenericError_WhenExceptionIsThrown()
    {
        _mockCatController.DeleteAsync(ItemId, Arg.Any<string?>(), Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var result = await _functions.DeleteCat(Mocks.CreateHttpRequestData("DELETE"), ItemId, TestContext.Current.CancellationToken);

        AssertUnexpectedError(result);
    }
}
