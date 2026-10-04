namespace KittenClaws.Api.Tests.Unit;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KittenClaws.Api.Dtos;
using KittenClaws.Api.Entities;
using KittenClaws.Api.Interfaces;
using KittenClaws.Api.Repositories;
using KittenClaws.Api.Utils;
using NSubstitute;
using Xunit;

public class EntityRepositoryTests
{
    private const string ItemId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
    private const string StoredTimestamp = "2026-01-01T00:00:00.000Z";
    private const string TimestampPattern = @"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3}Z$";
    private readonly IDocumentStore<CatEntity> _mockStore = Substitute.For<IDocumentStore<CatEntity>>();
    private readonly TestRepository _repository;

    public EntityRepositoryTests()
    {
        _repository = new TestRepository(_mockStore);
    }

    private sealed class TestDto : BaseDto
    {
        public string Name { get; set; } = default!;
    }

    private sealed class TestRepository(IDocumentStore<CatEntity> store) : EntityRepository<CatEntity, TestDto>(store, "Thing")
    {
        protected override void MapFields(CatEntity item, TestDto dto) => dto.Name = item.Name;

        public Task<TestDto> Get(string id) => GetDtoAsync(id, TestContext.Current.CancellationToken);

        public Task<IEnumerable<TestDto>> List(int limit) => ListAsync(limit, TestContext.Current.CancellationToken);

        public Task<TestDto> Insert(CatEntity item, string? userId = null) => InsertAsync(item, userId, TestContext.Current.CancellationToken);

        public Task<TestDto> Merge(CatEntity changes, string? userId = null) =>
            MergeAsync(changes, (current, update) => current.Name = update.Name, userId, TestContext.Current.CancellationToken);

        public Task Delete(string id, string? userId = null) => SoftDeleteAsync(id, userId, TestContext.Current.CancellationToken);
    }

    private static CatEntity StoredItem(string name = "stored", bool isDeleted = false) => new()
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
    public async Task Get_ReturnsTheStoredFields()
    {
        _mockStore.GetAsync(ItemId, Arg.Any<CancellationToken>()).Returns(StoredItem());

        var result = await _repository.Get(ItemId);

        Assert.Equal((ItemId, "stored", StoredTimestamp, "User2", StoredTimestamp, "User3"),
            (result.Id, result.Name, result.CreatedTimestamp, result.CreatedBy, result.UpdatedTimestamp, result.UpdatedBy));
    }

    [Fact]
    public async Task Get_LeavesUserIdsNull_WhenNotStored()
    {
        var stored = StoredItem();
        stored.CreatedBy = null;
        stored.UpdatedBy = null;
        _mockStore.GetAsync(ItemId, Arg.Any<CancellationToken>()).Returns(stored);

        var result = await _repository.Get(ItemId);

        Assert.Null(result.CreatedBy);
        Assert.Null(result.UpdatedBy);
    }

    [Fact]
    public async Task Get_ThrowsNotFound_WhenMissing()
    {
        _mockStore.GetAsync(ItemId, Arg.Any<CancellationToken>()).Returns((CatEntity?)null);

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
        _mockStore.GetLiveListAsync(1, Arg.Any<CancellationToken>()).Returns(new List<CatEntity> { StoredItem("first"), StoredItem("second") });

        var result = await _repository.List(1);

        Assert.Equal("first", Assert.Single(result).Name);
    }

    [Fact]
    public async Task List_ReturnsEmpty_WhenNothingMatches()
    {
        _mockStore.GetLiveListAsync(100, Arg.Any<CancellationToken>()).Returns(new List<CatEntity>());

        Assert.Empty(await _repository.List(100));
    }

    [Theory]
    [InlineData("User1")]
    [InlineData(null)]
    public async Task Insert_StampsIdOneTimestampReadingAndUserId(string? userId)
    {
        var item = new CatEntity { Name = "new" };

        var result = await _repository.Insert(item, userId);

        await _mockStore.Received(1).CreateAsync(Arg.Is<CatEntity>(k => IsNewItem(k, userId)), Arg.Any<CancellationToken>());
        Assert.Equal((item.Id, "new", item.CreatedTimestamp, userId, item.CreatedTimestamp, userId),
            (result.Id, result.Name, result.CreatedTimestamp, result.CreatedBy, result.UpdatedTimestamp, result.UpdatedBy));
    }

    private static bool IsNewItem(CatEntity item, string? userId) =>
        Guid.TryParseExact(item.Id, "D", out _) && item.Name == "new" && !item.IsDeleted
        && System.Text.RegularExpressions.Regex.IsMatch(item.CreatedTimestamp, TimestampPattern)
        && item.UpdatedTimestamp == item.CreatedTimestamp && item.CreatedBy == userId && item.UpdatedBy == userId;

    private void StoreHolds(CatEntity? stored) =>
        _mockStore.UpdateAsync(ItemId, Arg.Any<Action<CatEntity>>(), Arg.Any<CancellationToken>()).Returns(call =>
        {
            if (stored != null)
            {
                call.Arg<Action<CatEntity>>()(stored);
            }
            return stored;
        });

    [Theory]
    [InlineData("User1")]
    [InlineData(null)]
    public async Task Merge_KeepsCreatedFieldsAndRefreshesUpdatedFields(string? userId)
    {
        var stored = StoredItem();
        StoreHolds(stored);

        var result = await _repository.Merge(new CatEntity { Id = ItemId, Name = "changed" }, userId);

        Assert.True(stored.Id == ItemId && stored.Name == "changed" && stored.CreatedBy == "User2" && stored.UpdatedBy == userId
            && stored.CreatedTimestamp == StoredTimestamp && stored.UpdatedTimestamp != StoredTimestamp
            && System.Text.RegularExpressions.Regex.IsMatch(stored.UpdatedTimestamp, TimestampPattern));
        Assert.Equal((ItemId, "changed", StoredTimestamp, "User2", stored.UpdatedTimestamp, userId),
            (result.Id, result.Name, result.CreatedTimestamp, result.CreatedBy, result.UpdatedTimestamp, result.UpdatedBy));
    }

    [Fact]
    public async Task Merge_ThrowsNotFound_WhenSoftDeleted()
    {
        var stored = StoredItem(isDeleted: true);
        StoreHolds(stored);

        await Assert.ThrowsAsync<NotFoundException>(() => _repository.Merge(new CatEntity { Id = ItemId, Name = "changed" }));
        Assert.Equal("stored", stored.Name);
    }

    [Fact]
    public async Task Merge_ThrowsNotFound_WhenMissing()
    {
        StoreHolds(null);

        await Assert.ThrowsAsync<NotFoundException>(() => _repository.Merge(new CatEntity { Id = ItemId, Name = "changed" }));
    }

    [Theory]
    [InlineData("User1")]
    [InlineData(null)]
    public async Task Delete_SoftDeletesAndRefreshesUpdatedFields(string? userId)
    {
        var stored = StoredItem();
        StoreHolds(stored);

        await _repository.Delete(ItemId, userId);

        Assert.True(stored.IsDeleted && stored.CreatedBy == "User2" && stored.UpdatedBy == userId
            && stored.CreatedTimestamp == StoredTimestamp && stored.UpdatedTimestamp != StoredTimestamp);
    }
}
