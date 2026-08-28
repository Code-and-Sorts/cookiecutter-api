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
        return services;
    }
    public static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        services.AddSingleton<IAmazonDynamoDB, AmazonDynamoDBClient>();

        services.AddSingleton<IKittenClawsController>(provider =>
        {
            var dynamoClient = provider.GetRequiredService<IAmazonDynamoDB>();
            string tableName = Environment.GetEnvironmentVariable("DYNAMODB_TABLE_NAME_KITTIES") ?? "kitties";
            var repository = new KittenClawsRepository(dynamoClient, tableName);
            return new KittenClawsController(new KittenClawsService(repository));
        });

        return services;
    }
}
