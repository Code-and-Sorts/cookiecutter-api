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
{% for resource in resources %}
{%- set r = resource.name %}
public class {{ r }}ServiceTest
{
    private readonly I{{ r }}Repository _{{ r | to_lower_camel }}RepositoryMock;
    private readonly {{ r }}Service _{{ r | to_lower_camel }}Service;

    public {{ r }}ServiceTest()
    {
        _{{ r | to_lower_camel }}RepositoryMock = Substitute.For<I{{ r }}Repository>();
        _{{ r | to_lower_camel }}Service = new {{ r }}Service(_{{ r | to_lower_camel }}RepositoryMock);
    }
{%- if "get_by_id" in resource.operations %}

    [Fact]
    public async Task GetAsync_ShouldReturn{{ r }}Dto()
    {
        var itemId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        var expectedItem = new {{ r }}Dto { Id = itemId, Name = "mock{{ r }}" };
        _{{ r | to_lower_camel }}RepositoryMock.GetAsync(itemId, Arg.Any<CancellationToken>())
            .Returns(expectedItem);

        var result = await _{{ r | to_lower_camel }}Service.GetAsync(itemId);

        Assert.Equal(expectedItem, result);
    }
{%- endif %}
{%- if "list" in resource.operations %}

    [Fact]
    public async Task GetListAsync_ShouldReturnListOf{{ r }}Dto()
    {
        var expectedItemList = new List<{{ r }}Dto>
        {
            new {{ r }}Dto { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mock{{ r }}1" },
            new {{ r }}Dto { Id = "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name = "mock{{ r }}2" }
        };
        _{{ r | to_lower_camel }}RepositoryMock.GetListAsync(Arg.Any<CancellationToken>())
            .Returns(expectedItemList);

        var result = await _{{ r | to_lower_camel }}Service.GetListAsync();

        Assert.Equal(expectedItemList, result);
    }
{%- endif %}
{%- if "create" in resource.operations %}

    [Fact]
    public async Task CreateAsync_ShouldReturnCreated{{ r }}Dto()
    {
        var createRequest = new Create{{ r }}Request { Name = "mockCreate{{ r }}" };
        var new{{ r }} = new {{ r }} { Id = Guid.NewGuid().ToString(), Name = createRequest.Name };
        var expectedItem = new {{ r }}Dto { Id = new{{ r }}.Id, Name = new{{ r }}.Name };
        _{{ r | to_lower_camel }}RepositoryMock.CreateAsync(Arg.Any<{{ r }}>(), Arg.Any<CancellationToken>())
            .Returns(expectedItem);

        var result = await _{{ r | to_lower_camel }}Service.CreateAsync(createRequest);

        Assert.Equal(expectedItem, result);
    }
{%- endif %}
{%- if "update" in resource.operations %}

    [Fact]
    public async Task UpdateAsync_ShouldReturnUpdated{{ r }}Dto()
    {
        var updateRequest = new Update{{ r }}Request { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockUpdate{{ r }}" };
        var updated{{ r }} = new {{ r }} { Id = updateRequest.Id, Name = updateRequest.Name };
        var expectedItem = new {{ r }}Dto { Id = updated{{ r }}.Id, Name = updated{{ r }}.Name };
        _{{ r | to_lower_camel }}RepositoryMock.UpdateAsync(Arg.Any<{{ r }}>(), Arg.Any<CancellationToken>())
            .Returns(expectedItem);

        var result = await _{{ r | to_lower_camel }}Service.UpdateAsync(updateRequest);

        Assert.Equal(expectedItem, result);
    }
{%- endif %}
{%- if "replace" in resource.operations %}

    [Fact]
    public async Task ReplaceAsync_ShouldReturnReplaced{{ r }}Dto()
    {
        var replaceRequest = new Replace{{ r }}Request { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockReplace{{ r }}" };
        var replaced{{ r }} = new {{ r }} { Id = replaceRequest.Id, Name = replaceRequest.Name };
        var expectedItem = new {{ r }}Dto { Id = replaced{{ r }}.Id, Name = replaced{{ r }}.Name };
        _{{ r | to_lower_camel }}RepositoryMock.ReplaceAsync(Arg.Any<{{ r }}>(), Arg.Any<CancellationToken>())
            .Returns(expectedItem);

        var result = await _{{ r | to_lower_camel }}Service.ReplaceAsync(replaceRequest);

        Assert.Equal(expectedItem, result);
    }
{%- endif %}
{%- if "delete" in resource.operations %}

    [Fact]
    public async Task DeleteAsync_ShouldCallRepositoryDelete()
    {
        var itemId = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c";
        _{{ r | to_lower_camel }}RepositoryMock.DeleteAsync(itemId, Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        await _{{ r | to_lower_camel }}Service.DeleteAsync(itemId);

        await _{{ r | to_lower_camel }}RepositoryMock.Received(1).DeleteAsync(itemId, Arg.Any<CancellationToken>());
    }
{%- endif %}
}
{% endfor %}
