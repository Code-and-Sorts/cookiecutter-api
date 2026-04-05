namespace {{cookiecutter.project_class_name}}.Api.Entities;

using Newtonsoft.Json;
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
using Google.Cloud.Firestore;
{%- endif %}

{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
[FirestoreData]
{%- endif %}
public class {{cookiecutter.project_class_name}} : BaseEntity
{
    [JsonProperty("name")]
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
    [FirestoreProperty("name")]
{%- endif %}
    public string Name { get; set; } = default!;
}
