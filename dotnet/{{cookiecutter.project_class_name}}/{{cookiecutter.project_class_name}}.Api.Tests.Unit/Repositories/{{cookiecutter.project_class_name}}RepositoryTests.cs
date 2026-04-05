namespace {{cookiecutter.project_class_name}}.Api.Tests.Unit;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using {{cookiecutter.project_class_name}}.Api.Entities;
using {{cookiecutter.project_class_name}}.Api.Repositories;
{%- if cookiecutter.cloud_service == 'Azure Function App' %}
using Microsoft.Azure.Cosmos;
{%- endif %}
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
using Google.Cloud.Firestore;
{%- endif %}
using NSubstitute;
using Xunit;

public class {{cookiecutter.project_class_name}}RepositoryTest
{
{%- if cookiecutter.cloud_service == 'Azure Function App' %}
    private readonly Container _mockContainer;
    private readonly {{cookiecutter.project_class_name}}Repository _repository;

    public {{cookiecutter.project_class_name}}RepositoryTest()
    {
        var mockCosmosClient = Substitute.For<CosmosClient>();
        _mockContainer = Substitute.For<Container>();
        mockCosmosClient.GetContainer(Arg.Any<string>(), Arg.Any<string>()).Returns(_mockContainer);
        _repository = new {{cookiecutter.project_class_name}}Repository(mockCosmosClient, "mockDatabaseName", "mockContainerName");
    }

    [Fact]
    public async Task GetAsync_ShouldReturn{{cookiecutter.project_class_name}}Dto()
    {
        // Arrange
        var {{cookiecutter.project_lower_camel_name}} = new {{cookiecutter.project_class_name}} { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mock{{cookiecutter.project_class_name}}" };
        var response = Substitute.For<ItemResponse<{{cookiecutter.project_class_name}}>>();
        response.Resource.Returns({{cookiecutter.project_lower_camel_name}});
        _mockContainer.ReadItemAsync<{{cookiecutter.project_class_name}}>(Arg.Any<string>(), Arg.Any<PartitionKey>(), Arg.Any<ItemRequestOptions>(), Arg.Any<CancellationToken>())
                        .Returns(response);

        // Act
        var result = await _repository.GetAsync("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", result.Id);
        Assert.Equal("mock{{cookiecutter.project_class_name}}", result.Name);
    }

    [Fact]
    public async Task GetListAsync_ShouldReturnListOf{{cookiecutter.project_class_name}}Dto()
    {
        // Arrange
        var {{cookiecutter.project_lower_camel_name}}List = new List<{{cookiecutter.project_class_name}}>
        {
            new() { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mock{{cookiecutter.project_class_name}}1" },
            new() { Id = "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name = "mock{{cookiecutter.project_class_name}}2" }
        };
        var feedResponse = Substitute.For<FeedResponse<{{cookiecutter.project_class_name}}>>();
        feedResponse.Resource.Returns({{cookiecutter.project_lower_camel_name}}List);

        var feedIterator = Substitute.For<FeedIterator<{{cookiecutter.project_class_name}}>>();
        feedIterator.HasMoreResults.Returns(true, false);
        feedIterator.ReadNextAsync(Arg.Any<CancellationToken>()).Returns(feedResponse);

        _mockContainer.GetItemQueryIterator<{{cookiecutter.project_class_name}}>("SELECT * FROM c WHERE c.isDeleted = false")
            .Returns(feedIterator);

        // Act
        var result = await _repository.GetListAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Contains(result, r => r.Id == "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c" && r.Name == "mock{{cookiecutter.project_class_name}}1");
        Assert.Contains(result, r => r.Id == "5615ff05-3032-4459-88ad-b6a4c3e51ca0" && r.Name == "mock{{cookiecutter.project_class_name}}2");
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnCreated{{cookiecutter.project_class_name}}Dto()
    {
        // Arrange
        var {{cookiecutter.project_lower_camel_name}} = new {{cookiecutter.project_class_name}} { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mock{{cookiecutter.project_class_name}}" };
        var response = Substitute.For<ItemResponse<{{cookiecutter.project_class_name}}>>();
        response.Resource.Returns({{cookiecutter.project_lower_camel_name}});
        _mockContainer.CreateItemAsync(Arg.Any<{{cookiecutter.project_class_name}}>(), Arg.Any<PartitionKey>(), Arg.Any<ItemRequestOptions>(), Arg.Any<CancellationToken>())
                        .Returns(response);

        // Act
        var result = await _repository.CreateAsync({{cookiecutter.project_lower_camel_name}}, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", result.Id);
        Assert.Equal("mock{{cookiecutter.project_class_name}}", result.Name);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnUpdated{{cookiecutter.project_class_name}}Dto()
    {
        // Arrange
        var {{cookiecutter.project_lower_camel_name}} = new {{cookiecutter.project_class_name}} { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mock{{cookiecutter.project_class_name}}New", UpdatedBy = "User1" };
        var current{{cookiecutter.project_class_name}} = new {{cookiecutter.project_class_name}} { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mock{{cookiecutter.project_class_name}}Old", CreatedBy = "User2", CreatedTimestamp = DateTime.UtcNow };
        var readResponse = Substitute.For<ItemResponse<{{cookiecutter.project_class_name}}>>();
        readResponse.Resource.Returns(current{{cookiecutter.project_class_name}});
        var replaceResponse = Substitute.For<ItemResponse<{{cookiecutter.project_class_name}}>>();
        replaceResponse.Resource.Returns({{cookiecutter.project_lower_camel_name}});
        _mockContainer.ReadItemAsync<{{cookiecutter.project_class_name}}>(Arg.Any<string>(), Arg.Any<PartitionKey>(), Arg.Any<ItemRequestOptions>(), Arg.Any<CancellationToken>())
                        .Returns(readResponse);
        _mockContainer.ReplaceItemAsync(Arg.Any<{{cookiecutter.project_class_name}}>(), Arg.Any<string>(), Arg.Any<PartitionKey>(), Arg.Any<ItemRequestOptions>(), Arg.Any<CancellationToken>())
                        .Returns(replaceResponse);

        // Act
        var result = await _repository.UpdateAsync({{cookiecutter.project_lower_camel_name}}, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", result.Id);
        Assert.Equal("mock{{cookiecutter.project_class_name}}New", result.Name);
    }

    [Fact]
    public async Task DeleteAsync_ShouldMarkItemAsDeleted()
    {
        // Arrange
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var existing{{cookiecutter.project_class_name}} = new {{cookiecutter.project_class_name}}
        {
            Id = id,
            Name = "mock{{cookiecutter.project_class_name}}",
            IsDeleted = false
        };

        var mockResponse = Substitute.For<ItemResponse<{{cookiecutter.project_class_name}}>>();
        mockResponse.Resource.Returns(existing{{cookiecutter.project_class_name}});

        _mockContainer.ReadItemAsync<{{cookiecutter.project_class_name}}>(
            id,
            new PartitionKey(id),
            null,
            Arg.Any<CancellationToken>())
            .Returns(mockResponse);

        // Act
        await _repository.DeleteAsync(id, CancellationToken.None);

        // Assert
        await _mockContainer.Received(1).ReplaceItemAsync(
            Arg.Is<{{cookiecutter.project_class_name}}>(k => k.IsDeleted == true),
            id,
            Arg.Any<PartitionKey>(),
            null,
            Arg.Any<CancellationToken>());
    }
{%- endif %}
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
    private readonly FirestoreDb _mockFirestoreDb;
    private readonly CollectionReference _mockCollection;
    private readonly {{cookiecutter.project_class_name}}Repository _repository;

    public {{cookiecutter.project_class_name}}RepositoryTest()
    {
        _mockFirestoreDb = Substitute.For<FirestoreDb>();
        _mockCollection = Substitute.For<CollectionReference>();
        _mockFirestoreDb.Collection(Arg.Any<string>()).Returns(_mockCollection);
        _repository = new {{cookiecutter.project_class_name}}Repository(_mockFirestoreDb, "mockCollectionName");
    }

    [Fact]
    public async Task GetAsync_ShouldReturn{{cookiecutter.project_class_name}}Dto()
    {
        // Arrange
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var mockDocRef = Substitute.For<DocumentReference>();
        var mockSnapshot = Substitute.For<DocumentSnapshot>();
        mockSnapshot.Exists.Returns(true);
        mockSnapshot.ConvertTo<{{cookiecutter.project_class_name}}>().Returns(new {{cookiecutter.project_class_name}} { Id = id, Name = "mock{{cookiecutter.project_class_name}}" });
        _mockCollection.Document(id).Returns(mockDocRef);
        mockDocRef.GetSnapshotAsync(Arg.Any<CancellationToken>()).Returns(mockSnapshot);

        // Act
        var result = await _repository.GetAsync(id, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("mock{{cookiecutter.project_class_name}}", result.Name);
    }

    [Fact]
    public async Task GetAsync_ShouldThrowWhenNotFound()
    {
        // Arrange
        var id = "non-existent-id";
        var mockDocRef = Substitute.For<DocumentReference>();
        var mockSnapshot = Substitute.For<DocumentSnapshot>();
        mockSnapshot.Exists.Returns(false);
        _mockCollection.Document(id).Returns(mockDocRef);
        mockDocRef.GetSnapshotAsync(Arg.Any<CancellationToken>()).Returns(mockSnapshot);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _repository.GetAsync(id, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnCreated{{cookiecutter.project_class_name}}Dto()
    {
        // Arrange
        var {{cookiecutter.project_lower_camel_name}} = new {{cookiecutter.project_class_name}} { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mock{{cookiecutter.project_class_name}}" };
        var mockDocRef = Substitute.For<DocumentReference>();
        _mockCollection.Document({{cookiecutter.project_lower_camel_name}}.Id).Returns(mockDocRef);

        // Act
        var result = await _repository.CreateAsync({{cookiecutter.project_lower_camel_name}}, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", result.Id);
        Assert.Equal("mock{{cookiecutter.project_class_name}}", result.Name);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnUpdated{{cookiecutter.project_class_name}}Dto()
    {
        // Arrange
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var {{cookiecutter.project_lower_camel_name}} = new {{cookiecutter.project_class_name}} { Id = id, Name = "mock{{cookiecutter.project_class_name}}New", UpdatedBy = "User1" };
        var current{{cookiecutter.project_class_name}} = new {{cookiecutter.project_class_name}} { Id = id, Name = "mock{{cookiecutter.project_class_name}}Old", CreatedBy = "User2", CreatedTimestamp = DateTime.UtcNow };

        var mockDocRef = Substitute.For<DocumentReference>();
        var mockSnapshot = Substitute.For<DocumentSnapshot>();
        mockSnapshot.Exists.Returns(true);
        mockSnapshot.ConvertTo<{{cookiecutter.project_class_name}}>().Returns(current{{cookiecutter.project_class_name}});
        _mockCollection.Document(id).Returns(mockDocRef);
        mockDocRef.GetSnapshotAsync(Arg.Any<CancellationToken>()).Returns(mockSnapshot);

        // Act
        var result = await _repository.UpdateAsync({{cookiecutter.project_lower_camel_name}}, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("mock{{cookiecutter.project_class_name}}New", result.Name);
    }

    [Fact]
    public async Task DeleteAsync_ShouldMarkItemAsDeleted()
    {
        // Arrange
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var existing{{cookiecutter.project_class_name}} = new {{cookiecutter.project_class_name}} { Id = id, Name = "mock{{cookiecutter.project_class_name}}", IsDeleted = false };

        var mockDocRef = Substitute.For<DocumentReference>();
        var mockSnapshot = Substitute.For<DocumentSnapshot>();
        mockSnapshot.Exists.Returns(true);
        mockSnapshot.ConvertTo<{{cookiecutter.project_class_name}}>().Returns(existing{{cookiecutter.project_class_name}});
        _mockCollection.Document(id).Returns(mockDocRef);
        mockDocRef.GetSnapshotAsync(Arg.Any<CancellationToken>()).Returns(mockSnapshot);

        // Act
        await _repository.DeleteAsync(id, CancellationToken.None);

        // Assert
        await mockDocRef.Received(1).SetAsync(
            Arg.Is<{{cookiecutter.project_class_name}}>(k => k.IsDeleted == true),
            Arg.Any<SetOptions>(),
            Arg.Any<CancellationToken>());
    }
{%- endif %}
}
