
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

public class KittenClawsRepositoryTest
{
    private readonly Container _mockContainer;
    private readonly KittenClawsRepository _repository;

    public KittenClawsRepositoryTest()
    {
        var mockCosmosClient = Substitute.For<CosmosClient>();
        _mockContainer = Substitute.For<Container>();
        mockCosmosClient.GetContainer(Arg.Any<string>(), Arg.Any<string>()).Returns(_mockContainer);
        _repository = new KittenClawsRepository(mockCosmosClient, "mockDatabaseName", "mockContainerName");
    }

    [Fact]
    public async Task GetAsync_ShouldReturnKittenClawsDto()
    {
        // Arrange
        var kittenClaws = new KittenClaws { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockKittenClaws" };
        var response = Substitute.For<ItemResponse<KittenClaws>>();
        response.Resource.Returns(kittenClaws);
        _mockContainer.ReadItemAsync<KittenClaws>(Arg.Any<string>(), Arg.Any<PartitionKey>(), Arg.Any<ItemRequestOptions>(), Arg.Any<CancellationToken>())
                        .Returns(response);

        // Act
        var result = await _repository.GetAsync("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", result.Id);
        Assert.Equal("mockKittenClaws", result.Name);
    }

    [Fact]
    public async Task GetListAsync_ShouldReturnListOfKittenClawsDto()
    {
        // Arrange
        var kittenClawsList = new List<KittenClaws>
        {
            new() { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockKittenClaws1" },
            new() { Id = "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name = "mockKittenClaws2" }
        };
        var feedResponse = Substitute.For<FeedResponse<KittenClaws>>();
        feedResponse.Resource.Returns(kittenClawsList);

        var feedIterator = Substitute.For<FeedIterator<KittenClaws>>();
        feedIterator.HasMoreResults.Returns(true, false);
        feedIterator.ReadNextAsync(Arg.Any<CancellationToken>()).Returns(feedResponse);

        _mockContainer.GetItemQueryIterator<KittenClaws>("SELECT * FROM c WHERE c.isDeleted = false OFFSET 0 LIMIT 100")
            .Returns(feedIterator);

        // Act
        var result = await _repository.GetListAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Contains(result, r => r.Id == "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c" && r.Name == "mockKittenClaws1");
        Assert.Contains(result, r => r.Id == "5615ff05-3032-4459-88ad-b6a4c3e51ca0" && r.Name == "mockKittenClaws2");
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnCreatedKittenClawsDto()
    {
        // Arrange
        var kittenClaws = new KittenClaws { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockKittenClaws" };
        var response = Substitute.For<ItemResponse<KittenClaws>>();
        response.Resource.Returns(kittenClaws);
        _mockContainer.CreateItemAsync(Arg.Any<KittenClaws>(), Arg.Any<PartitionKey>(), Arg.Any<ItemRequestOptions>(), Arg.Any<CancellationToken>())
                        .Returns(response);

        // Act
        var result = await _repository.CreateAsync(kittenClaws, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", result.Id);
        Assert.Equal("mockKittenClaws", result.Name);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnUpdatedKittenClawsDto()
    {
        // Arrange
        var kittenClaws = new KittenClaws { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockKittenClawsNew", UpdatedBy = "User1" };
        var currentKittenClaws = new KittenClaws { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockKittenClawsOld", CreatedBy = "User2", CreatedTimestamp = DateTime.UtcNow };
        var readResponse = Substitute.For<ItemResponse<KittenClaws>>();
        readResponse.Resource.Returns(currentKittenClaws);
        var replaceResponse = Substitute.For<ItemResponse<KittenClaws>>();
        replaceResponse.Resource.Returns(kittenClaws);
        _mockContainer.ReadItemAsync<KittenClaws>(Arg.Any<string>(), Arg.Any<PartitionKey>(), Arg.Any<ItemRequestOptions>(), Arg.Any<CancellationToken>())
                        .Returns(readResponse);
        _mockContainer.ReplaceItemAsync(Arg.Any<KittenClaws>(), Arg.Any<string>(), Arg.Any<PartitionKey>(), Arg.Any<ItemRequestOptions>(), Arg.Any<CancellationToken>())
                        .Returns(replaceResponse);

        // Act
        var result = await _repository.UpdateAsync(kittenClaws, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", result.Id);
        Assert.Equal("mockKittenClawsNew", result.Name);
    }

    [Fact]
    public async Task DeleteAsync_ShouldMarkItemAsDeleted()
    {
        // Arrange
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var existingKittenClaws = new KittenClaws
        {
            Id = id,
            Name = "mockKittenClaws",
            IsDeleted = false
        };

        var mockResponse = Substitute.For<ItemResponse<KittenClaws>>();
        mockResponse.Resource.Returns(existingKittenClaws);

        _mockContainer.ReadItemAsync<KittenClaws>(
            id,
            new PartitionKey(id),
            null,
            Arg.Any<CancellationToken>())
            .Returns(mockResponse);

        // Act
        await _repository.DeleteAsync(id, CancellationToken.None);

        // Assert
        await _mockContainer.Received(1).ReplaceItemAsync(
            Arg.Is<KittenClaws>(k => k.IsDeleted == true),
            id,
            Arg.Any<PartitionKey>(),
            null,
            Arg.Any<CancellationToken>());
    }
}
