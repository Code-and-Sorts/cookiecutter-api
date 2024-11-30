namespace {{cookiecutter.project_name}}.Api.Tests.Unit;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using {{cookiecutter.project_name}}.Api.Entities;
using {{cookiecutter.project_name}}.Api.Repositories;
using Microsoft.Azure.Cosmos;
using NSubstitute;
using Xunit;

public class {{cookiecutter.project_name}}RepositoryTest
{
    private readonly Container _mockContainer;
    private readonly {{cookiecutter.project_name}}Repository _repository;

    public {{cookiecutter.project_name}}RepositoryTest()
    {
        var mockCosmosClient = Substitute.For<CosmosClient>();
        _mockContainer = Substitute.For<Container>();
        mockCosmosClient.GetContainer(Arg.Any<string>(), Arg.Any<string>()).Returns(_mockContainer);
        _repository = new {{cookiecutter.project_name}}Repository(mockCosmosClient, "mockDatabaseName", "mockContainerName");
    }

    [Fact]
    public async Task GetAsync_ShouldReturn{{cookiecutter.project_name}}Dto()
    {
        // Arrange
        var {{cookiecutter.project_lower_camel_name}} = new {{cookiecutter.project_name}} { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mock{{cookiecutter.project_name}}" };
        var response = Substitute.For<ItemResponse<{{cookiecutter.project_name}}>>();
        response.Resource.Returns({{cookiecutter.project_lower_camel_name}});
        _mockContainer.ReadItemAsync<{{cookiecutter.project_name}}>(Arg.Any<string>(), Arg.Any<PartitionKey>(), Arg.Any<ItemRequestOptions>(), Arg.Any<CancellationToken>())
                        .Returns(response);

        // Act
        var result = await _repository.GetAsync("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", result.Id);
        Assert.Equal("mock{{cookiecutter.project_name}}", result.Name);
    }

    [Fact]
    public async Task GetListAsync_ShouldReturnListOf{{cookiecutter.project_name}}Dto()
    {
        // Arrange
        var {{cookiecutter.project_lower_camel_name}}List = new List<{{cookiecutter.project_name}}>
        {
            new() { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mock{{cookiecutter.project_name}}1" },
            new() { Id = "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name = "mock{{cookiecutter.project_name}}2" }
        };
        var feedResponse = Substitute.For<FeedResponse<{{cookiecutter.project_name}}>>();
        feedResponse.Resource.Returns({{cookiecutter.project_lower_camel_name}}List);

        var feedIterator = Substitute.For<FeedIterator<{{cookiecutter.project_name}}>>();
        feedIterator.HasMoreResults.Returns(true, false);
        feedIterator.ReadNextAsync(Arg.Any<CancellationToken>()).Returns(feedResponse);

        _mockContainer.GetItemQueryIterator<{{cookiecutter.project_name}}>()
            .Returns(feedIterator);

        // Act
        var result = await _repository.GetListAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Contains(result, r => r.Id == "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c" && r.Name == "mock{{cookiecutter.project_name}}1");
        Assert.Contains(result, r => r.Id == "5615ff05-3032-4459-88ad-b6a4c3e51ca0" && r.Name == "mock{{cookiecutter.project_name}}2");
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnCreated{{cookiecutter.project_name}}Dto()
    {
        // Arrange
        var {{cookiecutter.project_lower_camel_name}} = new {{cookiecutter.project_name}} { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mock{{cookiecutter.project_name}}" };
        var response = Substitute.For<ItemResponse<{{cookiecutter.project_name}}>>();
        response.Resource.Returns({{cookiecutter.project_lower_camel_name}});
        _mockContainer.CreateItemAsync(Arg.Any<{{cookiecutter.project_name}}>(), Arg.Any<PartitionKey>(), Arg.Any<ItemRequestOptions>(), Arg.Any<CancellationToken>())
                        .Returns(response);

        // Act
        var result = await _repository.CreateAsync({{cookiecutter.project_lower_camel_name}}, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", result.Id);
        Assert.Equal("mock{{cookiecutter.project_name}}", result.Name);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnUpdated{{cookiecutter.project_name}}Dto()
    {
        // Arrange
        var {{cookiecutter.project_lower_camel_name}} = new {{cookiecutter.project_name}} { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mock{{cookiecutter.project_name}}New", UpdatedBy = "User1" };
        var current{{cookiecutter.project_name}} = new {{cookiecutter.project_name}} { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mock{{cookiecutter.project_name}}Old", CreatedBy = "User2", CreatedTimestamp = DateTime.UtcNow };
        var readResponse = Substitute.For<ItemResponse<{{cookiecutter.project_name}}>>();
        readResponse.Resource.Returns(current{{cookiecutter.project_name}});
        var replaceResponse = Substitute.For<ItemResponse<{{cookiecutter.project_name}}>>();
        replaceResponse.Resource.Returns({{cookiecutter.project_lower_camel_name}});
        _mockContainer.ReadItemAsync<{{cookiecutter.project_name}}>(Arg.Any<string>(), Arg.Any<PartitionKey>(), Arg.Any<ItemRequestOptions>(), Arg.Any<CancellationToken>())
                        .Returns(readResponse);
        _mockContainer.ReplaceItemAsync(Arg.Any<{{cookiecutter.project_name}}>(), Arg.Any<string>(), Arg.Any<PartitionKey>(), Arg.Any<ItemRequestOptions>(), Arg.Any<CancellationToken>())
                        .Returns(replaceResponse);

        // Act
        var result = await _repository.UpdateAsync({{cookiecutter.project_lower_camel_name}}, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", result.Id);
        Assert.Equal("mock{{cookiecutter.project_name}}New", result.Name);
    }

    [Fact]
    public async Task DeleteAsync_ShouldCallDeleteItemAsync()
    {
        // Arrange
        var mockResponse = Substitute.For<ItemResponse<{{cookiecutter.project_name}}>>();
        _mockContainer.DeleteItemAsync<{{cookiecutter.project_name}}>(
            Arg.Any<string>(),
            Arg.Any<PartitionKey>(),
            Arg.Any<ItemRequestOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(mockResponse));


        // Act
        await _repository.DeleteAsync("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", CancellationToken.None);

        // Assert
        await _mockContainer.Received(1).DeleteItemAsync<{{cookiecutter.project_name}}>("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", new PartitionKey("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c"), Arg.Any<ItemRequestOptions>(), Arg.Any<CancellationToken>());
    }
}
