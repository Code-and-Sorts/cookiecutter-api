namespace KittenClaws.Api.Tests.Unit;

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

public class DogServiceTests
{
    private readonly IDogRepository _dogRepositoryMock;
    private readonly DogService _dogService;

    public DogServiceTests()
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

        var result = await _dogService.GetAsync(itemId, TestContext.Current.CancellationToken);

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
        _dogRepositoryMock.GetListAsync(25, Arg.Any<CancellationToken>())
            .Returns(expectedItemList);

        var result = await _dogService.GetListAsync(25, TestContext.Current.CancellationToken);

        Assert.Equal(expectedItemList, result);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnCreatedDogDto()
    {
        var createRequest = new CreateDogRequest { Name = "mockCreateDog" };
        var expectedItem = new DogDto { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = createRequest.Name };
        _dogRepositoryMock.CreateAsync(Arg.Any<DogEntity>(), "User1", Arg.Any<CancellationToken>())
            .Returns(expectedItem);

        var result = await _dogService.CreateAsync(createRequest, "User1", TestContext.Current.CancellationToken);

        Assert.Equal(expectedItem, result);
        await _dogRepositoryMock.Received(1).CreateAsync(
            Arg.Is<DogEntity>(entity => entity.Name == createRequest.Name), "User1", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReplaceAsync_ShouldReturnReplacedDogDto()
    {
        var replaceRequest = new ReplaceDogRequest { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockReplaceDog" };
        var replacedDog = new DogEntity { Id = replaceRequest.Id, Name = replaceRequest.Name };
        var expectedItem = new DogDto { Id = replacedDog.Id, Name = replacedDog.Name };
        _dogRepositoryMock.ReplaceAsync(Arg.Any<DogEntity>(), "User1", Arg.Any<CancellationToken>())
            .Returns(expectedItem);

        var result = await _dogService.ReplaceAsync(replaceRequest, "User1", TestContext.Current.CancellationToken);

        Assert.Equal(expectedItem, result);
    }

    [Fact]
    public async Task DeleteAsync_ShouldCallRepositoryDelete()
    {
        var itemId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        _dogRepositoryMock.DeleteAsync(itemId, "User1", Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        await _dogService.DeleteAsync(itemId, "User1", TestContext.Current.CancellationToken);

        await _dogRepositoryMock.Received(1).DeleteAsync(itemId, "User1", Arg.Any<CancellationToken>());
    }
}
