namespace {{project_class_name}}.Api.Tests.Unit;

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using {{project_class_name}}.Api.Controllers;
using {{project_class_name}}.Api.Dtos;
using {{project_class_name}}.Api.Interfaces;
using {{project_class_name}}.Api.Requests;
using Newtonsoft.Json;
using NSubstitute;
using Xunit;
{% for resource in resources %}
{%- set r = resource.name %}
{%- set uses_body = "create" in resource.operations or "update" in resource.operations or "replace" in resource.operations %}
public class {{ r }}ControllerTest
{
    private readonly I{{ r }}Service _mock{{ r }}Service;
    private readonly {{ r }}Controller _{{ r | to_lower_camel }}Controller;

    public {{ r }}ControllerTest()
    {
        _mock{{ r }}Service = Substitute.For<I{{ r }}Service>();
        _{{ r | to_lower_camel }}Controller = new {{ r }}Controller(_mock{{ r }}Service);
    }
{%- if uses_body %}

    private static MemoryStream CreateMemoryStream<T>(T itemRequest)
    {
        var stream = new MemoryStream();
        using var writer = new StreamWriter(stream, leaveOpen: true);
        writer.Write(JsonConvert.SerializeObject(itemRequest));
        writer.Flush();
        stream.Position = 0;
        return stream;
    }
{%- endif %}
{%- if "get_by_id" in resource.operations %}

    [Fact]
    public async Task GetAsync_Returns{{ r }}Dto()
    {
        // Arrange
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var expectedItem = new {{ r }}Dto { Id = id };
        _mock{{ r }}Service.GetAsync(id, Arg.Any<CancellationToken>()).Returns(expectedItem);

        // Act
        var result = await _{{ r | to_lower_camel }}Controller.GetAsync(id);

        // Assert
        await _mock{{ r }}Service.Received(1).GetAsync(id, Arg.Any<CancellationToken>());
    }
{%- endif %}
{%- if "list" in resource.operations %}

    [Fact]
    public async Task GetListAsync_ReturnsListOf{{ r }}Dto()
    {
        // Arrange
        var expectedItemList = new List<{{ r }}Dto> { new {{ r }}Dto { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c" }, new {{ r }}Dto { Id = "5615ff05-3032-4459-88ad-b6a4c3e51ca0" } };
        _mock{{ r }}Service.GetListAsync(Arg.Any<CancellationToken>()).Returns(expectedItemList);

        // Act
        await _{{ r | to_lower_camel }}Controller.GetListAsync();

        // Assert
        await _mock{{ r }}Service.Received(1).GetListAsync(Arg.Any<CancellationToken>());
    }
{%- endif %}
{%- if "create" in resource.operations %}

    [Fact]
    public async Task CreateAsync_ReturnsCreated{{ r }}Dto()
    {
        // Arrange
        var createRequest = new Create{{ r }}Request { Name = "mockCreate{{ r }}", CreatedBy = "TestUser", UpdatedBy = "TestUser" };
        var expectedItem = new {{ r }}Dto { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockCreate{{ r }}" };

        _mock{{ r }}Service
            .CreateAsync(Arg.Is<Create{{ r }}Request>(req => req.Name == createRequest.Name), Arg.Any<CancellationToken>())
            .Returns(expectedItem);

        var stream = CreateMemoryStream(createRequest);

        // Act
        var result = await _{{ r | to_lower_camel }}Controller.CreateAsync(stream);

        // Assert
        await _mock{{ r }}Service.Received(1).CreateAsync(
            Arg.Is<Create{{ r }}Request>(req => req.Name == createRequest.Name), Arg.Any<CancellationToken>());
        Assert.Equal(expectedItem, result);
    }
{%- endif %}
{%- if "update" in resource.operations %}

    [Fact]
    public async Task UpdateAsync_ReturnsUpdated{{ r }}Dto()
    {
        // Arrange
        var itemId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var updateRequest = new Update{{ r }}Request { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockUpdated{{ r }}" };
        var expectedItem = new {{ r }}Dto { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockUpdated{{ r }}" };

        _mock{{ r }}Service
            .UpdateAsync(Arg.Is<Update{{ r }}Request>(req => req.Name == updateRequest.Name), Arg.Any<CancellationToken>())
            .Returns(expectedItem);

        // Act
        var stream = CreateMemoryStream(updateRequest);
        var result = await _{{ r | to_lower_camel }}Controller.UpdateAsync(itemId, stream);

        // Assert
        await _mock{{ r }}Service.Received(1).UpdateAsync(
            Arg.Is<Update{{ r }}Request>(req => req.Name == updateRequest.Name), Arg.Any<CancellationToken>());
        Assert.Equal(expectedItem, result);
    }
{%- endif %}
{%- if "replace" in resource.operations %}

    [Fact]
    public async Task ReplaceAsync_ReturnsReplaced{{ r }}Dto()
    {
        // Arrange
        var itemId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var replaceRequest = new Replace{{ r }}Request { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockReplaced{{ r }}" };
        var expectedItem = new {{ r }}Dto { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockReplaced{{ r }}" };

        _mock{{ r }}Service
            .ReplaceAsync(Arg.Is<Replace{{ r }}Request>(req => req.Name == replaceRequest.Name), Arg.Any<CancellationToken>())
            .Returns(expectedItem);

        // Act
        var stream = CreateMemoryStream(replaceRequest);
        var result = await _{{ r | to_lower_camel }}Controller.ReplaceAsync(itemId, stream);

        // Assert
        await _mock{{ r }}Service.Received(1).ReplaceAsync(
            Arg.Is<Replace{{ r }}Request>(req => req.Name == replaceRequest.Name), Arg.Any<CancellationToken>());
        Assert.Equal(expectedItem, result);
    }
{%- endif %}
{%- if "delete" in resource.operations %}

    [Fact]
    public async Task DeleteAsync_CallsDeleteOnService()
    {
        // Arrange
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        _mock{{ r }}Service.DeleteAsync(id, Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        // Act
        await _{{ r | to_lower_camel }}Controller.DeleteAsync(id);

        // Assert
        await _mock{{ r }}Service.Received(1).DeleteAsync(id, Arg.Any<CancellationToken>());
    }
{%- endif %}
}
{% endfor %}
