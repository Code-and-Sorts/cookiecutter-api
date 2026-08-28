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

public class CatControllerTest
{
    private readonly ICatService _mockCatService;
    private readonly CatController _catController;

    public CatControllerTest()
    {
        _mockCatService = Substitute.For<ICatService>();
        _catController = new CatController(_mockCatService);
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
    public async Task GetAsync_ReturnsCatDto()
    {
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var expectedItem = new CatDto { Id = id };
        _mockCatService.GetAsync(id, Arg.Any<CancellationToken>()).Returns(expectedItem);

        var result = await _catController.GetAsync(id);

        await _mockCatService.Received(1).GetAsync(id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetListAsync_ReturnsListOfCatDto()
    {
        var expectedItemList = new List<CatDto> { new CatDto { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c" }, new CatDto { Id = "5615ff05-3032-4459-88ad-b6a4c3e51ca0" } };
        _mockCatService.GetListAsync(Arg.Any<CancellationToken>()).Returns(expectedItemList);

        await _catController.GetListAsync();

        await _mockCatService.Received(1).GetListAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_ReturnsCreatedCatDto()
    {
        var createRequest = new CreateCatRequest { Name = "mockCreateCat", CreatedBy = "TestUser", UpdatedBy = "TestUser" };
        var expectedItem = new CatDto { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockCreateCat" };

        _mockCatService
            .CreateAsync(Arg.Is<CreateCatRequest>(req => req.Name == createRequest.Name), Arg.Any<CancellationToken>())
            .Returns(expectedItem);

        var stream = CreateMemoryStream(createRequest);

        var result = await _catController.CreateAsync(stream);

        await _mockCatService.Received(1).CreateAsync(
            Arg.Is<CreateCatRequest>(req => req.Name == createRequest.Name), Arg.Any<CancellationToken>());
        Assert.Equal(expectedItem, result);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsUpdatedCatDto()
    {
        var itemId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var updateRequest = new UpdateCatRequest { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockUpdatedCat" };
        var expectedItem = new CatDto { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockUpdatedCat" };

        _mockCatService
            .UpdateAsync(Arg.Is<UpdateCatRequest>(req => req.Name == updateRequest.Name), Arg.Any<CancellationToken>())
            .Returns(expectedItem);

        var stream = CreateMemoryStream(updateRequest);
        var result = await _catController.UpdateAsync(itemId, stream);

        await _mockCatService.Received(1).UpdateAsync(
            Arg.Is<UpdateCatRequest>(req => req.Name == updateRequest.Name), Arg.Any<CancellationToken>());
        Assert.Equal(expectedItem, result);
    }

    [Fact]
    public async Task DeleteAsync_CallsDeleteOnService()
    {
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        _mockCatService.DeleteAsync(id, Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        await _catController.DeleteAsync(id);

        await _mockCatService.Received(1).DeleteAsync(id, Arg.Any<CancellationToken>());
    }
}

public class DogControllerTest
{
    private readonly IDogService _mockDogService;
    private readonly DogController _dogController;

    public DogControllerTest()
    {
        _mockDogService = Substitute.For<IDogService>();
        _dogController = new DogController(_mockDogService);
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
    public async Task GetAsync_ReturnsDogDto()
    {
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var expectedItem = new DogDto { Id = id };
        _mockDogService.GetAsync(id, Arg.Any<CancellationToken>()).Returns(expectedItem);

        var result = await _dogController.GetAsync(id);

        await _mockDogService.Received(1).GetAsync(id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetListAsync_ReturnsListOfDogDto()
    {
        var expectedItemList = new List<DogDto> { new DogDto { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c" }, new DogDto { Id = "5615ff05-3032-4459-88ad-b6a4c3e51ca0" } };
        _mockDogService.GetListAsync(Arg.Any<CancellationToken>()).Returns(expectedItemList);

        await _dogController.GetListAsync();

        await _mockDogService.Received(1).GetListAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_ReturnsCreatedDogDto()
    {
        var createRequest = new CreateDogRequest { Name = "mockCreateDog", CreatedBy = "TestUser", UpdatedBy = "TestUser" };
        var expectedItem = new DogDto { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockCreateDog" };

        _mockDogService
            .CreateAsync(Arg.Is<CreateDogRequest>(req => req.Name == createRequest.Name), Arg.Any<CancellationToken>())
            .Returns(expectedItem);

        var stream = CreateMemoryStream(createRequest);

        var result = await _dogController.CreateAsync(stream);

        await _mockDogService.Received(1).CreateAsync(
            Arg.Is<CreateDogRequest>(req => req.Name == createRequest.Name), Arg.Any<CancellationToken>());
        Assert.Equal(expectedItem, result);
    }

    [Fact]
    public async Task ReplaceAsync_ReturnsReplacedDogDto()
    {
        var itemId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var replaceRequest = new ReplaceDogRequest { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockReplacedDog" };
        var expectedItem = new DogDto { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockReplacedDog" };

        _mockDogService
            .ReplaceAsync(Arg.Is<ReplaceDogRequest>(req => req.Name == replaceRequest.Name), Arg.Any<CancellationToken>())
            .Returns(expectedItem);

        var stream = CreateMemoryStream(replaceRequest);
        var result = await _dogController.ReplaceAsync(itemId, stream);

        await _mockDogService.Received(1).ReplaceAsync(
            Arg.Is<ReplaceDogRequest>(req => req.Name == replaceRequest.Name), Arg.Any<CancellationToken>());
        Assert.Equal(expectedItem, result);
    }

    [Fact]
    public async Task DeleteAsync_CallsDeleteOnService()
    {
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        _mockDogService.DeleteAsync(id, Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        await _dogController.DeleteAsync(id);

        await _mockDogService.Received(1).DeleteAsync(id, Arg.Any<CancellationToken>());
    }
}

