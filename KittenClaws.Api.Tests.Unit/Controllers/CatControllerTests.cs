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

public class CatControllerTests
{
    private const string ItemId = CatSamples.ItemId;

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

    private readonly ICatService _mockCatService;
    private readonly CatController _catController;

    public CatControllerTests()
    {
        _mockCatService = Substitute.For<ICatService>();
        _catController = new CatController(_mockCatService);
    }

    [Fact]
    public async Task GetAsync_ReturnsCatDto()
    {
        var expectedItem = CatSamples.Dto();
        _mockCatService.GetAsync(ItemId, Arg.Any<CancellationToken>()).Returns(expectedItem);

        var result = await _catController.GetAsync(ItemId, TestContext.Current.CancellationToken);

        Assert.Equal(expectedItem, result);
    }

    [Fact]
    public async Task GetAsync_ThrowsNotFound_WhenIdIsNotAUuid()
    {
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _catController.GetAsync("not-a-uuid", TestContext.Current.CancellationToken));

        Assert.Equal("Cat with id not-a-uuid was not found.", exception.Message);
        await _mockCatService.DidNotReceiveWithAnyArgs().GetAsync(default!, TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData(null, 100)]
    [InlineData("5", 5)]
    [InlineData("invalid", 100)]
    [InlineData("5000", 1000)]
    public async Task GetListAsync_PassesParsedLimitToService(string? limit, int expectedLimit)
    {
        var expectedItemList = new List<CatDto> { CatSamples.Dto() };
        _mockCatService.GetListAsync(expectedLimit, Arg.Any<CancellationToken>()).Returns(expectedItemList);

        var result = await _catController.GetListAsync(limit, TestContext.Current.CancellationToken);

        Assert.Equal(expectedItemList, result);
        await _mockCatService.Received(1).GetListAsync(expectedLimit, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_PassesTheValidatedRequest()
    {
        CreateCatRequest? received = null;
        _mockCatService.CreateAsync(Arg.Do<CreateCatRequest>(request => received = request), "User1", Arg.Any<CancellationToken>()).Returns(CatSamples.Dto());

        var result = await _catController.CreateAsync(Mocks.CreateStream(CatSamples.CreateBody), "User1", TestContext.Current.CancellationToken);

        Assert.Equal(ItemId, result.Id);
        Assert.NotNull(received);
        JsonAssert.Equal(CatSamples.CreateBody, received);
    }

    [Fact]
    public async Task CreateAsync_ThrowsBadRequest_WhenARequiredFieldIsMissing()
    {
        var exception = await Assert.ThrowsAsync<BadRequestException>(() => _catController.CreateAsync(Mocks.CreateStream("{}"), null, TestContext.Current.CancellationToken));

        Assert.Equal("name is required.", exception.Message);
    }

    public static TheoryData<string> InvalidCreateBodies =>
        RequestBodies.Invalid(CatSamples.CreateBody, ["name"], [], Rejected);

    [Theory]
    [MemberData(nameof(InvalidCreateBodies))]
    public async Task CreateAsync_ThrowsBadRequest_WhenBodyIsInvalid(string body)
    {
        await Assert.ThrowsAsync<BadRequestException>(() => _catController.CreateAsync(Mocks.CreateStream(body), null, TestContext.Current.CancellationToken));

        await _mockCatService.DidNotReceiveWithAnyArgs().CreateAsync(default!, default, TestContext.Current.CancellationToken);
    }

    public static TheoryData<string> CreateEdgeBodies => RequestBodies.WithEdgeValues(CatSamples.CreateBody, Boundaries);

    [Theory]
    [MemberData(nameof(CreateEdgeBodies))]
    public async Task CreateAsync_AcceptsEdgeValues(string body)
    {
        CreateCatRequest? received = null;
        _mockCatService.CreateAsync(Arg.Do<CreateCatRequest>(request => received = request), null, Arg.Any<CancellationToken>()).Returns(CatSamples.Dto());

        await _catController.CreateAsync(Mocks.CreateStream(body), null, TestContext.Current.CancellationToken);

        JsonAssert.Equal(body, received);
    }

    [Fact]
    public async Task UpdateAsync_PassesTheValidatedRequest()
    {
        UpdateCatRequest? received = null;
        _mockCatService.UpdateAsync(Arg.Do<UpdateCatRequest>(request => received = request), "User1", Arg.Any<CancellationToken>()).Returns(CatSamples.Dto());

        var result = await _catController.UpdateAsync(ItemId, Mocks.CreateStream(CatSamples.UpdateBody), "User1", TestContext.Current.CancellationToken);

        Assert.Equal(ItemId, result.Id);
        Assert.NotNull(received);
        Assert.Equal(ItemId, received.Id);
        Assert.Equal(new string[] { "name" }, received.Sent.Order(StringComparer.Ordinal));
        JsonAssert.Equal(CatSamples.UpdateBody, received);
    }

    [Fact]
    public async Task UpdateAsync_LeavesFieldsTheBodyOmits()
    {
        UpdateCatRequest? received = null;
        _mockCatService.UpdateAsync(Arg.Do<UpdateCatRequest>(request => received = request), null, Arg.Any<CancellationToken>()).Returns(CatSamples.Dto());

        await _catController.UpdateAsync(ItemId, Mocks.CreateStream("{}"), null, TestContext.Current.CancellationToken);

        Assert.NotNull(received);
        Assert.Empty(received.Sent);
    }

    public static TheoryData<string> InvalidUpdateBodies =>
        RequestBodies.Invalid(CatSamples.UpdateBody, [], [], Rejected);

    [Theory]
    [MemberData(nameof(InvalidUpdateBodies))]
    public async Task UpdateAsync_ThrowsBadRequest_WhenBodyIsInvalid(string body)
    {
        await Assert.ThrowsAsync<BadRequestException>(() => _catController.UpdateAsync(ItemId, Mocks.CreateStream(body), null, TestContext.Current.CancellationToken));

        await _mockCatService.DidNotReceiveWithAnyArgs().UpdateAsync(default!, default, TestContext.Current.CancellationToken);
    }

    public static TheoryData<string> UpdateEdgeBodies => RequestBodies.WithEdgeValues(CatSamples.UpdateBody, Boundaries);

    [Theory]
    [MemberData(nameof(UpdateEdgeBodies))]
    public async Task UpdateAsync_AcceptsEdgeValues(string body)
    {
        UpdateCatRequest? received = null;
        _mockCatService.UpdateAsync(Arg.Do<UpdateCatRequest>(request => received = request), null, Arg.Any<CancellationToken>()).Returns(CatSamples.Dto());

        await _catController.UpdateAsync(ItemId, Mocks.CreateStream(body), null, TestContext.Current.CancellationToken);

        JsonAssert.Equal(body, received);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsNotFound_WhenIdIsNotAUuid()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _catController.UpdateAsync("not-a-uuid", Mocks.CreateStream(CatSamples.UpdateBody), null, TestContext.Current.CancellationToken));

        await _mockCatService.DidNotReceiveWithAnyArgs().UpdateAsync(default!, default, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task DeleteAsync_CallsDeleteOnServiceAndReturnsMessage()
    {
        _mockCatService.DeleteAsync(ItemId, "User1", Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var result = await _catController.DeleteAsync(ItemId, "User1", TestContext.Current.CancellationToken);

        Assert.Equal($"Cat with id {ItemId} was deleted successfully.", result.Message);
        await _mockCatService.Received(1).DeleteAsync(ItemId, "User1", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_ThrowsNotFound_WhenIdIsNotAUuid()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _catController.DeleteAsync("not-a-uuid", null, TestContext.Current.CancellationToken));

        await _mockCatService.DidNotReceiveWithAnyArgs().DeleteAsync(default!, default, TestContext.Current.CancellationToken);
    }
}
