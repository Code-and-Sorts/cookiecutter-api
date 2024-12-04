namespace {{cookiecutter.{{cookiecutter.project_class_name}}}}.Api.Tests.Unit;

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using {{cookiecutter.{{cookiecutter.project_class_name}}}}.Api.Controllers;
using {{cookiecutter.{{cookiecutter.project_class_name}}}}.Api.Dtos;
using {{cookiecutter.{{cookiecutter.project_class_name}}}}.Api.Interfaces;
using {{cookiecutter.{{cookiecutter.project_class_name}}}}.Api.Requests;
using Newtonsoft.Json;
using NSubstitute;
using Xunit;

public class {{cookiecutter.{{cookiecutter.project_class_name}}}}ControllerTest
{
    private readonly I{{cookiecutter.{{cookiecutter.project_class_name}}}}Service _mock{{cookiecutter.{{cookiecutter.project_class_name}}}}Service;
    private readonly {{cookiecutter.{{cookiecutter.project_class_name}}}}Controller _{{cookiecutter.project_lower_camel_name}}Controller;

    public {{cookiecutter.{{cookiecutter.project_class_name}}}}ControllerTest()
    {
        _mock{{cookiecutter.{{cookiecutter.project_class_name}}}}Service = Substitute.For<I{{cookiecutter.{{cookiecutter.project_class_name}}}}Service>();
        _{{cookiecutter.project_lower_camel_name}}Controller = new {{cookiecutter.{{cookiecutter.project_class_name}}}}Controller(_mock{{cookiecutter.{{cookiecutter.project_class_name}}}}Service);
    }

    private static MemoryStream CreateMemoryStream<T>(T {{cookiecutter.project_lower_camel_name}}Request)
    {
        var stream = new MemoryStream();
        using var writer = new StreamWriter(stream, leaveOpen: true);
        writer.Write(JsonConvert.SerializeObject({{cookiecutter.project_lower_camel_name}}Request));
        writer.Flush();
        stream.Position = 0;
        return stream;
    }

    [Fact]
    public async Task GetAsync_Returns{{cookiecutter.{{cookiecutter.project_class_name}}}}Dto()
    {
        // Arrange
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var expected{{cookiecutter.{{cookiecutter.project_class_name}}}} = new {{cookiecutter.{{cookiecutter.project_class_name}}}}Dto { Id = id };
        _mock{{cookiecutter.{{cookiecutter.project_class_name}}}}Service.GetAsync(id, Arg.Any<CancellationToken>()).Returns(expected{{cookiecutter.{{cookiecutter.project_class_name}}}});

        // Act
        var result = await _{{cookiecutter.project_lower_camel_name}}Controller.GetAsync(id);

        // Assert
        await _mock{{cookiecutter.{{cookiecutter.project_class_name}}}}Service.Received(1).GetAsync(id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetListAsync_ReturnsListOf{{cookiecutter.{{cookiecutter.project_class_name}}}}Dto()
    {
        // Arrange
        var expected{{cookiecutter.{{cookiecutter.project_class_name}}}}List = new List<{{cookiecutter.{{cookiecutter.project_class_name}}}}Dto> { new {{cookiecutter.{{cookiecutter.project_class_name}}}}Dto { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c" }, new {{cookiecutter.{{cookiecutter.project_class_name}}}}Dto { Id = "5615ff05-3032-4459-88ad-b6a4c3e51ca0" } };
        _mock{{cookiecutter.{{cookiecutter.project_class_name}}}}Service.GetListAsync(Arg.Any<CancellationToken>()).Returns(expected{{cookiecutter.{{cookiecutter.project_class_name}}}}List);

        // Act
        await _{{cookiecutter.project_lower_camel_name}}Controller.GetListAsync();

        // Assert
        await _mock{{cookiecutter.{{cookiecutter.project_class_name}}}}Service.Received(1).GetListAsync(Arg.Any<CancellationToken>());
    }

    // [Fact]
    // public async Task CreateAsync_ReturnsCreated{{cookiecutter.{{cookiecutter.project_class_name}}}}Dto()
    // {
    //     // Arrange
    //     var createRequest = new Create{{cookiecutter.{{cookiecutter.project_class_name}}}}Request { Name = "mockCreate{{cookiecutter.{{cookiecutter.project_class_name}}}}", CreatedBy = null, UpdatedBy = null };
    //     var expected{{cookiecutter.{{cookiecutter.project_class_name}}}} = new {{cookiecutter.{{cookiecutter.project_class_name}}}}Dto { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockCreate{{cookiecutter.{{cookiecutter.project_class_name}}}}" };
    //     _mock{{cookiecutter.{{cookiecutter.project_class_name}}}}Service.CreateAsync(createRequest, Arg.Any<CancellationToken>()).Returns(expected{{cookiecutter.{{cookiecutter.project_class_name}}}});

    //     // Act
    //     var stream = CreateMemoryStream(createRequest);
    //     await _{{cookiecutter.project_lower_camel_name}}Controller.CreateAsync(stream);

    //     // Assert
    //     await _mock{{cookiecutter.{{cookiecutter.project_class_name}}}}Service.Received(1).CreateAsync(createRequest, Arg.Any<CancellationToken>());
    // }

    [Fact]
    public async Task CreateAsync_ReturnsCreated{{cookiecutter.{{cookiecutter.project_class_name}}}}Dto()
    {
        // Arrange
        var createRequest = new Create{{cookiecutter.{{cookiecutter.project_class_name}}}}Request { Name = "mockCreate{{cookiecutter.{{cookiecutter.project_class_name}}}}", CreatedBy = "TestUser", UpdatedBy = "TestUser" };
        var expected{{cookiecutter.{{cookiecutter.project_class_name}}}} = new {{cookiecutter.{{cookiecutter.project_class_name}}}}Dto { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockCreate{{cookiecutter.{{cookiecutter.project_class_name}}}}" };

        _mock{{cookiecutter.{{cookiecutter.project_class_name}}}}Service
            .CreateAsync(Arg.Is<Create{{cookiecutter.{{cookiecutter.project_class_name}}}}Request>(r => r.Name == createRequest.Name), Arg.Any<CancellationToken>())
            .Returns(expected{{cookiecutter.{{cookiecutter.project_class_name}}}});

        var stream = CreateMemoryStream(createRequest);

        // Act
        var result = await _{{cookiecutter.project_lower_camel_name}}Controller.CreateAsync(stream);

        // Assert
        await _mock{{cookiecutter.{{cookiecutter.project_class_name}}}}Service.Received(1).CreateAsync(
            Arg.Is<Create{{cookiecutter.{{cookiecutter.project_class_name}}}}Request>(r => r.Name == createRequest.Name), Arg.Any<CancellationToken>());
        Assert.Equal(expected{{cookiecutter.{{cookiecutter.project_class_name}}}}, result);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsUpdated{{cookiecutter.{{cookiecutter.project_class_name}}}}Dto()
    {
        // Arrange
        var {{cookiecutter.project_lower_camel_name}}Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var updateRequest = new Update{{cookiecutter.{{cookiecutter.project_class_name}}}}Request { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockUpdated{{cookiecutter.{{cookiecutter.project_class_name}}}}" };
        var expected{{cookiecutter.{{cookiecutter.project_class_name}}}} = new {{cookiecutter.{{cookiecutter.project_class_name}}}}Dto { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockUpdated{{cookiecutter.{{cookiecutter.project_class_name}}}}" };

        _mock{{cookiecutter.{{cookiecutter.project_class_name}}}}Service
            .UpdateAsync(Arg.Is<Update{{cookiecutter.{{cookiecutter.project_class_name}}}}Request>(r => r.Name == updateRequest.Name), Arg.Any<CancellationToken>())
            .Returns(expected{{cookiecutter.{{cookiecutter.project_class_name}}}});

        // Act
        var stream = CreateMemoryStream(updateRequest);
        var result = await _{{cookiecutter.project_lower_camel_name}}Controller.UpdateAsync({{cookiecutter.project_lower_camel_name}}Id, stream);

        // Assert
        await _mock{{cookiecutter.{{cookiecutter.project_class_name}}}}Service.Received(1).UpdateAsync(
            Arg.Is<Update{{cookiecutter.{{cookiecutter.project_class_name}}}}Request>(r => r.Name == updateRequest.Name), Arg.Any<CancellationToken>());
        Assert.Equal(expected{{cookiecutter.{{cookiecutter.project_class_name}}}}, result);
    }

    [Fact]
    public async Task DeleteAsync_CallsDeleteOnService()
    {
        // Arrange
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        _mock{{cookiecutter.{{cookiecutter.project_class_name}}}}Service.DeleteAsync(id, Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        // Act
        await _{{cookiecutter.project_lower_camel_name}}Controller.DeleteAsync(id);

        // Assert
        await _mock{{cookiecutter.{{cookiecutter.project_class_name}}}}Service.Received(1).DeleteAsync(id, Arg.Any<CancellationToken>());
    }
}
