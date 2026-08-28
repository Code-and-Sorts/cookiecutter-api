
namespace KittenClaws.Api.Tests.Unit;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KittenClaws.Api.Entities;
using KittenClaws.Api.Repositories;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using NSubstitute;
using Xunit;

public class CatRepositoryTest
{
    private readonly IAmazonDynamoDB _mockDynamoClient;
    private readonly CatRepository _repository;

    public CatRepositoryTest()
    {
        _mockDynamoClient = Substitute.For<IAmazonDynamoDB>();
        _repository = new CatRepository(_mockDynamoClient, "mockTableName");
    }

    [Fact]
    public async Task GetAsync_ShouldReturnCatDto()
    {
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var response = new GetItemResponse
        {
            Item = new Dictionary<string, AttributeValue>
            {
                { "id", new AttributeValue { S = id } },
                { "name", new AttributeValue { S = "mockCat" } },
                { "isDeleted", new AttributeValue { BOOL = false } },
                { "createdTimestamp", new AttributeValue { S = DateTime.UtcNow.ToString("o") } },
                { "updatedTimestamp", new AttributeValue { S = DateTime.UtcNow.ToString("o") } },
                { "createdBy", new AttributeValue { S = "testUser" } },
                { "updatedBy", new AttributeValue { S = "testUser" } },
            }
        };
        _mockDynamoClient.GetItemAsync(Arg.Any<GetItemRequest>(), Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _repository.GetAsync(id, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("mockCat", result.Name);
    }

    [Fact]
    public async Task GetListAsync_ShouldReturnListOfCatDto()
    {
        var response = new ScanResponse
        {
            Items = new List<Dictionary<string, AttributeValue>>
            {
                new()
                {
                    { "id", new AttributeValue { S = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c" } },
                    { "name", new AttributeValue { S = "mockCat1" } },
                    { "isDeleted", new AttributeValue { BOOL = false } },
                    { "createdTimestamp", new AttributeValue { S = DateTime.UtcNow.ToString("o") } },
                    { "updatedTimestamp", new AttributeValue { S = DateTime.UtcNow.ToString("o") } },
                    { "createdBy", new AttributeValue { S = "testUser" } },
                    { "updatedBy", new AttributeValue { S = "testUser" } },
                },
                new()
                {
                    { "id", new AttributeValue { S = "5615ff05-3032-4459-88ad-b6a4c3e51ca0" } },
                    { "name", new AttributeValue { S = "mockCat2" } },
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
        _mockDynamoClient.PutItemAsync(Arg.Any<PutItemRequest>(), Arg.Any<CancellationToken>())
            .Returns(new PutItemResponse());

        var result = await _repository.CreateAsync(item, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", result.Id);
        Assert.Equal("mockCat", result.Name);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnUpdatedCatDto()
    {
        var item = new Cat { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockCatNew", UpdatedBy = "User1" };
        var getResponse = new GetItemResponse
        {
            Item = new Dictionary<string, AttributeValue>
            {
                { "id", new AttributeValue { S = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c" } },
                { "name", new AttributeValue { S = "mockCatOld" } },
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

        var result = await _repository.UpdateAsync(item, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", result.Id);
        Assert.Equal("mockCatNew", result.Name);
    }

    [Fact]
    public async Task ReplaceAsync_ShouldReturnReplacedCatDto()
    {
        var item = new Cat { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockCatNew", UpdatedBy = "User1" };
        var getResponse = new GetItemResponse
        {
            Item = new Dictionary<string, AttributeValue>
            {
                { "id", new AttributeValue { S = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c" } },
                { "name", new AttributeValue { S = "mockCatOld" } },
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

        var result = await _repository.ReplaceAsync(item, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", result.Id);
        Assert.Equal("mockCatNew", result.Name);
    }

    [Fact]
    public async Task DeleteAsync_ShouldMarkCatAsDeleted()
    {
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var getResponse = new GetItemResponse
        {
            Item = new Dictionary<string, AttributeValue>
            {
                { "id", new AttributeValue { S = id } },
                { "name", new AttributeValue { S = "mockCat" } },
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

        await _repository.DeleteAsync(id, CancellationToken.None);

        await _mockDynamoClient.Received(1).PutItemAsync(
            Arg.Is<PutItemRequest>(req => req.Item["isDeleted"].BOOL == true),
            Arg.Any<CancellationToken>());
    }
}

public class DogRepositoryTest
{
    private readonly IAmazonDynamoDB _mockDynamoClient;
    private readonly DogRepository _repository;

    public DogRepositoryTest()
    {
        _mockDynamoClient = Substitute.For<IAmazonDynamoDB>();
        _repository = new DogRepository(_mockDynamoClient, "mockTableName");
    }

    [Fact]
    public async Task GetAsync_ShouldReturnDogDto()
    {
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var response = new GetItemResponse
        {
            Item = new Dictionary<string, AttributeValue>
            {
                { "id", new AttributeValue { S = id } },
                { "name", new AttributeValue { S = "mockDog" } },
                { "isDeleted", new AttributeValue { BOOL = false } },
                { "createdTimestamp", new AttributeValue { S = DateTime.UtcNow.ToString("o") } },
                { "updatedTimestamp", new AttributeValue { S = DateTime.UtcNow.ToString("o") } },
                { "createdBy", new AttributeValue { S = "testUser" } },
                { "updatedBy", new AttributeValue { S = "testUser" } },
            }
        };
        _mockDynamoClient.GetItemAsync(Arg.Any<GetItemRequest>(), Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _repository.GetAsync(id, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("mockDog", result.Name);
    }

    [Fact]
    public async Task GetListAsync_ShouldReturnListOfDogDto()
    {
        var response = new ScanResponse
        {
            Items = new List<Dictionary<string, AttributeValue>>
            {
                new()
                {
                    { "id", new AttributeValue { S = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c" } },
                    { "name", new AttributeValue { S = "mockDog1" } },
                    { "isDeleted", new AttributeValue { BOOL = false } },
                    { "createdTimestamp", new AttributeValue { S = DateTime.UtcNow.ToString("o") } },
                    { "updatedTimestamp", new AttributeValue { S = DateTime.UtcNow.ToString("o") } },
                    { "createdBy", new AttributeValue { S = "testUser" } },
                    { "updatedBy", new AttributeValue { S = "testUser" } },
                },
                new()
                {
                    { "id", new AttributeValue { S = "5615ff05-3032-4459-88ad-b6a4c3e51ca0" } },
                    { "name", new AttributeValue { S = "mockDog2" } },
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
        _mockDynamoClient.PutItemAsync(Arg.Any<PutItemRequest>(), Arg.Any<CancellationToken>())
            .Returns(new PutItemResponse());

        var result = await _repository.CreateAsync(item, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", result.Id);
        Assert.Equal("mockDog", result.Name);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnUpdatedDogDto()
    {
        var item = new Dog { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockDogNew", UpdatedBy = "User1" };
        var getResponse = new GetItemResponse
        {
            Item = new Dictionary<string, AttributeValue>
            {
                { "id", new AttributeValue { S = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c" } },
                { "name", new AttributeValue { S = "mockDogOld" } },
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

        var result = await _repository.UpdateAsync(item, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", result.Id);
        Assert.Equal("mockDogNew", result.Name);
    }

    [Fact]
    public async Task ReplaceAsync_ShouldReturnReplacedDogDto()
    {
        var item = new Dog { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockDogNew", UpdatedBy = "User1" };
        var getResponse = new GetItemResponse
        {
            Item = new Dictionary<string, AttributeValue>
            {
                { "id", new AttributeValue { S = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c" } },
                { "name", new AttributeValue { S = "mockDogOld" } },
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

        var result = await _repository.ReplaceAsync(item, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", result.Id);
        Assert.Equal("mockDogNew", result.Name);
    }

    [Fact]
    public async Task DeleteAsync_ShouldMarkDogAsDeleted()
    {
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var getResponse = new GetItemResponse
        {
            Item = new Dictionary<string, AttributeValue>
            {
                { "id", new AttributeValue { S = id } },
                { "name", new AttributeValue { S = "mockDog" } },
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

        await _repository.DeleteAsync(id, CancellationToken.None);

        await _mockDynamoClient.Received(1).PutItemAsync(
            Arg.Is<PutItemRequest>(req => req.Item["isDeleted"].BOOL == true),
            Arg.Any<CancellationToken>());
    }
}

