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

public class CatServiceTests
{
    private const string ItemId = CatSamples.ItemId;
    private readonly ICatRepository _catRepositoryMock;
    private readonly CatService _catService;

    public CatServiceTests()
    {
        _catRepositoryMock = Substitute.For<ICatRepository>();
        _catService = new CatService(_catRepositoryMock);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnCatDto()
    {
        var expectedItem = CatSamples.Dto();
        _catRepositoryMock.GetAsync(ItemId, Arg.Any<CancellationToken>()).Returns(expectedItem);

        var result = await _catService.GetAsync(ItemId, TestContext.Current.CancellationToken);

        Assert.Equal(expectedItem, result);
    }

    [Fact]
    public async Task GetListAsync_ShouldReturnListOfCatDto()
    {
        var expectedItemList = new List<CatDto> { CatSamples.Dto(), CatSamples.Dto() };
        _catRepositoryMock.GetListAsync(25, Arg.Any<CancellationToken>()).Returns(expectedItemList);

        var result = await _catService.GetListAsync(25, TestContext.Current.CancellationToken);

        Assert.Equal(expectedItemList, result);
    }

    [Fact]
    public async Task CreateAsync_StoresTheRequestAsAnEntity()
    {
        var request = await RequestBody.DeserializeAsync<CreateCatRequest>(Mocks.CreateStream(CatSamples.CreateBody), TestContext.Current.CancellationToken);
        var expectedItem = CatSamples.Dto();
        CatEntity? stored = null;
        _catRepositoryMock.CreateAsync(Arg.Do<CatEntity>(item => stored = item), "User1", Arg.Any<CancellationToken>()).Returns(expectedItem);

        var result = await _catService.CreateAsync(request, "User1", TestContext.Current.CancellationToken);

        Assert.Equal(expectedItem, result);
        Assert.NotNull(stored);
        JsonAssert.Contains(request, stored);
    }

    [Fact]
    public async Task UpdateAsync_AppliesTheRequestToTheStoredEntity()
    {
        var request = await RequestBody.DeserializeAsync<UpdateCatRequest>(Mocks.CreateStream(CatSamples.UpdateBody), TestContext.Current.CancellationToken);
        request.Id = ItemId;
        var expectedItem = CatSamples.Dto();
        Action<CatEntity>? apply = null;
        _catRepositoryMock.UpdateAsync(ItemId, Arg.Do<Action<CatEntity>>(action => apply = action), "User1", Arg.Any<CancellationToken>()).Returns(expectedItem);

        var result = await _catService.UpdateAsync(request, "User1", TestContext.Current.CancellationToken);

        Assert.Equal(expectedItem, result);
        Assert.NotNull(apply);
        var item = new CatEntity();
        apply(item);
        JsonAssert.Contains(request, item);
    }

    [Fact]
    public async Task ReplaceAsync_AppliesTheRequestToTheStoredEntity()
    {
        var request = await RequestBody.DeserializeAsync<ReplaceCatRequest>(Mocks.CreateStream(CatSamples.ReplaceBody), TestContext.Current.CancellationToken);
        request.Id = ItemId;
        var expectedItem = CatSamples.Dto();
        Action<CatEntity>? apply = null;
        _catRepositoryMock.ReplaceAsync(ItemId, Arg.Do<Action<CatEntity>>(action => apply = action), "User1", Arg.Any<CancellationToken>()).Returns(expectedItem);

        var result = await _catService.ReplaceAsync(request, "User1", TestContext.Current.CancellationToken);

        Assert.Equal(expectedItem, result);
        Assert.NotNull(apply);
        var item = new CatEntity();
        apply(item);
        JsonAssert.Contains(request, item);
    }

    [Fact]
    public async Task DeleteAsync_ShouldCallRepositoryDelete()
    {
        _catRepositoryMock.DeleteAsync(ItemId, "User1", Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        await _catService.DeleteAsync(ItemId, "User1", TestContext.Current.CancellationToken);

        await _catRepositoryMock.Received(1).DeleteAsync(ItemId, "User1", Arg.Any<CancellationToken>());
    }
}
