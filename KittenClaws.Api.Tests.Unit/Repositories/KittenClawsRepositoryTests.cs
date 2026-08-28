
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

public class CatRepositoryTest
{
    private readonly IFirestoreContext<Cat> _mockContext;
    private readonly CatRepository _repository;

    public CatRepositoryTest()
    {
        _mockContext = Substitute.For<IFirestoreContext<Cat>>();
        _repository = new CatRepository(_mockContext);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnCatDto()
    {
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var item = new Cat { Id = id, Name = "mockCat" };
        _mockContext.GetAsync(id, Arg.Any<CancellationToken>()).Returns(item);

        var result = await _repository.GetAsync(id, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("mockCat", result.Name);
    }

    [Fact]
    public async Task GetAsync_ShouldThrowWhenNotFound()
    {
        var id = "non-existent-id";
        _mockContext.GetAsync(id, Arg.Any<CancellationToken>()).Returns((Cat?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _repository.GetAsync(id, CancellationToken.None));
    }

    [Fact]
    public async Task GetListAsync_ShouldReturnListOfCatDto()
    {
        var itemList = new List<Cat>
        {
            new() { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockCat1" },
            new() { Id = "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name = "mockCat2" }
        };
        _mockContext.GetListAsync("isDeleted", false, Arg.Any<CancellationToken>()).Returns(itemList);

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

        var result = await _repository.CreateAsync(item, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", result.Id);
        Assert.Equal("mockCat", result.Name);
        await _mockContext.Received(1).SetAsync(item.Id, item, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnUpdatedCatDto()
    {
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var item = new Cat { Id = id, Name = "mockCatNew", UpdatedBy = "User1" };
        var currentItem = new Cat { Id = id, Name = "mockCatOld", CreatedBy = "User2", CreatedTimestamp = DateTime.UtcNow };
        _mockContext.GetAsync(id, Arg.Any<CancellationToken>()).Returns(currentItem);

        var result = await _repository.UpdateAsync(item, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("mockCatNew", result.Name);
        await _mockContext.Received(1).SetAsync(id, Arg.Any<Cat>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReplaceAsync_ShouldReturnReplacedCatDto()
    {
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var item = new Cat { Id = id, Name = "mockCatNew", UpdatedBy = "User1" };
        var currentItem = new Cat { Id = id, Name = "mockCatOld", CreatedBy = "User2", CreatedTimestamp = DateTime.UtcNow };
        _mockContext.GetAsync(id, Arg.Any<CancellationToken>()).Returns(currentItem);

        var result = await _repository.ReplaceAsync(item, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("mockCatNew", result.Name);
        await _mockContext.Received(1).SetAsync(id, Arg.Any<Cat>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_ShouldMarkCatAsDeleted()
    {
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var existingItem = new Cat { Id = id, Name = "mockCat", IsDeleted = false };
        _mockContext.GetAsync(id, Arg.Any<CancellationToken>()).Returns(existingItem);

        await _repository.DeleteAsync(id, CancellationToken.None);

        await _mockContext.Received(1).SetAsync(
            id,
            Arg.Is<Cat>(k => k.IsDeleted == true),
            Arg.Any<CancellationToken>());
    }
}

public class DogRepositoryTest
{
    private readonly IFirestoreContext<Dog> _mockContext;
    private readonly DogRepository _repository;

    public DogRepositoryTest()
    {
        _mockContext = Substitute.For<IFirestoreContext<Dog>>();
        _repository = new DogRepository(_mockContext);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnDogDto()
    {
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var item = new Dog { Id = id, Name = "mockDog" };
        _mockContext.GetAsync(id, Arg.Any<CancellationToken>()).Returns(item);

        var result = await _repository.GetAsync(id, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("mockDog", result.Name);
    }

    [Fact]
    public async Task GetAsync_ShouldThrowWhenNotFound()
    {
        var id = "non-existent-id";
        _mockContext.GetAsync(id, Arg.Any<CancellationToken>()).Returns((Dog?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _repository.GetAsync(id, CancellationToken.None));
    }

    [Fact]
    public async Task GetListAsync_ShouldReturnListOfDogDto()
    {
        var itemList = new List<Dog>
        {
            new() { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockDog1" },
            new() { Id = "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name = "mockDog2" }
        };
        _mockContext.GetListAsync("isDeleted", false, Arg.Any<CancellationToken>()).Returns(itemList);

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

        var result = await _repository.CreateAsync(item, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", result.Id);
        Assert.Equal("mockDog", result.Name);
        await _mockContext.Received(1).SetAsync(item.Id, item, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnUpdatedDogDto()
    {
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var item = new Dog { Id = id, Name = "mockDogNew", UpdatedBy = "User1" };
        var currentItem = new Dog { Id = id, Name = "mockDogOld", CreatedBy = "User2", CreatedTimestamp = DateTime.UtcNow };
        _mockContext.GetAsync(id, Arg.Any<CancellationToken>()).Returns(currentItem);

        var result = await _repository.UpdateAsync(item, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("mockDogNew", result.Name);
        await _mockContext.Received(1).SetAsync(id, Arg.Any<Dog>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReplaceAsync_ShouldReturnReplacedDogDto()
    {
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var item = new Dog { Id = id, Name = "mockDogNew", UpdatedBy = "User1" };
        var currentItem = new Dog { Id = id, Name = "mockDogOld", CreatedBy = "User2", CreatedTimestamp = DateTime.UtcNow };
        _mockContext.GetAsync(id, Arg.Any<CancellationToken>()).Returns(currentItem);

        var result = await _repository.ReplaceAsync(item, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("mockDogNew", result.Name);
        await _mockContext.Received(1).SetAsync(id, Arg.Any<Dog>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_ShouldMarkDogAsDeleted()
    {
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var existingItem = new Dog { Id = id, Name = "mockDog", IsDeleted = false };
        _mockContext.GetAsync(id, Arg.Any<CancellationToken>()).Returns(existingItem);

        await _repository.DeleteAsync(id, CancellationToken.None);

        await _mockContext.Received(1).SetAsync(
            id,
            Arg.Is<Dog>(k => k.IsDeleted == true),
            Arg.Any<CancellationToken>());
    }
}

