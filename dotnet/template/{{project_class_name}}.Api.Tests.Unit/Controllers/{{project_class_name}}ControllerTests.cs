namespace {{project_class_name}}.Api.Tests.Unit;

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using {{project_class_name}}.Api.Controllers;
using {{project_class_name}}.Api.Dtos;
using {{project_class_name}}.Api.Interfaces;
using {{project_class_name}}.Api.Requests;
using Newtonsoft.Json;
using NSubstitute;
using Xunit;

public class ItemControllerTest
{
    private readonly IItemService _mockItemService;
    private readonly ItemController _itemController;

    public ItemControllerTest()
    {
        _mockItemService = Substitute.For<IItemService>();
        _itemController = new ItemController(_mockItemService);
    }

    private static MemoryStream CreateMemoryStream<T>(T itemRequest)
    {
        var stream = new MemoryStream();
        using var writer = new StreamWriter(stream, leaveOpen: true);
        writer.Write(JsonConvert.SerializeObject(itemRequest));
        writer.Flush();
        stream.Position = 0;
        return stream;
    }

    [Fact]
    public async Task GetAsync_ReturnsItemDto()
    {
        // Arrange
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var expectedItem = new ItemDto { Id = id };
        _mockItemService.GetAsync(id, Arg.Any<CancellationToken>()).Returns(expectedItem);

        // Act
        var result = await _itemController.GetAsync(id);

        // Assert
        await _mockItemService.Received(1).GetAsync(id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetListAsync_ReturnsListOfItemDto()
    {
        // Arrange
        var expectedItemList = new List<ItemDto> { new ItemDto { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c" }, new ItemDto { Id = "5615ff05-3032-4459-88ad-b6a4c3e51ca0" } };
        _mockItemService.GetListAsync(Arg.Any<CancellationToken>()).Returns(expectedItemList);

        // Act
        await _itemController.GetListAsync();

        // Assert
        await _mockItemService.Received(1).GetListAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_ReturnsCreatedItemDto()
    {
        // Arrange
        var createRequest = new CreateItemRequest { Name = "mockCreateItem", CreatedBy = "TestUser", UpdatedBy = "TestUser" };
        var expectedItem = new ItemDto { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockCreateItem" };

        _mockItemService
            .CreateAsync(Arg.Is<CreateItemRequest>(r => r.Name == createRequest.Name), Arg.Any<CancellationToken>())
            .Returns(expectedItem);

        var stream = CreateMemoryStream(createRequest);

        // Act
        var result = await _itemController.CreateAsync(stream);

        // Assert
        await _mockItemService.Received(1).CreateAsync(
            Arg.Is<CreateItemRequest>(r => r.Name == createRequest.Name), Arg.Any<CancellationToken>());
        Assert.Equal(expectedItem, result);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsUpdatedItemDto()
    {
        // Arrange
        var itemId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var updateRequest = new UpdateItemRequest { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockUpdatedItem" };
        var expectedItem = new ItemDto { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockUpdatedItem" };

        _mockItemService
            .UpdateAsync(Arg.Is<UpdateItemRequest>(r => r.Name == updateRequest.Name), Arg.Any<CancellationToken>())
            .Returns(expectedItem);

        // Act
        var stream = CreateMemoryStream(updateRequest);
        var result = await _itemController.UpdateAsync(itemId, stream);

        // Assert
        await _mockItemService.Received(1).UpdateAsync(
            Arg.Is<UpdateItemRequest>(r => r.Name == updateRequest.Name), Arg.Any<CancellationToken>());
        Assert.Equal(expectedItem, result);
    }

    [Fact]
    public async Task DeleteAsync_CallsDeleteOnService()
    {
        // Arrange
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        _mockItemService.DeleteAsync(id, Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        // Act
        await _itemController.DeleteAsync(id);

        // Assert
        await _mockItemService.Received(1).DeleteAsync(id, Arg.Any<CancellationToken>());
    }
}
