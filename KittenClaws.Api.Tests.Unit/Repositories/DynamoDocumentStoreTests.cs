namespace KittenClaws.Api.Tests.Unit;

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

public class DynamoDocumentStoreTests
{
    private const string ItemId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
    private const string StoredTimestamp = "2026-01-01T00:00:00.000Z";
    private readonly IAmazonDynamoDB _mockDynamoClient = Substitute.For<IAmazonDynamoDB>();
    private readonly DynamoDocumentStore<CatEntity> _store;

    public DynamoDocumentStoreTests()
    {
        _store = new DynamoDocumentStore<CatEntity>(_mockDynamoClient, "mockTableName");
    }

    private static Dictionary<string, AttributeValue> StoredItem(string id = ItemId, string name = "mock", bool isDeleted = false) => new()
    {
        { "id", new AttributeValue { S = id } },
        { "name", new AttributeValue { S = name } },
        { "isDeleted", new AttributeValue { BOOL = isDeleted } },
        { "createdTimestamp", new AttributeValue { S = StoredTimestamp } },
        { "updatedTimestamp", new AttributeValue { S = StoredTimestamp } },
        { "createdBy", new AttributeValue { S = "User2" } },
    };

    [Fact]
    public async Task GetAsync_MapsBaseFieldsAndEntityFields()
    {
        _mockDynamoClient.GetItemAsync(Arg.Is<GetItemRequest>(req => req.TableName == "mockTableName" && req.Key["id"].S == ItemId), Arg.Any<CancellationToken>())
            .Returns(new GetItemResponse { Item = StoredItem(isDeleted: true) });

        var item = await _store.GetAsync(ItemId, TestContext.Current.CancellationToken);

        Assert.NotNull(item);
        Assert.Equal(ItemId, item.Id);
        Assert.Equal("mock", item.Name);
        Assert.True(item.IsDeleted);
        Assert.Equal(StoredTimestamp, item.CreatedTimestamp);
        Assert.Equal("User2", item.CreatedBy);
        Assert.Null(item.UpdatedBy);
    }

    [Fact]
    public async Task GetAsync_ReturnsNull_WhenTheItemIsMissing()
    {
        _mockDynamoClient.GetItemAsync(Arg.Any<GetItemRequest>(), Arg.Any<CancellationToken>()).Returns(new GetItemResponse());

        Assert.Null(await _store.GetAsync(ItemId, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetLiveListAsync_ScansLiveItemsWithTheLimit()
    {
        _mockDynamoClient.ScanAsync(Arg.Any<ScanRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ScanResponse { Items = [StoredItem(ItemId, "first")] });

        var items = await _store.GetLiveListAsync(1, TestContext.Current.CancellationToken);

        Assert.Equal("first", Assert.Single(items).Name);
        await _mockDynamoClient.Received(1).ScanAsync(
            Arg.Is<ScanRequest>(req => req.Limit == 1 && req.FilterExpression == "isDeleted = :isDeleted" && req.ExpressionAttributeValues[":isDeleted"].BOOL == false),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetLiveListAsync_KeepsScanning_UntilTheLimitIsReached()
    {
        var lastKey = new Dictionary<string, AttributeValue> { { "id", new AttributeValue { S = ItemId } } };
        _mockDynamoClient.ScanAsync(Arg.Any<ScanRequest>(), Arg.Any<CancellationToken>())
            .Returns(
                new ScanResponse { Items = [StoredItem(ItemId, "first")], LastEvaluatedKey = lastKey },
                new ScanResponse { Items = [StoredItem("5615ff05-3032-4459-88ad-b6a4c3e51ca0", "second")] });

        var items = await _store.GetLiveListAsync(5, TestContext.Current.CancellationToken);

        Assert.Equal(new[] { "first", "second" }, items.Select(item => item.Name).ToArray());
        await _mockDynamoClient.Received(2).ScanAsync(Arg.Any<ScanRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetLiveListAsync_ReturnsEmpty_WhenTheScanReturnsNoItems()
    {
        _mockDynamoClient.ScanAsync(Arg.Any<ScanRequest>(), Arg.Any<CancellationToken>()).Returns(new ScanResponse());

        Assert.Empty(await _store.GetLiveListAsync(100, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task CreateAsync_PutsTheItemWithoutUnsetFields()
    {
        var item = new CatEntity { Id = ItemId, Name = "mock", CreatedTimestamp = StoredTimestamp, UpdatedTimestamp = StoredTimestamp };

        await _store.CreateAsync(item, TestContext.Current.CancellationToken);

        await _mockDynamoClient.Received(1).PutItemAsync(
            Arg.Is<PutItemRequest>(req => req.TableName == "mockTableName" && req.Item.Count == 5 && req.Item["name"].S == "mock"
                && req.Item["isDeleted"].BOOL == false && !req.Item.ContainsKey("createdBy") && !req.Item.ContainsKey("updatedBy")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SaveAsync_PutsCreatedByAndUpdatedByWhenSet()
    {
        var item = new CatEntity { Id = ItemId, Name = "mock", CreatedTimestamp = StoredTimestamp, UpdatedTimestamp = StoredTimestamp, CreatedBy = "User2", UpdatedBy = "User3" };

        await _store.SaveAsync(item, TestContext.Current.CancellationToken);

        await _mockDynamoClient.Received(1).PutItemAsync(
            Arg.Is<PutItemRequest>(req => req.Item["createdBy"].S == "User2" && req.Item["updatedBy"].S == "User3"),
            Arg.Any<CancellationToken>());
    }
}
