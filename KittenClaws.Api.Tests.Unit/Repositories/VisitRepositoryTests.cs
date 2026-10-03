namespace KittenClaws.Api.Tests.Unit;

using System;
using System.Threading;
using System.Threading.Tasks;
using KittenClaws.Api.Dtos;
using KittenClaws.Api.Entities;
using KittenClaws.Api.Interfaces;
using KittenClaws.Api.Repositories;
using KittenClaws.Api.Utils;
using NSubstitute;
using Xunit;

public class VisitRepositoryTests
{
    private const string ItemId = VisitSamples.ItemId;
    private readonly IDocumentStore<VisitEntity> _mockStore = Substitute.For<IDocumentStore<VisitEntity>>();
    private readonly VisitRepository _repository;

    public VisitRepositoryTests()
    {
        _repository = new VisitRepository(_mockStore);
    }

    [Fact]
    public async Task GetAsync_ReturnsTheStoredItemAsADto()
    {
        _mockStore.GetAsync(ItemId, Arg.Any<CancellationToken>()).Returns(VisitSamples.Entity());

        var result = await _repository.GetAsync(ItemId, TestContext.Current.CancellationToken);

        Assert.Equal(Json.Serialize(VisitSamples.Dto()), Json.Serialize(result));
    }

    [Fact]
    public async Task CreateAsync_StoresTheItemWithANewIdAndReturnsItsDto()
    {
        var item = VisitSamples.Entity();

        var result = await _repository.CreateAsync(item, null, TestContext.Current.CancellationToken);

        Assert.NotEqual(ItemId, result.Id);
        await _mockStore.Received(1).CreateAsync(Arg.Is<VisitEntity>(stored => stored.Id == result.Id && !stored.IsDeleted), Arg.Any<CancellationToken>());
        Assert.Equal(Json.Serialize(new VisitDto(item)), Json.Serialize(result));
    }

    private VisitEntity StoreHoldsTheSample()
    {
        var stored = VisitSamples.Entity();
        _mockStore.UpdateAsync(ItemId, Arg.Any<Action<VisitEntity>>(), Arg.Any<CancellationToken>()).Returns(call =>
        {
            call.Arg<Action<VisitEntity>>()(stored);
            return stored;
        });
        return stored;
    }

    [Fact]
    public async Task UpdateAsync_AppliesTheChangesAndKeepsCreatedFields()
    {
        var stored = StoreHoldsTheSample();
        VisitEntity? applied = null;

        var result = await _repository.UpdateAsync(ItemId, item => applied = item, "User1", TestContext.Current.CancellationToken);

        Assert.Same(stored, applied);
        Assert.Equal(ItemId, result.Id);
        Assert.True(stored.CreatedBy == "User2" && stored.UpdatedBy == "User1"
            && stored.CreatedTimestamp == VisitSamples.StoredTimestamp && stored.UpdatedTimestamp != VisitSamples.StoredTimestamp);
    }
}
