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

public class {{project_class_name}}RepositoryTest
{
{%- if cloud_service == 'Azure Function App' %}
    private readonly Container _mockContainer;
    private readonly {{project_class_name}}Repository _repository;

    public {{project_class_name}}RepositoryTest()
    {
        var mockCosmosClient = Substitute.For<CosmosClient>();
        _mockContainer = Substitute.For<Container>();
        mockCosmosClient.GetContainer(Arg.Any<string>(), Arg.Any<string>()).Returns(_mockContainer);
        _repository = new {{project_class_name}}Repository(mockCosmosClient, "mockDatabaseName", "mockContainerName");
    }

    [Fact]
    public async Task GetAsync_ShouldReturn{{project_class_name}}Dto()
    {
        // Arrange
        var {{project_lower_camel_name}} = new {{project_class_name}} { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mock{{project_class_name}}" };
        var response = Substitute.For<ItemResponse<{{project_class_name}}>>();
        response.Resource.Returns({{project_lower_camel_name}});
        _mockContainer.ReadItemAsync<{{project_class_name}}>(Arg.Any<string>(), Arg.Any<PartitionKey>(), Arg.Any<ItemRequestOptions>(), Arg.Any<CancellationToken>())
                        .Returns(response);

        // Act
        var result = await _repository.GetAsync("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", result.Id);
        Assert.Equal("mock{{project_class_name}}", result.Name);
    }

    [Fact]
    public async Task GetListAsync_ShouldReturnListOf{{project_class_name}}Dto()
    {
        // Arrange
        var {{project_lower_camel_name}}List = new List<{{project_class_name}}>
        {
            new() { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mock{{project_class_name}}1" },
            new() { Id = "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name = "mock{{project_class_name}}2" }
        };
        var feedResponse = Substitute.For<FeedResponse<{{project_class_name}}>>();
        feedResponse.Resource.Returns({{project_lower_camel_name}}List);

        var feedIterator = Substitute.For<FeedIterator<{{project_class_name}}>>();
        feedIterator.HasMoreResults.Returns(true, false);
        feedIterator.ReadNextAsync(Arg.Any<CancellationToken>()).Returns(feedResponse);

        _mockContainer.GetItemQueryIterator<{{project_class_name}}>("SELECT * FROM c WHERE c.isDeleted = false OFFSET 0 LIMIT 100")
            .Returns(feedIterator);

        // Act
        var result = await _repository.GetListAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Contains(result, r => r.Id == "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c" && r.Name == "mock{{project_class_name}}1");
        Assert.Contains(result, r => r.Id == "5615ff05-3032-4459-88ad-b6a4c3e51ca0" && r.Name == "mock{{project_class_name}}2");
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnCreated{{project_class_name}}Dto()
    {
        // Arrange
        var {{project_lower_camel_name}} = new {{project_class_name}} { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mock{{project_class_name}}" };
        var response = Substitute.For<ItemResponse<{{project_class_name}}>>();
        response.Resource.Returns({{project_lower_camel_name}});
        _mockContainer.CreateItemAsync(Arg.Any<{{project_class_name}}>(), Arg.Any<PartitionKey>(), Arg.Any<ItemRequestOptions>(), Arg.Any<CancellationToken>())
                        .Returns(response);

        // Act
        var result = await _repository.CreateAsync({{project_lower_camel_name}}, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", result.Id);
        Assert.Equal("mock{{project_class_name}}", result.Name);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnUpdated{{project_class_name}}Dto()
    {
        // Arrange
        var {{project_lower_camel_name}} = new {{project_class_name}} { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mock{{project_class_name}}New", UpdatedBy = "User1" };
        var current{{project_class_name}} = new {{project_class_name}} { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mock{{project_class_name}}Old", CreatedBy = "User2", CreatedTimestamp = DateTime.UtcNow };
        var readResponse = Substitute.For<ItemResponse<{{project_class_name}}>>();
        readResponse.Resource.Returns(current{{project_class_name}});
        var replaceResponse = Substitute.For<ItemResponse<{{project_class_name}}>>();
        replaceResponse.Resource.Returns({{project_lower_camel_name}});
        _mockContainer.ReadItemAsync<{{project_class_name}}>(Arg.Any<string>(), Arg.Any<PartitionKey>(), Arg.Any<ItemRequestOptions>(), Arg.Any<CancellationToken>())
                        .Returns(readResponse);
        _mockContainer.ReplaceItemAsync(Arg.Any<{{project_class_name}}>(), Arg.Any<string>(), Arg.Any<PartitionKey>(), Arg.Any<ItemRequestOptions>(), Arg.Any<CancellationToken>())
                        .Returns(replaceResponse);

        // Act
        var result = await _repository.UpdateAsync({{project_lower_camel_name}}, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", result.Id);
        Assert.Equal("mock{{project_class_name}}New", result.Name);
    }

    [Fact]
    public async Task DeleteAsync_ShouldMarkItemAsDeleted()
    {
        // Arrange
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var existing{{project_class_name}} = new {{project_class_name}}
        {
            Id = id,
            Name = "mock{{project_class_name}}",
            IsDeleted = false
        };

        var mockResponse = Substitute.For<ItemResponse<{{project_class_name}}>>();
        mockResponse.Resource.Returns(existing{{project_class_name}});

        _mockContainer.ReadItemAsync<{{project_class_name}}>(
            id,
            new PartitionKey(id),
            null,
            Arg.Any<CancellationToken>())
            .Returns(mockResponse);

        // Act
        await _repository.DeleteAsync(id, CancellationToken.None);

        // Assert
        await _mockContainer.Received(1).ReplaceItemAsync(
            Arg.Is<{{project_class_name}}>(k => k.IsDeleted == true),
            id,
            Arg.Any<PartitionKey>(),
            null,
            Arg.Any<CancellationToken>());
    }
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
    private readonly IFirestoreContext<{{project_class_name}}> _mockContext;
    private readonly {{project_class_name}}Repository _repository;

    public {{project_class_name}}RepositoryTest()
    {
        _mockContext = Substitute.For<IFirestoreContext<{{project_class_name}}>>();
        _repository = new {{project_class_name}}Repository(_mockContext);
    }

    [Fact]
    public async Task GetAsync_ShouldReturn{{project_class_name}}Dto()
    {
        // Arrange
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var {{project_lower_camel_name}} = new {{project_class_name}} { Id = id, Name = "mock{{project_class_name}}" };
        _mockContext.GetAsync(id, Arg.Any<CancellationToken>()).Returns({{project_lower_camel_name}});

        // Act
        var result = await _repository.GetAsync(id, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("mock{{project_class_name}}", result.Name);
    }

    [Fact]
    public async Task GetAsync_ShouldThrowWhenNotFound()
    {
        // Arrange
        var id = "non-existent-id";
        _mockContext.GetAsync(id, Arg.Any<CancellationToken>()).Returns(({{project_class_name}}?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _repository.GetAsync(id, CancellationToken.None));
    }

    [Fact]
    public async Task GetListAsync_ShouldReturnListOf{{project_class_name}}Dto()
    {
        // Arrange
        var {{project_lower_camel_name}}List = new List<{{project_class_name}}>
        {
            new() { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mock{{project_class_name}}1" },
            new() { Id = "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name = "mock{{project_class_name}}2" }
        };
        _mockContext.GetListAsync("isDeleted", false, Arg.Any<CancellationToken>()).Returns({{project_lower_camel_name}}List);

        // Act
        var result = await _repository.GetListAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Contains(result, r => r.Id == "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c" && r.Name == "mock{{project_class_name}}1");
        Assert.Contains(result, r => r.Id == "5615ff05-3032-4459-88ad-b6a4c3e51ca0" && r.Name == "mock{{project_class_name}}2");
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnCreated{{project_class_name}}Dto()
    {
        // Arrange
        var {{project_lower_camel_name}} = new {{project_class_name}} { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mock{{project_class_name}}" };

        // Act
        var result = await _repository.CreateAsync({{project_lower_camel_name}}, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", result.Id);
        Assert.Equal("mock{{project_class_name}}", result.Name);
        await _mockContext.Received(1).SetAsync({{project_lower_camel_name}}.Id, {{project_lower_camel_name}}, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnUpdated{{project_class_name}}Dto()
    {
        // Arrange
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var {{project_lower_camel_name}} = new {{project_class_name}} { Id = id, Name = "mock{{project_class_name}}New", UpdatedBy = "User1" };
        var current{{project_class_name}} = new {{project_class_name}} { Id = id, Name = "mock{{project_class_name}}Old", CreatedBy = "User2", CreatedTimestamp = DateTime.UtcNow };
        _mockContext.GetAsync(id, Arg.Any<CancellationToken>()).Returns(current{{project_class_name}});

        // Act
        var result = await _repository.UpdateAsync({{project_lower_camel_name}}, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("mock{{project_class_name}}New", result.Name);
        await _mockContext.Received(1).SetAsync(id, Arg.Any<{{project_class_name}}>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_ShouldMarkItemAsDeleted()
    {
        // Arrange
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var existing{{project_class_name}} = new {{project_class_name}} { Id = id, Name = "mock{{project_class_name}}", IsDeleted = false };
        _mockContext.GetAsync(id, Arg.Any<CancellationToken>()).Returns(existing{{project_class_name}});

        // Act
        await _repository.DeleteAsync(id, CancellationToken.None);

        // Assert
        await _mockContext.Received(1).SetAsync(
            id,
            Arg.Is<{{project_class_name}}>(k => k.IsDeleted == true),
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

public class {{project_class_name}}RepositoryTest
{
    private readonly IAmazonDynamoDB _mockDynamoClient;
    private readonly {{project_class_name}}Repository _repository;

    public {{project_class_name}}RepositoryTest()
    {
        _mockDynamoClient = Substitute.For<IAmazonDynamoDB>();
        _repository = new {{project_class_name}}Repository(_mockDynamoClient, "mockTableName");
    }

    [Fact]
    public async Task GetAsync_ShouldReturn{{project_class_name}}Dto()
    {
        // Arrange
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var response = new GetItemResponse
        {
            Item = new Dictionary<string, AttributeValue>
            {
                { "id", new AttributeValue { S = id } },
                { "name", new AttributeValue { S = "mock{{project_class_name}}" } },
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
        Assert.Equal("mock{{project_class_name}}", result.Name);
    }

    [Fact]
    public async Task GetListAsync_ShouldReturnListOf{{project_class_name}}Dto()
    {
        // Arrange
        var response = new ScanResponse
        {
            Items = new List<Dictionary<string, AttributeValue>>
            {
                new()
                {
                    { "id", new AttributeValue { S = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c" } },
                    { "name", new AttributeValue { S = "mock{{project_class_name}}1" } },
                    { "isDeleted", new AttributeValue { BOOL = false } },
                    { "createdTimestamp", new AttributeValue { S = DateTime.UtcNow.ToString("o") } },
                    { "updatedTimestamp", new AttributeValue { S = DateTime.UtcNow.ToString("o") } },
                    { "createdBy", new AttributeValue { S = "testUser" } },
                    { "updatedBy", new AttributeValue { S = "testUser" } },
                },
                new()
                {
                    { "id", new AttributeValue { S = "5615ff05-3032-4459-88ad-b6a4c3e51ca0" } },
                    { "name", new AttributeValue { S = "mock{{project_class_name}}2" } },
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
        Assert.Contains(result, r => r.Id == "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c" && r.Name == "mock{{project_class_name}}1");
        Assert.Contains(result, r => r.Id == "5615ff05-3032-4459-88ad-b6a4c3e51ca0" && r.Name == "mock{{project_class_name}}2");
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnCreated{{project_class_name}}Dto()
    {
        // Arrange
        var {{project_lower_camel_name}} = new {{project_class_name}} { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mock{{project_class_name}}" };
        _mockDynamoClient.PutItemAsync(Arg.Any<PutItemRequest>(), Arg.Any<CancellationToken>())
            .Returns(new PutItemResponse());

        // Act
        var result = await _repository.CreateAsync({{project_lower_camel_name}}, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", result.Id);
        Assert.Equal("mock{{project_class_name}}", result.Name);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnUpdated{{project_class_name}}Dto()
    {
        // Arrange
        var {{project_lower_camel_name}} = new {{project_class_name}} { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mock{{project_class_name}}New", UpdatedBy = "User1" };
        var getResponse = new GetItemResponse
        {
            Item = new Dictionary<string, AttributeValue>
            {
                { "id", new AttributeValue { S = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c" } },
                { "name", new AttributeValue { S = "mock{{project_class_name}}Old" } },
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
        var result = await _repository.UpdateAsync({{project_lower_camel_name}}, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", result.Id);
        Assert.Equal("mock{{project_class_name}}New", result.Name);
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
                { "name", new AttributeValue { S = "mock{{project_class_name}}" } },
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
