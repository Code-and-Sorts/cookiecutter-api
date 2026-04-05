{%- if cookiecutter.cloud_service == 'Azure Function App' %}
namespace {{cookiecutter.project_class_name}}.Api.Interfaces;

using Microsoft.Azure.Cosmos;

public interface IDbConnectionFactory
{
    CosmosClient CreateClient();
}
{%- endif %}
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
namespace {{cookiecutter.project_class_name}}.Api.Interfaces;

using Google.Cloud.Firestore;

public interface IDbConnectionFactory
{
    FirestoreDb CreateClient();
}
{%- endif %}
