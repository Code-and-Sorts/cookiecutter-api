namespace KittenClaws.Api.Tests.Unit;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KittenClaws.Api.Dtos;
using KittenClaws.Api.Entities;
using KittenClaws.Api.Interfaces;
using KittenClaws.Api.Repositories;
using KittenClaws.Api.Utils;
using NSubstitute;
using Xunit;

public class DogRepositoryTests
{
    private const string ItemId = DogSamples.ItemId;
    private readonly IDocumentStore<DogEntity> _mockStore = Substitute.For<IDocumentStore<DogEntity>>();
    private readonly DogRepository _repository;

    public DogRepositoryTests()
    {
        _repository = new DogRepository(_mockStore);
    }

    [Fact]
    public async Task GetAsync_ReturnsTheStoredItemAsADto()
    {
        _mockStore.GetAsync(ItemId, Arg.Any<CancellationToken>()).Returns(DogSamples.Entity());

        var result = await _repository.GetAsync(ItemId, TestContext.Current.CancellationToken);

        Assert.Equal(Json.Serialize(DogSamples.Dto()), Json.Serialize(result));
    }

    [Fact]
    public async Task GetListAsync_ReturnsTheStoredItemsAsDtos()
    {
        var second = DogSamples.Entity();
        second.Id = "5615ff05-3032-4459-88ad-b6a4c3e51ca0";
        _mockStore.GetLiveListAsync(2, Arg.Any<CancellationToken>()).Returns(new List<DogEntity> { DogSamples.Entity(), second });

        var result = await _repository.GetListAsync(2, TestContext.Current.CancellationToken);

        Assert.Equal(new[] { ItemId, second.Id }, result.Select(dto => dto.Id).ToArray());
        Assert.Equal(Json.Serialize(DogSamples.Dto()), Json.Serialize(result.First()));
    }

    [Fact]
    public async Task CreateAsync_StoresTheItemWithANewIdAndReturnsItsDto()
    {
        var item = DogSamples.Entity();

        var result = await _repository.CreateAsync(item, null, TestContext.Current.CancellationToken);

        Assert.NotEqual(ItemId, result.Id);
        await _mockStore.Received(1).CreateAsync(Arg.Is<DogEntity>(stored => stored.Id == result.Id && !stored.IsDeleted), Arg.Any<CancellationToken>());
        Assert.Equal(Json.Serialize(new DogDto(item)), Json.Serialize(result));
    }

    private DogEntity StoreHoldsTheSample()
    {
        var stored = DogSamples.Entity();
        _mockStore.UpdateAsync(ItemId, Arg.Any<Action<DogEntity>>(), Arg.Any<CancellationToken>()).Returns(call =>
        {
            call.Arg<Action<DogEntity>>()(stored);
            return stored;
        });
        return stored;
    }

    [Fact]
    public async Task ReplaceAsync_AppliesTheChangesAndKeepsCreatedFields()
    {
        var stored = StoreHoldsTheSample();
        DogEntity? applied = null;

        var result = await _repository.ReplaceAsync(ItemId, item => applied = item, "User1", TestContext.Current.CancellationToken);

        Assert.Same(stored, applied);
        Assert.Equal(ItemId, result.Id);
        Assert.True(stored.CreatedBy == "User2" && stored.UpdatedBy == "User1"
            && stored.CreatedTimestamp == DogSamples.StoredTimestamp && stored.UpdatedTimestamp != DogSamples.StoredTimestamp);
    }

    [Fact]
    public async Task DeleteAsync_SoftDeletesTheItem()
    {
        var stored = StoreHoldsTheSample();

        await _repository.DeleteAsync(ItemId, null, TestContext.Current.CancellationToken);

        Assert.True(stored.IsDeleted && stored.CreatedTimestamp == DogSamples.StoredTimestamp);
    }
}
