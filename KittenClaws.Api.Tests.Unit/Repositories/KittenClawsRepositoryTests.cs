namespace KittenClaws.Api.Tests.Unit;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KittenClaws.Api.Entities;
using KittenClaws.Api.Interfaces;
using KittenClaws.Api.Repositories;
using NSubstitute;
using Xunit;

public class KittenClawsRepositoryTests
{
    private const string ItemId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
    private const string StoredTimestamp = "2026-01-01T00:00:00.000Z";
    private readonly IDocumentStore<KittenClawsEntity> _mockStore = Substitute.For<IDocumentStore<KittenClawsEntity>>();
    private readonly KittenClawsRepository _repository;

    public KittenClawsRepositoryTests()
    {
        _repository = new KittenClawsRepository(_mockStore);
    }

    private static KittenClawsEntity StoredItem(string name = "mockKittenClaws") => new()
    {
        Id = ItemId,
        Name = name,
        CreatedTimestamp = StoredTimestamp,
        UpdatedTimestamp = StoredTimestamp,
        CreatedBy = "User2",
    };

    private KittenClawsEntity StoreHolds(KittenClawsEntity stored)
    {
        _mockStore.UpdateAsync(ItemId, Arg.Any<Action<KittenClawsEntity>>(), Arg.Any<CancellationToken>()).Returns(call =>
        {
            call.Arg<Action<KittenClawsEntity>>()(stored);
            return stored;
        });
        return stored;
    }

    [Fact]
    public async Task GetAsync_ShouldReturnKittenClawsDto()
    {
        _mockStore.GetAsync(ItemId, Arg.Any<CancellationToken>()).Returns(StoredItem());

        var result = await _repository.GetAsync(ItemId, TestContext.Current.CancellationToken);

        Assert.Equal((ItemId, "mockKittenClaws", StoredTimestamp, "User2", StoredTimestamp, null),
            (result.Id, result.Name, result.CreatedTimestamp, result.CreatedBy, result.UpdatedTimestamp, result.UpdatedBy));
    }

    [Fact]
    public async Task GetListAsync_ShouldReturnKittenClawsDtos()
    {
        _mockStore.GetLiveListAsync(2, Arg.Any<CancellationToken>()).Returns(new List<KittenClawsEntity> { StoredItem("mockKittenClaws1"), StoredItem("mockKittenClaws2") });

        var result = await _repository.GetListAsync(2, TestContext.Current.CancellationToken);

        Assert.Equal(new[] { "mockKittenClaws1", "mockKittenClaws2" }, result.Select(dto => dto.Name).ToArray());
    }

    [Fact]
    public async Task CreateAsync_ShouldStoreItemAndReturnKittenClawsDto()
    {
        var result = await _repository.CreateAsync(new KittenClawsEntity { Name = "mockKittenClaws" }, "User1", TestContext.Current.CancellationToken);

        Assert.True(result.Name == "mockKittenClaws" && result.CreatedBy == "User1" && result.UpdatedBy == "User1");
        Assert.Equal(result.CreatedTimestamp, result.UpdatedTimestamp);
        await _mockStore.Received(1).CreateAsync(Arg.Is<KittenClawsEntity>(k => k.Id == result.Id && k.Name == "mockKittenClaws"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_ShouldChangeNameAndKeepCreatedFields()
    {
        var stored = StoreHolds(StoredItem("mockKittenClawsOld"));

        var result = await _repository.UpdateAsync(new KittenClawsEntity { Id = ItemId, Name = "mockKittenClawsNew" }, "User1", TestContext.Current.CancellationToken);

        Assert.Equal((ItemId, "mockKittenClawsNew", StoredTimestamp, "User2", stored.UpdatedTimestamp, "User1"),
            (result.Id, result.Name, result.CreatedTimestamp, result.CreatedBy, result.UpdatedTimestamp, result.UpdatedBy));
        Assert.NotEqual(StoredTimestamp, result.UpdatedTimestamp);
        Assert.True(stored.Name == "mockKittenClawsNew" && stored.CreatedBy == "User2" && stored.CreatedTimestamp == StoredTimestamp);
    }

    [Fact]
    public async Task UpdateAsync_ShouldKeepStoredName_WhenNameIsNotGiven()
    {
        StoreHolds(StoredItem("mockKittenClawsOld"));

        var result = await _repository.UpdateAsync(new KittenClawsEntity { Id = ItemId, Name = null! }, null, TestContext.Current.CancellationToken);

        Assert.Equal("mockKittenClawsOld", result.Name);
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteKittenClaws()
    {
        var stored = StoreHolds(StoredItem());

        await _repository.DeleteAsync(ItemId, null, TestContext.Current.CancellationToken);

        Assert.True(stored.IsDeleted && stored.CreatedTimestamp == StoredTimestamp);
    }
}
