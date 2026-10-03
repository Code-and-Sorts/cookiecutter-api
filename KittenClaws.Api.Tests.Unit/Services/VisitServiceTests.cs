namespace KittenClaws.Api.Tests.Unit;

using System;
using System.Threading;
using System.Threading.Tasks;
using KittenClaws.Api.Dtos;
using KittenClaws.Api.Entities;
using KittenClaws.Api.Interfaces;
using KittenClaws.Api.Requests;
using KittenClaws.Api.Services;
using KittenClaws.Api.Utils;
using NSubstitute;
using Xunit;

public class VisitServiceTests
{
    private const string ItemId = VisitSamples.ItemId;
    private readonly IVisitRepository _visitRepositoryMock;
    private readonly VisitService _visitService;

    public VisitServiceTests()
    {
        _visitRepositoryMock = Substitute.For<IVisitRepository>();
        _visitService = new VisitService(_visitRepositoryMock);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnVisitDto()
    {
        var expectedItem = VisitSamples.Dto();
        _visitRepositoryMock.GetAsync(ItemId, Arg.Any<CancellationToken>()).Returns(expectedItem);

        var result = await _visitService.GetAsync(ItemId, TestContext.Current.CancellationToken);

        Assert.Equal(expectedItem, result);
    }

    [Fact]
    public async Task CreateAsync_StoresTheRequestAsAnEntity()
    {
        var request = await RequestBody.DeserializeAsync<CreateVisitRequest>(Mocks.CreateStream(VisitSamples.CreateBody), TestContext.Current.CancellationToken);
        var expectedItem = VisitSamples.Dto();
        VisitEntity? stored = null;
        _visitRepositoryMock.CreateAsync(Arg.Do<VisitEntity>(item => stored = item), "User1", Arg.Any<CancellationToken>()).Returns(expectedItem);

        var result = await _visitService.CreateAsync(request, "User1", TestContext.Current.CancellationToken);

        Assert.Equal(expectedItem, result);
        Assert.NotNull(stored);
        JsonAssert.Contains(request, stored);
    }

    [Fact]
    public async Task UpdateAsync_AppliesTheRequestToTheStoredEntity()
    {
        var request = await RequestBody.DeserializeAsync<UpdateVisitRequest>(Mocks.CreateStream(VisitSamples.UpdateBody), TestContext.Current.CancellationToken);
        request.Id = ItemId;
        var expectedItem = VisitSamples.Dto();
        Action<VisitEntity>? apply = null;
        _visitRepositoryMock.UpdateAsync(ItemId, Arg.Do<Action<VisitEntity>>(action => apply = action), "User1", Arg.Any<CancellationToken>()).Returns(expectedItem);

        var result = await _visitService.UpdateAsync(request, "User1", TestContext.Current.CancellationToken);

        Assert.Equal(expectedItem, result);
        Assert.NotNull(apply);
        var item = new VisitEntity();
        apply(item);
        JsonAssert.Contains(request, item);
    }
}
