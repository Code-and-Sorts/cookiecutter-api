{%- from 'shared/_fields.jinja' import REQUEST_KINDS -%}
{%- from 'go/_model.jinja' import field_rows, read_value, request_types, struct_fields -%}
{%- set all_ops = path_resources | map(attribute='operations') | sum(start=[]) | unique | list -%}
{%- set system_rows = {
    "id": ("Id", "string", "`json:\"id\" dynamodbav:\"id\" firestore:\"id\"`"),
    "isDeleted": ("IsDeleted", "bool", "`json:\"isDeleted\" dynamodbav:\"isDeleted\" firestore:\"isDeleted\"`"),
    "createdTimestamp": ("CreatedTimestamp", "string", "`json:\"createdTimestamp\" dynamodbav:\"createdTimestamp\" firestore:\"createdTimestamp\"`"),
    "updatedTimestamp": ("UpdatedTimestamp", "string", "`json:\"updatedTimestamp\" dynamodbav:\"updatedTimestamp\" firestore:\"updatedTimestamp\"`"),
    "createdBy": ("CreatedBy", "string", "`json:\"createdBy,omitempty\" dynamodbav:\"createdBy,omitempty\" firestore:\"createdBy,omitempty\"`"),
    "updatedBy": ("UpdatedBy", "string", "`json:\"updatedBy,omitempty\" dynamodbav:\"updatedBy,omitempty\" firestore:\"updatedBy,omitempty\"`"),
} -%}
{%- set ns = namespace(rows=[]) -%}
{%- for f in base_fields -%}
{%- set ns.rows = ns.rows + ([system_rows[f.name]] if f.system else (field_rows([f], stored=true) | from_json)) -%}
{%- endfor -%}
{%- set response_fields = client_base_fields | rejectattr("hidden") | list -%}
package models

import "github.com/google/uuid"

type BaseEntity struct {
{{- struct_fields(ns.rows) }}
}

func NewBaseEntity() BaseEntity {
	now := Now()
	return BaseEntity{Id: uuid.New().String(), CreatedTimestamp: now, UpdatedTimestamp: now}
}

// Promoted to every record type, so generic repository code can reach the shared fields.
func (b *BaseEntity) Base() *BaseEntity {
	return b
}

{%- if 'create' in all_ops %}

func (b *BaseEntity) StampCreate(userID string) {
	b.CreatedBy = userID
	b.UpdatedBy = userID
}
{%- endif %}
{%- if all_ops | select('in', ['update', 'replace', 'delete']) | list %}

// An empty userID removes updatedBy, so it always describes the latest write.
func (b *BaseEntity) StampWrite(userID string) {
	b.UpdatedTimestamp = Now()
	b.UpdatedBy = userID
}
{%- endif %}

{%- for op, kind in REQUEST_KINDS %}

{{ request_types(op, "Base" ~ kind ~ "Request", "BaseEntity", "", "NewBaseEntity()", client_base_fields | selectattr("in_" ~ op) | list) }}
{%- endfor %}

type BaseResponse struct {
{{- struct_fields([("Id", "string", "`json:\"id\"`")] + (field_rows(response_fields) | from_json)) }}
}

func NewBaseResponse(entity BaseEntity) BaseResponse {
	response := BaseResponse{Id: entity.Id}
{%- for f in response_fields %}
	response.{{ f.pascal }} = {{ read_value(f, "entity") }}
{%- endfor %}
	return response
}
