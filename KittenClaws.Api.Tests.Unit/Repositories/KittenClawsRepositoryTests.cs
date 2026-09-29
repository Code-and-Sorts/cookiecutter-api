
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
        var item = new CatEntity { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockCat" };
        var response = Substitute.For<ItemResponse<CatEntity>>();
        response.Resource.Returns(item);
        _mockContainer.ReadItemAsync<CatEntity>(Arg.Any<string>(), Arg.Any<PartitionKey>(), Arg.Any<ItemRequestOptions>(), Arg.Any<CancellationToken>())
                        .Returns(response);

        var result = await _repository.GetAsync("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", result.Id);
        Assert.Equal("mockCat", result.Name);
    }

    [Fact]
    public async Task GetListAsync_ShouldReturnListOfCatDto()
    {
        var itemList = new List<CatEntity>
        {
            new() { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockCat1" },
            new() { Id = "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name = "mockCat2" }
        };
        var feedResponse = Substitute.For<FeedResponse<CatEntity>>();
        feedResponse.Resource.Returns(itemList);

        var feedIterator = Substitute.For<FeedIterator<CatEntity>>();
        feedIterator.HasMoreResults.Returns(true, false);
        feedIterator.ReadNextAsync(Arg.Any<CancellationToken>()).Returns(feedResponse);

        _mockContainer.GetItemQueryIterator<CatEntity>("SELECT * FROM c WHERE c.isDeleted = false OFFSET 0 LIMIT 100")
            .Returns(feedIterator);

        var result = await _repository.GetListAsync(TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Contains(result, res => res.Id == "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c" && res.Name == "mockCat1");
        Assert.Contains(result, res => res.Id == "5615ff05-3032-4459-88ad-b6a4c3e51ca0" && res.Name == "mockCat2");
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnCreatedCatDto()
    {
        var item = new CatEntity { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockCat" };
        var response = Substitute.For<ItemResponse<CatEntity>>();
        response.Resource.Returns(item);
        _mockContainer.CreateItemAsync(Arg.Any<CatEntity>(), Arg.Any<PartitionKey>(), Arg.Any<ItemRequestOptions>(), Arg.Any<CancellationToken>())
                        .Returns(response);

        var result = await _repository.CreateAsync(item, TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", result.Id);
        Assert.Equal("mockCat", result.Name);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnUpdatedCatDto()
    {
        var item = new CatEntity { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockCatNew", UpdatedBy = "User1" };
        var currentItem = new CatEntity { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockCatOld", CreatedBy = "User2", CreatedTimestamp = DateTime.UtcNow };
        var readResponse = Substitute.For<ItemResponse<CatEntity>>();
        readResponse.Resource.Returns(currentItem);
        var replaceResponse = Substitute.For<ItemResponse<CatEntity>>();
        replaceResponse.Resource.Returns(item);
        _mockContainer.ReadItemAsync<CatEntity>(Arg.Any<string>(), Arg.Any<PartitionKey>(), Arg.Any<ItemRequestOptions>(), Arg.Any<CancellationToken>())
                        .Returns(readResponse);
        _mockContainer.ReplaceItemAsync(Arg.Any<CatEntity>(), Arg.Any<string>(), Arg.Any<PartitionKey>(), Arg.Any<ItemRequestOptions>(), Arg.Any<CancellationToken>())
                        .Returns(replaceResponse);

        var result = await _repository.UpdateAsync(item, TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", result.Id);
        Assert.Equal("mockCatNew", result.Name);
    }

    [Fact]
    public async Task ReplaceAsync_ShouldReturnReplacedCatDto()
    {
        var item = new CatEntity { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockCatNew", UpdatedBy = "User1" };
        var currentItem = new CatEntity { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockCatOld", CreatedBy = "User2", CreatedTimestamp = DateTime.UtcNow };
        var readResponse = Substitute.For<ItemResponse<CatEntity>>();
        readResponse.Resource.Returns(currentItem);
        var replaceResponse = Substitute.For<ItemResponse<CatEntity>>();
        replaceResponse.Resource.Returns(item);
        _mockContainer.ReadItemAsync<CatEntity>(Arg.Any<string>(), Arg.Any<PartitionKey>(), Arg.Any<ItemRequestOptions>(), Arg.Any<CancellationToken>())
                        .Returns(readResponse);
        _mockContainer.ReplaceItemAsync(Arg.Any<CatEntity>(), Arg.Any<string>(), Arg.Any<PartitionKey>(), Arg.Any<ItemRequestOptions>(), Arg.Any<CancellationToken>())
                        .Returns(replaceResponse);

        var result = await _repository.ReplaceAsync(item, TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", result.Id);
        Assert.Equal("mockCatNew", result.Name);
    }

    [Fact]
    public async Task DeleteAsync_ShouldMarkCatAsDeleted()
    {
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var existingItem = new CatEntity
        {
            Id = id,
            Name = "mockCat",
            IsDeleted = false
        };

        var mockResponse = Substitute.For<ItemResponse<CatEntity>>();
        mockResponse.Resource.Returns(existingItem);

        _mockContainer.ReadItemAsync<CatEntity>(
            id,
            new PartitionKey(id),
            null,
            Arg.Any<CancellationToken>())
            .Returns(mockResponse);

        await _repository.DeleteAsync(id, TestContext.Current.CancellationToken);

        await _mockContainer.Received(1).ReplaceItemAsync(
            Arg.Is<CatEntity>(k => k.IsDeleted == true),
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
        var item = new DogEntity { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockDog" };
        var response = Substitute.For<ItemResponse<DogEntity>>();
        response.Resource.Returns(item);
        _mockContainer.ReadItemAsync<DogEntity>(Arg.Any<string>(), Arg.Any<PartitionKey>(), Arg.Any<ItemRequestOptions>(), Arg.Any<CancellationToken>())
                        .Returns(response);

        var result = await _repository.GetAsync("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", result.Id);
        Assert.Equal("mockDog", result.Name);
    }

    [Fact]
    public async Task GetListAsync_ShouldReturnListOfDogDto()
    {
        var itemList = new List<DogEntity>
        {
            new() { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockDog1" },
            new() { Id = "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name = "mockDog2" }
        };
        var feedResponse = Substitute.For<FeedResponse<DogEntity>>();
        feedResponse.Resource.Returns(itemList);

        var feedIterator = Substitute.For<FeedIterator<DogEntity>>();
        feedIterator.HasMoreResults.Returns(true, false);
        feedIterator.ReadNextAsync(Arg.Any<CancellationToken>()).Returns(feedResponse);

        _mockContainer.GetItemQueryIterator<DogEntity>("SELECT * FROM c WHERE c.isDeleted = false OFFSET 0 LIMIT 100")
            .Returns(feedIterator);

        var result = await _repository.GetListAsync(TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Contains(result, res => res.Id == "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c" && res.Name == "mockDog1");
        Assert.Contains(result, res => res.Id == "5615ff05-3032-4459-88ad-b6a4c3e51ca0" && res.Name == "mockDog2");
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnCreatedDogDto()
    {
        var item = new DogEntity { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockDog" };
        var response = Substitute.For<ItemResponse<DogEntity>>();
        response.Resource.Returns(item);
        _mockContainer.CreateItemAsync(Arg.Any<DogEntity>(), Arg.Any<PartitionKey>(), Arg.Any<ItemRequestOptions>(), Arg.Any<CancellationToken>())
                        .Returns(response);

        var result = await _repository.CreateAsync(item, TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", result.Id);
        Assert.Equal("mockDog", result.Name);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnUpdatedDogDto()
    {
        var item = new DogEntity { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockDogNew", UpdatedBy = "User1" };
        var currentItem = new DogEntity { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockDogOld", CreatedBy = "User2", CreatedTimestamp = DateTime.UtcNow };
        var readResponse = Substitute.For<ItemResponse<DogEntity>>();
        readResponse.Resource.Returns(currentItem);
        var replaceResponse = Substitute.For<ItemResponse<DogEntity>>();
        replaceResponse.Resource.Returns(item);
        _mockContainer.ReadItemAsync<DogEntity>(Arg.Any<string>(), Arg.Any<PartitionKey>(), Arg.Any<ItemRequestOptions>(), Arg.Any<CancellationToken>())
                        .Returns(readResponse);
        _mockContainer.ReplaceItemAsync(Arg.Any<DogEntity>(), Arg.Any<string>(), Arg.Any<PartitionKey>(), Arg.Any<ItemRequestOptions>(), Arg.Any<CancellationToken>())
                        .Returns(replaceResponse);

        var result = await _repository.UpdateAsync(item, TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", result.Id);
        Assert.Equal("mockDogNew", result.Name);
    }

    [Fact]
    public async Task ReplaceAsync_ShouldReturnReplacedDogDto()
    {
        var item = new DogEntity { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockDogNew", UpdatedBy = "User1" };
        var currentItem = new DogEntity { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockDogOld", CreatedBy = "User2", CreatedTimestamp = DateTime.UtcNow };
        var readResponse = Substitute.For<ItemResponse<DogEntity>>();
        readResponse.Resource.Returns(currentItem);
        var replaceResponse = Substitute.For<ItemResponse<DogEntity>>();
        replaceResponse.Resource.Returns(item);
        _mockContainer.ReadItemAsync<DogEntity>(Arg.Any<string>(), Arg.Any<PartitionKey>(), Arg.Any<ItemRequestOptions>(), Arg.Any<CancellationToken>())
                        .Returns(readResponse);
        _mockContainer.ReplaceItemAsync(Arg.Any<DogEntity>(), Arg.Any<string>(), Arg.Any<PartitionKey>(), Arg.Any<ItemRequestOptions>(), Arg.Any<CancellationToken>())
                        .Returns(replaceResponse);

        var result = await _repository.ReplaceAsync(item, TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", result.Id);
        Assert.Equal("mockDogNew", result.Name);
    }

    [Fact]
    public async Task DeleteAsync_ShouldMarkDogAsDeleted()
    {
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var existingItem = new DogEntity
        {
            Id = id,
            Name = "mockDog",
            IsDeleted = false
        };

        var mockResponse = Substitute.For<ItemResponse<DogEntity>>();
        mockResponse.Resource.Returns(existingItem);

        _mockContainer.ReadItemAsync<DogEntity>(
            id,
            new PartitionKey(id),
            null,
            Arg.Any<CancellationToken>())
            .Returns(mockResponse);

        await _repository.DeleteAsync(id, TestContext.Current.CancellationToken);

        await _mockContainer.Received(1).ReplaceItemAsync(
            Arg.Is<DogEntity>(k => k.IsDeleted == true),
            id,
            Arg.Any<PartitionKey>(),
            null,
            Arg.Any<CancellationToken>());
    }
}

