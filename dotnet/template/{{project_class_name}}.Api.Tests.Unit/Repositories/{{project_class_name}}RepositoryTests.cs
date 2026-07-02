{%- if cloud_service in ['Azure Function App', 'GCP Cloud Function'] %}
namespace {{project_class_name}}.Api.Tests.Unit;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using {{project_class_name}}.Api.Entities;
using {{project_class_name}}.Api.Repositories;
{%- if cloud_service == 'Azure Function App' %}
using Microsoft.Azure.Cosmos;
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
using {{project_class_name}}.Api.Interfaces;
{%- endif %}
using NSubstitute;
using Xunit;

public class ItemRepositoryTest
{
{%- if cloud_service == 'Azure Function App' %}
    private readonly Container _mockContainer;
    private readonly ItemRepository _repository;

    public ItemRepositoryTest()
    {
        var mockCosmosClient = Substitute.For<CosmosClient>();
        _mockContainer = Substitute.For<Container>();
        mockCosmosClient.GetContainer(Arg.Any<string>(), Arg.Any<string>()).Returns(_mockContainer);
        _repository = new ItemRepository(mockCosmosClient, "mockDatabaseName", "mockContainerName");
    }

    [Fact]
    public async Task GetAsync_ShouldReturnItemDto()
    {
        // Arrange
        var item = new Item { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockItem" };
        var response = Substitute.For<ItemResponse<Item>>();
        response.Resource.Returns(item);
        _mockContainer.ReadItemAsync<Item>(Arg.Any<string>(), Arg.Any<PartitionKey>(), Arg.Any<ItemRequestOptions>(), Arg.Any<CancellationToken>())
                        .Returns(response);

        // Act
        var result = await _repository.GetAsync("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", result.Id);
        Assert.Equal("mockItem", result.Name);
    }

    [Fact]
    public async Task GetListAsync_ShouldReturnListOfItemDto()
    {
        // Arrange
        var itemList = new List<Item>
        {
            new() { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockItem1" },
            new() { Id = "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name = "mockItem2" }
        };
        var feedResponse = Substitute.For<FeedResponse<Item>>();
        feedResponse.Resource.Returns(itemList);

        var feedIterator = Substitute.For<FeedIterator<Item>>();
        feedIterator.HasMoreResults.Returns(true, false);
        feedIterator.ReadNextAsync(Arg.Any<CancellationToken>()).Returns(feedResponse);

        _mockContainer.GetItemQueryIterator<Item>("SELECT * FROM c WHERE c.isDeleted = false OFFSET 0 LIMIT 100")
            .Returns(feedIterator);

        // Act
        var result = await _repository.GetListAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Contains(result, r => r.Id == "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c" && r.Name == "mockItem1");
        Assert.Contains(result, r => r.Id == "5615ff05-3032-4459-88ad-b6a4c3e51ca0" && r.Name == "mockItem2");
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnCreatedItemDto()
    {
        // Arrange
        var item = new Item { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockItem" };
        var response = Substitute.For<ItemResponse<Item>>();
        response.Resource.Returns(item);
        _mockContainer.CreateItemAsync(Arg.Any<Item>(), Arg.Any<PartitionKey>(), Arg.Any<ItemRequestOptions>(), Arg.Any<CancellationToken>())
                        .Returns(response);

        // Act
        var result = await _repository.CreateAsync(item, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", result.Id);
        Assert.Equal("mockItem", result.Name);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnUpdatedItemDto()
    {
        // Arrange
        var item = new Item { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockItemNew", UpdatedBy = "User1" };
        var currentItem = new Item { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockItemOld", CreatedBy = "User2", CreatedTimestamp = DateTime.UtcNow };
        var readResponse = Substitute.For<ItemResponse<Item>>();
        readResponse.Resource.Returns(currentItem);
        var replaceResponse = Substitute.For<ItemResponse<Item>>();
        replaceResponse.Resource.Returns(item);
        _mockContainer.ReadItemAsync<Item>(Arg.Any<string>(), Arg.Any<PartitionKey>(), Arg.Any<ItemRequestOptions>(), Arg.Any<CancellationToken>())
                        .Returns(readResponse);
        _mockContainer.ReplaceItemAsync(Arg.Any<Item>(), Arg.Any<string>(), Arg.Any<PartitionKey>(), Arg.Any<ItemRequestOptions>(), Arg.Any<CancellationToken>())
                        .Returns(replaceResponse);

        // Act
        var result = await _repository.UpdateAsync(item, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", result.Id);
        Assert.Equal("mockItemNew", result.Name);
    }

    [Fact]
    public async Task DeleteAsync_ShouldMarkItemAsDeleted()
    {
        // Arrange
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var existingItem = new Item
        {
            Id = id,
            Name = "mockItem",
            IsDeleted = false
        };

        var mockResponse = Substitute.For<ItemResponse<Item>>();
        mockResponse.Resource.Returns(existingItem);

        _mockContainer.ReadItemAsync<Item>(
            id,
            new PartitionKey(id),
            null,
            Arg.Any<CancellationToken>())
            .Returns(mockResponse);

        // Act
        await _repository.DeleteAsync(id, CancellationToken.None);

        // Assert
        await _mockContainer.Received(1).ReplaceItemAsync(
            Arg.Is<Item>(k => k.IsDeleted == true),
            id,
            Arg.Any<PartitionKey>(),
            null,
            Arg.Any<CancellationToken>());
    }
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
    private readonly IFirestoreContext<Item> _mockContext;
    private readonly ItemRepository _repository;

    public ItemRepositoryTest()
    {
        _mockContext = Substitute.For<IFirestoreContext<Item>>();
        _repository = new ItemRepository(_mockContext);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnItemDto()
    {
        // Arrange
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var item = new Item { Id = id, Name = "mockItem" };
        _mockContext.GetAsync(id, Arg.Any<CancellationToken>()).Returns(item);

        // Act
        var result = await _repository.GetAsync(id, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("mockItem", result.Name);
    }

    [Fact]
    public async Task GetAsync_ShouldThrowWhenNotFound()
    {
        // Arrange
        var id = "non-existent-id";
        _mockContext.GetAsync(id, Arg.Any<CancellationToken>()).Returns((Item?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _repository.GetAsync(id, CancellationToken.None));
    }

    [Fact]
    public async Task GetListAsync_ShouldReturnListOfItemDto()
    {
        // Arrange
        var itemList = new List<Item>
        {
            new() { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockItem1" },
            new() { Id = "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name = "mockItem2" }
        };
        _mockContext.GetListAsync("isDeleted", false, Arg.Any<CancellationToken>()).Returns(itemList);

        // Act
        var result = await _repository.GetListAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Contains(result, r => r.Id == "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c" && r.Name == "mockItem1");
        Assert.Contains(result, r => r.Id == "5615ff05-3032-4459-88ad-b6a4c3e51ca0" && r.Name == "mockItem2");
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnCreatedItemDto()
    {
        // Arrange
        var item = new Item { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockItem" };

        // Act
        var result = await _repository.CreateAsync(item, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", result.Id);
        Assert.Equal("mockItem", result.Name);
        await _mockContext.Received(1).SetAsync(item.Id, item, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnUpdatedItemDto()
    {
        // Arrange
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var item = new Item { Id = id, Name = "mockItemNew", UpdatedBy = "User1" };
        var currentItem = new Item { Id = id, Name = "mockItemOld", CreatedBy = "User2", CreatedTimestamp = DateTime.UtcNow };
        _mockContext.GetAsync(id, Arg.Any<CancellationToken>()).Returns(currentItem);

        // Act
        var result = await _repository.UpdateAsync(item, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("mockItemNew", result.Name);
        await _mockContext.Received(1).SetAsync(id, Arg.Any<Item>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_ShouldMarkItemAsDeleted()
    {
        // Arrange
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var existingItem = new Item { Id = id, Name = "mockItem", IsDeleted = false };
        _mockContext.GetAsync(id, Arg.Any<CancellationToken>()).Returns(existingItem);

        // Act
        await _repository.DeleteAsync(id, CancellationToken.None);

        // Assert
        await _mockContext.Received(1).SetAsync(
            id,
            Arg.Is<Item>(k => k.IsDeleted == true),
            Arg.Any<CancellationToken>());
    }
{%- endif %}
}
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
namespace {{project_class_name}}.Api.Tests.Unit;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using {{project_class_name}}.Api.Entities;
using {{project_class_name}}.Api.Repositories;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using NSubstitute;
using Xunit;

public class ItemRepositoryTest
{
    private readonly IAmazonDynamoDB _mockDynamoClient;
    private readonly ItemRepository _repository;

    public ItemRepositoryTest()
    {
        _mockDynamoClient = Substitute.For<IAmazonDynamoDB>();
        _repository = new ItemRepository(_mockDynamoClient, "mockTableName");
    }

    [Fact]
    public async Task GetAsync_ShouldReturnItemDto()
    {
        // Arrange
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var response = new GetItemResponse
        {
            Item = new Dictionary<string, AttributeValue>
            {
                { "id", new AttributeValue { S = id } },
                { "name", new AttributeValue { S = "mockItem" } },
                { "isDeleted", new AttributeValue { BOOL = false } },
                { "createdTimestamp", new AttributeValue { S = DateTime.UtcNow.ToString("o") } },
                { "updatedTimestamp", new AttributeValue { S = DateTime.UtcNow.ToString("o") } },
                { "createdBy", new AttributeValue { S = "testUser" } },
                { "updatedBy", new AttributeValue { S = "testUser" } },
            }
        };
        _mockDynamoClient.GetItemAsync(Arg.Any<GetItemRequest>(), Arg.Any<CancellationToken>())
            .Returns(response);

        // Act
        var result = await _repository.GetAsync(id, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("mockItem", result.Name);
    }

    [Fact]
    public async Task GetListAsync_ShouldReturnListOfItemDto()
    {
        // Arrange
        var response = new ScanResponse
        {
            Items = new List<Dictionary<string, AttributeValue>>
            {
                new()
                {
                    { "id", new AttributeValue { S = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c" } },
                    { "name", new AttributeValue { S = "mockItem1" } },
                    { "isDeleted", new AttributeValue { BOOL = false } },
                    { "createdTimestamp", new AttributeValue { S = DateTime.UtcNow.ToString("o") } },
                    { "updatedTimestamp", new AttributeValue { S = DateTime.UtcNow.ToString("o") } },
                    { "createdBy", new AttributeValue { S = "testUser" } },
                    { "updatedBy", new AttributeValue { S = "testUser" } },
                },
                new()
                {
                    { "id", new AttributeValue { S = "5615ff05-3032-4459-88ad-b6a4c3e51ca0" } },
                    { "name", new AttributeValue { S = "mockItem2" } },
                    { "isDeleted", new AttributeValue { BOOL = false } },
                    { "createdTimestamp", new AttributeValue { S = DateTime.UtcNow.ToString("o") } },
                    { "updatedTimestamp", new AttributeValue { S = DateTime.UtcNow.ToString("o") } },
                    { "createdBy", new AttributeValue { S = "testUser" } },
                    { "updatedBy", new AttributeValue { S = "testUser" } },
                }
            },
            LastEvaluatedKey = new Dictionary<string, AttributeValue>()
        };
        _mockDynamoClient.ScanAsync(Arg.Any<ScanRequest>(), Arg.Any<CancellationToken>())
            .Returns(response);

        // Act
        var result = await _repository.GetListAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Contains(result, r => r.Id == "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c" && r.Name == "mockItem1");
        Assert.Contains(result, r => r.Id == "5615ff05-3032-4459-88ad-b6a4c3e51ca0" && r.Name == "mockItem2");
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnCreatedItemDto()
    {
        // Arrange
        var item = new Item { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockItem" };
        _mockDynamoClient.PutItemAsync(Arg.Any<PutItemRequest>(), Arg.Any<CancellationToken>())
            .Returns(new PutItemResponse());

        // Act
        var result = await _repository.CreateAsync(item, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", result.Id);
        Assert.Equal("mockItem", result.Name);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnUpdatedItemDto()
    {
        // Arrange
        var item = new Item { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockItemNew", UpdatedBy = "User1" };
        var getResponse = new GetItemResponse
        {
            Item = new Dictionary<string, AttributeValue>
            {
                { "id", new AttributeValue { S = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c" } },
                { "name", new AttributeValue { S = "mockItemOld" } },
                { "isDeleted", new AttributeValue { BOOL = false } },
                { "createdTimestamp", new AttributeValue { S = DateTime.UtcNow.ToString("o") } },
                { "updatedTimestamp", new AttributeValue { S = DateTime.UtcNow.ToString("o") } },
                { "createdBy", new AttributeValue { S = "User2" } },
                { "updatedBy", new AttributeValue { S = "User2" } },
            }
        };
        _mockDynamoClient.GetItemAsync(Arg.Any<GetItemRequest>(), Arg.Any<CancellationToken>())
            .Returns(getResponse);
        _mockDynamoClient.PutItemAsync(Arg.Any<PutItemRequest>(), Arg.Any<CancellationToken>())
            .Returns(new PutItemResponse());

        // Act
        var result = await _repository.UpdateAsync(item, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", result.Id);
        Assert.Equal("mockItemNew", result.Name);
    }

    [Fact]
    public async Task DeleteAsync_ShouldMarkItemAsDeleted()
    {
        // Arrange
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var getResponse = new GetItemResponse
        {
            Item = new Dictionary<string, AttributeValue>
            {
                { "id", new AttributeValue { S = id } },
                { "name", new AttributeValue { S = "mockItem" } },
                { "isDeleted", new AttributeValue { BOOL = false } },
                { "createdTimestamp", new AttributeValue { S = DateTime.UtcNow.ToString("o") } },
                { "updatedTimestamp", new AttributeValue { S = DateTime.UtcNow.ToString("o") } },
                { "createdBy", new AttributeValue { S = "testUser" } },
                { "updatedBy", new AttributeValue { S = "testUser" } },
            }
        };
        _mockDynamoClient.GetItemAsync(Arg.Any<GetItemRequest>(), Arg.Any<CancellationToken>())
            .Returns(getResponse);
        _mockDynamoClient.PutItemAsync(Arg.Any<PutItemRequest>(), Arg.Any<CancellationToken>())
            .Returns(new PutItemResponse());

        // Act
        await _repository.DeleteAsync(id, CancellationToken.None);

        // Assert
        await _mockDynamoClient.Received(1).PutItemAsync(
            Arg.Is<PutItemRequest>(r => r.Item["isDeleted"].BOOL == true),
            Arg.Any<CancellationToken>());
    }
}
{%- endif %}
