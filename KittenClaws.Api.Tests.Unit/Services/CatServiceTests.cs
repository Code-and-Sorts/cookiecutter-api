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

public class CatServiceTests
{
    private readonly ICatRepository _catRepositoryMock;
    private readonly CatService _catService;

    public CatServiceTests()
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

        var result = await _catService.GetAsync(itemId, TestContext.Current.CancellationToken);

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
        _catRepositoryMock.GetListAsync(25, Arg.Any<CancellationToken>())
            .Returns(expectedItemList);

        var result = await _catService.GetListAsync(25, TestContext.Current.CancellationToken);

        Assert.Equal(expectedItemList, result);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnCreatedCatDto()
    {
        var createRequest = new CreateCatRequest { Name = "mockCreateCat" };
        var expectedItem = new CatDto { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = createRequest.Name };
        _catRepositoryMock.CreateAsync(Arg.Any<CatEntity>(), "User1", Arg.Any<CancellationToken>())
            .Returns(expectedItem);

        var result = await _catService.CreateAsync(createRequest, "User1", TestContext.Current.CancellationToken);

        Assert.Equal(expectedItem, result);
        await _catRepositoryMock.Received(1).CreateAsync(
            Arg.Is<CatEntity>(entity => entity.Name == createRequest.Name), "User1", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnUpdatedCatDto()
    {
        var updateRequest = new UpdateCatRequest { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockUpdateCat" };
        var updatedCat = new CatEntity { Id = updateRequest.Id, Name = updateRequest.Name };
        var expectedItem = new CatDto { Id = updatedCat.Id, Name = updatedCat.Name };
        _catRepositoryMock.UpdateAsync(Arg.Any<CatEntity>(), "User1", Arg.Any<CancellationToken>())
            .Returns(expectedItem);

        var result = await _catService.UpdateAsync(updateRequest, "User1", TestContext.Current.CancellationToken);

        Assert.Equal(expectedItem, result);
    }

    [Fact]
    public async Task DeleteAsync_ShouldCallRepositoryDelete()
    {
        var itemId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        _catRepositoryMock.DeleteAsync(itemId, "User1", Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        await _catService.DeleteAsync(itemId, "User1", TestContext.Current.CancellationToken);

        await _catRepositoryMock.Received(1).DeleteAsync(itemId, "User1", Arg.Any<CancellationToken>());
    }
}
