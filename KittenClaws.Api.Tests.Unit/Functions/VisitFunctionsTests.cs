namespace KittenClaws.Api.Tests.Unit;

using System;
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

public class VisitFunctionsTests
{
    private const string ItemId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
    private readonly IVisitController _mockVisitController;
    private readonly RecordingLogger<VisitFunctions> _logger = new();
    private readonly VisitFunctions _functions;

    public VisitFunctionsTests()
    {
        _mockVisitController = Substitute.For<IVisitController>();
        _functions = new VisitFunctions(_mockVisitController, _logger);
    }

    private void AssertUnexpectedError(IActionResult result)
    {
        Assert.Equal((500, "{\"errorMessage\":\"An unexpected error occurred.\"}"), Mocks.ReadJsonResult(result));
        Assert.Contains(_logger.Entries, entry => entry.Level == LogLevel.Error && entry.Exception?.Message == "Mock exception");
    }

    [Fact]
    public async Task GetVisit_ReturnsCamelCaseItem_WhenVisitIsFound()
    {
        _mockVisitController.GetAsync(ItemId, Arg.Any<CancellationToken>()).Returns(VisitSamples.Dto());

        var result = await _functions.GetVisit(Mocks.CreateHttpRequestData("GET"), ItemId, TestContext.Current.CancellationToken);

        Assert.Equal((200, Json.Serialize(VisitSamples.Dto())), Mocks.ReadJsonResult(result));
    }

    [Fact]
    public async Task GetVisit_ReturnsNotFound_WithoutLoggingAnError()
    {
        _mockVisitController.GetAsync("not-a-uuid", Arg.Any<CancellationToken>()).Throws(new NotFoundException("Visit", "not-a-uuid"));

        var result = await _functions.GetVisit(Mocks.CreateHttpRequestData("GET"), "not-a-uuid", TestContext.Current.CancellationToken);

        Assert.Equal((404, "{\"errorMessage\":\"Visit with id not-a-uuid was not found.\"}"), Mocks.ReadJsonResult(result));
        Assert.Empty(_logger.Entries);
    }

    [Fact]
    public async Task GetVisit_ReturnsGenericError_WhenExceptionIsThrown()
    {
        _mockVisitController.GetAsync(ItemId, Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var result = await _functions.GetVisit(Mocks.CreateHttpRequestData("GET"), ItemId, TestContext.Current.CancellationToken);

        AssertUnexpectedError(result);
    }

    [Fact]
    public async Task CreateVisit_ReturnsCreated_WhenVisitIsCreated()
    {
        _mockVisitController.CreateAsync(Arg.Any<Stream>(), Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(VisitSamples.Dto());

        var result = await _functions.CreateVisit(Mocks.CreateHttpRequestData(new CreateVisitRequest(), "POST"), TestContext.Current.CancellationToken);

        Assert.Equal((201, Json.Serialize(VisitSamples.Dto())), Mocks.ReadJsonResult(result));
    }

    [Fact]
    public async Task CreateVisit_ReturnsBadRequest_WhenBodyIsInvalid()
    {
        _mockVisitController.CreateAsync(Arg.Any<Stream>(), Arg.Any<string?>(), Arg.Any<CancellationToken>()).Throws(new BadRequestException("Request body must be valid JSON."));

        var result = await _functions.CreateVisit(Mocks.CreateHttpRequestData("POST"), TestContext.Current.CancellationToken);

        Assert.Equal((400, "{\"errorMessage\":\"Request body must be valid JSON.\"}"), Mocks.ReadJsonResult(result));
        Assert.Empty(_logger.Entries);
    }

    [Fact]
    public async Task CreateVisit_ReturnsGenericError_WhenExceptionIsThrown()
    {
        _mockVisitController.CreateAsync(Arg.Any<Stream>(), Arg.Any<string?>(), Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var result = await _functions.CreateVisit(Mocks.CreateHttpRequestData(new CreateVisitRequest(), "POST"), TestContext.Current.CancellationToken);

        AssertUnexpectedError(result);
    }

    [Fact]
    public async Task UpdateVisit_ReturnsOk_WhenVisitIsUpdated()
    {
        _mockVisitController.UpdateAsync(ItemId, Arg.Any<Stream>(), Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(VisitSamples.Dto());

        var result = await _functions.UpdateVisit(Mocks.CreateHttpRequestData(new UpdateVisitRequest(), "PATCH"), ItemId, TestContext.Current.CancellationToken);

        Assert.Equal((200, Json.Serialize(VisitSamples.Dto())), Mocks.ReadJsonResult(result));
    }

    [Fact]
    public async Task UpdateVisit_ReturnsGenericError_WhenExceptionIsThrown()
    {
        _mockVisitController.UpdateAsync(ItemId, Arg.Any<Stream>(), Arg.Any<string?>(), Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        var result = await _functions.UpdateVisit(Mocks.CreateHttpRequestData(new UpdateVisitRequest(), "PATCH"), ItemId, TestContext.Current.CancellationToken);

        AssertUnexpectedError(result);
    }
}
