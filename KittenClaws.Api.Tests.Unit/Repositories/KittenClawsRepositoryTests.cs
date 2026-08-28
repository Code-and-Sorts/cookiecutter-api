
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
    private readonly IFirestoreContext<KittenClaws> _mockContext;
    private readonly KittenClawsRepository _repository;

    public KittenClawsRepositoryTest()
    {
        _mockContext = Substitute.For<IFirestoreContext<KittenClaws>>();
        _repository = new KittenClawsRepository(_mockContext);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnKittenClawsDto()
    {
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var item = new KittenClaws { Id = id, Name = "mockKittenClaws" };
        _mockContext.GetAsync(id, Arg.Any<CancellationToken>()).Returns(item);

        var result = await _repository.GetAsync(id, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("mockKittenClaws", result.Name);
    }

    [Fact]
    public async Task GetAsync_ShouldThrowWhenNotFound()
    {
        var id = "non-existent-id";
        _mockContext.GetAsync(id, Arg.Any<CancellationToken>()).Returns((KittenClaws?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _repository.GetAsync(id, CancellationToken.None));
    }

    [Fact]
    public async Task GetListAsync_ShouldReturnListOfKittenClawsDto()
    {
        var itemList = new List<KittenClaws>
        {
            new() { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockKittenClaws1" },
            new() { Id = "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name = "mockKittenClaws2" }
        };
        _mockContext.GetListAsync("isDeleted", false, Arg.Any<CancellationToken>()).Returns(itemList);

        var result = await _repository.GetListAsync(CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Contains(result, res => res.Id == "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c" && res.Name == "mockKittenClaws1");
        Assert.Contains(result, res => res.Id == "5615ff05-3032-4459-88ad-b6a4c3e51ca0" && res.Name == "mockKittenClaws2");
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnCreatedKittenClawsDto()
    {
        var item = new KittenClaws { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockKittenClaws" };

        var result = await _repository.CreateAsync(item, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", result.Id);
        Assert.Equal("mockKittenClaws", result.Name);
        await _mockContext.Received(1).SetAsync(item.Id, item, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnUpdatedKittenClawsDto()
    {
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var item = new KittenClaws { Id = id, Name = "mockKittenClawsNew", UpdatedBy = "User1" };
        var currentItem = new KittenClaws { Id = id, Name = "mockKittenClawsOld", CreatedBy = "User2", CreatedTimestamp = DateTime.UtcNow };
        _mockContext.GetAsync(id, Arg.Any<CancellationToken>()).Returns(currentItem);

        var result = await _repository.UpdateAsync(item, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("mockKittenClawsNew", result.Name);
        await _mockContext.Received(1).SetAsync(id, Arg.Any<KittenClaws>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReplaceAsync_ShouldReturnReplacedKittenClawsDto()
    {
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var item = new KittenClaws { Id = id, Name = "mockKittenClawsNew", UpdatedBy = "User1" };
        var currentItem = new KittenClaws { Id = id, Name = "mockKittenClawsOld", CreatedBy = "User2", CreatedTimestamp = DateTime.UtcNow };
        _mockContext.GetAsync(id, Arg.Any<CancellationToken>()).Returns(currentItem);

        var result = await _repository.ReplaceAsync(item, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("mockKittenClawsNew", result.Name);
        await _mockContext.Received(1).SetAsync(id, Arg.Any<KittenClaws>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_ShouldMarkKittenClawsAsDeleted()
    {
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var existingItem = new KittenClaws { Id = id, Name = "mockKittenClaws", IsDeleted = false };
        _mockContext.GetAsync(id, Arg.Any<CancellationToken>()).Returns(existingItem);

        await _repository.DeleteAsync(id, CancellationToken.None);

        await _mockContext.Received(1).SetAsync(
            id,
            Arg.Is<KittenClaws>(k => k.IsDeleted == true),
            Arg.Any<CancellationToken>());
    }
}

