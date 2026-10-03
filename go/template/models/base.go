{%- from 'go/_model.jinja' import default_pointer, entity_rows, json_rows, literal, struct_fields -%}
{%- set all_ops = path_resources | map(attribute='operations') | sum(start=[]) | unique | list -%}
{%- set user_fields = base_fields | rejectattr("system") | list -%}
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
{%- set ns.rows = ns.rows + ([system_rows[f.name]] if f.system else (entity_rows([f]) | from_json)) -%}
{%- endfor -%}
{%- set create_fields = user_fields | selectattr("in_create") | list -%}
{%- set replace_fields = user_fields | selectattr("in_replace") | list -%}
{%- set update_fields = user_fields | selectattr("in_update") | list -%}
{%- set response_fields = user_fields | rejectattr("hidden") | list -%}
{%- set needs_uuid_default = (create_fields + replace_fields) | selectattr("dynamic", "equalto", "uuid") | list -%}
package models

import "github.com/google/uuid"

// The base model every resource record embeds: the built-in fields the server sets and the fields base_model adds.
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

// The base_model fields a create body may set; NewBaseCreateRequest holds the defaults the body overrides.
type BaseCreateRequest struct {
{{- struct_fields(json_rows(create_fields) | from_json) if create_fields }}
}

func NewBaseCreateRequest() BaseCreateRequest {
	req := BaseCreateRequest{}
{%- for f in create_fields if f.has_default and f.default is not none or f.dynamic %}
	req.{{ f.pascal }} = {{ default_pointer(f) }}
{%- endfor %}
	return req
}

func (r BaseCreateRequest) ToEntity() BaseEntity {
	entity := NewBaseEntity()
{%- for f in create_fields %}
	entity.{{ f.pascal }} = r.{{ f.pascal }}
{%- endfor %}
	return entity
}

// The base_model fields a replace body may set; omitted ones get their defaults.
type BaseReplaceRequest struct {
{{- struct_fields(json_rows(replace_fields) | from_json) if replace_fields }}
}

func NewBaseReplaceRequest() BaseReplaceRequest {
	req := BaseReplaceRequest{}
{%- for f in replace_fields if f.has_default and f.default is not none or f.dynamic %}
	req.{{ f.pascal }} = {{ default_pointer(f) }}
{%- endfor %}
	return req
}

func (r BaseReplaceRequest) ApplyTo(entity *BaseEntity) {
{%- for f in replace_fields %}
	entity.{{ f.pascal }} = r.{{ f.pascal }}
{%- endfor %}
}

// The base_model fields an update body may set. Sent holds the properties the body named, so a field
// left out stays unchanged while an explicit null clears it.
type BaseUpdateRequest struct {
{{- struct_fields([("Sent", "map[string]bool", "`json:\"-\"`")] + (json_rows(update_fields) | from_json)) }}
}

func (r BaseUpdateRequest) ApplyTo(entity *BaseEntity) {
{%- for f in update_fields %}
	if r.Sent["{{ f.name }}"] {
		entity.{{ f.pascal }} = r.{{ f.pascal }}
	}
{%- endfor %}
}

// What every response holds: the id and the base_model fields that are not hidden.
type BaseResponse struct {
{{- struct_fields([("Id", "string", "`json:\"id\"`")] + (json_rows(response_fields) | from_json)) }}
}

func NewBaseResponse(entity BaseEntity) BaseResponse {
	response := BaseResponse{Id: entity.Id}
{%- for f in response_fields %}
{%- if f.has_default and not f.dynamic and not f.nullable and f.default is not none %}
	response.{{ f.pascal }} = orDefault(entity.{{ f.pascal }}, {{ literal(f, f.default) }})
{%- else %}
	response.{{ f.pascal }} = entity.{{ f.pascal }}
{%- endif %}
{%- endfor %}
	return response
}
