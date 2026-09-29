namespace {{project_class_name}}.Api.Entities;

{% if cloud_service == 'GCP Cloud Function' %}using System.Collections.Generic;
{% endif %}using Newtonsoft.Json;
{%- if cloud_service == 'GCP Cloud Function' %}
using Google.Cloud.Firestore;
{%- endif %}

// The stored record. Timestamps are ISO-8601 UTC strings with millisecond precision
// (see Utils.Timestamps); createdBy and updatedBy are only stored when set.
{%- if cloud_service == 'GCP Cloud Function' %}
[FirestoreData]
{%- endif %}
public class BaseEntity
{
    [JsonProperty("id")]
{%- if cloud_service == 'GCP Cloud Function' %}
    [FirestoreProperty("id")]
{%- endif %}
    public string Id { get; set; } = default!;

    [JsonProperty("isDeleted")]
{%- if cloud_service == 'GCP Cloud Function' %}
    [FirestoreProperty("isDeleted")]
{%- endif %}
    public bool IsDeleted { get; set; } = false;

    [JsonProperty("createdTimestamp")]
{%- if cloud_service == 'GCP Cloud Function' %}
    [FirestoreProperty("createdTimestamp")]
{%- endif %}
    public string CreatedTimestamp { get; set; } = default!;

    [JsonProperty("updatedTimestamp")]
{%- if cloud_service == 'GCP Cloud Function' %}
    [FirestoreProperty("updatedTimestamp")]
{%- endif %}
    public string UpdatedTimestamp { get; set; } = default!;

    [JsonProperty("createdBy", NullValueHandling = NullValueHandling.Ignore)]
{%- if cloud_service == 'GCP Cloud Function' %}
    [FirestoreProperty("createdBy")]
{%- endif %}
    public string? CreatedBy { get; set; }

    [JsonProperty("updatedBy", NullValueHandling = NullValueHandling.Ignore)]
{%- if cloud_service == 'GCP Cloud Function' %}
    [FirestoreProperty("updatedBy")]
{%- endif %}
    public string? UpdatedBy { get; set; }
{%- if cloud_service == 'GCP Cloud Function' %}

    /// <summary>The Firestore document fields, leaving out createdBy/updatedBy when unset.</summary>
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
}
