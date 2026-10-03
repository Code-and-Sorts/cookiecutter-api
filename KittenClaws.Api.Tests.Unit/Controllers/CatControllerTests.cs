namespace KittenClaws.Api.Tests.Unit;

using System;
using System.Collections.Generic;
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

public class CatControllerTests
{
    private const string ItemId = CatSamples.ItemId;

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
        ("name", "42"),
        ("name", "null"),
        ("name", "\"\""),
        ("name", "\"sssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssss\""),
        ("name", "\"\""),
        ("name", "\"\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\""),
        ("breed", "\"__invalid__\""),
        ("breed", "null"),
        ("breed", "\"SIAMESE\""),
        ("ageYears", "\"1\""),
        ("ageYears", "null"),
        ("ageYears", "-1"),
        ("ageYears", "41"),
        ("ageYears", "1.5"),
        ("ageYears", "9007199254740992"),
        ("weightKg", "\"1\""),
        ("weightKg", "0"),
        ("weightKg", "100"),
        ("indoor", "\"true\""),
        ("indoor", "null"),
        ("birthDate", "\"not-a-date\""),
        ("birthDate", "null"),
        ("birthDate", "\"2026-02-30\""),
        ("birthDate", "\"2026-13-01\""),
        ("birthDate", "\"0000-01-01\""),
        ("microchipId", "\"not-a-uuid\""),
        ("microchipId", "null"),
        ("ownerEmail", "42"),
        ("ownerEmail", "null"),
        ("ownerEmail", "\"not-an-email\""),
        ("website", "42"),
        ("website", "\"not a uri\""),
        ("tagCode", "42"),
        ("tagCode", "null"),
        ("tagCode", "\"!\""),
        ("tags", "\"not-a-list\""),
        ("tags", "null"),
        ("tags", "[\"item1\", \"item1\"]"),
        ("tags", "[\"item1\", \"item2\", \"item3\", \"item4\"]"),
        ("scores", "\"not-a-list\""),
        ("scores", "[null]"),
        ("scores", "[]"),
        ("adoptedAt", "\"not-a-date-time\""),
        ("adoptedAt", "\"2026-01-31T09:30:00\""),
        ("adoptedAt", "\"2026-01-31T24:00:00Z\""),
        ("adoptedAt", "\"2026-01-31T23:59:60Z\""),
        ("adoptedAt", "\"2026-01-31T09:30:00+14:60\""),
        ("adoptedAt", "\"0000-12-31T23:00:00-01:00\""),
        ("adoptedAt", "\"0001-01-01T00:00:00+01:00\""),
        ("adoptedAt", "\"9999-12-31T23:59:59-01:00\""),
        ("lastVisit", "\"not-a-date-time\""),
        ("lastVisit", "null"),
        ("lastVisit", "\"2026-01-31T09:30:00\""),
        ("lastVisit", "\"2026-01-31T24:00:00Z\""),
        ("lastVisit", "\"2026-01-31T23:59:60Z\""),
        ("lastVisit", "\"2026-01-31T09:30:00+14:60\""),
        ("lastVisit", "\"0000-12-31T23:00:00-01:00\""),
        ("lastVisit", "\"0001-01-01T00:00:00+01:00\""),
        ("lastVisit", "\"9999-12-31T23:59:59-01:00\""),
        ("notes", "42"),
        ("notes", "null"),
    ];

    private static readonly (string Name, string Value)[] Boundaries =
    [
        ("tenantId", "\"s\""),
        ("tenantId", "\"\\ud83d\\ude3a\""),
        ("tenantId", "\"ssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssss\""),
        ("tenantId", "\"\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\""),
        ("priority", "0"),
        ("priority", "1.0"),
        ("name", "\"s\""),
        ("name", "\"\\ud83d\\ude3a\""),
        ("name", "\"ssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssss\""),
        ("name", "\"\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\\ud83d\\ude3a\""),
        ("ageYears", "0"),
        ("ageYears", "40"),
        ("ageYears", "0.0"),
        ("tags", "[\"item1\", \"item2\", \"item3\"]"),
        ("scores", "[1]"),
        ("adoptedAt", "\"0001-01-01T00:00:00.000Z\""),
        ("adoptedAt", "\"9999-12-31T23:59:59.999Z\""),
        ("lastVisit", "\"0001-01-01T00:00:00.000Z\""),
        ("lastVisit", "\"9999-12-31T23:59:59.999Z\""),
    ];

    private readonly ICatService _mockCatService;
    private readonly CatController _catController;

    public CatControllerTests()
    {
        _mockCatService = Substitute.For<ICatService>();
        _catController = new CatController(_mockCatService);
    }

    [Fact]
    public async Task GetAsync_ReturnsCatDto()
    {
        var expectedItem = CatSamples.Dto();
        _mockCatService.GetAsync(ItemId, Arg.Any<CancellationToken>()).Returns(expectedItem);

        var result = await _catController.GetAsync(ItemId, TestContext.Current.CancellationToken);

        Assert.Equal(expectedItem, result);
    }

    [Fact]
    public async Task GetAsync_ThrowsNotFound_WhenIdIsNotAUuid()
    {
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _catController.GetAsync("not-a-uuid", TestContext.Current.CancellationToken));

        Assert.Equal("Cat with id not-a-uuid was not found.", exception.Message);
        await _mockCatService.DidNotReceiveWithAnyArgs().GetAsync(default!, TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData(null, 100)]
    [InlineData("5", 5)]
    [InlineData("invalid", 100)]
    [InlineData("5000", 1000)]
    public async Task GetListAsync_PassesParsedLimitToService(string? limit, int expectedLimit)
    {
        var expectedItemList = new List<CatDto> { CatSamples.Dto() };
        _mockCatService.GetListAsync(expectedLimit, Arg.Any<CancellationToken>()).Returns(expectedItemList);

        var result = await _catController.GetListAsync(limit, TestContext.Current.CancellationToken);

        Assert.Equal(expectedItemList, result);
        await _mockCatService.Received(1).GetListAsync(expectedLimit, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_PassesTheValidatedRequest()
    {
        CreateCatRequest? received = null;
        _mockCatService.CreateAsync(Arg.Do<CreateCatRequest>(request => received = request), "User1", Arg.Any<CancellationToken>()).Returns(CatSamples.Dto());

        var result = await _catController.CreateAsync(Mocks.CreateStream(CatSamples.CreateBody), "User1", TestContext.Current.CancellationToken);

        Assert.Equal(ItemId, result.Id);
        Assert.NotNull(received);
        JsonAssert.Equal(CatSamples.CreateBody, received);
    }

    [Fact]
    public async Task CreateAsync_GivesFieldsLeftOutTheirDefaults()
    {
        CreateCatRequest? received = null;
        _mockCatService.CreateAsync(Arg.Do<CreateCatRequest>(request => received = request), null, Arg.Any<CancellationToken>()).Returns(CatSamples.Dto());

        await _catController.CreateAsync(Mocks.CreateStream("{\"name\": \"sample\", \"rank\": 1.5}"), null, TestContext.Current.CancellationToken);

        Assert.NotNull(received);
        JsonAssert.Equal("\"public\"", received.TenantId);
        JsonAssert.Equal("\"eu\"", received.Region);
        JsonAssert.Equal("null", received.Priority);
        JsonAssert.Equal("[]", received.Labels);
        JsonAssert.Equal("\"tabby\"", received.Breed);
        JsonAssert.Equal("0", received.AgeYears);
        JsonAssert.Equal("null", received.WeightKg);
        JsonAssert.Equal("true", received.Indoor);
        Assert.NotNull(received.BirthDate);
        Assert.NotNull(received.MicrochipId);
        JsonAssert.Equal("\"unknown@example.com\"", received.OwnerEmail);
        JsonAssert.Equal("null", received.Website);
        JsonAssert.Equal("null", received.TagCode);
        JsonAssert.Equal("[]", received.Tags);
        JsonAssert.Equal("null", received.Scores);
        Assert.NotNull(received.AdoptedAt);
        JsonAssert.Equal("\"2026-01-01T00:00:00.000Z\"", received.LastVisit);
        JsonAssert.Equal("\"$none\"", received.Notes);
    }

    [Fact]
    public async Task CreateAsync_ThrowsBadRequest_WhenARequiredFieldIsMissing()
    {
        var exception = await Assert.ThrowsAsync<BadRequestException>(() => _catController.CreateAsync(Mocks.CreateStream("{}"), null, TestContext.Current.CancellationToken));

        Assert.Equal("rank is required. name is required.", exception.Message);
    }

    public static TheoryData<string> InvalidCreateBodies =>
        RequestBodies.Invalid(CatSamples.CreateBody, ["rank", "name"], [], Rejected);

    [Theory]
    [MemberData(nameof(InvalidCreateBodies))]
    public async Task CreateAsync_ThrowsBadRequest_WhenBodyIsInvalid(string body)
    {
        await Assert.ThrowsAsync<BadRequestException>(() => _catController.CreateAsync(Mocks.CreateStream(body), null, TestContext.Current.CancellationToken));

        await _mockCatService.DidNotReceiveWithAnyArgs().CreateAsync(default!, default, TestContext.Current.CancellationToken);
    }

    public static TheoryData<string> CreateEdgeBodies => RequestBodies.WithEdgeValues(CatSamples.CreateBody, Boundaries);

    [Theory]
    [MemberData(nameof(CreateEdgeBodies))]
    public async Task CreateAsync_AcceptsEdgeValues(string body)
    {
        CreateCatRequest? received = null;
        _mockCatService.CreateAsync(Arg.Do<CreateCatRequest>(request => received = request), null, Arg.Any<CancellationToken>()).Returns(CatSamples.Dto());

        await _catController.CreateAsync(Mocks.CreateStream(body), null, TestContext.Current.CancellationToken);

        JsonAssert.Equal(body, received);
    }

    [Fact]
    public async Task ReplaceAsync_PassesTheValidatedRequest()
    {
        ReplaceCatRequest? received = null;
        _mockCatService.ReplaceAsync(Arg.Do<ReplaceCatRequest>(request => received = request), "User1", Arg.Any<CancellationToken>()).Returns(CatSamples.Dto());

        var result = await _catController.ReplaceAsync(ItemId, Mocks.CreateStream(CatSamples.ReplaceBody), "User1", TestContext.Current.CancellationToken);

        Assert.Equal(ItemId, result.Id);
        Assert.NotNull(received);
        Assert.Equal(ItemId, received.Id);
        JsonAssert.Equal(CatSamples.ReplaceBody, received);
    }

    [Fact]
    public async Task ReplaceAsync_GivesFieldsLeftOutTheirDefaults()
    {
        ReplaceCatRequest? received = null;
        _mockCatService.ReplaceAsync(Arg.Do<ReplaceCatRequest>(request => received = request), null, Arg.Any<CancellationToken>()).Returns(CatSamples.Dto());

        await _catController.ReplaceAsync(ItemId, Mocks.CreateStream("{\"name\": \"sample\", \"rank\": 1.5}"), null, TestContext.Current.CancellationToken);

        Assert.NotNull(received);
        JsonAssert.Equal("\"public\"", received.TenantId);
        JsonAssert.Equal("null", received.Priority);
        JsonAssert.Equal("[]", received.Labels);
        JsonAssert.Equal("\"tabby\"", received.Breed);
        JsonAssert.Equal("0", received.AgeYears);
        JsonAssert.Equal("null", received.WeightKg);
        JsonAssert.Equal("true", received.Indoor);
        Assert.NotNull(received.BirthDate);
        JsonAssert.Equal("\"unknown@example.com\"", received.OwnerEmail);
        JsonAssert.Equal("null", received.Website);
        JsonAssert.Equal("null", received.TagCode);
        JsonAssert.Equal("[]", received.Tags);
        JsonAssert.Equal("null", received.Scores);
        Assert.NotNull(received.AdoptedAt);
        JsonAssert.Equal("\"2026-01-01T00:00:00.000Z\"", received.LastVisit);
        JsonAssert.Equal("\"$none\"", received.Notes);
    }

    [Fact]
    public async Task ReplaceAsync_ThrowsBadRequest_WhenARequiredFieldIsMissing()
    {
        var exception = await Assert.ThrowsAsync<BadRequestException>(() => _catController.ReplaceAsync(ItemId, Mocks.CreateStream("{}"), null, TestContext.Current.CancellationToken));

        Assert.Equal("rank is required. name is required.", exception.Message);
    }

    public static TheoryData<string> InvalidReplaceBodies =>
        RequestBodies.Invalid(CatSamples.ReplaceBody, ["rank", "name"], ["region", "microchipId"], Rejected);

    [Theory]
    [MemberData(nameof(InvalidReplaceBodies))]
    public async Task ReplaceAsync_ThrowsBadRequest_WhenBodyIsInvalid(string body)
    {
        await Assert.ThrowsAsync<BadRequestException>(() => _catController.ReplaceAsync(ItemId, Mocks.CreateStream(body), null, TestContext.Current.CancellationToken));

        await _mockCatService.DidNotReceiveWithAnyArgs().ReplaceAsync(default!, default, TestContext.Current.CancellationToken);
    }

    public static TheoryData<string> ReplaceEdgeBodies => RequestBodies.WithEdgeValues(CatSamples.ReplaceBody, Boundaries);

    [Theory]
    [MemberData(nameof(ReplaceEdgeBodies))]
    public async Task ReplaceAsync_AcceptsEdgeValues(string body)
    {
        ReplaceCatRequest? received = null;
        _mockCatService.ReplaceAsync(Arg.Do<ReplaceCatRequest>(request => received = request), null, Arg.Any<CancellationToken>()).Returns(CatSamples.Dto());

        await _catController.ReplaceAsync(ItemId, Mocks.CreateStream(body), null, TestContext.Current.CancellationToken);

        JsonAssert.Equal(body, received);
    }

    [Fact]
    public async Task ReplaceAsync_ThrowsNotFound_WhenIdIsNotAUuid()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _catController.ReplaceAsync("not-a-uuid", Mocks.CreateStream(CatSamples.ReplaceBody), null, TestContext.Current.CancellationToken));

        await _mockCatService.DidNotReceiveWithAnyArgs().ReplaceAsync(default!, default, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task UpdateAsync_PassesTheValidatedRequest()
    {
        UpdateCatRequest? received = null;
        _mockCatService.UpdateAsync(Arg.Do<UpdateCatRequest>(request => received = request), "User1", Arg.Any<CancellationToken>()).Returns(CatSamples.Dto());

        var result = await _catController.UpdateAsync(ItemId, Mocks.CreateStream(CatSamples.UpdateBody), "User1", TestContext.Current.CancellationToken);

        Assert.Equal(ItemId, result.Id);
        Assert.NotNull(received);
        Assert.Equal(ItemId, received.Id);
        Assert.Equal(new string[] { "adoptedAt", "ageYears", "indoor", "labels", "name", "notes", "ownerEmail", "priority", "rank", "tags", "tenantId", "website", "weightKg" }, received.Sent.Order(StringComparer.Ordinal));
        JsonAssert.Equal(CatSamples.UpdateBody, received);
    }

    [Fact]
    public async Task UpdateAsync_LeavesFieldsTheBodyOmits()
    {
        UpdateCatRequest? received = null;
        _mockCatService.UpdateAsync(Arg.Do<UpdateCatRequest>(request => received = request), null, Arg.Any<CancellationToken>()).Returns(CatSamples.Dto());

        await _catController.UpdateAsync(ItemId, Mocks.CreateStream("{}"), null, TestContext.Current.CancellationToken);

        Assert.NotNull(received);
        Assert.Empty(received.Sent);
    }

    [Theory]
    [InlineData("priority")]
    [InlineData("weightKg")]
    [InlineData("website")]
    [InlineData("adoptedAt")]
    public async Task UpdateAsync_AcceptsNullForANullableField(string name)
    {
        UpdateCatRequest? received = null;
        _mockCatService.UpdateAsync(Arg.Do<UpdateCatRequest>(request => received = request), null, Arg.Any<CancellationToken>()).Returns(CatSamples.Dto());

        await _catController.UpdateAsync(ItemId, Mocks.CreateStream("{\"" + name + "\":null}"), null, TestContext.Current.CancellationToken);

        Assert.NotNull(received);
        Assert.Equal(new[] { name }, received.Sent);
    }

    public static TheoryData<string> InvalidUpdateBodies =>
        RequestBodies.Invalid(CatSamples.UpdateBody, [], ["region", "breed", "birthDate", "microchipId", "tagCode", "scores", "lastVisit"], Rejected);

    [Theory]
    [MemberData(nameof(InvalidUpdateBodies))]
    public async Task UpdateAsync_ThrowsBadRequest_WhenBodyIsInvalid(string body)
    {
        await Assert.ThrowsAsync<BadRequestException>(() => _catController.UpdateAsync(ItemId, Mocks.CreateStream(body), null, TestContext.Current.CancellationToken));

        await _mockCatService.DidNotReceiveWithAnyArgs().UpdateAsync(default!, default, TestContext.Current.CancellationToken);
    }

    public static TheoryData<string> UpdateEdgeBodies => RequestBodies.WithEdgeValues(CatSamples.UpdateBody, Boundaries);

    [Theory]
    [MemberData(nameof(UpdateEdgeBodies))]
    public async Task UpdateAsync_AcceptsEdgeValues(string body)
    {
        UpdateCatRequest? received = null;
        _mockCatService.UpdateAsync(Arg.Do<UpdateCatRequest>(request => received = request), null, Arg.Any<CancellationToken>()).Returns(CatSamples.Dto());

        await _catController.UpdateAsync(ItemId, Mocks.CreateStream(body), null, TestContext.Current.CancellationToken);

        JsonAssert.Equal(body, received);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsNotFound_WhenIdIsNotAUuid()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _catController.UpdateAsync("not-a-uuid", Mocks.CreateStream(CatSamples.UpdateBody), null, TestContext.Current.CancellationToken));

        await _mockCatService.DidNotReceiveWithAnyArgs().UpdateAsync(default!, default, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task DeleteAsync_CallsDeleteOnServiceAndReturnsMessage()
    {
        _mockCatService.DeleteAsync(ItemId, "User1", Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var result = await _catController.DeleteAsync(ItemId, "User1", TestContext.Current.CancellationToken);

        Assert.Equal($"Cat with id {ItemId} was deleted successfully.", result.Message);
        await _mockCatService.Received(1).DeleteAsync(ItemId, "User1", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_ThrowsNotFound_WhenIdIsNotAUuid()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _catController.DeleteAsync("not-a-uuid", null, TestContext.Current.CancellationToken));

        await _mockCatService.DidNotReceiveWithAnyArgs().DeleteAsync(default!, default, TestContext.Current.CancellationToken);
    }
}
