namespace {{project_class_name}}.Api.Entities;
{%- if cloud_service == 'GCP Cloud Function' %}

using System.Collections.Generic;
using Google.Cloud.Firestore;
using {{project_class_name}}.Api.Utils;
{%- elif cloud_service == 'AWS Lambda' %}

using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;
{%- endif %}
{% if cloud_service == 'GCP Cloud Function' %}
[FirestoreData]
{%- endif %}
public class BaseEntity
{
{%- if cloud_service == 'GCP Cloud Function' %}
    [FirestoreProperty("id")]
{%- endif %}
    public string Id { get; set; } = default!;
{% if cloud_service == 'GCP Cloud Function' %}
    [FirestoreProperty("isDeleted")]
{%- endif %}
    public bool IsDeleted { get; set; } = false;
{% if cloud_service == 'GCP Cloud Function' %}
    [FirestoreProperty("createdTimestamp", ConverterType = typeof(TimestampConverter))]
{%- endif %}
    public string CreatedTimestamp { get; set; } = default!;
{% if cloud_service == 'GCP Cloud Function' %}
    [FirestoreProperty("updatedTimestamp", ConverterType = typeof(TimestampConverter))]
{%- endif %}
    public string UpdatedTimestamp { get; set; } = default!;
{% if cloud_service == 'GCP Cloud Function' %}
    [FirestoreProperty("createdBy")]
{%- endif %}
    public string? CreatedBy { get; set; }
{% if cloud_service == 'GCP Cloud Function' %}
    [FirestoreProperty("updatedBy")]
{%- endif %}
    public string? UpdatedBy { get; set; }
{%- if cloud_service == 'GCP Cloud Function' %}

    public virtual Dictionary<string, object> ToDocument()
    {
        var document = new Dictionary<string, object>
        {
            { "id", Id },
            { "isDeleted", IsDeleted },
            { "createdTimestamp", CreatedTimestamp },
            { "updatedTimestamp", UpdatedTimestamp },
        };
        if (CreatedBy != null)
        {
            document["createdBy"] = CreatedBy;
        }
        if (UpdatedBy != null)
        {
            document["updatedBy"] = UpdatedBy;
        }
        return document;
    }
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}

    public virtual void WriteAttributes(Dictionary<string, AttributeValue> item)
    {
    }

    public virtual void ReadAttributes(Dictionary<string, AttributeValue> item)
    {
    }
{%- endif %}
}
