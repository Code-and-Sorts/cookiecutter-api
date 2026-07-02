namespace {{project_class_name}}.Api.Tests.Unit;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using {{project_class_name}}.Api.Dtos;
using {{project_class_name}}.Api.Entities;
using {{project_class_name}}.Api.Interfaces;
using {{project_class_name}}.Api.Requests;
using {{project_class_name}}.Api.Services;
using NSubstitute;
using Xunit;

public class ItemServiceTest
{
    private readonly IItemRepository _itemRepositoryMock;
    private readonly ItemService _itemService;

    public ItemServiceTest()
    {
        _itemRepositoryMock = Substitute.For<IItemRepository>();
        _itemService = new ItemService(_itemRepositoryMock);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnItemDto()
    {
        // Arrange
        var itemId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var expectedItem = new ItemDto { Id = itemId, Name = "mockItem" };
        _itemRepositoryMock.GetAsync(itemId, Arg.Any<CancellationToken>())
            .Returns(expectedItem);

        // Act
        var result = await _itemService.GetAsync(itemId);

        // Assert
        Assert.Equal(expectedItem, result);
    }

    [Fact]
    public async Task GetListAsync_ShouldReturnListOfItemDto()
    {
        // Arrange
        var expectedItemList = new List<ItemDto>
        {
            new ItemDto { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockItem1" },
            new ItemDto { Id = "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name = "mockItem2" }
        };
        _itemRepositoryMock.GetListAsync(Arg.Any<CancellationToken>())
            .Returns(expectedItemList);

        // Act
        var result = await _itemService.GetListAsync();

        // Assert
        Assert.Equal(expectedItemList, result);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnCreatedItemDto()
    {
        // Arrange
        var createRequest = new CreateItemRequest { Name = "mockCreateItem" };
        var newItem = new Item { Id = Guid.NewGuid().ToString(), Name = createRequest.Name };
        var expectedItem = new ItemDto { Id = newItem.Id, Name = newItem.Name };
        _itemRepositoryMock.CreateAsync(Arg.Any<Item>(), Arg.Any<CancellationToken>())
            .Returns(expectedItem);

        // Act
        var result = await _itemService.CreateAsync(createRequest);

        // Assert
        Assert.Equal(expectedItem, result);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnUpdatedItemDto()
    {
        // Arrange
        var updateRequest = new UpdateItemRequest { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockUpdateItem" };
        var updatedItem = new Item { Id = updateRequest.Id, Name = updateRequest.Name };
        var expectedItem = new ItemDto { Id = updatedItem.Id, Name = updatedItem.Name };
        _itemRepositoryMock.UpdateAsync(Arg.Any<Item>(), Arg.Any<CancellationToken>())
            .Returns(expectedItem);

        // Act
        var result = await _itemService.UpdateAsync(updateRequest);

        // Assert
        Assert.Equal(expectedItem, result);
    }

    [Fact]
    public async Task DeleteAsync_ShouldCallRepositoryDelete()
    {
        // Arrange
        var itemId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        _itemRepositoryMock.DeleteAsync(itemId, Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        await _itemService.DeleteAsync(itemId);

        // Assert
        await _itemRepositoryMock.Received(1).DeleteAsync(itemId, Arg.Any<CancellationToken>());
    }
}
