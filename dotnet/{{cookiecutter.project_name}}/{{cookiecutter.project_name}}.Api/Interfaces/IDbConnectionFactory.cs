namespace {{cookiecutter.project_name}}.Api.Interfaces;

using Microsoft.Azure.Cosmos;

public interface IDbConnectionFactory
{
    CosmosClient CreateClient();
}
