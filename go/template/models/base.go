{%- from 'shared/_fields.jinja' import REQUEST_KINDS -%}
{%- from 'go/_model.jinja' import field_rows, read_value, request_types, struct_fields -%}
{%- set all_ops = path_resources | map(attribute='operations') | sum(start=[]) | unique | list -%}
{%- set response_fields = client_base_fields | rejectattr("hidden") | list -%}
package models

import "github.com/google/uuid"

type BaseEntity struct {
{{- struct_fields(field_rows(base_fields, stored=true) | from_json) }}
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
