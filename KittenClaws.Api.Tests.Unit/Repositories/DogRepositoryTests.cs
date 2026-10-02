namespace KittenClaws.Api.Tests.Unit;

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

    [Fact]
    public async Task GetAsync_ShouldReturnDogDto()
    {
        _mockStore.GetAsync(ItemId, Arg.Any<CancellationToken>()).Returns(StoredItem());

        var result = await _repository.GetAsync(ItemId, TestContext.Current.CancellationToken);

        Assert.Equal(ItemId, result.Id);
        Assert.Equal("mockDog", result.Name);
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
        var result = await _repository.CreateAsync(new DogEntity { Name = "mockDog" }, null, TestContext.Current.CancellationToken);

        Assert.Equal("mockDog", result.Name);
        await _mockStore.Received(1).CreateAsync(Arg.Is<DogEntity>(k => k.Id == result.Id && k.Name == "mockDog"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReplaceAsync_ShouldChangeNameAndKeepCreatedFields()
    {
        _mockStore.GetAsync(ItemId, Arg.Any<CancellationToken>()).Returns(StoredItem("mockDogOld"));

        var result = await _repository.ReplaceAsync(new DogEntity { Id = ItemId, Name = "mockDogNew" }, null, TestContext.Current.CancellationToken);

        Assert.Equal("mockDogNew", result.Name);
        await _mockStore.Received(1).SaveAsync(
            Arg.Is<DogEntity>(k => k.Name == "mockDogNew" && k.CreatedBy == "User2" && k.CreatedTimestamp == StoredTimestamp),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteDog()
    {
        _mockStore.GetAsync(ItemId, Arg.Any<CancellationToken>()).Returns(StoredItem());

        await _repository.DeleteAsync(ItemId, null, TestContext.Current.CancellationToken);

        await _mockStore.Received(1).SaveAsync(Arg.Is<DogEntity>(k => k.IsDeleted && k.CreatedTimestamp == StoredTimestamp), Arg.Any<CancellationToken>());
    }
}
