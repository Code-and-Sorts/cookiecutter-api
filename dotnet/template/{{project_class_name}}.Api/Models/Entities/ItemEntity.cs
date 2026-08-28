namespace {{project_class_name}}.Api.Entities;

using Newtonsoft.Json;
{%- if cloud_service == 'GCP Cloud Function' %}
using Google.Cloud.Firestore;
{%- endif %}
{% for resource in resources %}
{%- if cloud_service == 'GCP Cloud Function' %}
[FirestoreData]
{%- endif %}
public class {{ resource.name }} : BaseEntity
{
    [JsonProperty("name")]
{%- if cloud_service == 'GCP Cloud Function' %}
    [FirestoreProperty("name")]
{%- endif %}
    public string Name { get; set; } = default!;
}
{% endfor %}