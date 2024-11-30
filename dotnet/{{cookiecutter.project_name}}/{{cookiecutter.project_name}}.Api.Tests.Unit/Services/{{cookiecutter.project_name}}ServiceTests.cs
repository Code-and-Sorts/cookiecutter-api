namespace {{cookiecutter.project_name}}.Api.Tests.Unit;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using {{cookiecutter.project_name}}.Api.Dtos;
using {{cookiecutter.project_name}}.Api.Entities;
using {{cookiecutter.project_name}}.Api.Interfaces;
using {{cookiecutter.project_name}}.Api.Requests;
using {{cookiecutter.project_name}}.Api.Services;
using NSubstitute;
using Xunit;

public class {{cookiecutter.project_name}}ServiceTest
{
    private readonly I{{cookiecutter.project_name}}Repository _{{cookiecutter.project_lower_camel_name}}RepositoryMock;
    private readonly {{cookiecutter.project_name}}Service _{{cookiecutter.project_lower_camel_name}}Service;

    public {{cookiecutter.project_name}}ServiceTest()
    {
        _{{cookiecutter.project_lower_camel_name}}RepositoryMock = Substitute.For<I{{cookiecutter.project_name}}Repository>();
        _{{cookiecutter.project_lower_camel_name}}Service = new {{cookiecutter.project_name}}Service(_{{cookiecutter.project_lower_camel_name}}RepositoryMock);
    }

    [Fact]
    public async Task GetAsync_ShouldReturn{{cookiecutter.project_name}}Dto()
    {
        // Arrange
        var {{cookiecutter.project_lower_camel_name}}Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var expected{{cookiecutter.project_name}} = new {{cookiecutter.project_name}}Dto { Id = {{cookiecutter.project_lower_camel_name}}Id, Name = "mock{{cookiecutter.project_name}}" };
        _{{cookiecutter.project_lower_camel_name}}RepositoryMock.GetAsync({{cookiecutter.project_lower_camel_name}}Id, Arg.Any<CancellationToken>())
            .Returns(expected{{cookiecutter.project_name}});

        // Act
        var result = await _{{cookiecutter.project_lower_camel_name}}Service.GetAsync({{cookiecutter.project_lower_camel_name}}Id);

        // Assert
        Assert.Equal(expected{{cookiecutter.project_name}}, result);
    }

    [Fact]
    public async Task GetListAsync_ShouldReturnListOf{{cookiecutter.project_name}}Dto()
    {
        // Arrange
        var expected{{cookiecutter.project_name}}List = new List<{{cookiecutter.project_name}}Dto>
        {
            new {{cookiecutter.project_name}}Dto { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mock{{cookiecutter.project_name}}1" },
            new {{cookiecutter.project_name}}Dto { Id = "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name = "mock{{cookiecutter.project_name}}2" }
        };
        _{{cookiecutter.project_lower_camel_name}}RepositoryMock.GetListAsync(Arg.Any<CancellationToken>())
            .Returns(expected{{cookiecutter.project_name}}List);

        // Act
        var result = await _{{cookiecutter.project_lower_camel_name}}Service.GetListAsync();

        // Assert
        Assert.Equal(expected{{cookiecutter.project_name}}List, result);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnCreated{{cookiecutter.project_name}}Dto()
    {
        // Arrange
        var createRequest = new Create{{cookiecutter.project_name}}Request { Name = "mockCreate{{cookiecutter.project_name}}" };
        var new{{cookiecutter.project_name}} = new {{cookiecutter.project_name}} { Id = Guid.NewGuid().ToString(), Name = createRequest.Name };
        var expected{{cookiecutter.project_name}} = new {{cookiecutter.project_name}}Dto { Id = new{{cookiecutter.project_name}}.Id, Name = new{{cookiecutter.project_name}}.Name };
        _{{cookiecutter.project_lower_camel_name}}RepositoryMock.CreateAsync(Arg.Any<{{cookiecutter.project_name}}>(), Arg.Any<CancellationToken>())
            .Returns(expected{{cookiecutter.project_name}});

        // Act
        var result = await _{{cookiecutter.project_lower_camel_name}}Service.CreateAsync(createRequest);

        // Assert
        Assert.Equal(expected{{cookiecutter.project_name}}, result);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnUpdated{{cookiecutter.project_name}}Dto()
    {
        // Arrange
        var updateRequest = new Update{{cookiecutter.project_name}}Request { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockUpdate{{cookiecutter.project_name}}" };
        var updated{{cookiecutter.project_name}} = new {{cookiecutter.project_name}} { Id = updateRequest.Id, Name = updateRequest.Name };
        var expected{{cookiecutter.project_name}} = new {{cookiecutter.project_name}}Dto { Id = updated{{cookiecutter.project_name}}.Id, Name = updated{{cookiecutter.project_name}}.Name };
        _{{cookiecutter.project_lower_camel_name}}RepositoryMock.UpdateAsync(Arg.Any<{{cookiecutter.project_name}}>(), Arg.Any<CancellationToken>())
            .Returns(expected{{cookiecutter.project_name}});

        // Act
        var result = await _{{cookiecutter.project_lower_camel_name}}Service.UpdateAsync(updateRequest);

        // Assert
        Assert.Equal(expected{{cookiecutter.project_name}}, result);
    }

    [Fact]
    public async Task DeleteAsync_ShouldCallRepositoryDelete()
    {
        // Arrange
        var {{cookiecutter.project_lower_camel_name}}Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        _{{cookiecutter.project_lower_camel_name}}RepositoryMock.DeleteAsync({{cookiecutter.project_lower_camel_name}}Id, Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        await _{{cookiecutter.project_lower_camel_name}}Service.DeleteAsync({{cookiecutter.project_lower_camel_name}}Id);

        // Assert
        await _{{cookiecutter.project_lower_camel_name}}RepositoryMock.Received(1).DeleteAsync({{cookiecutter.project_lower_camel_name}}Id, Arg.Any<CancellationToken>());
    }
}
