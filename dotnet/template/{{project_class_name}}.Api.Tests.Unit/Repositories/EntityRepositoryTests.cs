namespace {{project_class_name}}.Api.Tests.Unit;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
    private readonly IDocumentStore<TestRecord> _mockStore = Substitute.For<IDocumentStore<TestRecord>>();
    private readonly TestRepository _repository;

    public EntityRepositoryTests()
    {
        _repository = new TestRepository(_mockStore);
    }

    private sealed class TestRepository(IDocumentStore<TestRecord> store) : EntityRepository<TestRecord, string>(store, "Thing")
    {
        protected override string ToDto(TestRecord item) => item.Text!;

        public Task<string> Get(string id) => GetDtoAsync(id, TestContext.Current.CancellationToken);

        public Task<IEnumerable<string>> List(int limit) => ListAsync(limit, TestContext.Current.CancellationToken);

        public Task<string> Insert(TestRecord item, string? userId = null) => InsertAsync(item, userId, TestContext.Current.CancellationToken);

        public Task<string> Merge(string text, string? userId = null) =>
            MergeAsync(ItemId, current => current.Text = text, userId, TestContext.Current.CancellationToken);

        public Task Delete(string id, string? userId = null) => SoftDeleteAsync(id, userId, TestContext.Current.CancellationToken);
    }

    private static TestRecord StoredItem(string name = "stored", bool isDeleted = false) => new()
    {
        Id = ItemId,
        Text = name,
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
        _mockStore.GetAsync(ItemId, Arg.Any<CancellationToken>()).Returns((TestRecord?)null);

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
        _mockStore.GetLiveListAsync(1, Arg.Any<CancellationToken>()).Returns(new List<TestRecord> { StoredItem("first"), StoredItem("second") });

        var result = await _repository.List(1);

        Assert.Equal("first", Assert.Single(result));
    }

    [Fact]
    public async Task List_ReturnsEmpty_WhenNothingMatches()
    {
        _mockStore.GetLiveListAsync(100, Arg.Any<CancellationToken>()).Returns(new List<TestRecord>());

        Assert.Empty(await _repository.List(100));
    }

    [Theory]
    [InlineData("User1")]
    [InlineData(null)]
    public async Task Insert_StampsIdOneTimestampReadingAndUserId(string? userId)
    {
        await _repository.Insert(new TestRecord { Text = "new" }, userId);

        await _mockStore.Received(1).CreateAsync(Arg.Is<TestRecord>(k => IsNewItem(k, userId)), Arg.Any<CancellationToken>());
    }

    private static bool IsNewItem(TestRecord item, string? userId) =>
        Guid.TryParseExact(item.Id, "D", out _) && item.Text == "new" && !item.IsDeleted
        && System.Text.RegularExpressions.Regex.IsMatch(item.CreatedTimestamp, TimestampPattern)
        && item.UpdatedTimestamp == item.CreatedTimestamp && item.CreatedBy == userId && item.UpdatedBy == userId;

    [Theory]
    [InlineData("User1")]
    [InlineData(null)]
    public async Task Merge_KeepsCreatedFieldsAndRefreshesUpdatedFields(string? userId)
    {
        _mockStore.GetAsync(ItemId, Arg.Any<CancellationToken>()).Returns(StoredItem());

        Assert.Equal("changed", await _repository.Merge("changed", userId));

        await _mockStore.Received(1).SaveAsync(
            Arg.Is<TestRecord>(k => k.Id == ItemId && k.Text == "changed" && k.CreatedBy == "User2" && k.UpdatedBy == userId
                && k.CreatedTimestamp == StoredTimestamp && k.UpdatedTimestamp != StoredTimestamp
                && System.Text.RegularExpressions.Regex.IsMatch(k.UpdatedTimestamp, TimestampPattern)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Merge_ThrowsNotFound_WhenSoftDeleted()
    {
        _mockStore.GetAsync(ItemId, Arg.Any<CancellationToken>()).Returns(StoredItem(isDeleted: true));

        await Assert.ThrowsAsync<NotFoundException>(() => _repository.Merge("changed"));
        await _mockStore.DidNotReceiveWithAnyArgs().SaveAsync(default!, TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData("User1")]
    [InlineData(null)]
    public async Task Delete_SoftDeletesAndRefreshesUpdatedFields(string? userId)
    {
        _mockStore.GetAsync(ItemId, Arg.Any<CancellationToken>()).Returns(StoredItem());

        await _repository.Delete(ItemId, userId);

        await _mockStore.Received(1).SaveAsync(
            Arg.Is<TestRecord>(k => k.IsDeleted && k.CreatedBy == "User2" && k.UpdatedBy == userId
                && k.CreatedTimestamp == StoredTimestamp && k.UpdatedTimestamp != StoredTimestamp),
            Arg.Any<CancellationToken>());
    }
}
