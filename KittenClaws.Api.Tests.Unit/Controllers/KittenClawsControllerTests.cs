namespace KittenClaws.Api.Tests.Unit;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KittenClaws.Api.Controllers;
using KittenClaws.Api.Dtos;
using KittenClaws.Api.Interfaces;
using KittenClaws.Api.Requests;
using KittenClaws.Api.Utils;
using NSubstitute;
using Xunit;

public class KittenClawsControllerTests
{
    private const string ItemId = KittenClawsSamples.ItemId;

    private static readonly (string Name, string Value)[] Rejected =
    [
        ("name", "42"),
        ("name", "null"),
        ("name", "\"\""),
        ("name", "\"\""),
    ];

    private static readonly (string Name, string Value)[] Boundaries =
    [
        ("name", "\"s\""),
        ("name", "\"\\ud83d\\ude3a\""),
    ];

    private readonly IKittenClawsService _mockKittenClawsService;
    private readonly KittenClawsController _kittenClawsController;

    public KittenClawsControllerTests()
    {
        _mockKittenClawsService = Substitute.For<IKittenClawsService>();
        _kittenClawsController = new KittenClawsController(_mockKittenClawsService);
    }

    [Fact]
    public async Task GetAsync_ReturnsKittenClawsDto()
    {
        var expectedItem = KittenClawsSamples.Dto();
        _mockKittenClawsService.GetAsync(ItemId, Arg.Any<CancellationToken>()).Returns(expectedItem);

        var result = await _kittenClawsController.GetAsync(ItemId, TestContext.Current.CancellationToken);

        Assert.Equal(expectedItem, result);
    }

    [Fact]
    public async Task GetAsync_ThrowsNotFound_WhenIdIsNotAUuid()
    {
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _kittenClawsController.GetAsync("not-a-uuid", TestContext.Current.CancellationToken));

        Assert.Equal("KittenClaws with id not-a-uuid was not found.", exception.Message);
        await _mockKittenClawsService.DidNotReceiveWithAnyArgs().GetAsync(default!, TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData(null, 100)]
    [InlineData("5", 5)]
    [InlineData("invalid", 100)]
    [InlineData("5000", 1000)]
    public async Task GetListAsync_PassesParsedLimitToService(string? limit, int expectedLimit)
    {
        var expectedItemList = new List<KittenClawsDto> { KittenClawsSamples.Dto() };
        _mockKittenClawsService.GetListAsync(expectedLimit, Arg.Any<CancellationToken>()).Returns(expectedItemList);

        var result = await _kittenClawsController.GetListAsync(limit, TestContext.Current.CancellationToken);

        Assert.Equal(expectedItemList, result);
        await _mockKittenClawsService.Received(1).GetListAsync(expectedLimit, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_PassesTheValidatedRequest()
    {
        CreateKittenClawsRequest? received = null;
        _mockKittenClawsService.CreateAsync(Arg.Do<CreateKittenClawsRequest>(request => received = request), "User1", Arg.Any<CancellationToken>()).Returns(KittenClawsSamples.Dto());

        var result = await _kittenClawsController.CreateAsync(Mocks.CreateStream(KittenClawsSamples.CreateBody), "User1", TestContext.Current.CancellationToken);

        Assert.Equal(ItemId, result.Id);
        Assert.NotNull(received);
        JsonAssert.Equal(KittenClawsSamples.CreateBody, received);
    }

    [Fact]
    public async Task CreateAsync_ThrowsBadRequest_WhenARequiredFieldIsMissing()
    {
        var exception = await Assert.ThrowsAsync<BadRequestException>(() => _kittenClawsController.CreateAsync(Mocks.CreateStream("{}"), null, TestContext.Current.CancellationToken));

        Assert.Equal("name is required.", exception.Message);
    }

    public static TheoryData<string> InvalidCreateBodies =>
        RequestBodies.Invalid(KittenClawsSamples.CreateBody, ["name"], [], Rejected);

    [Theory]
    [MemberData(nameof(InvalidCreateBodies))]
    public async Task CreateAsync_ThrowsBadRequest_WhenBodyIsInvalid(string body)
    {
        await Assert.ThrowsAsync<BadRequestException>(() => _kittenClawsController.CreateAsync(Mocks.CreateStream(body), null, TestContext.Current.CancellationToken));

        await _mockKittenClawsService.DidNotReceiveWithAnyArgs().CreateAsync(default!, default, TestContext.Current.CancellationToken);
    }

    public static TheoryData<string> CreateEdgeBodies => RequestBodies.WithEdgeValues(KittenClawsSamples.CreateBody, Boundaries);

    [Theory]
    [MemberData(nameof(CreateEdgeBodies))]
    public async Task CreateAsync_AcceptsEdgeValues(string body)
    {
        CreateKittenClawsRequest? received = null;
        _mockKittenClawsService.CreateAsync(Arg.Do<CreateKittenClawsRequest>(request => received = request), null, Arg.Any<CancellationToken>()).Returns(KittenClawsSamples.Dto());

        await _kittenClawsController.CreateAsync(Mocks.CreateStream(body), null, TestContext.Current.CancellationToken);

        JsonAssert.Equal(body, received);
    }

    [Fact]
    public async Task UpdateAsync_PassesTheValidatedRequest()
    {
        UpdateKittenClawsRequest? received = null;
        _mockKittenClawsService.UpdateAsync(Arg.Do<UpdateKittenClawsRequest>(request => received = request), "User1", Arg.Any<CancellationToken>()).Returns(KittenClawsSamples.Dto());

        var result = await _kittenClawsController.UpdateAsync(ItemId, Mocks.CreateStream(KittenClawsSamples.UpdateBody), "User1", TestContext.Current.CancellationToken);

        Assert.Equal(ItemId, result.Id);
        Assert.NotNull(received);
        Assert.Equal(ItemId, received.Id);
        Assert.Equal(new string[] { "name" }, received.Sent.Order(StringComparer.Ordinal));
        JsonAssert.Equal(KittenClawsSamples.UpdateBody, received);
    }

    [Fact]
    public async Task UpdateAsync_LeavesFieldsTheBodyOmits()
    {
        UpdateKittenClawsRequest? received = null;
        _mockKittenClawsService.UpdateAsync(Arg.Do<UpdateKittenClawsRequest>(request => received = request), null, Arg.Any<CancellationToken>()).Returns(KittenClawsSamples.Dto());

        await _kittenClawsController.UpdateAsync(ItemId, Mocks.CreateStream("{}"), null, TestContext.Current.CancellationToken);

        Assert.NotNull(received);
        Assert.Empty(received.Sent);
    }

    public static TheoryData<string> InvalidUpdateBodies =>
        RequestBodies.Invalid(KittenClawsSamples.UpdateBody, [], [], Rejected);

    [Theory]
    [MemberData(nameof(InvalidUpdateBodies))]
    public async Task UpdateAsync_ThrowsBadRequest_WhenBodyIsInvalid(string body)
    {
        await Assert.ThrowsAsync<BadRequestException>(() => _kittenClawsController.UpdateAsync(ItemId, Mocks.CreateStream(body), null, TestContext.Current.CancellationToken));

        await _mockKittenClawsService.DidNotReceiveWithAnyArgs().UpdateAsync(default!, default, TestContext.Current.CancellationToken);
    }

    public static TheoryData<string> UpdateEdgeBodies => RequestBodies.WithEdgeValues(KittenClawsSamples.UpdateBody, Boundaries);

    [Theory]
    [MemberData(nameof(UpdateEdgeBodies))]
    public async Task UpdateAsync_AcceptsEdgeValues(string body)
    {
        UpdateKittenClawsRequest? received = null;
        _mockKittenClawsService.UpdateAsync(Arg.Do<UpdateKittenClawsRequest>(request => received = request), null, Arg.Any<CancellationToken>()).Returns(KittenClawsSamples.Dto());

        await _kittenClawsController.UpdateAsync(ItemId, Mocks.CreateStream(body), null, TestContext.Current.CancellationToken);

        JsonAssert.Equal(body, received);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsNotFound_WhenIdIsNotAUuid()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _kittenClawsController.UpdateAsync("not-a-uuid", Mocks.CreateStream(KittenClawsSamples.UpdateBody), null, TestContext.Current.CancellationToken));

        await _mockKittenClawsService.DidNotReceiveWithAnyArgs().UpdateAsync(default!, default, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task DeleteAsync_CallsDeleteOnServiceAndReturnsMessage()
    {
        _mockKittenClawsService.DeleteAsync(ItemId, "User1", Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var result = await _kittenClawsController.DeleteAsync(ItemId, "User1", TestContext.Current.CancellationToken);

        Assert.Equal($"KittenClaws with id {ItemId} was deleted successfully.", result.Message);
        await _mockKittenClawsService.Received(1).DeleteAsync(ItemId, "User1", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_ThrowsNotFound_WhenIdIsNotAUuid()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _kittenClawsController.DeleteAsync("not-a-uuid", null, TestContext.Current.CancellationToken));

        await _mockKittenClawsService.DidNotReceiveWithAnyArgs().DeleteAsync(default!, default, TestContext.Current.CancellationToken);
    }
}
