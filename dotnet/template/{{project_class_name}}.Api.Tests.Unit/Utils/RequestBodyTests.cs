namespace {{project_class_name}}.Api.Tests.Unit;

using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using {{project_class_name}}.Api.Utils;
using FluentValidation;
using Xunit;

public class RequestBodyTests
{
    public class SampleRequest : ISentFields
    {
        [JsonIgnore]
        public HashSet<string> Sent { get; } = [];

        [JsonIgnore]
        public string Id { get; set; } = default!;

        [JsonPropertyName("name")]
        public string? Name { get; set; } = "default";

        [JsonPropertyName("count")]
        public long? Count { get; set; }

        [JsonPropertyName("ratio")]
        public double? Ratio { get; set; }

        [JsonPropertyName("flag")]
        public bool? Flag { get; set; }

        [JsonPropertyName("tags")]
        public List<string>? Tags { get; set; }

        [JsonPropertyName("counts")]
        public List<long>? Counts { get; set; }
    }

    public class SampleValidator : AbstractValidator<SampleRequest>
    {
        public SampleValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("name is required.");
            RuleFor(x => x.Name).MaximumLength(3).WithMessage("name is too long.");
        }
    }

    [Fact]
    public async Task ReadValidAsync_ReturnsTheRequest_WhenValid()
    {
        var request = await RequestBody.ReadValidAsync<SampleRequest, SampleValidator>(Mocks.CreateStream("{\"name\":\"Tom\"}"), TestContext.Current.CancellationToken);

        Assert.Equal("Tom", request.Name);
    }

    [Theory]
    [InlineData("{\"name\":null}", "name is required.")]
    [InlineData("{\"name\":\"Thomas\"}", "name is too long.")]
    public async Task ReadValidAsync_ThrowsBadRequestWithTheFailureMessages(string body, string expectedMessage)
    {
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => RequestBody.ReadValidAsync<SampleRequest, SampleValidator>(Mocks.CreateStream(body), TestContext.Current.CancellationToken));

        Assert.Equal(400, exception.StatusCode);
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public async Task DeserializeAsync_ReadsEveryKnownFieldAndRecordsItAsSent()
    {
        var request = await RequestBody.DeserializeAsync<SampleRequest>(
            Mocks.CreateStream("{\"name\":\"Tom\",\"count\":-9007199254740991,\"ratio\":2,\"flag\":false,\"tags\":[],\"counts\":[1,2]}"), TestContext.Current.CancellationToken);

        Assert.Equal("Tom", request.Name);
        Assert.Equal(-9007199254740991, request.Count);
        Assert.Equal(2.0, request.Ratio);
        Assert.False(request.Flag);
        Assert.Empty(request.Tags!);
        Assert.Equal(new long[] { 1, 2 }, request.Counts);
        Assert.Equal(new[] { "count", "counts", "flag", "name", "ratio", "tags" }, request.Sent.Order());
    }

    [Theory]
    [InlineData("2.0", 2)]
    [InlineData("1e3", 1000)]
    public async Task DeserializeAsync_ReadsAWholeNumberAsAnInteger(string sent, long expected)
    {
        var request = await RequestBody.DeserializeAsync<SampleRequest>(
            Mocks.CreateStream($"{{ '{{' }}\"count\":{sent},\"counts\":[{sent}]{{ '}}' }}"), TestContext.Current.CancellationToken);

        Assert.Equal(expected, request.Count);
        Assert.Equal(new[] { expected }, request.Counts);
    }

    [Fact]
    public async Task DeserializeAsync_KeepsDefaultsForFieldsLeftOut()
    {
        var request = await RequestBody.DeserializeAsync<SampleRequest>(Mocks.CreateStream("{}"), TestContext.Current.CancellationToken);

        Assert.Equal("default", request.Name);
        Assert.Null(request.Count);
        Assert.Empty(request.Sent);
    }

    [Fact]
    public async Task DeserializeAsync_SetsAFieldSentAsNullToNull()
    {
        var request = await RequestBody.DeserializeAsync<SampleRequest>(Mocks.CreateStream("{\"name\":null}"), TestContext.Current.CancellationToken);

        Assert.Null(request.Name);
        Assert.Equal(new[] { "name" }, request.Sent);
    }

    [Theory]
    [InlineData("", "Request body must be valid JSON.")]
    [InlineData("{\"name\":", "Request body must be valid JSON.")]
    [InlineData("{name: 'Tom'}", "Request body must be valid JSON.")]
    [InlineData("{\"name\":\"Tom\"} trailing", "Request body must be valid JSON.")]
    [InlineData("[]", "Request body must be a JSON object.")]
    [InlineData("\"Tom\"", "Request body must be a JSON object.")]
    [InlineData("null", "Request body must be a JSON object.")]
    [InlineData("{\"name\":123}", "name must be a string.")]
    [InlineData("{\"name\":true}", "name must be a string.")]
    [InlineData("{\"count\":\"1\"}", "count must be an integer.")]
    [InlineData("{\"count\":1.5}", "count must be an integer.")]
    [InlineData("{\"count\":1e30}", "count must be an integer.")]
    [InlineData("{\"ratio\":\"1\"}", "ratio must be a number.")]
    [InlineData("{\"ratio\":1e400}", "ratio must be a number.")]
    [InlineData("{\"flag\":\"true\"}", "flag must be true or false.")]
    [InlineData("{\"flag\":0}", "flag must be true or false.")]
    [InlineData("{\"tags\":\"a\"}", "tags must be a list of strings.")]
    [InlineData("{\"tags\":[null]}", "tags must be a list of strings.")]
    [InlineData("{\"counts\":[1,\"2\"]}", "counts must be a list of integers.")]
    [InlineData("{\"name\":\"Tom\",\"id\":\"0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c\"}", "Unknown field: id.")]
    [InlineData("{\"name\":\"Tom\",\"isDeleted\":true}", "Unknown field: isDeleted.")]
    [InlineData("{\"name\":\"Tom\",\"createdBy\":\"me\"}", "Unknown field: createdBy.")]
    [InlineData("{\"Name\":\"Tom\"}", "Unknown field: Name.")]
    public async Task DeserializeAsync_RejectsInvalidBodyWithBadRequest(string body, string expectedMessage)
    {
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => RequestBody.DeserializeAsync<SampleRequest>(Mocks.CreateStream(body), TestContext.Current.CancellationToken));

        Assert.Equal(400, exception.StatusCode);
        Assert.Equal(expectedMessage, exception.Message);
    }
}
