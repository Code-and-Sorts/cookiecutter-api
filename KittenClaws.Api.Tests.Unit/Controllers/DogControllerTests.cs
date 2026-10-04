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
    private const string ItemId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
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
        var expectedItem = new DogDto { Id = ItemId, Name = "mockDog" };
        _mockDogService.GetAsync(ItemId, Arg.Any<CancellationToken>()).Returns(expectedItem);

        var result = await _dogController.GetAsync(ItemId, TestContext.Current.CancellationToken);

        Assert.Equal(expectedItem, result);
    }

    [Fact]
    public async Task GetAsync_ThrowsNotFound_WhenIdIsNotAUuid()
    {
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _dogController.GetAsync("not-a-uuid", TestContext.Current.CancellationToken));

        Assert.Equal("Dog with id not-a-uuid was not found.", exception.Message);
        await _mockDogService.DidNotReceiveWithAnyArgs().GetAsync(default!, default);
    }

    [Theory]
    [InlineData(null, 100)]
    [InlineData("5", 5)]
    [InlineData("invalid", 100)]
    [InlineData("5000", 1000)]
    public async Task GetListAsync_PassesParsedLimitToService(string? limit, int expectedLimit)
    {
        var expectedItemList = new List<DogDto> { new() { Id = ItemId, Name = "mockDog" } };
        _mockDogService.GetListAsync(expectedLimit, Arg.Any<CancellationToken>()).Returns(expectedItemList);

        var result = await _dogController.GetListAsync(limit, TestContext.Current.CancellationToken);

        Assert.Equal(expectedItemList, result);
        await _mockDogService.Received(1).GetListAsync(expectedLimit, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_ReturnsCreatedDogDto()
    {
        var expectedItem = new DogDto { Id = ItemId, Name = "mockCreateDog" };
        _mockDogService
            .CreateAsync(Arg.Is<CreateDogRequest>(req => req.Name == "mockCreateDog"), "User1", Arg.Any<CancellationToken>())
            .Returns(expectedItem);

        var result = await _dogController.CreateAsync(Mocks.CreateStream("{\"name\":\"mockCreateDog\"}"), "User1", TestContext.Current.CancellationToken);

        Assert.Equal(expectedItem, result);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"name\":\"\"}")]
    public async Task CreateAsync_ThrowsBadRequest_WhenNameIsMissingOrEmpty(string body)
    {
        var exception = await Assert.ThrowsAsync<BadRequestException>(() => _dogController.CreateAsync(Mocks.CreateStream(body), null, TestContext.Current.CancellationToken));

        Assert.Equal("name is required and must be a non-empty string.", exception.Message);

        await _mockDogService.DidNotReceiveWithAnyArgs().CreateAsync(default!, default, default);
    }

    [Theory]
    [InlineData("{\"name\":123}")]
    [InlineData("{not json")]
    [InlineData("[]")]
    public async Task CreateAsync_ThrowsBadRequest_WhenBodyIsInvalid(string body)
    {
        await Assert.ThrowsAsync<BadRequestException>(() => _dogController.CreateAsync(Mocks.CreateStream(body), null, TestContext.Current.CancellationToken));

        await _mockDogService.DidNotReceiveWithAnyArgs().CreateAsync(default!, default, default);
    }

    [Theory]
    [MemberData(nameof(Mocks.ServerOwnedAndUnknownFields), MemberType = typeof(Mocks))]
    public async Task CreateAsync_ThrowsBadRequest_WhenBodyHasServerOwnedOrUnknownField(string field)
    {
        var exception = await Assert.ThrowsAsync<BadRequestException>(() => _dogController.CreateAsync(Mocks.CreateBodyWithField(field), "User1", TestContext.Current.CancellationToken));

        Assert.Equal($"Unknown field: {field}.", exception.Message);
        await _mockDogService.DidNotReceiveWithAnyArgs().CreateAsync(default!, default, default);
    }

    [Fact]
    public async Task ReplaceAsync_ReturnsReplacedDogDto()
    {
        var expectedItem = new DogDto { Id = ItemId, Name = "mockReplacedDog" };
        _mockDogService
            .ReplaceAsync(Arg.Is<ReplaceDogRequest>(req => req.Id == ItemId && req.Name == "mockReplacedDog"), "User1", Arg.Any<CancellationToken>())
            .Returns(expectedItem);

        var result = await _dogController.ReplaceAsync(ItemId, Mocks.CreateStream("{\"name\":\"mockReplacedDog\"}"), "User1", TestContext.Current.CancellationToken);

        Assert.Equal(expectedItem, result);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"name\":\"\"}")]
    public async Task ReplaceAsync_ThrowsBadRequest_WhenNameIsMissingOrEmpty(string body)
    {
        var exception = await Assert.ThrowsAsync<BadRequestException>(() => _dogController.ReplaceAsync(ItemId, Mocks.CreateStream(body), null, TestContext.Current.CancellationToken));

        Assert.Equal("name is required and must be a non-empty string.", exception.Message);
    }

    [Theory]
    [InlineData("{\"name\":1}")]
    [InlineData("[]")]
    public async Task ReplaceAsync_ThrowsBadRequest_WhenBodyIsInvalid(string body)
    {
        await Assert.ThrowsAsync<BadRequestException>(() => _dogController.ReplaceAsync(ItemId, Mocks.CreateStream(body), null, TestContext.Current.CancellationToken));

        await _mockDogService.DidNotReceiveWithAnyArgs().ReplaceAsync(default!, default, default);
    }

    [Theory]
    [MemberData(nameof(Mocks.ServerOwnedAndUnknownFields), MemberType = typeof(Mocks))]
    public async Task ReplaceAsync_ThrowsBadRequest_WhenBodyHasServerOwnedOrUnknownField(string field)
    {
        var exception = await Assert.ThrowsAsync<BadRequestException>(() => _dogController.ReplaceAsync(ItemId, Mocks.CreateBodyWithField(field), "User1", TestContext.Current.CancellationToken));

        Assert.Equal($"Unknown field: {field}.", exception.Message);
        await _mockDogService.DidNotReceiveWithAnyArgs().ReplaceAsync(default!, default, default);
    }

    [Fact]
    public async Task ReplaceAsync_ThrowsNotFound_WhenIdIsNotAUuid()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _dogController.ReplaceAsync("not-a-uuid", Mocks.CreateStream("{\"name\":\"mockDog\"}"), null, TestContext.Current.CancellationToken));
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

        await _mockDogService.DidNotReceiveWithAnyArgs().DeleteAsync(default!, default, default);
    }
}
