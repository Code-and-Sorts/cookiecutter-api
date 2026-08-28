
namespace KittenClaws.Api.Tests.Unit;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KittenClaws.Api.Entities;
using KittenClaws.Api.Repositories;
using Microsoft.Azure.Cosmos;
using NSubstitute;
using Xunit;

public class CatRepositoryTest
{
    private readonly Container _mockContainer;
    private readonly CatRepository _repository;

    public CatRepositoryTest()
    {
        var mockCosmosClient = Substitute.For<CosmosClient>();
        _mockContainer = Substitute.For<Container>();
        mockCosmosClient.GetContainer(Arg.Any<string>(), Arg.Any<string>()).Returns(_mockContainer);
        _repository = new CatRepository(mockCosmosClient, "mockDatabaseName", "mockContainerName");
    }

    [Fact]
    public async Task GetAsync_ShouldReturnCatDto()
    {
        var item = new Cat { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockCat" };
        var response = Substitute.For<ItemResponse<Cat>>();
        response.Resource.Returns(item);
        _mockContainer.ReadItemAsync<Cat>(Arg.Any<string>(), Arg.Any<PartitionKey>(), Arg.Any<ItemRequestOptions>(), Arg.Any<CancellationToken>())
                        .Returns(response);

        var result = await _repository.GetAsync("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", result.Id);
        Assert.Equal("mockCat", result.Name);
    }

    [Fact]
    public async Task GetListAsync_ShouldReturnListOfCatDto()
    {
        var itemList = new List<Cat>
        {
            new() { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockCat1" },
            new() { Id = "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name = "mockCat2" }
        };
        var feedResponse = Substitute.For<FeedResponse<Cat>>();
        feedResponse.Resource.Returns(itemList);

        var feedIterator = Substitute.For<FeedIterator<Cat>>();
        feedIterator.HasMoreResults.Returns(true, false);
        feedIterator.ReadNextAsync(Arg.Any<CancellationToken>()).Returns(feedResponse);

        _mockContainer.GetItemQueryIterator<Cat>("SELECT * FROM c WHERE c.isDeleted = false OFFSET 0 LIMIT 100")
            .Returns(feedIterator);

        var result = await _repository.GetListAsync(CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Contains(result, res => res.Id == "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c" && res.Name == "mockCat1");
        Assert.Contains(result, res => res.Id == "5615ff05-3032-4459-88ad-b6a4c3e51ca0" && res.Name == "mockCat2");
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnCreatedCatDto()
    {
        var item = new Cat { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockCat" };
        var response = Substitute.For<ItemResponse<Cat>>();
        response.Resource.Returns(item);
        _mockContainer.CreateItemAsync(Arg.Any<Cat>(), Arg.Any<PartitionKey>(), Arg.Any<ItemRequestOptions>(), Arg.Any<CancellationToken>())
                        .Returns(response);

        var result = await _repository.CreateAsync(item, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", result.Id);
        Assert.Equal("mockCat", result.Name);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnUpdatedCatDto()
    {
        var item = new Cat { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockCatNew", UpdatedBy = "User1" };
        var currentItem = new Cat { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockCatOld", CreatedBy = "User2", CreatedTimestamp = DateTime.UtcNow };
        var readResponse = Substitute.For<ItemResponse<Cat>>();
        readResponse.Resource.Returns(currentItem);
        var replaceResponse = Substitute.For<ItemResponse<Cat>>();
        replaceResponse.Resource.Returns(item);
        _mockContainer.ReadItemAsync<Cat>(Arg.Any<string>(), Arg.Any<PartitionKey>(), Arg.Any<ItemRequestOptions>(), Arg.Any<CancellationToken>())
                        .Returns(readResponse);
        _mockContainer.ReplaceItemAsync(Arg.Any<Cat>(), Arg.Any<string>(), Arg.Any<PartitionKey>(), Arg.Any<ItemRequestOptions>(), Arg.Any<CancellationToken>())
                        .Returns(replaceResponse);

        var result = await _repository.UpdateAsync(item, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", result.Id);
        Assert.Equal("mockCatNew", result.Name);
    }

    [Fact]
    public async Task ReplaceAsync_ShouldReturnReplacedCatDto()
    {
        var item = new Cat { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockCatNew", UpdatedBy = "User1" };
        var currentItem = new Cat { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockCatOld", CreatedBy = "User2", CreatedTimestamp = DateTime.UtcNow };
        var readResponse = Substitute.For<ItemResponse<Cat>>();
        readResponse.Resource.Returns(currentItem);
        var replaceResponse = Substitute.For<ItemResponse<Cat>>();
        replaceResponse.Resource.Returns(item);
        _mockContainer.ReadItemAsync<Cat>(Arg.Any<string>(), Arg.Any<PartitionKey>(), Arg.Any<ItemRequestOptions>(), Arg.Any<CancellationToken>())
                        .Returns(readResponse);
        _mockContainer.ReplaceItemAsync(Arg.Any<Cat>(), Arg.Any<string>(), Arg.Any<PartitionKey>(), Arg.Any<ItemRequestOptions>(), Arg.Any<CancellationToken>())
                        .Returns(replaceResponse);

        var result = await _repository.ReplaceAsync(item, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", result.Id);
        Assert.Equal("mockCatNew", result.Name);
    }

    [Fact]
    public async Task DeleteAsync_ShouldMarkCatAsDeleted()
    {
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var existingItem = new Cat
        {
            Id = id,
            Name = "mockCat",
            IsDeleted = false
        };

        var mockResponse = Substitute.For<ItemResponse<Cat>>();
        mockResponse.Resource.Returns(existingItem);

        _mockContainer.ReadItemAsync<Cat>(
            id,
            new PartitionKey(id),
            null,
            Arg.Any<CancellationToken>())
            .Returns(mockResponse);

        await _repository.DeleteAsync(id, CancellationToken.None);

        await _mockContainer.Received(1).ReplaceItemAsync(
            Arg.Is<Cat>(k => k.IsDeleted == true),
            id,
            Arg.Any<PartitionKey>(),
            null,
            Arg.Any<CancellationToken>());
    }
}

public class DogRepositoryTest
{
    private readonly Container _mockContainer;
    private readonly DogRepository _repository;

    public DogRepositoryTest()
    {
        var mockCosmosClient = Substitute.For<CosmosClient>();
        _mockContainer = Substitute.For<Container>();
        mockCosmosClient.GetContainer(Arg.Any<string>(), Arg.Any<string>()).Returns(_mockContainer);
        _repository = new DogRepository(mockCosmosClient, "mockDatabaseName", "mockContainerName");
    }

    [Fact]
    public async Task GetAsync_ShouldReturnDogDto()
    {
        var item = new Dog { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockDog" };
        var response = Substitute.For<ItemResponse<Dog>>();
        response.Resource.Returns(item);
        _mockContainer.ReadItemAsync<Dog>(Arg.Any<string>(), Arg.Any<PartitionKey>(), Arg.Any<ItemRequestOptions>(), Arg.Any<CancellationToken>())
                        .Returns(response);

        var result = await _repository.GetAsync("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", result.Id);
        Assert.Equal("mockDog", result.Name);
    }

    [Fact]
    public async Task GetListAsync_ShouldReturnListOfDogDto()
    {
        var itemList = new List<Dog>
        {
            new() { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockDog1" },
            new() { Id = "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name = "mockDog2" }
        };
        var feedResponse = Substitute.For<FeedResponse<Dog>>();
        feedResponse.Resource.Returns(itemList);

        var feedIterator = Substitute.For<FeedIterator<Dog>>();
        feedIterator.HasMoreResults.Returns(true, false);
        feedIterator.ReadNextAsync(Arg.Any<CancellationToken>()).Returns(feedResponse);

        _mockContainer.GetItemQueryIterator<Dog>("SELECT * FROM c WHERE c.isDeleted = false OFFSET 0 LIMIT 100")
            .Returns(feedIterator);

        var result = await _repository.GetListAsync(CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Contains(result, res => res.Id == "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c" && res.Name == "mockDog1");
        Assert.Contains(result, res => res.Id == "5615ff05-3032-4459-88ad-b6a4c3e51ca0" && res.Name == "mockDog2");
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnCreatedDogDto()
    {
        var item = new Dog { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockDog" };
        var response = Substitute.For<ItemResponse<Dog>>();
        response.Resource.Returns(item);
        _mockContainer.CreateItemAsync(Arg.Any<Dog>(), Arg.Any<PartitionKey>(), Arg.Any<ItemRequestOptions>(), Arg.Any<CancellationToken>())
                        .Returns(response);

        var result = await _repository.CreateAsync(item, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", result.Id);
        Assert.Equal("mockDog", result.Name);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnUpdatedDogDto()
    {
        var item = new Dog { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockDogNew", UpdatedBy = "User1" };
        var currentItem = new Dog { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockDogOld", CreatedBy = "User2", CreatedTimestamp = DateTime.UtcNow };
        var readResponse = Substitute.For<ItemResponse<Dog>>();
        readResponse.Resource.Returns(currentItem);
        var replaceResponse = Substitute.For<ItemResponse<Dog>>();
        replaceResponse.Resource.Returns(item);
        _mockContainer.ReadItemAsync<Dog>(Arg.Any<string>(), Arg.Any<PartitionKey>(), Arg.Any<ItemRequestOptions>(), Arg.Any<CancellationToken>())
                        .Returns(readResponse);
        _mockContainer.ReplaceItemAsync(Arg.Any<Dog>(), Arg.Any<string>(), Arg.Any<PartitionKey>(), Arg.Any<ItemRequestOptions>(), Arg.Any<CancellationToken>())
                        .Returns(replaceResponse);

        var result = await _repository.UpdateAsync(item, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", result.Id);
        Assert.Equal("mockDogNew", result.Name);
    }

    [Fact]
    public async Task ReplaceAsync_ShouldReturnReplacedDogDto()
    {
        var item = new Dog { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockDogNew", UpdatedBy = "User1" };
        var currentItem = new Dog { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockDogOld", CreatedBy = "User2", CreatedTimestamp = DateTime.UtcNow };
        var readResponse = Substitute.For<ItemResponse<Dog>>();
        readResponse.Resource.Returns(currentItem);
        var replaceResponse = Substitute.For<ItemResponse<Dog>>();
        replaceResponse.Resource.Returns(item);
        _mockContainer.ReadItemAsync<Dog>(Arg.Any<string>(), Arg.Any<PartitionKey>(), Arg.Any<ItemRequestOptions>(), Arg.Any<CancellationToken>())
                        .Returns(readResponse);
        _mockContainer.ReplaceItemAsync(Arg.Any<Dog>(), Arg.Any<string>(), Arg.Any<PartitionKey>(), Arg.Any<ItemRequestOptions>(), Arg.Any<CancellationToken>())
                        .Returns(replaceResponse);

        var result = await _repository.ReplaceAsync(item, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", result.Id);
        Assert.Equal("mockDogNew", result.Name);
    }

    [Fact]
    public async Task DeleteAsync_ShouldMarkDogAsDeleted()
    {
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var existingItem = new Dog
        {
            Id = id,
            Name = "mockDog",
            IsDeleted = false
        };

        var mockResponse = Substitute.For<ItemResponse<Dog>>();
        mockResponse.Resource.Returns(existingItem);

        _mockContainer.ReadItemAsync<Dog>(
            id,
            new PartitionKey(id),
            null,
            Arg.Any<CancellationToken>())
            .Returns(mockResponse);

        await _repository.DeleteAsync(id, CancellationToken.None);

        await _mockContainer.Received(1).ReplaceItemAsync(
            Arg.Is<Dog>(k => k.IsDeleted == true),
            id,
            Arg.Any<PartitionKey>(),
            null,
            Arg.Any<CancellationToken>());
    }
}

