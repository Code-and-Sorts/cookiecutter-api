namespace {{cookiecutter.project_name}}.Api;

using System;
using {{cookiecutter.project_name}}.Api.Controllers;
using {{cookiecutter.project_name}}.Api.Interfaces;
using {{cookiecutter.project_name}}.Api.Repositories;
using {{cookiecutter.project_name}}.Api.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfigurationRoot configuration)
    {
        services.AddScoped<I{{cookiecutter.project_name}}Service, {{cookiecutter.project_name}}Service>();
        services.AddScoped<I{{cookiecutter.project_name}}Controller, {{cookiecutter.project_name}}Controller>();

        return services;
    }

    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        string cosmosConnectionString = Environment.GetEnvironmentVariable("CosmosDbConnectionString");
        string databaseName = Environment.GetEnvironmentVariable("CosmosDbDatabaseName");
        string containerName = Environment.GetEnvironmentVariable("CosmosDbContainerName");

        services.AddSingleton<CosmosClient>(provider =>
            new CosmosClient(cosmosConnectionString));

        services.AddSingleton<I{{cookiecutter.project_name}}Repository>(provider =>
            new {{cookiecutter.project_name}}Repository(
                provider.GetRequiredService<CosmosClient>(),
                databaseName,
                containerName
            ));

        return services;
    }
}
