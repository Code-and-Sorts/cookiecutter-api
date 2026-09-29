namespace KittenClaws.Api.Tests.Unit;

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using KittenClaws.Api.Controllers;
using KittenClaws.Api.Dtos;
using KittenClaws.Api.Interfaces;
using KittenClaws.Api.Requests;
using Newtonsoft.Json;
using NSubstitute;
using Xunit;

public class KittenClawsControllerTest
{
    private readonly IKittenClawsService _mockKittenClawsService;
    private readonly KittenClawsController _kittenClawsController;

    public KittenClawsControllerTest()
    {
        _mockKittenClawsService = Substitute.For<IKittenClawsService>();
        _kittenClawsController = new KittenClawsController(_mockKittenClawsService);
    }

    private static MemoryStream CreateMemoryStream<T>(T kittenClawsRequest)
    {
        var stream = new MemoryStream();
        using var writer = new StreamWriter(stream, leaveOpen: true);
        writer.Write(JsonConvert.SerializeObject(kittenClawsRequest));
        writer.Flush();
        stream.Position = 0;
        return stream;
    }

    [Fact]
    public async Task GetAsync_ReturnsKittenClawsDto()
    {
        // Arrange
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var expectedKittenClaws = new KittenClawsDto { Id = id };
        _mockKittenClawsService.GetAsync(id, Arg.Any<CancellationToken>()).Returns(expectedKittenClaws);

        // Act
        var result = await _kittenClawsController.GetAsync(id);

        // Assert
        await _mockKittenClawsService.Received(1).GetAsync(id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetListAsync_ReturnsListOfKittenClawsDto()
    {
        // Arrange
        var expectedKittenClawsList = new List<KittenClawsDto> { new KittenClawsDto { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c" }, new KittenClawsDto { Id = "5615ff05-3032-4459-88ad-b6a4c3e51ca0" } };
        _mockKittenClawsService.GetListAsync(Arg.Any<CancellationToken>()).Returns(expectedKittenClawsList);

        // Act
        await _kittenClawsController.GetListAsync();

        // Assert
        await _mockKittenClawsService.Received(1).GetListAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_ReturnsCreatedKittenClawsDto()
    {
        // Arrange
        var createRequest = new CreateKittenClawsRequest { Name = "mockCreateKittenClaws", CreatedBy = "TestUser", UpdatedBy = "TestUser" };
        var expectedKittenClaws = new KittenClawsDto { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockCreateKittenClaws" };

        _mockKittenClawsService
            .CreateAsync(Arg.Is<CreateKittenClawsRequest>(r => r.Name == createRequest.Name), Arg.Any<CancellationToken>())
            .Returns(expectedKittenClaws);

        var stream = CreateMemoryStream(createRequest);

        // Act
        var result = await _kittenClawsController.CreateAsync(stream);

        // Assert
        await _mockKittenClawsService.Received(1).CreateAsync(
            Arg.Is<CreateKittenClawsRequest>(r => r.Name == createRequest.Name), Arg.Any<CancellationToken>());
        Assert.Equal(expectedKittenClaws, result);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsUpdatedKittenClawsDto()
    {
        // Arrange
        var kittenClawsId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var updateRequest = new UpdateKittenClawsRequest { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockUpdatedKittenClaws" };
        var expectedKittenClaws = new KittenClawsDto { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockUpdatedKittenClaws" };

        _mockKittenClawsService
            .UpdateAsync(Arg.Is<UpdateKittenClawsRequest>(r => r.Name == updateRequest.Name), Arg.Any<CancellationToken>())
            .Returns(expectedKittenClaws);

        // Act
        var stream = CreateMemoryStream(updateRequest);
        var result = await _kittenClawsController.UpdateAsync(kittenClawsId, stream);

        // Assert
        await _mockKittenClawsService.Received(1).UpdateAsync(
            Arg.Is<UpdateKittenClawsRequest>(r => r.Name == updateRequest.Name), Arg.Any<CancellationToken>());
        Assert.Equal(expectedKittenClaws, result);
    }

    [Fact]
    public async Task DeleteAsync_CallsDeleteOnService()
    {
        // Arrange
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        _mockKittenClawsService.DeleteAsync(id, Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        // Act
        await _kittenClawsController.DeleteAsync(id);

        // Assert
        await _mockKittenClawsService.Received(1).DeleteAsync(id, Arg.Any<CancellationToken>());
    }
}
