namespace KittenClaws.Api.Tests.Unit;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KittenClaws.Api.Dtos;
using KittenClaws.Api.Entities;
using KittenClaws.Api.Interfaces;
using KittenClaws.Api.Requests;
using KittenClaws.Api.Services;
using KittenClaws.Api.Utils;
using NSubstitute;
using Xunit;

public class KittenClawsServiceTests
{
    private const string ItemId = KittenClawsSamples.ItemId;
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
        var expectedItem = KittenClawsSamples.Dto();
        _kittenClawsRepositoryMock.GetAsync(ItemId, Arg.Any<CancellationToken>()).Returns(expectedItem);

        var result = await _kittenClawsService.GetAsync(ItemId, TestContext.Current.CancellationToken);

        Assert.Equal(expectedItem, result);
    }

    [Fact]
    public async Task GetListAsync_ShouldReturnListOfKittenClawsDto()
    {
        var expectedItemList = new List<KittenClawsDto> { KittenClawsSamples.Dto(), KittenClawsSamples.Dto() };
        _kittenClawsRepositoryMock.GetListAsync(25, Arg.Any<CancellationToken>()).Returns(expectedItemList);

        var result = await _kittenClawsService.GetListAsync(25, TestContext.Current.CancellationToken);

        Assert.Equal(expectedItemList, result);
    }

    [Fact]
    public async Task CreateAsync_StoresTheRequestAsAnEntity()
    {
        var request = await RequestBody.DeserializeAsync<CreateKittenClawsRequest>(Mocks.CreateStream(KittenClawsSamples.CreateBody), TestContext.Current.CancellationToken);
        var expectedItem = KittenClawsSamples.Dto();
        KittenClawsEntity? stored = null;
        _kittenClawsRepositoryMock.CreateAsync(Arg.Do<KittenClawsEntity>(item => stored = item), "User1", Arg.Any<CancellationToken>()).Returns(expectedItem);

        var result = await _kittenClawsService.CreateAsync(request, "User1", TestContext.Current.CancellationToken);

        Assert.Equal(expectedItem, result);
        Assert.NotNull(stored);
        JsonAssert.Contains(request, stored);
    }

    [Fact]
    public async Task UpdateAsync_AppliesTheRequestToTheStoredEntity()
    {
        var request = await RequestBody.DeserializeAsync<UpdateKittenClawsRequest>(Mocks.CreateStream(KittenClawsSamples.UpdateBody), TestContext.Current.CancellationToken);
        request.Id = ItemId;
        var expectedItem = KittenClawsSamples.Dto();
        Action<KittenClawsEntity>? apply = null;
        _kittenClawsRepositoryMock.UpdateAsync(ItemId, Arg.Do<Action<KittenClawsEntity>>(action => apply = action), "User1", Arg.Any<CancellationToken>()).Returns(expectedItem);

        var result = await _kittenClawsService.UpdateAsync(request, "User1", TestContext.Current.CancellationToken);

        Assert.Equal(expectedItem, result);
        Assert.NotNull(apply);
        var item = new KittenClawsEntity();
        apply(item);
        JsonAssert.Contains(request, item);
    }

    [Fact]
    public async Task DeleteAsync_ShouldCallRepositoryDelete()
    {
        _kittenClawsRepositoryMock.DeleteAsync(ItemId, "User1", Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        await _kittenClawsService.DeleteAsync(ItemId, "User1", TestContext.Current.CancellationToken);

        await _kittenClawsRepositoryMock.Received(1).DeleteAsync(ItemId, "User1", Arg.Any<CancellationToken>());
    }
}
