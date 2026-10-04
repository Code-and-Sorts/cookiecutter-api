namespace KittenClaws.Api.Tests.Unit;

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using KittenClaws.Api.Entities;
using KittenClaws.Api.Repositories;
using KittenClaws.Api.Utils;
using Microsoft.Azure.Cosmos;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

public class CosmosDocumentStoreTests
{
    private const string ItemId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
    private readonly Container _mockContainer = Substitute.For<Container>();
    private readonly CosmosDocumentStore<CatEntity> _store;

    public CosmosDocumentStoreTests()
    {
        _store = new CosmosDocumentStore<CatEntity>(_mockContainer);
    }

    private static CatEntity Item(string id = ItemId, string name = "mock") => new()
    {
        Id = id,
        Name = name,
        CreatedTimestamp = "2026-01-01T00:00:00.000Z",
        UpdatedTimestamp = "2026-01-01T00:00:00.000Z",
    };

    [Fact]
    public void Entity_IsStoredWithCamelCaseFieldsAndWithoutUnsetCreatedByOrUpdatedBy()
    {
        using var document = JsonDocument.Parse(JsonSerializer.Serialize(Item(), Json.Options));

        var fields = document.RootElement.EnumerateObject().Select(field => field.Name).Order().ToArray();
        Assert.Equal(new[] { "createdTimestamp", "id", "isDeleted", "name", "updatedTimestamp" }, fields);
        Assert.Contains("\"createdBy\":\"User2\"", JsonSerializer.Serialize(new CatEntity { Id = ItemId, CreatedBy = "User2" }, Json.Options));
    }

    [Fact]
    public async Task GetAsync_ReturnsTheItem()
    {
        var response = Substitute.For<ItemResponse<CatEntity>>();
        response.Resource.Returns(Item());
        _mockContainer.ReadItemAsync<CatEntity>(ItemId, new PartitionKey(ItemId), null, Arg.Any<CancellationToken>()).Returns(response);

        var item = await _store.GetAsync(ItemId, TestContext.Current.CancellationToken);

        Assert.Equal("mock", item?.Name);
    }

    [Fact]
    public async Task GetAsync_ReturnsNull_WhenCosmosReports404WithSubstatus0()
    {
        _mockContainer.ReadItemAsync<CatEntity>(ItemId, Arg.Any<PartitionKey>(), Arg.Any<ItemRequestOptions>(), Arg.Any<CancellationToken>())
            .Throws(new CosmosException("Resource Not Found. ActivityId: 123", HttpStatusCode.NotFound, 0, "123", 0));

        Assert.Null(await _store.GetAsync(ItemId, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetAsync_Throws_WhenTheContainerIsMissing()
    {
        _mockContainer.ReadItemAsync<CatEntity>(ItemId, Arg.Any<PartitionKey>(), Arg.Any<ItemRequestOptions>(), Arg.Any<CancellationToken>())
            .Throws(new CosmosException("Owner resource does not exist", HttpStatusCode.NotFound, 1003, "123", 0));

        var exception = await Assert.ThrowsAsync<CosmosException>(() => _store.GetAsync(ItemId, TestContext.Current.CancellationToken));

        Assert.Equal(1003, exception.SubStatusCode);
    }

    [Fact]
    public async Task GetLiveListAsync_QueriesLiveItemsWithTheLimit()
    {
        var feedResponse = Substitute.For<FeedResponse<CatEntity>>();
        feedResponse.Resource.Returns(new List<CatEntity> { Item(name: "first") });
        var feedIterator = Substitute.For<FeedIterator<CatEntity>>();
        feedIterator.HasMoreResults.Returns(true, false);
        feedIterator.ReadNextAsync(Arg.Any<CancellationToken>()).Returns(feedResponse);
        _mockContainer.GetItemQueryIterator<CatEntity>("SELECT * FROM c WHERE c.isDeleted = false OFFSET 0 LIMIT 5").Returns(feedIterator);

        var items = await _store.GetLiveListAsync(5, TestContext.Current.CancellationToken);

        Assert.Equal("first", Assert.Single(items).Name);
    }

    [Fact]
    public async Task CreateAsync_CreatesTheItemInItsPartition()
    {
        var item = Item();

        await _store.CreateAsync(item, TestContext.Current.CancellationToken);

        await _mockContainer.Received(1).CreateItemAsync(item, new PartitionKey(ItemId), null, Arg.Any<CancellationToken>());
    }

    private void ContainerHolds(CatEntity item)
    {
        var response = Substitute.For<ItemResponse<CatEntity>>();
        response.Resource.Returns(item);
        response.ETag.Returns("etag-1");
        _mockContainer.ReadItemAsync<CatEntity>(ItemId, new PartitionKey(ItemId), null, Arg.Any<CancellationToken>()).Returns(response);
    }

    [Fact]
    public async Task UpdateAsync_ReplacesTheItemItReadOnlyIfItsETagStillMatches()
    {
        var stored = Item();
        ContainerHolds(stored);

        var item = await _store.UpdateAsync(ItemId, current => current.Name = "changed", TestContext.Current.CancellationToken);

        Assert.Same(stored, item);
        Assert.Equal("changed", item?.Name);
        await _mockContainer.Received(1).ReadItemAsync<CatEntity>(ItemId, new PartitionKey(ItemId), null, Arg.Any<CancellationToken>());
        await _mockContainer.Received(1).ReplaceItemAsync(
            stored, ItemId, new PartitionKey(ItemId), Arg.Is<ItemRequestOptions>(options => options.IfMatchEtag == "etag-1"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenTheItemIsMissing()
    {
        _mockContainer.ReadItemAsync<CatEntity>(ItemId, Arg.Any<PartitionKey>(), Arg.Any<ItemRequestOptions>(), Arg.Any<CancellationToken>())
            .Throws(new CosmosException("Resource Not Found. ActivityId: 123", HttpStatusCode.NotFound, 0, "123", 0));

        Assert.Null(await _store.UpdateAsync(ItemId, _ => { }, TestContext.Current.CancellationToken));
        await _mockContainer.DidNotReceiveWithAnyArgs().ReplaceItemAsync<CatEntity>(default!, default!, default, default, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task UpdateAsync_LosingARaceIsAGeneric500()
    {
        ContainerHolds(Item());
        _mockContainer.ReplaceItemAsync(Arg.Any<CatEntity>(), ItemId, Arg.Any<PartitionKey>(), Arg.Any<ItemRequestOptions>(), Arg.Any<CancellationToken>())
            .Throws(new CosmosException("Precondition Failed", HttpStatusCode.PreconditionFailed, 0, "123", 0));

        var exception = await Assert.ThrowsAsync<CosmosException>(() => _store.UpdateAsync(ItemId, _ => { }, TestContext.Current.CancellationToken));

        Assert.Equal(500, ErrorDetector.Classify(exception, new RecordingLogger<CosmosDocumentStoreTests>(), TestContext.Current.CancellationToken).StatusCode);
    }
}
