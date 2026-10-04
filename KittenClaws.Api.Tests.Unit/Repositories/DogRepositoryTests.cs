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

public class DogRepositoryTests
{
    private const string ItemId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
    private const string StoredTimestamp = "2026-01-01T00:00:00.000Z";
    private readonly IDocumentStore<DogEntity> _mockStore = Substitute.For<IDocumentStore<DogEntity>>();
    private readonly DogRepository _repository;

    public DogRepositoryTests()
    {
        _repository = new DogRepository(_mockStore);
    }

    private static DogEntity StoredItem(string name = "mockDog") => new()
    {
        Id = ItemId,
        Name = name,
        CreatedTimestamp = StoredTimestamp,
        UpdatedTimestamp = StoredTimestamp,
        CreatedBy = "User2",
    };

    private DogEntity StoreHolds(DogEntity stored)
    {
        _mockStore.UpdateAsync(ItemId, Arg.Any<Action<DogEntity>>(), Arg.Any<CancellationToken>()).Returns(call =>
        {
            call.Arg<Action<DogEntity>>()(stored);
            return stored;
        });
        return stored;
    }

    [Fact]
    public async Task GetAsync_ShouldReturnDogDto()
    {
        _mockStore.GetAsync(ItemId, Arg.Any<CancellationToken>()).Returns(StoredItem());

        var result = await _repository.GetAsync(ItemId, TestContext.Current.CancellationToken);

        Assert.Equal((ItemId, "mockDog", StoredTimestamp, "User2", StoredTimestamp, null),
            (result.Id, result.Name, result.CreatedTimestamp, result.CreatedBy, result.UpdatedTimestamp, result.UpdatedBy));
    }

    [Fact]
    public async Task GetListAsync_ShouldReturnDogDtos()
    {
        _mockStore.GetLiveListAsync(2, Arg.Any<CancellationToken>()).Returns(new List<DogEntity> { StoredItem("mockDog1"), StoredItem("mockDog2") });

        var result = await _repository.GetListAsync(2, TestContext.Current.CancellationToken);

        Assert.Equal(new[] { "mockDog1", "mockDog2" }, result.Select(dto => dto.Name).ToArray());
    }

    [Fact]
    public async Task CreateAsync_ShouldStoreItemAndReturnDogDto()
    {
        var result = await _repository.CreateAsync(new DogEntity { Name = "mockDog" }, "User1", TestContext.Current.CancellationToken);

        Assert.True(result.Name == "mockDog" && result.CreatedBy == "User1" && result.UpdatedBy == "User1");
        Assert.Equal(result.CreatedTimestamp, result.UpdatedTimestamp);
        await _mockStore.Received(1).CreateAsync(Arg.Is<DogEntity>(k => k.Id == result.Id && k.Name == "mockDog"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReplaceAsync_ShouldChangeNameAndKeepCreatedFields()
    {
        var stored = StoreHolds(StoredItem("mockDogOld"));

        var result = await _repository.ReplaceAsync(new DogEntity { Id = ItemId, Name = "mockDogNew" }, "User1", TestContext.Current.CancellationToken);

        Assert.Equal((ItemId, "mockDogNew", StoredTimestamp, "User2", stored.UpdatedTimestamp, "User1"),
            (result.Id, result.Name, result.CreatedTimestamp, result.CreatedBy, result.UpdatedTimestamp, result.UpdatedBy));
        Assert.NotEqual(StoredTimestamp, result.UpdatedTimestamp);
        Assert.True(stored.Name == "mockDogNew" && stored.CreatedBy == "User2" && stored.CreatedTimestamp == StoredTimestamp);
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteDog()
    {
        var stored = StoreHolds(StoredItem());

        await _repository.DeleteAsync(ItemId, null, TestContext.Current.CancellationToken);

        Assert.True(stored.IsDeleted && stored.CreatedTimestamp == StoredTimestamp);
    }
}
