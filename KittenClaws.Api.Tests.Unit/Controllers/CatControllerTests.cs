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

public class CatControllerTests
{
    private const string ItemId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
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
        var expectedItem = new CatDto { Id = ItemId, Name = "mockCat" };
        _mockCatService.GetAsync(ItemId, Arg.Any<CancellationToken>()).Returns(expectedItem);

        var result = await _catController.GetAsync(ItemId, TestContext.Current.CancellationToken);

        Assert.Equal(expectedItem, result);
    }

    [Fact]
    public async Task GetAsync_ThrowsNotFound_WhenIdIsNotAUuid()
    {
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _catController.GetAsync("not-a-uuid", TestContext.Current.CancellationToken));

        Assert.Equal("Cat with id not-a-uuid was not found.", exception.Message);
        await _mockCatService.DidNotReceiveWithAnyArgs().GetAsync(default!, default);
    }

    [Theory]
    [InlineData(null, 100)]
    [InlineData("5", 5)]
    [InlineData("invalid", 100)]
    [InlineData("5000", 1000)]
    public async Task GetListAsync_PassesParsedLimitToService(string? limit, int expectedLimit)
    {
        var expectedItemList = new List<CatDto> { new() { Id = ItemId, Name = "mockCat" } };
        _mockCatService.GetListAsync(expectedLimit, Arg.Any<CancellationToken>()).Returns(expectedItemList);

        var result = await _catController.GetListAsync(limit, TestContext.Current.CancellationToken);

        Assert.Equal(expectedItemList, result);
        await _mockCatService.Received(1).GetListAsync(expectedLimit, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_ReturnsCreatedCatDto()
    {
        var expectedItem = new CatDto { Id = ItemId, Name = "mockCreateCat" };
        _mockCatService
            .CreateAsync(Arg.Is<CreateCatRequest>(req => req.Name == "mockCreateCat"), "User1", Arg.Any<CancellationToken>())
            .Returns(expectedItem);

        var result = await _catController.CreateAsync(Mocks.CreateStream("{\"name\":\"mockCreateCat\"}"), "User1", TestContext.Current.CancellationToken);

        Assert.Equal(expectedItem, result);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"name\":\"\"}")]
    public async Task CreateAsync_ThrowsBadRequest_WhenNameIsMissingOrEmpty(string body)
    {
        var exception = await Assert.ThrowsAsync<BadRequestException>(() => _catController.CreateAsync(Mocks.CreateStream(body), null, TestContext.Current.CancellationToken));

        Assert.Equal("name is required and must be a non-empty string.", exception.Message);

        await _mockCatService.DidNotReceiveWithAnyArgs().CreateAsync(default!, default, default);
    }

    [Theory]
    [InlineData("{\"name\":123}")]
    [InlineData("{not json")]
    [InlineData("[]")]
    public async Task CreateAsync_ThrowsBadRequest_WhenBodyIsInvalid(string body)
    {
        await Assert.ThrowsAsync<BadRequestException>(() => _catController.CreateAsync(Mocks.CreateStream(body), null, TestContext.Current.CancellationToken));

        await _mockCatService.DidNotReceiveWithAnyArgs().CreateAsync(default!, default, default);
    }

    [Theory]
    [MemberData(nameof(Mocks.ServerOwnedAndUnknownFields), MemberType = typeof(Mocks))]
    public async Task CreateAsync_ThrowsBadRequest_WhenBodyHasServerOwnedOrUnknownField(string field)
    {
        var exception = await Assert.ThrowsAsync<BadRequestException>(() => _catController.CreateAsync(Mocks.CreateBodyWithField(field), "User1", TestContext.Current.CancellationToken));

        Assert.Equal($"Unknown field: {field}.", exception.Message);
        await _mockCatService.DidNotReceiveWithAnyArgs().CreateAsync(default!, default, default);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsUpdatedCatDto()
    {
        var expectedItem = new CatDto { Id = ItemId, Name = "mockUpdatedCat" };
        _mockCatService
            .UpdateAsync(Arg.Is<UpdateCatRequest>(req => req.Id == ItemId && req.Name == "mockUpdatedCat"), "User1", Arg.Any<CancellationToken>())
            .Returns(expectedItem);

        var result = await _catController.UpdateAsync(ItemId, Mocks.CreateStream("{\"name\":\"mockUpdatedCat\"}"), "User1", TestContext.Current.CancellationToken);

        Assert.Equal(expectedItem, result);
    }

    [Fact]
    public async Task UpdateAsync_AllowsBodyWithoutName()
    {
        await _catController.UpdateAsync(ItemId, Mocks.CreateStream("{}"), "User1", TestContext.Current.CancellationToken);

        await _mockCatService.Received(1).UpdateAsync(Arg.Is<UpdateCatRequest>(req => req.Id == ItemId && req.Name == null), "User1", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_ThrowsBadRequest_WhenNameIsEmpty()
    {
        var exception = await Assert.ThrowsAsync<BadRequestException>(() => _catController.UpdateAsync(ItemId, Mocks.CreateStream("{\"name\":\"\"}"), null, TestContext.Current.CancellationToken));

        Assert.Equal("name must be a non-empty string.", exception.Message);
    }

    [Theory]
    [InlineData("{\"name\":false}")]
    [InlineData("\"mockCat\"")]
    public async Task UpdateAsync_ThrowsBadRequest_WhenBodyIsInvalid(string body)
    {
        await Assert.ThrowsAsync<BadRequestException>(() => _catController.UpdateAsync(ItemId, Mocks.CreateStream(body), null, TestContext.Current.CancellationToken));

        await _mockCatService.DidNotReceiveWithAnyArgs().UpdateAsync(default!, default, default);
    }

    [Theory]
    [MemberData(nameof(Mocks.ServerOwnedAndUnknownFields), MemberType = typeof(Mocks))]
    public async Task UpdateAsync_ThrowsBadRequest_WhenBodyHasServerOwnedOrUnknownField(string field)
    {
        var exception = await Assert.ThrowsAsync<BadRequestException>(() => _catController.UpdateAsync(ItemId, Mocks.CreateBodyWithField(field), "User1", TestContext.Current.CancellationToken));

        Assert.Equal($"Unknown field: {field}.", exception.Message);
        await _mockCatService.DidNotReceiveWithAnyArgs().UpdateAsync(default!, default, default);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsNotFound_WhenIdIsNotAUuid()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _catController.UpdateAsync("not-a-uuid", Mocks.CreateStream("{\"name\":\"mockCat\"}"), null, TestContext.Current.CancellationToken));
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

        await _mockCatService.DidNotReceiveWithAnyArgs().DeleteAsync(default!, default, default);
    }
}
