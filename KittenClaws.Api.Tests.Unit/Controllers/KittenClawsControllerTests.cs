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

public class KittenClawsControllerTests
{
    private const string ItemId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
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
        var expectedItem = new KittenClawsDto { Id = ItemId, Name = "mockKittenClaws" };
        _mockKittenClawsService.GetAsync(ItemId, Arg.Any<CancellationToken>()).Returns(expectedItem);

        var result = await _kittenClawsController.GetAsync(ItemId, TestContext.Current.CancellationToken);

        Assert.Equal(expectedItem, result);
    }

    [Fact]
    public async Task GetAsync_ThrowsNotFound_WhenIdIsNotAUuid()
    {
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _kittenClawsController.GetAsync("not-a-uuid", TestContext.Current.CancellationToken));

        Assert.Equal("KittenClaws with id not-a-uuid was not found.", exception.Message);
        await _mockKittenClawsService.DidNotReceiveWithAnyArgs().GetAsync(default!, default);
    }

    [Theory]
    [InlineData(null, 100)]
    [InlineData("5", 5)]
    [InlineData("invalid", 100)]
    [InlineData("5000", 1000)]
    public async Task GetListAsync_PassesParsedLimitToService(string? limit, int expectedLimit)
    {
        var expectedItemList = new List<KittenClawsDto> { new() { Id = ItemId, Name = "mockKittenClaws" } };
        _mockKittenClawsService.GetListAsync(expectedLimit, Arg.Any<CancellationToken>()).Returns(expectedItemList);

        var result = await _kittenClawsController.GetListAsync(limit, TestContext.Current.CancellationToken);

        Assert.Equal(expectedItemList, result);
        await _mockKittenClawsService.Received(1).GetListAsync(expectedLimit, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_ReturnsCreatedKittenClawsDto()
    {
        var expectedItem = new KittenClawsDto { Id = ItemId, Name = "mockCreateKittenClaws" };
        _mockKittenClawsService
            .CreateAsync(Arg.Is<CreateKittenClawsRequest>(req => req.Name == "mockCreateKittenClaws"), "User1", Arg.Any<CancellationToken>())
            .Returns(expectedItem);

        var result = await _kittenClawsController.CreateAsync(Mocks.CreateStream("{\"name\":\"mockCreateKittenClaws\"}"), "User1", TestContext.Current.CancellationToken);

        Assert.Equal(expectedItem, result);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"name\":\"\"}")]
    public async Task CreateAsync_ThrowsBadRequest_WhenNameIsMissingOrEmpty(string body)
    {
        var exception = await Assert.ThrowsAsync<BadRequestException>(() => _kittenClawsController.CreateAsync(Mocks.CreateStream(body), null, TestContext.Current.CancellationToken));

        Assert.Equal("name is required and must be a non-empty string.", exception.Message);

        await _mockKittenClawsService.DidNotReceiveWithAnyArgs().CreateAsync(default!, default, default);
    }

    [Theory]
    [InlineData("{\"name\":123}")]
    [InlineData("{\"name\":\"mockKittenClaws\",\"id\":\"0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c\"}")]
    [InlineData("{\"name\":\"mockKittenClaws\",\"updatedBy\":\"me\"}")]
    [InlineData("{not json")]
    [InlineData("[]")]
    public async Task CreateAsync_ThrowsBadRequest_WhenBodyIsInvalid(string body)
    {
        await Assert.ThrowsAsync<BadRequestException>(() => _kittenClawsController.CreateAsync(Mocks.CreateStream(body), null, TestContext.Current.CancellationToken));

        await _mockKittenClawsService.DidNotReceiveWithAnyArgs().CreateAsync(default!, default, default);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsUpdatedKittenClawsDto()
    {
        var expectedItem = new KittenClawsDto { Id = ItemId, Name = "mockUpdatedKittenClaws" };
        _mockKittenClawsService
            .UpdateAsync(Arg.Is<UpdateKittenClawsRequest>(req => req.Id == ItemId && req.Name == "mockUpdatedKittenClaws"), "User1", Arg.Any<CancellationToken>())
            .Returns(expectedItem);

        var result = await _kittenClawsController.UpdateAsync(ItemId, Mocks.CreateStream("{\"name\":\"mockUpdatedKittenClaws\"}"), "User1", TestContext.Current.CancellationToken);

        Assert.Equal(expectedItem, result);
    }

    [Fact]
    public async Task UpdateAsync_AllowsBodyWithoutName()
    {
        await _kittenClawsController.UpdateAsync(ItemId, Mocks.CreateStream("{}"), "User1", TestContext.Current.CancellationToken);

        await _mockKittenClawsService.Received(1).UpdateAsync(Arg.Is<UpdateKittenClawsRequest>(req => req.Id == ItemId && req.Name == null), "User1", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_ThrowsBadRequest_WhenNameIsEmpty()
    {
        var exception = await Assert.ThrowsAsync<BadRequestException>(() => _kittenClawsController.UpdateAsync(ItemId, Mocks.CreateStream("{\"name\":\"\"}"), null, TestContext.Current.CancellationToken));

        Assert.Equal("name must be a non-empty string.", exception.Message);
    }

    [Theory]
    [InlineData("{\"name\":false}")]
    [InlineData("{\"createdTimestamp\":\"2026-01-01T00:00:00.000Z\"}")]
    [InlineData("\"mockKittenClaws\"")]
    public async Task UpdateAsync_ThrowsBadRequest_WhenBodyIsInvalid(string body)
    {
        await Assert.ThrowsAsync<BadRequestException>(() => _kittenClawsController.UpdateAsync(ItemId, Mocks.CreateStream(body), null, TestContext.Current.CancellationToken));

        await _mockKittenClawsService.DidNotReceiveWithAnyArgs().UpdateAsync(default!, default, default);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsNotFound_WhenIdIsNotAUuid()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _kittenClawsController.UpdateAsync("not-a-uuid", Mocks.CreateStream("{\"name\":\"mockKittenClaws\"}"), null, TestContext.Current.CancellationToken));
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

        await _mockKittenClawsService.DidNotReceiveWithAnyArgs().DeleteAsync(default!, default, default);
    }
}
