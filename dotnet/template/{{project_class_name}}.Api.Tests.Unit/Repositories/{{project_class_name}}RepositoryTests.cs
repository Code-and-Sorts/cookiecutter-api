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
{% for resource in resources %}
{%- set r = resource.name %}
public class {{ r }}RepositoryTest
{
{%- if cloud_service == 'Azure Function App' %}
    private readonly Container _mockContainer;
    private readonly {{ r }}Repository _repository;

    public {{ r }}RepositoryTest()
    {
        var mockCosmosClient = Substitute.For<CosmosClient>();
        _mockContainer = Substitute.For<Container>();
        mockCosmosClient.GetContainer(Arg.Any<string>(), Arg.Any<string>()).Returns(_mockContainer);
        _repository = new {{ r }}Repository(mockCosmosClient, "mockDatabaseName", "mockContainerName");
    }

    [Fact]
    public async Task GetAsync_ShouldReturn{{ r }}Dto()
    {
        // Arrange
        var item = new {{ r }} { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mock{{ r }}" };
        var response = Substitute.For<ItemResponse<{{ r }}>>();
        response.Resource.Returns(item);
        _mockContainer.ReadItemAsync<{{ r }}>(Arg.Any<string>(), Arg.Any<PartitionKey>(), Arg.Any<ItemRequestOptions>(), Arg.Any<CancellationToken>())
                        .Returns(response);

        // Act
        var result = await _repository.GetAsync("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", result.Id);
        Assert.Equal("mock{{ r }}", result.Name);
    }

    [Fact]
    public async Task GetListAsync_ShouldReturnListOf{{ r }}Dto()
    {
        // Arrange
        var itemList = new List<{{ r }}>
        {
            new() { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mock{{ r }}1" },
            new() { Id = "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name = "mock{{ r }}2" }
        };
        var feedResponse = Substitute.For<FeedResponse<{{ r }}>>();
        feedResponse.Resource.Returns(itemList);

        var feedIterator = Substitute.For<FeedIterator<{{ r }}>>();
        feedIterator.HasMoreResults.Returns(true, false);
        feedIterator.ReadNextAsync(Arg.Any<CancellationToken>()).Returns(feedResponse);

        _mockContainer.GetItemQueryIterator<{{ r }}>("SELECT * FROM c WHERE c.isDeleted = false OFFSET 0 LIMIT 100")
            .Returns(feedIterator);

        // Act
        var result = await _repository.GetListAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Contains(result, res => res.Id == "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c" && res.Name == "mock{{ r }}1");
        Assert.Contains(result, res => res.Id == "5615ff05-3032-4459-88ad-b6a4c3e51ca0" && res.Name == "mock{{ r }}2");
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnCreated{{ r }}Dto()
    {
        // Arrange
        var item = new {{ r }} { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mock{{ r }}" };
        var response = Substitute.For<ItemResponse<{{ r }}>>();
        response.Resource.Returns(item);
        _mockContainer.CreateItemAsync(Arg.Any<{{ r }}>(), Arg.Any<PartitionKey>(), Arg.Any<ItemRequestOptions>(), Arg.Any<CancellationToken>())
                        .Returns(response);

        // Act
        var result = await _repository.CreateAsync(item, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", result.Id);
        Assert.Equal("mock{{ r }}", result.Name);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnUpdated{{ r }}Dto()
    {
        // Arrange
        var item = new {{ r }} { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mock{{ r }}New", UpdatedBy = "User1" };
        var currentItem = new {{ r }} { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mock{{ r }}Old", CreatedBy = "User2", CreatedTimestamp = DateTime.UtcNow };
        var readResponse = Substitute.For<ItemResponse<{{ r }}>>();
        readResponse.Resource.Returns(currentItem);
        var replaceResponse = Substitute.For<ItemResponse<{{ r }}>>();
        replaceResponse.Resource.Returns(item);
        _mockContainer.ReadItemAsync<{{ r }}>(Arg.Any<string>(), Arg.Any<PartitionKey>(), Arg.Any<ItemRequestOptions>(), Arg.Any<CancellationToken>())
                        .Returns(readResponse);
        _mockContainer.ReplaceItemAsync(Arg.Any<{{ r }}>(), Arg.Any<string>(), Arg.Any<PartitionKey>(), Arg.Any<ItemRequestOptions>(), Arg.Any<CancellationToken>())
                        .Returns(replaceResponse);

        // Act
        var result = await _repository.UpdateAsync(item, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", result.Id);
        Assert.Equal("mock{{ r }}New", result.Name);
    }

    [Fact]
    public async Task ReplaceAsync_ShouldReturnReplaced{{ r }}Dto()
    {
        // Arrange
        var item = new {{ r }} { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mock{{ r }}New", UpdatedBy = "User1" };
        var currentItem = new {{ r }} { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mock{{ r }}Old", CreatedBy = "User2", CreatedTimestamp = DateTime.UtcNow };
        var readResponse = Substitute.For<ItemResponse<{{ r }}>>();
        readResponse.Resource.Returns(currentItem);
        var replaceResponse = Substitute.For<ItemResponse<{{ r }}>>();
        replaceResponse.Resource.Returns(item);
        _mockContainer.ReadItemAsync<{{ r }}>(Arg.Any<string>(), Arg.Any<PartitionKey>(), Arg.Any<ItemRequestOptions>(), Arg.Any<CancellationToken>())
                        .Returns(readResponse);
        _mockContainer.ReplaceItemAsync(Arg.Any<{{ r }}>(), Arg.Any<string>(), Arg.Any<PartitionKey>(), Arg.Any<ItemRequestOptions>(), Arg.Any<CancellationToken>())
                        .Returns(replaceResponse);

        // Act
        var result = await _repository.ReplaceAsync(item, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", result.Id);
        Assert.Equal("mock{{ r }}New", result.Name);
    }

    [Fact]
    public async Task DeleteAsync_ShouldMark{{ r }}AsDeleted()
    {
        // Arrange
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var existingItem = new {{ r }}
        {
            Id = id,
            Name = "mock{{ r }}",
            IsDeleted = false
        };

        var mockResponse = Substitute.For<ItemResponse<{{ r }}>>();
        mockResponse.Resource.Returns(existingItem);

        _mockContainer.ReadItemAsync<{{ r }}>(
            id,
            new PartitionKey(id),
            null,
            Arg.Any<CancellationToken>())
            .Returns(mockResponse);

        // Act
        await _repository.DeleteAsync(id, CancellationToken.None);

        // Assert
        await _mockContainer.Received(1).ReplaceItemAsync(
            Arg.Is<{{ r }}>(k => k.IsDeleted == true),
            id,
            Arg.Any<PartitionKey>(),
            null,
            Arg.Any<CancellationToken>());
    }
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
    private readonly IFirestoreContext<{{ r }}> _mockContext;
    private readonly {{ r }}Repository _repository;

    public {{ r }}RepositoryTest()
    {
        _mockContext = Substitute.For<IFirestoreContext<{{ r }}>>();
        _repository = new {{ r }}Repository(_mockContext);
    }

    [Fact]
    public async Task GetAsync_ShouldReturn{{ r }}Dto()
    {
        // Arrange
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var item = new {{ r }} { Id = id, Name = "mock{{ r }}" };
        _mockContext.GetAsync(id, Arg.Any<CancellationToken>()).Returns(item);

        // Act
        var result = await _repository.GetAsync(id, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("mock{{ r }}", result.Name);
    }

    [Fact]
    public async Task GetAsync_ShouldThrowWhenNotFound()
    {
        // Arrange
        var id = "non-existent-id";
        _mockContext.GetAsync(id, Arg.Any<CancellationToken>()).Returns(({{ r }}?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _repository.GetAsync(id, CancellationToken.None));
    }

    [Fact]
    public async Task GetListAsync_ShouldReturnListOf{{ r }}Dto()
    {
        // Arrange
        var itemList = new List<{{ r }}>
        {
            new() { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mock{{ r }}1" },
            new() { Id = "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name = "mock{{ r }}2" }
        };
        _mockContext.GetListAsync("isDeleted", false, Arg.Any<CancellationToken>()).Returns(itemList);

        // Act
        var result = await _repository.GetListAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Contains(result, res => res.Id == "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c" && res.Name == "mock{{ r }}1");
        Assert.Contains(result, res => res.Id == "5615ff05-3032-4459-88ad-b6a4c3e51ca0" && res.Name == "mock{{ r }}2");
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnCreated{{ r }}Dto()
    {
        // Arrange
        var item = new {{ r }} { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mock{{ r }}" };

        // Act
        var result = await _repository.CreateAsync(item, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", result.Id);
        Assert.Equal("mock{{ r }}", result.Name);
        await _mockContext.Received(1).SetAsync(item.Id, item, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnUpdated{{ r }}Dto()
    {
        // Arrange
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var item = new {{ r }} { Id = id, Name = "mock{{ r }}New", UpdatedBy = "User1" };
        var currentItem = new {{ r }} { Id = id, Name = "mock{{ r }}Old", CreatedBy = "User2", CreatedTimestamp = DateTime.UtcNow };
        _mockContext.GetAsync(id, Arg.Any<CancellationToken>()).Returns(currentItem);

        // Act
        var result = await _repository.UpdateAsync(item, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("mock{{ r }}New", result.Name);
        await _mockContext.Received(1).SetAsync(id, Arg.Any<{{ r }}>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReplaceAsync_ShouldReturnReplaced{{ r }}Dto()
    {
        // Arrange
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var item = new {{ r }} { Id = id, Name = "mock{{ r }}New", UpdatedBy = "User1" };
        var currentItem = new {{ r }} { Id = id, Name = "mock{{ r }}Old", CreatedBy = "User2", CreatedTimestamp = DateTime.UtcNow };
        _mockContext.GetAsync(id, Arg.Any<CancellationToken>()).Returns(currentItem);

        // Act
        var result = await _repository.ReplaceAsync(item, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("mock{{ r }}New", result.Name);
        await _mockContext.Received(1).SetAsync(id, Arg.Any<{{ r }}>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_ShouldMark{{ r }}AsDeleted()
    {
        // Arrange
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var existingItem = new {{ r }} { Id = id, Name = "mock{{ r }}", IsDeleted = false };
        _mockContext.GetAsync(id, Arg.Any<CancellationToken>()).Returns(existingItem);

        // Act
        await _repository.DeleteAsync(id, CancellationToken.None);

        // Assert
        await _mockContext.Received(1).SetAsync(
            id,
            Arg.Is<{{ r }}>(k => k.IsDeleted == true),
            Arg.Any<CancellationToken>());
    }
{%- endif %}
}
{% endfor %}
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
{% for resource in resources %}
{%- set r = resource.name %}
public class {{ r }}RepositoryTest
{
    private readonly IAmazonDynamoDB _mockDynamoClient;
    private readonly {{ r }}Repository _repository;

    public {{ r }}RepositoryTest()
    {
        _mockDynamoClient = Substitute.For<IAmazonDynamoDB>();
        _repository = new {{ r }}Repository(_mockDynamoClient, "mockTableName");
    }

    [Fact]
    public async Task GetAsync_ShouldReturn{{ r }}Dto()
    {
        // Arrange
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var response = new GetItemResponse
        {
            Item = new Dictionary<string, AttributeValue>
            {
                { "id", new AttributeValue { S = id } },
                { "name", new AttributeValue { S = "mock{{ r }}" } },
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
        Assert.Equal("mock{{ r }}", result.Name);
    }

    [Fact]
    public async Task GetListAsync_ShouldReturnListOf{{ r }}Dto()
    {
        // Arrange
        var response = new ScanResponse
        {
            Items = new List<Dictionary<string, AttributeValue>>
            {
                new()
                {
                    { "id", new AttributeValue { S = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c" } },
                    { "name", new AttributeValue { S = "mock{{ r }}1" } },
                    { "isDeleted", new AttributeValue { BOOL = false } },
                    { "createdTimestamp", new AttributeValue { S = DateTime.UtcNow.ToString("o") } },
                    { "updatedTimestamp", new AttributeValue { S = DateTime.UtcNow.ToString("o") } },
                    { "createdBy", new AttributeValue { S = "testUser" } },
                    { "updatedBy", new AttributeValue { S = "testUser" } },
                },
                new()
                {
                    { "id", new AttributeValue { S = "5615ff05-3032-4459-88ad-b6a4c3e51ca0" } },
                    { "name", new AttributeValue { S = "mock{{ r }}2" } },
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
        Assert.Contains(result, res => res.Id == "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c" && res.Name == "mock{{ r }}1");
        Assert.Contains(result, res => res.Id == "5615ff05-3032-4459-88ad-b6a4c3e51ca0" && res.Name == "mock{{ r }}2");
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnCreated{{ r }}Dto()
    {
        // Arrange
        var item = new {{ r }} { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mock{{ r }}" };
        _mockDynamoClient.PutItemAsync(Arg.Any<PutItemRequest>(), Arg.Any<CancellationToken>())
            .Returns(new PutItemResponse());

        // Act
        var result = await _repository.CreateAsync(item, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", result.Id);
        Assert.Equal("mock{{ r }}", result.Name);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnUpdated{{ r }}Dto()
    {
        // Arrange
        var item = new {{ r }} { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mock{{ r }}New", UpdatedBy = "User1" };
        var getResponse = new GetItemResponse
        {
            Item = new Dictionary<string, AttributeValue>
            {
                { "id", new AttributeValue { S = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c" } },
                { "name", new AttributeValue { S = "mock{{ r }}Old" } },
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
        Assert.Equal("mock{{ r }}New", result.Name);
    }

    [Fact]
    public async Task ReplaceAsync_ShouldReturnReplaced{{ r }}Dto()
    {
        // Arrange
        var item = new {{ r }} { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mock{{ r }}New", UpdatedBy = "User1" };
        var getResponse = new GetItemResponse
        {
            Item = new Dictionary<string, AttributeValue>
            {
                { "id", new AttributeValue { S = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c" } },
                { "name", new AttributeValue { S = "mock{{ r }}Old" } },
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
        var result = await _repository.ReplaceAsync(item, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", result.Id);
        Assert.Equal("mock{{ r }}New", result.Name);
    }

    [Fact]
    public async Task DeleteAsync_ShouldMark{{ r }}AsDeleted()
    {
        // Arrange
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var getResponse = new GetItemResponse
        {
            Item = new Dictionary<string, AttributeValue>
            {
                { "id", new AttributeValue { S = id } },
                { "name", new AttributeValue { S = "mock{{ r }}" } },
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
            Arg.Is<PutItemRequest>(req => req.Item["isDeleted"].BOOL == true),
            Arg.Any<CancellationToken>());
    }
}
{% endfor %}
{%- endif %}
