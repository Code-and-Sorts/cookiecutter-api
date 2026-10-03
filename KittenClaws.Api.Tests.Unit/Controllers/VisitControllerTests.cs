namespace KittenClaws.Api.Tests.Unit;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KittenClaws.Api.Controllers;
using KittenClaws.Api.Dtos;
using KittenClaws.Api.Interfaces;
using KittenClaws.Api.Requests;
using KittenClaws.Api.Utils;
using NSubstitute;
using Xunit;

public class VisitControllerTests
{
    private const string ItemId = VisitSamples.ItemId;

    private static readonly (string Name, string Value)[] Rejected =
    [
        ("tenantId", "42"),
        ("tenantId", "null"),
        ("tenantId", "\"\""),
        ("tenantId", "\"sssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssss\""),
        ("tenantId", "\"\""),
        ("tenantId", "\"\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\""),
        ("region", "\"__invalid__\""),
        ("region", "null"),
        ("region", "\"EU\""),
        ("priority", "\"1\""),
        ("priority", "-1"),
        ("priority", "1.5"),
        ("priority", "9007199254740992"),
        ("rank", "\"1\""),
        ("rank", "null"),
        ("labels", "\"not-a-list\""),
        ("labels", "null"),
        ("labels", "[\"item1\", \"item1\"]"),
        ("reason", "42"),
        ("reason", "null"),
        ("visitedOn", "\"not-a-date\""),
        ("visitedOn", "null"),
        ("visitedOn", "\"2026-02-30\""),
        ("visitedOn", "\"2026-13-01\""),
        ("visitedOn", "\"0000-01-01\""),
        ("cost", "\"1\""),
        ("cost", "null"),
        ("cost", "-1"),
        ("paid", "\"true\""),
        ("paid", "null"),
        ("checkedAt", "\"not-a-list\""),
        ("checkedAt", "null"),
        ("checkedAt", "[null]"),
    ];

    private static readonly (string Name, string Value)[] Boundaries =
    [
        ("tenantId", "\"s\""),
        ("tenantId", "\"\\ud83d\\ude3a\""),
        ("tenantId", "\"ssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssss\""),
        ("tenantId", "\"\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\""),
        ("priority", "0"),
        ("priority", "1.0"),
        ("cost", "0"),
    ];

    private readonly IVisitService _mockVisitService;
    private readonly VisitController _visitController;

    public VisitControllerTests()
    {
        _mockVisitService = Substitute.For<IVisitService>();
        _visitController = new VisitController(_mockVisitService);
    }

    [Fact]
    public async Task GetAsync_ReturnsVisitDto()
    {
        var expectedItem = VisitSamples.Dto();
        _mockVisitService.GetAsync(ItemId, Arg.Any<CancellationToken>()).Returns(expectedItem);

        var result = await _visitController.GetAsync(ItemId, TestContext.Current.CancellationToken);

        Assert.Equal(expectedItem, result);
    }

    [Fact]
    public async Task GetAsync_ThrowsNotFound_WhenIdIsNotAUuid()
    {
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _visitController.GetAsync("not-a-uuid", TestContext.Current.CancellationToken));

        Assert.Equal("Visit with id not-a-uuid was not found.", exception.Message);
        await _mockVisitService.DidNotReceiveWithAnyArgs().GetAsync(default!, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task CreateAsync_PassesTheValidatedRequest()
    {
        CreateVisitRequest? received = null;
        _mockVisitService.CreateAsync(Arg.Do<CreateVisitRequest>(request => received = request), "User1", Arg.Any<CancellationToken>()).Returns(VisitSamples.Dto());

        var result = await _visitController.CreateAsync(Mocks.CreateStream(VisitSamples.CreateBody), "User1", TestContext.Current.CancellationToken);

        Assert.Equal(ItemId, result.Id);
        Assert.NotNull(received);
        JsonAssert.Equal(VisitSamples.CreateBody, received);
    }

    [Fact]
    public async Task CreateAsync_GivesFieldsLeftOutTheirDefaults()
    {
        CreateVisitRequest? received = null;
        _mockVisitService.CreateAsync(Arg.Do<CreateVisitRequest>(request => received = request), null, Arg.Any<CancellationToken>()).Returns(VisitSamples.Dto());

        await _visitController.CreateAsync(Mocks.CreateStream("{\"rank\": 1.5, \"reason\": \"sample\", \"visitedOn\": \"2026-01-01\"}"), null, TestContext.Current.CancellationToken);

        Assert.NotNull(received);
        JsonAssert.Equal("\"public\"", received.TenantId);
        JsonAssert.Equal("\"eu\"", received.Region);
        JsonAssert.Equal("null", received.Priority);
        JsonAssert.Equal("[]", received.Labels);
        JsonAssert.Equal("null", received.Cost);
    }

    [Fact]
    public async Task CreateAsync_ThrowsBadRequest_WhenARequiredFieldIsMissing()
    {
        var exception = await Assert.ThrowsAsync<BadRequestException>(() => _visitController.CreateAsync(Mocks.CreateStream("{}"), null, TestContext.Current.CancellationToken));

        Assert.Equal("rank is required. reason is required. visitedOn is required.", exception.Message);
    }

    public static TheoryData<string> InvalidCreateBodies =>
        RequestBodies.Invalid(VisitSamples.CreateBody, ["rank", "reason", "visitedOn"], ["paid", "checkedAt"], Rejected);

    [Theory]
    [MemberData(nameof(InvalidCreateBodies))]
    public async Task CreateAsync_ThrowsBadRequest_WhenBodyIsInvalid(string body)
    {
        await Assert.ThrowsAsync<BadRequestException>(() => _visitController.CreateAsync(Mocks.CreateStream(body), null, TestContext.Current.CancellationToken));

        await _mockVisitService.DidNotReceiveWithAnyArgs().CreateAsync(default!, default, TestContext.Current.CancellationToken);
    }

    public static TheoryData<string> CreateEdgeBodies => RequestBodies.WithEdgeValues(VisitSamples.CreateBody, Boundaries);

    [Theory]
    [MemberData(nameof(CreateEdgeBodies))]
    public async Task CreateAsync_AcceptsEdgeValues(string body)
    {
        CreateVisitRequest? received = null;
        _mockVisitService.CreateAsync(Arg.Do<CreateVisitRequest>(request => received = request), null, Arg.Any<CancellationToken>()).Returns(VisitSamples.Dto());

        await _visitController.CreateAsync(Mocks.CreateStream(body), null, TestContext.Current.CancellationToken);

        JsonAssert.Equal(body, received);
    }

    [Fact]
    public async Task UpdateAsync_PassesTheValidatedRequest()
    {
        UpdateVisitRequest? received = null;
        _mockVisitService.UpdateAsync(Arg.Do<UpdateVisitRequest>(request => received = request), "User1", Arg.Any<CancellationToken>()).Returns(VisitSamples.Dto());

        var result = await _visitController.UpdateAsync(ItemId, Mocks.CreateStream(VisitSamples.UpdateBody), "User1", TestContext.Current.CancellationToken);

        Assert.Equal(ItemId, result.Id);
        Assert.NotNull(received);
        Assert.Equal(ItemId, received.Id);
        Assert.Equal(new string[] { "checkedAt", "cost", "labels", "paid", "priority", "rank", "tenantId", "visitedOn" }, received.Sent.Order(StringComparer.Ordinal));
        JsonAssert.Equal(VisitSamples.UpdateBody, received);
    }

    [Fact]
    public async Task UpdateAsync_LeavesFieldsTheBodyOmits()
    {
        UpdateVisitRequest? received = null;
        _mockVisitService.UpdateAsync(Arg.Do<UpdateVisitRequest>(request => received = request), null, Arg.Any<CancellationToken>()).Returns(VisitSamples.Dto());

        await _visitController.UpdateAsync(ItemId, Mocks.CreateStream("{}"), null, TestContext.Current.CancellationToken);

        Assert.NotNull(received);
        Assert.Empty(received.Sent);
    }

    [Theory]
    [InlineData("priority")]
    public async Task UpdateAsync_AcceptsNullForANullableField(string name)
    {
        UpdateVisitRequest? received = null;
        _mockVisitService.UpdateAsync(Arg.Do<UpdateVisitRequest>(request => received = request), null, Arg.Any<CancellationToken>()).Returns(VisitSamples.Dto());

        await _visitController.UpdateAsync(ItemId, Mocks.CreateStream("{\"" + name + "\":null}"), null, TestContext.Current.CancellationToken);

        Assert.NotNull(received);
        Assert.Equal(new[] { name }, received.Sent);
    }

    public static TheoryData<string> InvalidUpdateBodies =>
        RequestBodies.Invalid(VisitSamples.UpdateBody, [], ["region", "reason"], Rejected);

    [Theory]
    [MemberData(nameof(InvalidUpdateBodies))]
    public async Task UpdateAsync_ThrowsBadRequest_WhenBodyIsInvalid(string body)
    {
        await Assert.ThrowsAsync<BadRequestException>(() => _visitController.UpdateAsync(ItemId, Mocks.CreateStream(body), null, TestContext.Current.CancellationToken));

        await _mockVisitService.DidNotReceiveWithAnyArgs().UpdateAsync(default!, default, TestContext.Current.CancellationToken);
    }

    public static TheoryData<string> UpdateEdgeBodies => RequestBodies.WithEdgeValues(VisitSamples.UpdateBody, Boundaries);

    [Theory]
    [MemberData(nameof(UpdateEdgeBodies))]
    public async Task UpdateAsync_AcceptsEdgeValues(string body)
    {
        UpdateVisitRequest? received = null;
        _mockVisitService.UpdateAsync(Arg.Do<UpdateVisitRequest>(request => received = request), null, Arg.Any<CancellationToken>()).Returns(VisitSamples.Dto());

        await _visitController.UpdateAsync(ItemId, Mocks.CreateStream(body), null, TestContext.Current.CancellationToken);

        JsonAssert.Equal(body, received);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsNotFound_WhenIdIsNotAUuid()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _visitController.UpdateAsync("not-a-uuid", Mocks.CreateStream(VisitSamples.UpdateBody), null, TestContext.Current.CancellationToken));

        await _mockVisitService.DidNotReceiveWithAnyArgs().UpdateAsync(default!, default, TestContext.Current.CancellationToken);
    }
}
