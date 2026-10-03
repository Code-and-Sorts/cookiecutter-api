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

public class DogServiceTests
{
    private const string ItemId = DogSamples.ItemId;
    private readonly IDogRepository _dogRepositoryMock;
    private readonly DogService _dogService;

    public DogServiceTests()
    {
        _dogRepositoryMock = Substitute.For<IDogRepository>();
        _dogService = new DogService(_dogRepositoryMock);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnDogDto()
    {
        var expectedItem = DogSamples.Dto();
        _dogRepositoryMock.GetAsync(ItemId, Arg.Any<CancellationToken>()).Returns(expectedItem);

        var result = await _dogService.GetAsync(ItemId, TestContext.Current.CancellationToken);

        Assert.Equal(expectedItem, result);
    }

    [Fact]
    public async Task GetListAsync_ShouldReturnListOfDogDto()
    {
        var expectedItemList = new List<DogDto> { DogSamples.Dto(), DogSamples.Dto() };
        _dogRepositoryMock.GetListAsync(25, Arg.Any<CancellationToken>()).Returns(expectedItemList);

        var result = await _dogService.GetListAsync(25, TestContext.Current.CancellationToken);

        Assert.Equal(expectedItemList, result);
    }

    [Fact]
    public async Task CreateAsync_StoresTheRequestAsAnEntity()
    {
        var request = await RequestBody.DeserializeAsync<CreateDogRequest>(Mocks.CreateStream(DogSamples.CreateBody), TestContext.Current.CancellationToken);
        var expectedItem = DogSamples.Dto();
        DogEntity? stored = null;
        _dogRepositoryMock.CreateAsync(Arg.Do<DogEntity>(item => stored = item), "User1", Arg.Any<CancellationToken>()).Returns(expectedItem);

        var result = await _dogService.CreateAsync(request, "User1", TestContext.Current.CancellationToken);

        Assert.Equal(expectedItem, result);
        Assert.NotNull(stored);
        JsonAssert.Contains(request, stored);
    }

    [Fact]
    public async Task ReplaceAsync_AppliesTheRequestToTheStoredEntity()
    {
        var request = await RequestBody.DeserializeAsync<ReplaceDogRequest>(Mocks.CreateStream(DogSamples.ReplaceBody), TestContext.Current.CancellationToken);
        request.Id = ItemId;
        var expectedItem = DogSamples.Dto();
        Action<DogEntity>? apply = null;
        _dogRepositoryMock.ReplaceAsync(ItemId, Arg.Do<Action<DogEntity>>(action => apply = action), "User1", Arg.Any<CancellationToken>()).Returns(expectedItem);

        var result = await _dogService.ReplaceAsync(request, "User1", TestContext.Current.CancellationToken);

        Assert.Equal(expectedItem, result);
        Assert.NotNull(apply);
        var item = new DogEntity();
        apply(item);
        JsonAssert.Contains(request, item);
    }

    [Fact]
    public async Task DeleteAsync_ShouldCallRepositoryDelete()
    {
        _dogRepositoryMock.DeleteAsync(ItemId, "User1", Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        await _dogService.DeleteAsync(ItemId, "User1", TestContext.Current.CancellationToken);

        await _dogRepositoryMock.Received(1).DeleteAsync(ItemId, "User1", Arg.Any<CancellationToken>());
    }
}
