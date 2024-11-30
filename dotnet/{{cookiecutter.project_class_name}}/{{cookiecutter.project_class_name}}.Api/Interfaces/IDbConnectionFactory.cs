namespace {{cookiecutter.project_class_name}}.Api.Interfaces;

using Microsoft.Azure.Cosmos;

public interface IDbConnectionFactory
{
    CosmosClient CreateClient();
}
