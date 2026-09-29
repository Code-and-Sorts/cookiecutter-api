namespace KittenClaws.Api;

using System;
using KittenClaws.Api.Controllers;
using KittenClaws.Api.Interfaces;
using KittenClaws.Api.Repositories;
using KittenClaws.Api.Services;
using Amazon.DynamoDBv2;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IKittenClawsService, KittenClawsService>();
        services.AddScoped<IKittenClawsController, KittenClawsController>();

        return services;
    }
    public static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        string tableName = Environment.GetEnvironmentVariable("DYNAMODB_TABLE_NAME") ?? string.Empty;

        if (string.IsNullOrEmpty(tableName))
        {
            throw new InvalidOperationException("DynamoDB table name configuration is missing. Set the DYNAMODB_TABLE_NAME environment variable.");
        }

        services.AddSingleton<IAmazonDynamoDB, AmazonDynamoDBClient>();

        services.AddSingleton<IKittenClawsRepository>(provider =>
        {
            var dynamoClient = provider.GetRequiredService<IAmazonDynamoDB>();
            return new KittenClawsRepository(
                dynamoClient,
                tableName
            );
        });

        return services;
    }
}
