namespace KittenClaws.Api.Tests.Unit;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KittenClaws.Api.Dtos;
using KittenClaws.Api.Entities;
using KittenClaws.Api.Interfaces;
using KittenClaws.Api.Requests;
using KittenClaws.Api.Services;
using NSubstitute;
using Xunit;

public class KittenClawsServiceTests
{
    private readonly IKittenClawsRepository _kittenClawsRepositoryMock;
    private readonly KittenClawsService _kittenClawsService;

    public KittenClawsServiceTests()
    {
        _kittenClawsRepositoryMock = Substitute.For<IKittenClawsRepository>();
        _kittenClawsService = new KittenClawsService(_kittenClawsRepositoryMock);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnKittenClawsDto()
    {
        var itemId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var expectedItem = new KittenClawsDto { Id = itemId, Name = "mockKittenClaws" };
        _kittenClawsRepositoryMock.GetAsync(itemId, Arg.Any<CancellationToken>())
            .Returns(expectedItem);

        var result = await _kittenClawsService.GetAsync(itemId, TestContext.Current.CancellationToken);

        Assert.Equal(expectedItem, result);
    }

    [Fact]
    public async Task GetListAsync_ShouldReturnListOfKittenClawsDto()
    {
        var expectedItemList = new List<KittenClawsDto>
        {
            new KittenClawsDto { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockKittenClaws1" },
            new KittenClawsDto { Id = "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name = "mockKittenClaws2" }
        };
        _kittenClawsRepositoryMock.GetListAsync(25, Arg.Any<CancellationToken>())
            .Returns(expectedItemList);

        var result = await _kittenClawsService.GetListAsync(25, TestContext.Current.CancellationToken);

        Assert.Equal(expectedItemList, result);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnCreatedKittenClawsDto()
    {
        var createRequest = new CreateKittenClawsRequest { Name = "mockCreateKittenClaws" };
        var expectedItem = new KittenClawsDto { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = createRequest.Name };
        _kittenClawsRepositoryMock.CreateAsync(Arg.Any<KittenClawsEntity>(), "User1", Arg.Any<CancellationToken>())
            .Returns(expectedItem);

        var result = await _kittenClawsService.CreateAsync(createRequest, "User1", TestContext.Current.CancellationToken);

        Assert.Equal(expectedItem, result);
        await _kittenClawsRepositoryMock.Received(1).CreateAsync(
            Arg.Is<KittenClawsEntity>(entity => entity.Name == createRequest.Name), "User1", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnUpdatedKittenClawsDto()
    {
        var updateRequest = new UpdateKittenClawsRequest { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockUpdateKittenClaws" };
        var updatedKittenClaws = new KittenClawsEntity { Id = updateRequest.Id, Name = updateRequest.Name };
        var expectedItem = new KittenClawsDto { Id = updatedKittenClaws.Id, Name = updatedKittenClaws.Name };
        _kittenClawsRepositoryMock.UpdateAsync(Arg.Any<KittenClawsEntity>(), "User1", Arg.Any<CancellationToken>())
            .Returns(expectedItem);

        var result = await _kittenClawsService.UpdateAsync(updateRequest, "User1", TestContext.Current.CancellationToken);

        Assert.Equal(expectedItem, result);
    }

    [Fact]
    public async Task DeleteAsync_ShouldCallRepositoryDelete()
    {
        var itemId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        _kittenClawsRepositoryMock.DeleteAsync(itemId, "User1", Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        await _kittenClawsService.DeleteAsync(itemId, "User1", TestContext.Current.CancellationToken);

        await _kittenClawsRepositoryMock.Received(1).DeleteAsync(itemId, "User1", Arg.Any<CancellationToken>());
    }
}
