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

public class {{project_class_name}}ControllerTest
{
    private readonly I{{project_class_name}}Service _mock{{project_class_name}}Service;
    private readonly {{project_class_name}}Controller _{{project_lower_camel_name}}Controller;

    public {{project_class_name}}ControllerTest()
    {
        _mock{{project_class_name}}Service = Substitute.For<I{{project_class_name}}Service>();
        _{{project_lower_camel_name}}Controller = new {{project_class_name}}Controller(_mock{{project_class_name}}Service);
    }

    private static MemoryStream CreateMemoryStream<T>(T {{project_lower_camel_name}}Request)
    {
        var stream = new MemoryStream();
        using var writer = new StreamWriter(stream, leaveOpen: true);
        writer.Write(JsonConvert.SerializeObject({{project_lower_camel_name}}Request));
        writer.Flush();
        stream.Position = 0;
        return stream;
    }

    [Fact]
    public async Task GetAsync_Returns{{project_class_name}}Dto()
    {
        // Arrange
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var expected{{project_class_name}} = new {{project_class_name}}Dto { Id = id };
        _mock{{project_class_name}}Service.GetAsync(id, Arg.Any<CancellationToken>()).Returns(expected{{project_class_name}});

        // Act
        var result = await _{{project_lower_camel_name}}Controller.GetAsync(id);

        // Assert
        await _mock{{project_class_name}}Service.Received(1).GetAsync(id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetListAsync_ReturnsListOf{{project_class_name}}Dto()
    {
        // Arrange
        var expected{{project_class_name}}List = new List<{{project_class_name}}Dto> { new {{project_class_name}}Dto { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c" }, new {{project_class_name}}Dto { Id = "5615ff05-3032-4459-88ad-b6a4c3e51ca0" } };
        _mock{{project_class_name}}Service.GetListAsync(Arg.Any<CancellationToken>()).Returns(expected{{project_class_name}}List);

        // Act
        await _{{project_lower_camel_name}}Controller.GetListAsync();

        // Assert
        await _mock{{project_class_name}}Service.Received(1).GetListAsync(Arg.Any<CancellationToken>());
    }

    // [Fact]
    // public async Task CreateAsync_ReturnsCreated{{project_class_name}}Dto()
    // {
    //     // Arrange
    //     var createRequest = new Create{{project_class_name}}Request { Name = "mockCreate{{project_class_name}}", CreatedBy = null, UpdatedBy = null };
    //     var expected{{project_class_name}} = new {{project_class_name}}Dto { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockCreate{{project_class_name}}" };
    //     _mock{{project_class_name}}Service.CreateAsync(createRequest, Arg.Any<CancellationToken>()).Returns(expected{{project_class_name}});

    //     // Act
    //     var stream = CreateMemoryStream(createRequest);
    //     await _{{project_lower_camel_name}}Controller.CreateAsync(stream);

    //     // Assert
    //     await _mock{{project_class_name}}Service.Received(1).CreateAsync(createRequest, Arg.Any<CancellationToken>());
    // }

    [Fact]
    public async Task CreateAsync_ReturnsCreated{{project_class_name}}Dto()
    {
        // Arrange
        var createRequest = new Create{{project_class_name}}Request { Name = "mockCreate{{project_class_name}}", CreatedBy = "TestUser", UpdatedBy = "TestUser" };
        var expected{{project_class_name}} = new {{project_class_name}}Dto { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockCreate{{project_class_name}}" };

        _mock{{project_class_name}}Service
            .CreateAsync(Arg.Is<Create{{project_class_name}}Request>(r => r.Name == createRequest.Name), Arg.Any<CancellationToken>())
            .Returns(expected{{project_class_name}});

        var stream = CreateMemoryStream(createRequest);

        // Act
        var result = await _{{project_lower_camel_name}}Controller.CreateAsync(stream);

        // Assert
        await _mock{{project_class_name}}Service.Received(1).CreateAsync(
            Arg.Is<Create{{project_class_name}}Request>(r => r.Name == createRequest.Name), Arg.Any<CancellationToken>());
        Assert.Equal(expected{{project_class_name}}, result);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsUpdated{{project_class_name}}Dto()
    {
        // Arrange
        var {{project_lower_camel_name}}Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var updateRequest = new Update{{project_class_name}}Request { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockUpdated{{project_class_name}}" };
        var expected{{project_class_name}} = new {{project_class_name}}Dto { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockUpdated{{project_class_name}}" };

        _mock{{project_class_name}}Service
            .UpdateAsync(Arg.Is<Update{{project_class_name}}Request>(r => r.Name == updateRequest.Name), Arg.Any<CancellationToken>())
            .Returns(expected{{project_class_name}});

        // Act
        var stream = CreateMemoryStream(updateRequest);
        var result = await _{{project_lower_camel_name}}Controller.UpdateAsync({{project_lower_camel_name}}Id, stream);

        // Assert
        await _mock{{project_class_name}}Service.Received(1).UpdateAsync(
            Arg.Is<Update{{project_class_name}}Request>(r => r.Name == updateRequest.Name), Arg.Any<CancellationToken>());
        Assert.Equal(expected{{project_class_name}}, result);
    }

    [Fact]
    public async Task DeleteAsync_CallsDeleteOnService()
    {
        // Arrange
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        _mock{{project_class_name}}Service.DeleteAsync(id, Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        // Act
        await _{{project_lower_camel_name}}Controller.DeleteAsync(id);

        // Assert
        await _mock{{project_class_name}}Service.Received(1).DeleteAsync(id, Arg.Any<CancellationToken>());
    }
}
