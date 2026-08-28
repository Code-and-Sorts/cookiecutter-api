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

        services.AddSingleton<ICatController>(provider =>
        {
            var dynamoClient = provider.GetRequiredService<IAmazonDynamoDB>();
            string tableName = Environment.GetEnvironmentVariable("DYNAMODB_TABLE_NAME_ANIMALS") ?? "animals";
            var repository = new CatRepository(dynamoClient, tableName);
            return new CatController(new CatService(repository));
        });

        services.AddSingleton<IDogController>(provider =>
        {
            var dynamoClient = provider.GetRequiredService<IAmazonDynamoDB>();
            string tableName = Environment.GetEnvironmentVariable("DYNAMODB_TABLE_NAME_ANIMALS") ?? "animals";
            var repository = new DogRepository(dynamoClient, tableName);
            return new DogController(new DogService(repository));
        });

        return services;
    }
}
