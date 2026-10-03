namespace KittenClaws.Api.Tests.Unit;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KittenClaws.Api.Controllers;
using KittenClaws.Api.Dtos;
using KittenClaws.Api.Interfaces;
using KittenClaws.Api.Requests;
using KittenClaws.Api.Utils;
using NSubstitute;
using Xunit;

public class DogControllerTests
{
    private const string ItemId = DogSamples.ItemId;

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

    private readonly IDogService _mockDogService;
    private readonly DogController _dogController;

    public DogControllerTests()
    {
        _mockDogService = Substitute.For<IDogService>();
        _dogController = new DogController(_mockDogService);
    }

    [Fact]
    public async Task GetAsync_ReturnsDogDto()
    {
        var expectedItem = DogSamples.Dto();
        _mockDogService.GetAsync(ItemId, Arg.Any<CancellationToken>()).Returns(expectedItem);

        var result = await _dogController.GetAsync(ItemId, TestContext.Current.CancellationToken);

        Assert.Equal(expectedItem, result);
    }

    [Fact]
    public async Task GetAsync_ThrowsNotFound_WhenIdIsNotAUuid()
    {
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _dogController.GetAsync("not-a-uuid", TestContext.Current.CancellationToken));

        Assert.Equal("Dog with id not-a-uuid was not found.", exception.Message);
        await _mockDogService.DidNotReceiveWithAnyArgs().GetAsync(default!, TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData(null, 100)]
    [InlineData("5", 5)]
    [InlineData("invalid", 100)]
    [InlineData("5000", 1000)]
    public async Task GetListAsync_PassesParsedLimitToService(string? limit, int expectedLimit)
    {
        var expectedItemList = new List<DogDto> { DogSamples.Dto() };
        _mockDogService.GetListAsync(expectedLimit, Arg.Any<CancellationToken>()).Returns(expectedItemList);

        var result = await _dogController.GetListAsync(limit, TestContext.Current.CancellationToken);

        Assert.Equal(expectedItemList, result);
        await _mockDogService.Received(1).GetListAsync(expectedLimit, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_PassesTheValidatedRequest()
    {
        CreateDogRequest? received = null;
        _mockDogService.CreateAsync(Arg.Do<CreateDogRequest>(request => received = request), "User1", Arg.Any<CancellationToken>()).Returns(DogSamples.Dto());

        var result = await _dogController.CreateAsync(Mocks.CreateStream(DogSamples.CreateBody), "User1", TestContext.Current.CancellationToken);

        Assert.Equal(ItemId, result.Id);
        Assert.NotNull(received);
        JsonAssert.Equal(DogSamples.CreateBody, received);
    }

    [Fact]
    public async Task CreateAsync_ThrowsBadRequest_WhenARequiredFieldIsMissing()
    {
        var exception = await Assert.ThrowsAsync<BadRequestException>(() => _dogController.CreateAsync(Mocks.CreateStream("{}"), null, TestContext.Current.CancellationToken));

        Assert.Equal("name is required.", exception.Message);
    }

    public static TheoryData<string> InvalidCreateBodies =>
        RequestBodies.Invalid(DogSamples.CreateBody, ["name"], [], Rejected);

    [Theory]
    [MemberData(nameof(InvalidCreateBodies))]
    public async Task CreateAsync_ThrowsBadRequest_WhenBodyIsInvalid(string body)
    {
        await Assert.ThrowsAsync<BadRequestException>(() => _dogController.CreateAsync(Mocks.CreateStream(body), null, TestContext.Current.CancellationToken));

        await _mockDogService.DidNotReceiveWithAnyArgs().CreateAsync(default!, default, TestContext.Current.CancellationToken);
    }

    public static TheoryData<string> CreateEdgeBodies => RequestBodies.WithEdgeValues(DogSamples.CreateBody, Boundaries);

    [Theory]
    [MemberData(nameof(CreateEdgeBodies))]
    public async Task CreateAsync_AcceptsEdgeValues(string body)
    {
        CreateDogRequest? received = null;
        _mockDogService.CreateAsync(Arg.Do<CreateDogRequest>(request => received = request), null, Arg.Any<CancellationToken>()).Returns(DogSamples.Dto());

        await _dogController.CreateAsync(Mocks.CreateStream(body), null, TestContext.Current.CancellationToken);

        JsonAssert.Equal(body, received);
    }

    [Fact]
    public async Task ReplaceAsync_PassesTheValidatedRequest()
    {
        ReplaceDogRequest? received = null;
        _mockDogService.ReplaceAsync(Arg.Do<ReplaceDogRequest>(request => received = request), "User1", Arg.Any<CancellationToken>()).Returns(DogSamples.Dto());

        var result = await _dogController.ReplaceAsync(ItemId, Mocks.CreateStream(DogSamples.ReplaceBody), "User1", TestContext.Current.CancellationToken);

        Assert.Equal(ItemId, result.Id);
        Assert.NotNull(received);
        Assert.Equal(ItemId, received.Id);
        JsonAssert.Equal(DogSamples.ReplaceBody, received);
    }

    [Fact]
    public async Task ReplaceAsync_ThrowsBadRequest_WhenARequiredFieldIsMissing()
    {
        var exception = await Assert.ThrowsAsync<BadRequestException>(() => _dogController.ReplaceAsync(ItemId, Mocks.CreateStream("{}"), null, TestContext.Current.CancellationToken));

        Assert.Equal("name is required.", exception.Message);
    }

    public static TheoryData<string> InvalidReplaceBodies =>
        RequestBodies.Invalid(DogSamples.ReplaceBody, ["name"], [], Rejected);

    [Theory]
    [MemberData(nameof(InvalidReplaceBodies))]
    public async Task ReplaceAsync_ThrowsBadRequest_WhenBodyIsInvalid(string body)
    {
        await Assert.ThrowsAsync<BadRequestException>(() => _dogController.ReplaceAsync(ItemId, Mocks.CreateStream(body), null, TestContext.Current.CancellationToken));

        await _mockDogService.DidNotReceiveWithAnyArgs().ReplaceAsync(default!, default, TestContext.Current.CancellationToken);
    }

    public static TheoryData<string> ReplaceEdgeBodies => RequestBodies.WithEdgeValues(DogSamples.ReplaceBody, Boundaries);

    [Theory]
    [MemberData(nameof(ReplaceEdgeBodies))]
    public async Task ReplaceAsync_AcceptsEdgeValues(string body)
    {
        ReplaceDogRequest? received = null;
        _mockDogService.ReplaceAsync(Arg.Do<ReplaceDogRequest>(request => received = request), null, Arg.Any<CancellationToken>()).Returns(DogSamples.Dto());

        await _dogController.ReplaceAsync(ItemId, Mocks.CreateStream(body), null, TestContext.Current.CancellationToken);

        JsonAssert.Equal(body, received);
    }

    [Fact]
    public async Task ReplaceAsync_ThrowsNotFound_WhenIdIsNotAUuid()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _dogController.ReplaceAsync("not-a-uuid", Mocks.CreateStream(DogSamples.ReplaceBody), null, TestContext.Current.CancellationToken));

        await _mockDogService.DidNotReceiveWithAnyArgs().ReplaceAsync(default!, default, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task DeleteAsync_CallsDeleteOnServiceAndReturnsMessage()
    {
        _mockDogService.DeleteAsync(ItemId, "User1", Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var result = await _dogController.DeleteAsync(ItemId, "User1", TestContext.Current.CancellationToken);

        Assert.Equal($"Dog with id {ItemId} was deleted successfully.", result.Message);
        await _mockDogService.Received(1).DeleteAsync(ItemId, "User1", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_ThrowsNotFound_WhenIdIsNotAUuid()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _dogController.DeleteAsync("not-a-uuid", null, TestContext.Current.CancellationToken));

        await _mockDogService.DidNotReceiveWithAnyArgs().DeleteAsync(default!, default, TestContext.Current.CancellationToken);
    }
}
