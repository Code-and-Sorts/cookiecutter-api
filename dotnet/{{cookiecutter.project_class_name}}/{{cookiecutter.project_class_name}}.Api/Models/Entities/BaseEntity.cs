namespace {{cookiecutter.project_class_name}}.Api.Entities;

using System;
using Newtonsoft.Json;
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
using Google.Cloud.Firestore;
{%- endif %}

{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
[FirestoreData]
{%- endif %}
public class BaseEntity
{
    [JsonProperty("id")]
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
    [FirestoreProperty("id")]
{%- endif %}
    public string Id { get; set; } = default!;

    [JsonProperty("isDeleted")]
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
    [FirestoreProperty("isDeleted")]
{%- endif %}
    public bool IsDeleted { get; set; } = false;

    [JsonProperty("createdTimestamp")]
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
    [FirestoreProperty("createdTimestamp")]
{%- endif %}
    public DateTime CreatedTimestamp { get; set; } = DateTime.UtcNow;

    [JsonProperty("updatedTimestamp")]
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
    [FirestoreProperty("updatedTimestamp")]
{%- endif %}
    public DateTime UpdatedTimestamp { get; set; } = DateTime.UtcNow;

    [JsonProperty("createdBy")]
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
    [FirestoreProperty("createdBy")]
{%- endif %}
    public string CreatedBy { get; set; } = default!;

    [JsonProperty("updatedBy")]
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
    [FirestoreProperty("updatedBy")]
{%- endif %}
    public string UpdatedBy { get; set; } = default!;
}
