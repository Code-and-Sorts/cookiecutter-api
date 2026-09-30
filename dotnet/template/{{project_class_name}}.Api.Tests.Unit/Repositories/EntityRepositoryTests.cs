{%- set r = resources[0].name -%}
namespace {{project_class_name}}.Api.Tests.Unit;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using {{project_class_name}}.Api.Entities;
using {{project_class_name}}.Api.Interfaces;
using {{project_class_name}}.Api.Repositories;
using {{project_class_name}}.Api.Utils;
using NSubstitute;
using Xunit;

public class EntityRepositoryTests
{
    private const string ItemId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
    private const string StoredTimestamp = "2026-01-01T00:00:00.000Z";
    private const string TimestampPattern = @"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3}Z$";
    private readonly IDocumentStore<{{ r }}Entity> _mockStore = Substitute.For<IDocumentStore<{{ r }}Entity>>();
    private readonly TestRepository _repository;

    public EntityRepositoryTests()
    {
        _repository = new TestRepository(_mockStore);
    }

    private sealed class TestRepository(IDocumentStore<{{ r }}Entity> store) : EntityRepository<{{ r }}Entity, string>(store, "Thing")
    {
        protected override string ToDto({{ r }}Entity item) => item.Name;

        public Task<string> Get(string id) => GetDtoAsync(id, TestContext.Current.CancellationToken);

        public Task<IEnumerable<string>> List(int limit) => ListAsync(limit, TestContext.Current.CancellationToken);

        public Task<string> Insert({{ r }}Entity item) => InsertAsync(item, TestContext.Current.CancellationToken);

        public Task<string> Merge({{ r }}Entity changes) =>
            MergeAsync(changes, (current, update) => current.Name = update.Name, TestContext.Current.CancellationToken);

        public Task Delete(string id) => SoftDeleteAsync(id, TestContext.Current.CancellationToken);
    }

    private static {{ r }}Entity StoredItem(string name = "stored", bool isDeleted = false) => new()
    {
        Id = ItemId,
        Name = name,
        IsDeleted = isDeleted,
        CreatedTimestamp = StoredTimestamp,
        UpdatedTimestamp = StoredTimestamp,
        CreatedBy = "User2",
        UpdatedBy = "User3",
    };

    [Fact]
    public async Task Get_ReturnsTheStoredItem()
    {
        _mockStore.GetAsync(ItemId, Arg.Any<CancellationToken>()).Returns(StoredItem());

        Assert.Equal("stored", await _repository.Get(ItemId));
    }

    [Fact]
    public async Task Get_ThrowsNotFound_WhenMissing()
    {
        _mockStore.GetAsync(ItemId, Arg.Any<CancellationToken>()).Returns(({{ r }}Entity?)null);

        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _repository.Get(ItemId));

        Assert.Equal($"Thing with id {ItemId} was not found.", exception.Message);
    }

    [Fact]
    public async Task Get_ThrowsNotFound_WhenSoftDeleted()
    {
        _mockStore.GetAsync(ItemId, Arg.Any<CancellationToken>()).Returns(StoredItem(isDeleted: true));

        await Assert.ThrowsAsync<NotFoundException>(() => _repository.Get(ItemId));
    }

    [Fact]
    public async Task List_ReturnsAtMostLimitItems()
    {
        _mockStore.GetLiveListAsync(1, Arg.Any<CancellationToken>()).Returns(new List<{{ r }}Entity> { StoredItem("first"), StoredItem("second") });

        var result = await _repository.List(1);

        Assert.Equal("first", Assert.Single(result));
    }

    [Fact]
    public async Task List_ReturnsEmpty_WhenNothingMatches()
    {
        _mockStore.GetLiveListAsync(100, Arg.Any<CancellationToken>()).Returns(new List<{{ r }}Entity>());

        Assert.Empty(await _repository.List(100));
    }

    [Fact]
    public async Task Insert_StampsIdAndOneTimestampReading()
    {
        await _repository.Insert(new {{ r }}Entity { Name = "new" });

        await _mockStore.Received(1).CreateAsync(Arg.Is<{{ r }}Entity>(k => IsNewItem(k)), Arg.Any<CancellationToken>());
    }

    private static bool IsNewItem({{ r }}Entity item) =>
        Guid.TryParseExact(item.Id, "D", out _) && item.Name == "new" && !item.IsDeleted
        && System.Text.RegularExpressions.Regex.IsMatch(item.CreatedTimestamp, TimestampPattern)
        && item.UpdatedTimestamp == item.CreatedTimestamp && item.CreatedBy == null && item.UpdatedBy == null;

    [Fact]
    public async Task Merge_KeepsCreatedFieldsAndRefreshesUpdatedTimestamp()
    {
        _mockStore.GetAsync(ItemId, Arg.Any<CancellationToken>()).Returns(StoredItem());

        Assert.Equal("changed", await _repository.Merge(new {{ r }}Entity { Id = ItemId, Name = "changed" }));

        await _mockStore.Received(1).SaveAsync(
            Arg.Is<{{ r }}Entity>(k => k.Id == ItemId && k.Name == "changed" && k.CreatedBy == "User2" && k.UpdatedBy == "User3"
                && k.CreatedTimestamp == StoredTimestamp && k.UpdatedTimestamp != StoredTimestamp
                && System.Text.RegularExpressions.Regex.IsMatch(k.UpdatedTimestamp, TimestampPattern)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Merge_ThrowsNotFound_WhenSoftDeleted()
    {
        _mockStore.GetAsync(ItemId, Arg.Any<CancellationToken>()).Returns(StoredItem(isDeleted: true));

        await Assert.ThrowsAsync<NotFoundException>(() => _repository.Merge(new {{ r }}Entity { Id = ItemId, Name = "changed" }));
        await _mockStore.DidNotReceiveWithAnyArgs().SaveAsync(default!, default);
    }

    [Fact]
    public async Task Delete_SoftDeletesAndRefreshesUpdatedTimestamp()
    {
        _mockStore.GetAsync(ItemId, Arg.Any<CancellationToken>()).Returns(StoredItem());

        await _repository.Delete(ItemId);

        await _mockStore.Received(1).SaveAsync(
            Arg.Is<{{ r }}Entity>(k => k.IsDeleted && k.CreatedTimestamp == StoredTimestamp && k.UpdatedTimestamp != StoredTimestamp),
            Arg.Any<CancellationToken>());
    }
}
