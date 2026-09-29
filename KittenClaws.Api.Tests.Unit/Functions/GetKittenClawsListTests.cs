namespace KittenClaws.Api.Tests.Unit;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using KittenClaws.Api.Functions;
using KittenClaws.Api.Interfaces;
using KittenClaws.Api.Dtos;
using NSubstitute.ExceptionExtensions;
using KittenClaws.Api.Utils;
using System.Collections.Generic;

public class GetKittenClawsListTest
{
    private readonly IKittenClawsController _mockKittenClawsController;
    private readonly ILogger<GetKittenClawsList> _mockLogger;
    private readonly GetKittenClawsList _getKittenClawsFunction;

    public GetKittenClawsListTest()
    {
        _mockKittenClawsController = Substitute.For<IKittenClawsController>();
        _mockLogger = Substitute.For<ILogger<GetKittenClawsList>>();
        _getKittenClawsFunction = new GetKittenClawsList(_mockKittenClawsController, _mockLogger);
    }

    [Fact]
    public async Task Get_ReturnsGetListResult_WhenKittenClawsListIsGot()
    {
        // Arrange
        var getListKittenClawsDto = new List<KittenClawsDto>
        {
            new KittenClawsDto { Id = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name = "mockKittenClaws1" },
            new KittenClawsDto { Id = "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name = "mockKittenClaws2" },
        };

        _mockKittenClawsController.GetListAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult((IEnumerable<KittenClawsDto>)getListKittenClawsDto));

        // Act
        var result = await _getKittenClawsFunction.Get();

        var getResult = Assert.IsType<OkObjectResult>(result);

        // Assert
        Assert.Equal(200, getResult.StatusCode);
        Assert.Equal(getListKittenClawsDto, getResult.Value);
    }

    [Fact]
    public async Task Get_ReturnsErrorResult_WhenExceptionIsThrown()
    {
        // Arrange
        _mockKittenClawsController.GetListAsync(Arg.Any<CancellationToken>()).Throws(new Exception("Mock exception"));

        // Act
        var result = await _getKittenClawsFunction.Get();

        var objectResult = Assert.IsType<HttpResponseInit>(result);
        var errorResult = Assert.IsType<BaseError>(objectResult.Value);

        // Assert
        Assert.Equal(500, objectResult.StatusCode);
        Assert.Equal("Mock exception", errorResult.ErrorMessage);
    }
}
