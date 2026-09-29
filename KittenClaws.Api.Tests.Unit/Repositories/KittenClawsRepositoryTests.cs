
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
        // Arrange
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var kittenClaws = new KittenClaws { Id = id, Name = "mockKittenClaws" };
        _mockContext.GetAsync(id, Arg.Any<CancellationToken>()).Returns(kittenClaws);

        // Act
        var result = await _repository.GetAsync(id, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("mockKittenClaws", result.Name);
    }

    [Fact]
    public async Task GetAsync_ShouldThrowWhenNotFound()
    {
        // Arrange
        var id = "non-existent-id";
        _mockContext.GetAsync(id, Arg.Any<CancellationToken>()).Returns((KittenClaws?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _repository.GetAsync(id, CancellationToken.None));
    }

    [Fact]
    public async Task GetListAsync_ShouldReturnListOfKittenClawsDto()
    {
        // Arrange
        var kittenClawsList = new List<KittenClaws>
        {
            new() { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockKittenClaws1" },
            new() { Id = "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name = "mockKittenClaws2" }
        };
        _mockContext.GetListAsync("isDeleted", false, Arg.Any<CancellationToken>()).Returns(kittenClawsList);

        // Act
        var result = await _repository.GetListAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Contains(result, r => r.Id == "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c" && r.Name == "mockKittenClaws1");
        Assert.Contains(result, r => r.Id == "5615ff05-3032-4459-88ad-b6a4c3e51ca0" && r.Name == "mockKittenClaws2");
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnCreatedKittenClawsDto()
    {
        // Arrange
        var kittenClaws = new KittenClaws { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockKittenClaws" };

        // Act
        var result = await _repository.CreateAsync(kittenClaws, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", result.Id);
        Assert.Equal("mockKittenClaws", result.Name);
        await _mockContext.Received(1).SetAsync(kittenClaws.Id, kittenClaws, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnUpdatedKittenClawsDto()
    {
        // Arrange
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var kittenClaws = new KittenClaws { Id = id, Name = "mockKittenClawsNew", UpdatedBy = "User1" };
        var currentKittenClaws = new KittenClaws { Id = id, Name = "mockKittenClawsOld", CreatedBy = "User2", CreatedTimestamp = DateTime.UtcNow };
        _mockContext.GetAsync(id, Arg.Any<CancellationToken>()).Returns(currentKittenClaws);

        // Act
        var result = await _repository.UpdateAsync(kittenClaws, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("mockKittenClawsNew", result.Name);
        await _mockContext.Received(1).SetAsync(id, Arg.Any<KittenClaws>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_ShouldMarkItemAsDeleted()
    {
        // Arrange
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var existingKittenClaws = new KittenClaws { Id = id, Name = "mockKittenClaws", IsDeleted = false };
        _mockContext.GetAsync(id, Arg.Any<CancellationToken>()).Returns(existingKittenClaws);

        // Act
        await _repository.DeleteAsync(id, CancellationToken.None);

        // Assert
        await _mockContext.Received(1).SetAsync(
            id,
            Arg.Is<KittenClaws>(k => k.IsDeleted == true),
            Arg.Any<CancellationToken>());
    }
}
