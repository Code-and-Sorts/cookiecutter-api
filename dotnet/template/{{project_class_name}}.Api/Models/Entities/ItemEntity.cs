namespace {{project_class_name}}.Api.Entities;

using Newtonsoft.Json;
{%- if cloud_service == 'GCP Cloud Function' %}
using Google.Cloud.Firestore;
{%- endif %}

{%- if cloud_service == 'GCP Cloud Function' %}
[FirestoreData]
{%- endif %}
public class Item : BaseEntity
{
    [JsonProperty("name")]
{%- if cloud_service == 'GCP Cloud Function' %}
    [FirestoreProperty("name")]
{%- endif %}
    public string Name { get; set; } = default!;
}
