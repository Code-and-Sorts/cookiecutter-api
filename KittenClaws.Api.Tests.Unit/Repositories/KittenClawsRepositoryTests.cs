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

public class KittenClawsRepositoryTests
{
    private const string ItemId = KittenClawsSamples.ItemId;
    private readonly IDocumentStore<KittenClawsEntity> _mockStore = Substitute.For<IDocumentStore<KittenClawsEntity>>();
    private readonly KittenClawsRepository _repository;

    public KittenClawsRepositoryTests()
    {
        _repository = new KittenClawsRepository(_mockStore);
    }

    [Fact]
    public async Task GetAsync_ReturnsTheStoredItemAsADto()
    {
        _mockStore.GetAsync(ItemId, Arg.Any<CancellationToken>()).Returns(KittenClawsSamples.Entity());

        var result = await _repository.GetAsync(ItemId, TestContext.Current.CancellationToken);

        Assert.Equal(Json.Serialize(KittenClawsSamples.Dto()), Json.Serialize(result));
    }

    [Fact]
    public async Task GetListAsync_ReturnsTheStoredItemsAsDtos()
    {
        var second = KittenClawsSamples.Entity();
        second.Id = "5615ff05-3032-4459-88ad-b6a4c3e51ca0";
        _mockStore.GetLiveListAsync(2, Arg.Any<CancellationToken>()).Returns(new List<KittenClawsEntity> { KittenClawsSamples.Entity(), second });

        var result = await _repository.GetListAsync(2, TestContext.Current.CancellationToken);

        Assert.Equal(new[] { ItemId, second.Id }, result.Select(dto => dto.Id).ToArray());
        Assert.Equal(Json.Serialize(KittenClawsSamples.Dto()), Json.Serialize(result.First()));
    }

    [Fact]
    public async Task CreateAsync_StoresTheItemWithANewIdAndReturnsItsDto()
    {
        var item = KittenClawsSamples.Entity();

        var result = await _repository.CreateAsync(item, null, TestContext.Current.CancellationToken);

        Assert.NotEqual(ItemId, result.Id);
        await _mockStore.Received(1).CreateAsync(Arg.Is<KittenClawsEntity>(stored => stored.Id == result.Id && !stored.IsDeleted), Arg.Any<CancellationToken>());
        Assert.Equal(Json.Serialize(new KittenClawsDto(item)), Json.Serialize(result));
    }

    private KittenClawsEntity StoreHoldsTheSample()
    {
        var stored = KittenClawsSamples.Entity();
        _mockStore.UpdateAsync(ItemId, Arg.Any<Action<KittenClawsEntity>>(), Arg.Any<CancellationToken>()).Returns(call =>
        {
            call.Arg<Action<KittenClawsEntity>>()(stored);
            return stored;
        });
        return stored;
    }

    [Fact]
    public async Task UpdateAsync_AppliesTheChangesAndKeepsCreatedFields()
    {
        var stored = StoreHoldsTheSample();
        KittenClawsEntity? applied = null;

        var result = await _repository.UpdateAsync(ItemId, item => applied = item, "User1", TestContext.Current.CancellationToken);

        Assert.Same(stored, applied);
        Assert.Equal(ItemId, result.Id);
        Assert.True(stored.CreatedBy == "User2" && stored.UpdatedBy == "User1"
            && stored.CreatedTimestamp == KittenClawsSamples.StoredTimestamp && stored.UpdatedTimestamp != KittenClawsSamples.StoredTimestamp);
    }

    [Fact]
    public async Task DeleteAsync_SoftDeletesTheItem()
    {
        var stored = StoreHoldsTheSample();

        await _repository.DeleteAsync(ItemId, null, TestContext.Current.CancellationToken);

        Assert.True(stored.IsDeleted && stored.CreatedTimestamp == KittenClawsSamples.StoredTimestamp);
    }
}
