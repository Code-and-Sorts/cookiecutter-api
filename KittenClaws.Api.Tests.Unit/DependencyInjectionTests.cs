namespace KittenClaws.Api.Tests.Unit;

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Xunit;

public class DependencyInjectionTests
{
    private static IConfiguration Configuration(Dictionary<string, string?> values) =>
        new ConfigurationBuilder().AddInMemoryCollection(values).Build();

    [Theory]
    [InlineData(null, false)]
    [InlineData("http://localhost:8000", false)]
    [InlineData("", true)]
    public void CreateDynamoDbConfig_IgnoresOnlyAnEmptyEndpointOverride(string? endpoint, bool ignored)
    {
        var config = DependencyInjection.CreateDynamoDbConfig(Configuration(new() { ["AWS_ENDPOINT_URL_DYNAMODB"] = endpoint }));

        Assert.Equal(ignored, config.IgnoreConfiguredEndpointUrls);
        Assert.Equal(2, config.MaxErrorRetry);
    }
}
