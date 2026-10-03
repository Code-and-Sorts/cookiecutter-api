{%- from 'go/_model.jinja' import json_string, pointer -%}
{%- set all_ops = path_resources | map(attribute='operations') | sum(start=[]) | unique | list -%}
{%- set user_fields = client_base_fields -%}
package models

import (
	"encoding/json"
	"testing"

	"github.com/google/uuid"
	"github.com/stretchr/testify/assert"
)

func TestNewBaseEntity_SetsIdAndEqualTimestamps(t *testing.T) {
	entity := NewBaseEntity()

	_, err := uuid.Parse(entity.Id)
	assert.NoError(t, err)
	assert.False(t, entity.IsDeleted)
	assert.Empty(t, entity.CreatedBy)
	assert.Empty(t, entity.UpdatedBy)
	assert.Equal(t, entity.CreatedTimestamp, entity.UpdatedTimestamp)
	assert.Regexp(t, `^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3}Z$`, entity.CreatedTimestamp)
}

func TestNewBaseEntity_GeneratesUniqueIds(t *testing.T) {
	assert.NotEqual(t, NewBaseEntity().Id, NewBaseEntity().Id)
}

func TestBase_ReturnsTheEmbeddedFields(t *testing.T) {
	entity := NewBaseEntity()

	entity.Base().IsDeleted = true

	assert.True(t, entity.IsDeleted)
}
{%- if 'create' in all_ops %}

func TestStampCreate_SetsBothUserFields(t *testing.T) {
	entity := NewBaseEntity()

	entity.StampCreate("alice")

	assert.Equal(t, "alice", entity.CreatedBy)
	assert.Equal(t, "alice", entity.UpdatedBy)
}
{%- endif %}
{%- if all_ops | select('in', ['update', 'replace', 'delete']) | list %}

func TestStampWrite_SetsOrRemovesUpdatedByAndKeepsCreatedBy(t *testing.T) {
	entity := BaseEntity{CreatedBy: "alice", UpdatedBy: "alice", UpdatedTimestamp: "2026-01-01T00:00:00.000Z"}

	entity.StampWrite("bob")

	assert.Equal(t, "alice", entity.CreatedBy)
	assert.Equal(t, "bob", entity.UpdatedBy)
	assert.NotEqual(t, "2026-01-01T00:00:00.000Z", entity.UpdatedTimestamp)

	entity.StampWrite("")

	assert.Equal(t, "alice", entity.CreatedBy)
	assert.Empty(t, entity.UpdatedBy)
}
{%- endif %}

func TestBaseRequests_CarryTheBaseModelFields(t *testing.T) {
	create := NewBaseCreateRequest()
{%- for f in user_fields | selectattr("in_create") %}
	create.{{ f.pascal }} = {{ pointer(f, f.sample) }}
{%- endfor %}
	entity := create.ToEntity()
{%- for f in user_fields | selectattr("in_create") %}
	assert.Equal(t, create.{{ f.pascal }}, entity.{{ f.pascal }})
{%- endfor %}

	replace := NewBaseReplaceRequest()
	replace.ApplyTo(&entity)
{%- for f in user_fields | selectattr("in_replace") %}
	assert.Equal(t, replace.{{ f.pascal }}, entity.{{ f.pascal }})
{%- endfor %}

	before := entity
	BaseUpdateRequest{Sent: map[string]bool{}}.ApplyTo(&entity)
	assert.Equal(t, before, entity)
}

func TestNewBaseResponse_HoldsTheIdAndTheShownBaseFields(t *testing.T) {
	data, err := json.Marshal(NewBaseResponse(BaseEntity{Id: "x"}))

	assert.NoError(t, err)
	properties := map[string]json.RawMessage{}
	assert.NoError(t, json.Unmarshal(data, &properties))
	assert.JSONEq(t, `"x"`, string(properties["id"]))
	assert.Len(t, properties, {{ 1 + (user_fields | rejectattr("hidden") | list | length) }})
{%- for f in user_fields | rejectattr("hidden") %}
	assert.JSONEq(t, {{ json_string(f.read_default) }}, string(properties["{{ f.name }}"]))
{%- endfor %}
}
