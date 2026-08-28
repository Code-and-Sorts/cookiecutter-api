namespace KittenClaws.Api.Tests.Unit;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KittenClaws.Api.Dtos;
using KittenClaws.Api.Entities;
using KittenClaws.Api.Interfaces;
using KittenClaws.Api.Requests;
using KittenClaws.Api.Services;
using NSubstitute;
using Xunit;

public class CatServiceTest
{
    private readonly ICatRepository _catRepositoryMock;
    private readonly CatService _catService;

    public CatServiceTest()
    {
        _catRepositoryMock = Substitute.For<ICatRepository>();
        _catService = new CatService(_catRepositoryMock);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnCatDto()
    {
        var itemId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var expectedItem = new CatDto { Id = itemId, Name = "mockCat" };
        _catRepositoryMock.GetAsync(itemId, Arg.Any<CancellationToken>())
            .Returns(expectedItem);

        var result = await _catService.GetAsync(itemId);

        Assert.Equal(expectedItem, result);
    }

    [Fact]
    public async Task GetListAsync_ShouldReturnListOfCatDto()
    {
        var expectedItemList = new List<CatDto>
        {
            new CatDto { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockCat1" },
            new CatDto { Id = "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name = "mockCat2" }
        };
        _catRepositoryMock.GetListAsync(Arg.Any<CancellationToken>())
            .Returns(expectedItemList);

        var result = await _catService.GetListAsync();

        Assert.Equal(expectedItemList, result);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnCreatedCatDto()
    {
        var createRequest = new CreateCatRequest { Name = "mockCreateCat" };
        var newCat = new Cat { Id = Guid.NewGuid().ToString(), Name = createRequest.Name };
        var expectedItem = new CatDto { Id = newCat.Id, Name = newCat.Name };
        _catRepositoryMock.CreateAsync(Arg.Any<Cat>(), Arg.Any<CancellationToken>())
            .Returns(expectedItem);

        var result = await _catService.CreateAsync(createRequest);

        Assert.Equal(expectedItem, result);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnUpdatedCatDto()
    {
        var updateRequest = new UpdateCatRequest { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockUpdateCat" };
        var updatedCat = new Cat { Id = updateRequest.Id, Name = updateRequest.Name };
        var expectedItem = new CatDto { Id = updatedCat.Id, Name = updatedCat.Name };
        _catRepositoryMock.UpdateAsync(Arg.Any<Cat>(), Arg.Any<CancellationToken>())
            .Returns(expectedItem);

        var result = await _catService.UpdateAsync(updateRequest);

        Assert.Equal(expectedItem, result);
    }

    [Fact]
    public async Task DeleteAsync_ShouldCallRepositoryDelete()
    {
        var itemId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        _catRepositoryMock.DeleteAsync(itemId, Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        await _catService.DeleteAsync(itemId);

        await _catRepositoryMock.Received(1).DeleteAsync(itemId, Arg.Any<CancellationToken>());
    }
}

public class DogServiceTest
{
    private readonly IDogRepository _dogRepositoryMock;
    private readonly DogService _dogService;

    public DogServiceTest()
    {
        _dogRepositoryMock = Substitute.For<IDogRepository>();
        _dogService = new DogService(_dogRepositoryMock);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnDogDto()
    {
        var itemId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var expectedItem = new DogDto { Id = itemId, Name = "mockDog" };
        _dogRepositoryMock.GetAsync(itemId, Arg.Any<CancellationToken>())
            .Returns(expectedItem);

        var result = await _dogService.GetAsync(itemId);

        Assert.Equal(expectedItem, result);
    }

    [Fact]
    public async Task GetListAsync_ShouldReturnListOfDogDto()
    {
        var expectedItemList = new List<DogDto>
        {
            new DogDto { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockDog1" },
            new DogDto { Id = "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name = "mockDog2" }
        };
        _dogRepositoryMock.GetListAsync(Arg.Any<CancellationToken>())
            .Returns(expectedItemList);

        var result = await _dogService.GetListAsync();

        Assert.Equal(expectedItemList, result);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnCreatedDogDto()
    {
        var createRequest = new CreateDogRequest { Name = "mockCreateDog" };
        var newDog = new Dog { Id = Guid.NewGuid().ToString(), Name = createRequest.Name };
        var expectedItem = new DogDto { Id = newDog.Id, Name = newDog.Name };
        _dogRepositoryMock.CreateAsync(Arg.Any<Dog>(), Arg.Any<CancellationToken>())
            .Returns(expectedItem);

        var result = await _dogService.CreateAsync(createRequest);

        Assert.Equal(expectedItem, result);
    }

    [Fact]
    public async Task ReplaceAsync_ShouldReturnReplacedDogDto()
    {
        var replaceRequest = new ReplaceDogRequest { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockReplaceDog" };
        var replacedDog = new Dog { Id = replaceRequest.Id, Name = replaceRequest.Name };
        var expectedItem = new DogDto { Id = replacedDog.Id, Name = replacedDog.Name };
        _dogRepositoryMock.ReplaceAsync(Arg.Any<Dog>(), Arg.Any<CancellationToken>())
            .Returns(expectedItem);

        var result = await _dogService.ReplaceAsync(replaceRequest);

        Assert.Equal(expectedItem, result);
    }

    [Fact]
    public async Task DeleteAsync_ShouldCallRepositoryDelete()
    {
        var itemId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        _dogRepositoryMock.DeleteAsync(itemId, Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        await _dogService.DeleteAsync(itemId);

        await _dogRepositoryMock.Received(1).DeleteAsync(itemId, Arg.Any<CancellationToken>());
    }
}

