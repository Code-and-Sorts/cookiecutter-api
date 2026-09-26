
namespace KittenClaws.Api.Tests.Unit;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KittenClaws.Api.Entities;
using KittenClaws.Api.Repositories;
using KittenClaws.Api.Interfaces;
using NSubstitute;
using Xunit;

public class KittenClawsRepositoryTest
{
    private readonly IFirestoreContext<KittenClawsEntity> _mockContext;
    private readonly KittenClawsRepository _repository;

    public KittenClawsRepositoryTest()
    {
        _mockContext = Substitute.For<IFirestoreContext<KittenClawsEntity>>();
        _repository = new KittenClawsRepository(_mockContext);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnKittenClawsDto()
    {
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var item = new KittenClawsEntity { Id = id, Name = "mockKittenClaws" };
        _mockContext.GetAsync(id, Arg.Any<CancellationToken>()).Returns(item);

        var result = await _repository.GetAsync(id, TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("mockKittenClaws", result.Name);
    }

    [Fact]
    public async Task GetAsync_ShouldThrowWhenNotFound()
    {
        var id = "non-existent-id";
        _mockContext.GetAsync(id, Arg.Any<CancellationToken>()).Returns((KittenClawsEntity?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _repository.GetAsync(id, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetListAsync_ShouldReturnListOfKittenClawsDto()
    {
        var itemList = new List<KittenClawsEntity>
        {
            new() { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockKittenClaws1" },
            new() { Id = "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name = "mockKittenClaws2" }
        };
        _mockContext.GetListAsync("isDeleted", false, Arg.Any<CancellationToken>()).Returns(itemList);

        var result = await _repository.GetListAsync(TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Contains(result, res => res.Id == "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c" && res.Name == "mockKittenClaws1");
        Assert.Contains(result, res => res.Id == "5615ff05-3032-4459-88ad-b6a4c3e51ca0" && res.Name == "mockKittenClaws2");
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnCreatedKittenClawsDto()
    {
        var item = new KittenClawsEntity { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockKittenClaws" };

        var result = await _repository.CreateAsync(item, TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", result.Id);
        Assert.Equal("mockKittenClaws", result.Name);
        await _mockContext.Received(1).SetAsync(item.Id, item, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnUpdatedKittenClawsDto()
    {
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var item = new KittenClawsEntity { Id = id, Name = "mockKittenClawsNew", UpdatedBy = "User1" };
        var currentItem = new KittenClawsEntity { Id = id, Name = "mockKittenClawsOld", CreatedBy = "User2", CreatedTimestamp = DateTime.UtcNow };
        _mockContext.GetAsync(id, Arg.Any<CancellationToken>()).Returns(currentItem);

        var result = await _repository.UpdateAsync(item, TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("mockKittenClawsNew", result.Name);
        await _mockContext.Received(1).SetAsync(id, Arg.Any<KittenClawsEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReplaceAsync_ShouldReturnReplacedKittenClawsDto()
    {
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var item = new KittenClawsEntity { Id = id, Name = "mockKittenClawsNew", UpdatedBy = "User1" };
        var currentItem = new KittenClawsEntity { Id = id, Name = "mockKittenClawsOld", CreatedBy = "User2", CreatedTimestamp = DateTime.UtcNow };
        _mockContext.GetAsync(id, Arg.Any<CancellationToken>()).Returns(currentItem);

        var result = await _repository.ReplaceAsync(item, TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("mockKittenClawsNew", result.Name);
        await _mockContext.Received(1).SetAsync(id, Arg.Any<KittenClawsEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_ShouldMarkKittenClawsAsDeleted()
    {
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var existingItem = new KittenClawsEntity { Id = id, Name = "mockKittenClaws", IsDeleted = false };
        _mockContext.GetAsync(id, Arg.Any<CancellationToken>()).Returns(existingItem);

        await _repository.DeleteAsync(id, TestContext.Current.CancellationToken);

        await _mockContext.Received(1).SetAsync(
            id,
            Arg.Is<KittenClawsEntity>(k => k.IsDeleted == true),
            Arg.Any<CancellationToken>());
    }
}

