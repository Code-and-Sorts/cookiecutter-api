namespace KittenClaws.Api.Tests.Unit;

using System.Text.Json.Serialization;
using System.Threading.Tasks;
using KittenClaws.Api.Utils;
using FluentValidation;
using Xunit;

public class RequestBodyTests
{
    public class SampleRequest
    {
        [JsonIgnore]
        public string Id { get; set; } = default!;

        [JsonPropertyName("name")]
        public string? Name { get; set; }
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
    [InlineData("{}", "name is required.")]
    [InlineData("{\"name\":\"Thomas\"}", "name is too long.")]
    public async Task ReadValidAsync_ThrowsBadRequestWithTheFailureMessages(string body, string expectedMessage)
    {
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => RequestBody.ReadValidAsync<SampleRequest, SampleValidator>(Mocks.CreateStream(body), TestContext.Current.CancellationToken));

        Assert.Equal(400, exception.StatusCode);
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public async Task DeserializeAsync_ReadsKnownStringField()
    {
        var request = await RequestBody.DeserializeAsync<SampleRequest>(Mocks.CreateStream("{\"name\":\"Tom\"}"), TestContext.Current.CancellationToken);

        Assert.Equal("Tom", request.Name);
    }

    [Fact]
    public async Task DeserializeAsync_AllowsMissingOptionalField()
    {
        var request = await RequestBody.DeserializeAsync<SampleRequest>(Mocks.CreateStream("{}"), TestContext.Current.CancellationToken);

        Assert.Null(request.Name);
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
    [InlineData("{\"name\":null}", "name must be a string.")]
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
