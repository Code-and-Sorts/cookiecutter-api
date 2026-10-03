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

func TestStampCreate_SetsBothUserFields(t *testing.T) {
	entity := NewBaseEntity()

	entity.StampCreate("alice")

	assert.Equal(t, "alice", entity.CreatedBy)
	assert.Equal(t, "alice", entity.UpdatedBy)
}

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

func TestBaseRequests_CarryTheBaseModelFields(t *testing.T) {
	create := NewBaseCreateRequest()
	create.TenantId = new("public")
	create.Region = new("eu")
	create.Priority = new(int64(1))
	create.Rank = new(float64(1.5))
	create.Labels = new([]string{})
	entity := create.ToEntity()
	assert.Equal(t, create.TenantId, entity.TenantId)
	assert.Equal(t, create.Region, entity.Region)
	assert.Equal(t, create.Priority, entity.Priority)
	assert.Equal(t, create.Rank, entity.Rank)
	assert.Equal(t, create.Labels, entity.Labels)

	replace := NewBaseReplaceRequest()
	replace.ApplyTo(&entity)
	assert.Equal(t, replace.TenantId, entity.TenantId)
	assert.Equal(t, replace.Priority, entity.Priority)
	assert.Equal(t, replace.Rank, entity.Rank)
	assert.Equal(t, replace.Labels, entity.Labels)

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
	assert.Len(t, properties, 6)
	assert.JSONEq(t, "\"public\"", string(properties["tenantId"]))
	assert.JSONEq(t, "\"eu\"", string(properties["region"]))
	assert.JSONEq(t, "null", string(properties["priority"]))
	assert.JSONEq(t, "null", string(properties["rank"]))
	assert.JSONEq(t, "[]", string(properties["labels"]))
}
