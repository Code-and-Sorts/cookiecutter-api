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

public class CatRepositoryTests
{
    private const string ItemId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
    private const string StoredTimestamp = "2026-01-01T00:00:00.000Z";
    private readonly IDocumentStore<CatEntity> _mockStore = Substitute.For<IDocumentStore<CatEntity>>();
    private readonly CatRepository _repository;

    public CatRepositoryTests()
    {
        _repository = new CatRepository(_mockStore);
    }

    private static CatEntity StoredItem(string name = "mockCat") => new()
    {
        Id = ItemId,
        Name = name,
        CreatedTimestamp = StoredTimestamp,
        UpdatedTimestamp = StoredTimestamp,
        CreatedBy = "User2",
    };

    private CatEntity StoreHolds(CatEntity stored)
    {
        _mockStore.UpdateAsync(ItemId, Arg.Any<Action<CatEntity>>(), Arg.Any<CancellationToken>()).Returns(call =>
        {
            call.Arg<Action<CatEntity>>()(stored);
            return stored;
        });
        return stored;
    }

    [Fact]
    public async Task GetAsync_ShouldReturnCatDto()
    {
        _mockStore.GetAsync(ItemId, Arg.Any<CancellationToken>()).Returns(StoredItem());

        var result = await _repository.GetAsync(ItemId, TestContext.Current.CancellationToken);

        Assert.Equal((ItemId, "mockCat", StoredTimestamp, "User2", StoredTimestamp, null),
            (result.Id, result.Name, result.CreatedTimestamp, result.CreatedBy, result.UpdatedTimestamp, result.UpdatedBy));
    }

    [Fact]
    public async Task GetListAsync_ShouldReturnCatDtos()
    {
        _mockStore.GetLiveListAsync(2, Arg.Any<CancellationToken>()).Returns(new List<CatEntity> { StoredItem("mockCat1"), StoredItem("mockCat2") });

        var result = await _repository.GetListAsync(2, TestContext.Current.CancellationToken);

        Assert.Equal(new[] { "mockCat1", "mockCat2" }, result.Select(dto => dto.Name).ToArray());
    }

    [Fact]
    public async Task CreateAsync_ShouldStoreItemAndReturnCatDto()
    {
        var result = await _repository.CreateAsync(new CatEntity { Name = "mockCat" }, "User1", TestContext.Current.CancellationToken);

        Assert.True(result.Name == "mockCat" && result.CreatedBy == "User1" && result.UpdatedBy == "User1");
        Assert.Equal(result.CreatedTimestamp, result.UpdatedTimestamp);
        await _mockStore.Received(1).CreateAsync(Arg.Is<CatEntity>(k => k.Id == result.Id && k.Name == "mockCat"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_ShouldChangeNameAndKeepCreatedFields()
    {
        var stored = StoreHolds(StoredItem("mockCatOld"));

        var result = await _repository.UpdateAsync(new CatEntity { Id = ItemId, Name = "mockCatNew" }, "User1", TestContext.Current.CancellationToken);

        Assert.Equal((ItemId, "mockCatNew", StoredTimestamp, "User2", stored.UpdatedTimestamp, "User1"),
            (result.Id, result.Name, result.CreatedTimestamp, result.CreatedBy, result.UpdatedTimestamp, result.UpdatedBy));
        Assert.NotEqual(StoredTimestamp, result.UpdatedTimestamp);
        Assert.True(stored.Name == "mockCatNew" && stored.CreatedBy == "User2" && stored.CreatedTimestamp == StoredTimestamp);
    }

    [Fact]
    public async Task UpdateAsync_ShouldKeepStoredName_WhenNameIsNotGiven()
    {
        StoreHolds(StoredItem("mockCatOld"));

        var result = await _repository.UpdateAsync(new CatEntity { Id = ItemId, Name = null! }, null, TestContext.Current.CancellationToken);

        Assert.Equal("mockCatOld", result.Name);
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteCat()
    {
        var stored = StoreHolds(StoredItem());

        await _repository.DeleteAsync(ItemId, null, TestContext.Current.CancellationToken);

        Assert.True(stored.IsDeleted && stored.CreatedTimestamp == StoredTimestamp);
    }
}
