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
    public async Task GetAsync_ReturnsKittenClawsDto()
    {
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var expectedItem = new KittenClawsDto { Id = id };
        _mockKittenClawsService.GetAsync(id, Arg.Any<CancellationToken>()).Returns(expectedItem);

        var result = await _kittenClawsController.GetAsync(id);

        await _mockKittenClawsService.Received(1).GetAsync(id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetListAsync_ReturnsListOfKittenClawsDto()
    {
        var expectedItemList = new List<KittenClawsDto> { new KittenClawsDto { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c" }, new KittenClawsDto { Id = "5615ff05-3032-4459-88ad-b6a4c3e51ca0" } };
        _mockKittenClawsService.GetListAsync(Arg.Any<CancellationToken>()).Returns(expectedItemList);

        await _kittenClawsController.GetListAsync();

        await _mockKittenClawsService.Received(1).GetListAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_ReturnsCreatedKittenClawsDto()
    {
        var createRequest = new CreateKittenClawsRequest { Name = "mockCreateKittenClaws", CreatedBy = "TestUser", UpdatedBy = "TestUser" };
        var expectedItem = new KittenClawsDto { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockCreateKittenClaws" };

        _mockKittenClawsService
            .CreateAsync(Arg.Is<CreateKittenClawsRequest>(req => req.Name == createRequest.Name), Arg.Any<CancellationToken>())
            .Returns(expectedItem);

        var stream = CreateMemoryStream(createRequest);

        var result = await _kittenClawsController.CreateAsync(stream);

        await _mockKittenClawsService.Received(1).CreateAsync(
            Arg.Is<CreateKittenClawsRequest>(req => req.Name == createRequest.Name), Arg.Any<CancellationToken>());
        Assert.Equal(expectedItem, result);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsUpdatedKittenClawsDto()
    {
        var itemId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var updateRequest = new UpdateKittenClawsRequest { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockUpdatedKittenClaws" };
        var expectedItem = new KittenClawsDto { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockUpdatedKittenClaws" };

        _mockKittenClawsService
            .UpdateAsync(Arg.Is<UpdateKittenClawsRequest>(req => req.Name == updateRequest.Name), Arg.Any<CancellationToken>())
            .Returns(expectedItem);

        var stream = CreateMemoryStream(updateRequest);
        var result = await _kittenClawsController.UpdateAsync(itemId, stream);

        await _mockKittenClawsService.Received(1).UpdateAsync(
            Arg.Is<UpdateKittenClawsRequest>(req => req.Name == updateRequest.Name), Arg.Any<CancellationToken>());
        Assert.Equal(expectedItem, result);
    }

    [Fact]
    public async Task DeleteAsync_CallsDeleteOnService()
    {
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        _mockKittenClawsService.DeleteAsync(id, Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        await _kittenClawsController.DeleteAsync(id);

        await _mockKittenClawsService.Received(1).DeleteAsync(id, Arg.Any<CancellationToken>());
    }
}

