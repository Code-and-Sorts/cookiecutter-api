namespace {{project_class_name}}.Api.Tests.Unit;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using {{project_class_name}}.Api.Dtos;
using {{project_class_name}}.Api.Entities;
using {{project_class_name}}.Api.Interfaces;
using {{project_class_name}}.Api.Requests;
using {{project_class_name}}.Api.Services;
using NSubstitute;
using Xunit;

public class {{project_class_name}}ServiceTest
{
    private readonly I{{project_class_name}}Repository _{{project_lower_camel_name}}RepositoryMock;
    private readonly {{project_class_name}}Service _{{project_lower_camel_name}}Service;

    public {{project_class_name}}ServiceTest()
    {
        _{{project_lower_camel_name}}RepositoryMock = Substitute.For<I{{project_class_name}}Repository>();
        _{{project_lower_camel_name}}Service = new {{project_class_name}}Service(_{{project_lower_camel_name}}RepositoryMock);
    }

    [Fact]
    public async Task GetAsync_ShouldReturn{{project_class_name}}Dto()
    {
        // Arrange
        var {{project_lower_camel_name}}Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var expected{{project_class_name}} = new {{project_class_name}}Dto { Id = {{project_lower_camel_name}}Id, Name = "mock{{project_class_name}}" };
        _{{project_lower_camel_name}}RepositoryMock.GetAsync({{project_lower_camel_name}}Id, Arg.Any<CancellationToken>())
            .Returns(expected{{project_class_name}});

        // Act
        var result = await _{{project_lower_camel_name}}Service.GetAsync({{project_lower_camel_name}}Id);

        // Assert
        Assert.Equal(expected{{project_class_name}}, result);
    }

    [Fact]
    public async Task GetListAsync_ShouldReturnListOf{{project_class_name}}Dto()
    {
        // Arrange
        var expected{{project_class_name}}List = new List<{{project_class_name}}Dto>
        {
            new {{project_class_name}}Dto { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mock{{project_class_name}}1" },
            new {{project_class_name}}Dto { Id = "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name = "mock{{project_class_name}}2" }
        };
        _{{project_lower_camel_name}}RepositoryMock.GetListAsync(Arg.Any<CancellationToken>())
            .Returns(expected{{project_class_name}}List);

        // Act
        var result = await _{{project_lower_camel_name}}Service.GetListAsync();

        // Assert
        Assert.Equal(expected{{project_class_name}}List, result);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnCreated{{project_class_name}}Dto()
    {
        // Arrange
        var createRequest = new Create{{project_class_name}}Request { Name = "mockCreate{{project_class_name}}" };
        var new{{project_class_name}} = new {{project_class_name}} { Id = Guid.NewGuid().ToString(), Name = createRequest.Name };
        var expected{{project_class_name}} = new {{project_class_name}}Dto { Id = new{{project_class_name}}.Id, Name = new{{project_class_name}}.Name };
        _{{project_lower_camel_name}}RepositoryMock.CreateAsync(Arg.Any<{{project_class_name}}>(), Arg.Any<CancellationToken>())
            .Returns(expected{{project_class_name}});

        // Act
        var result = await _{{project_lower_camel_name}}Service.CreateAsync(createRequest);

        // Assert
        Assert.Equal(expected{{project_class_name}}, result);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnUpdated{{project_class_name}}Dto()
    {
        // Arrange
        var updateRequest = new Update{{project_class_name}}Request { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockUpdate{{project_class_name}}" };
        var updated{{project_class_name}} = new {{project_class_name}} { Id = updateRequest.Id, Name = updateRequest.Name };
        var expected{{project_class_name}} = new {{project_class_name}}Dto { Id = updated{{project_class_name}}.Id, Name = updated{{project_class_name}}.Name };
        _{{project_lower_camel_name}}RepositoryMock.UpdateAsync(Arg.Any<{{project_class_name}}>(), Arg.Any<CancellationToken>())
            .Returns(expected{{project_class_name}});

        // Act
        var result = await _{{project_lower_camel_name}}Service.UpdateAsync(updateRequest);

        // Assert
        Assert.Equal(expected{{project_class_name}}, result);
    }

    [Fact]
    public async Task DeleteAsync_ShouldCallRepositoryDelete()
    {
        // Arrange
        var {{project_lower_camel_name}}Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        _{{project_lower_camel_name}}RepositoryMock.DeleteAsync({{project_lower_camel_name}}Id, Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        await _{{project_lower_camel_name}}Service.DeleteAsync({{project_lower_camel_name}}Id);

        // Assert
        await _{{project_lower_camel_name}}RepositoryMock.Received(1).DeleteAsync({{project_lower_camel_name}}Id, Arg.Any<CancellationToken>());
    }
}
