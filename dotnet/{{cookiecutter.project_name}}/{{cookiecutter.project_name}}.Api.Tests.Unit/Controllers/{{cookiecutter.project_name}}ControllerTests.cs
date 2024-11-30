namespace {{cookiecutter.project_name}}.Api.Tests.Unit;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using {{cookiecutter.project_name}}.Api.Controllers;
using {{cookiecutter.project_name}}.Api.Dtos;
using {{cookiecutter.project_name}}.Api.Interfaces;
using {{cookiecutter.project_name}}.Api.Requests;
using NSubstitute;
using Xunit;

public class {{cookiecutter.project_name}}ControllerTest
{
    private readonly I{{cookiecutter.project_name}}Service _mock{{cookiecutter.project_name}}Service;
    private readonly {{cookiecutter.project_name}}Controller _{{cookiecutter.project_lower_camel_name}}Controller;

    public {{cookiecutter.project_name}}ControllerTest()
    {
        _mock{{cookiecutter.project_name}}Service = Substitute.For<I{{cookiecutter.project_name}}Service>();
        _{{cookiecutter.project_lower_camel_name}}Controller = new {{cookiecutter.project_name}}Controller(_mock{{cookiecutter.project_name}}Service);
    }

    [Fact]
    public async Task GetAsync_Returns{{cookiecutter.project_name}}Dto()
    {
        // Arrange
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var expected{{cookiecutter.project_name}} = new {{cookiecutter.project_name}}Dto { Id = id };
        _mock{{cookiecutter.project_name}}Service.GetAsync(id, Arg.Any<CancellationToken>()).Returns(expected{{cookiecutter.project_name}});

        // Act
        var result = await _{{cookiecutter.project_lower_camel_name}}Controller.GetAsync(id);

        // Assert
        Assert.Equal(expected{{cookiecutter.project_name}}, result);
    }

    [Fact]
    public async Task GetListAsync_ReturnsListOf{{cookiecutter.project_name}}Dto()
    {
        // Arrange
        var expected{{cookiecutter.project_name}}List = new List<{{cookiecutter.project_name}}Dto> { new {{cookiecutter.project_name}}Dto { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c" }, new {{cookiecutter.project_name}}Dto { Id = "5615ff05-3032-4459-88ad-b6a4c3e51ca0" } };
        _mock{{cookiecutter.project_name}}Service.GetListAsync(Arg.Any<CancellationToken>()).Returns(expected{{cookiecutter.project_name}}List);

        // Act
        var result = await _{{cookiecutter.project_lower_camel_name}}Controller.GetListAsync();

        // Assert
        Assert.Equal(expected{{cookiecutter.project_name}}List, result);
    }

    [Fact]
    public async Task CreateAsync_ReturnsCreated{{cookiecutter.project_name}}Dto()
    {
        // Arrange
        var createRequest = new Create{{cookiecutter.project_name}}Request { Name = "mockCreate{{cookiecutter.project_name}}" };
        var expected{{cookiecutter.project_name}} = new {{cookiecutter.project_name}}Dto { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockCreate{{cookiecutter.project_name}}" };
        _mock{{cookiecutter.project_name}}Service.CreateAsync(createRequest, Arg.Any<CancellationToken>()).Returns(expected{{cookiecutter.project_name}});

        // Act
        var result = await _{{cookiecutter.project_lower_camel_name}}Controller.CreateAsync(createRequest);

        // Assert
        Assert.Equal(expected{{cookiecutter.project_name}}, result);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsUpdated{{cookiecutter.project_name}}Dto()
    {
        // Arrange
        var updateRequest = new Update{{cookiecutter.project_name}}Request { Id = "1", Name = "mockUpdated{{cookiecutter.project_name}}" };
        var expected{{cookiecutter.project_name}} = new {{cookiecutter.project_name}}Dto { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockUpdated{{cookiecutter.project_name}}" };
        _mock{{cookiecutter.project_name}}Service.UpdateAsync(updateRequest, Arg.Any<CancellationToken>()).Returns(expected{{cookiecutter.project_name}});

        // Act
        var result = await _{{cookiecutter.project_lower_camel_name}}Controller.UpdateAsync(updateRequest);

        // Assert
        Assert.Equal(expected{{cookiecutter.project_name}}, result);
    }

    [Fact]
    public async Task DeleteAsync_CallsDeleteOnService()
    {
        // Arrange
        var id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        _mock{{cookiecutter.project_name}}Service.DeleteAsync(id, Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        // Act
        await _{{cookiecutter.project_lower_camel_name}}Controller.DeleteAsync(id);

        // Assert
        await _mock{{cookiecutter.project_name}}Service.Received(1).DeleteAsync(id, Arg.Any<CancellationToken>());
    }
}
