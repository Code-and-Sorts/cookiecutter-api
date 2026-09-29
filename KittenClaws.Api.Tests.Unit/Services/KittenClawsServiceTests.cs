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

public class KittenClawsServiceTest
{
    private readonly IKittenClawsRepository _kittenClawsRepositoryMock;
    private readonly KittenClawsService _kittenClawsService;

    public KittenClawsServiceTest()
    {
        _kittenClawsRepositoryMock = Substitute.For<IKittenClawsRepository>();
        _kittenClawsService = new KittenClawsService(_kittenClawsRepositoryMock);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnKittenClawsDto()
    {
        // Arrange
        var kittenClawsId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var expectedKittenClaws = new KittenClawsDto { Id = kittenClawsId, Name = "mockKittenClaws" };
        _kittenClawsRepositoryMock.GetAsync(kittenClawsId, Arg.Any<CancellationToken>())
            .Returns(expectedKittenClaws);

        // Act
        var result = await _kittenClawsService.GetAsync(kittenClawsId);

        // Assert
        Assert.Equal(expectedKittenClaws, result);
    }

    [Fact]
    public async Task GetListAsync_ShouldReturnListOfKittenClawsDto()
    {
        // Arrange
        var expectedKittenClawsList = new List<KittenClawsDto>
        {
            new KittenClawsDto { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockKittenClaws1" },
            new KittenClawsDto { Id = "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name = "mockKittenClaws2" }
        };
        _kittenClawsRepositoryMock.GetListAsync(Arg.Any<CancellationToken>())
            .Returns(expectedKittenClawsList);

        // Act
        var result = await _kittenClawsService.GetListAsync();

        // Assert
        Assert.Equal(expectedKittenClawsList, result);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnCreatedKittenClawsDto()
    {
        // Arrange
        var createRequest = new CreateKittenClawsRequest { Name = "mockCreateKittenClaws" };
        var newKittenClaws = new KittenClaws { Id = Guid.NewGuid().ToString(), Name = createRequest.Name };
        var expectedKittenClaws = new KittenClawsDto { Id = newKittenClaws.Id, Name = newKittenClaws.Name };
        _kittenClawsRepositoryMock.CreateAsync(Arg.Any<KittenClaws>(), Arg.Any<CancellationToken>())
            .Returns(expectedKittenClaws);

        // Act
        var result = await _kittenClawsService.CreateAsync(createRequest);

        // Assert
        Assert.Equal(expectedKittenClaws, result);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnUpdatedKittenClawsDto()
    {
        // Arrange
        var updateRequest = new UpdateKittenClawsRequest { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockUpdateKittenClaws" };
        var updatedKittenClaws = new KittenClaws { Id = updateRequest.Id, Name = updateRequest.Name };
        var expectedKittenClaws = new KittenClawsDto { Id = updatedKittenClaws.Id, Name = updatedKittenClaws.Name };
        _kittenClawsRepositoryMock.UpdateAsync(Arg.Any<KittenClaws>(), Arg.Any<CancellationToken>())
            .Returns(expectedKittenClaws);

        // Act
        var result = await _kittenClawsService.UpdateAsync(updateRequest);

        // Assert
        Assert.Equal(expectedKittenClaws, result);
    }

    [Fact]
    public async Task DeleteAsync_ShouldCallRepositoryDelete()
    {
        // Arrange
        var kittenClawsId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        _kittenClawsRepositoryMock.DeleteAsync(kittenClawsId, Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        await _kittenClawsService.DeleteAsync(kittenClawsId);

        // Assert
        await _kittenClawsRepositoryMock.Received(1).DeleteAsync(kittenClawsId, Arg.Any<CancellationToken>());
    }
}
