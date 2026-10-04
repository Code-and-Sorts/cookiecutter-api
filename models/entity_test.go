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

func TestToAuditDto_CopiesTheStoredAuditFields(t *testing.T) {
	entity := BaseEntity{
		Id:               "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c",
		IsDeleted:        true,
		CreatedTimestamp: "2026-01-01T00:00:00.000Z",
		CreatedBy:        "alice",
		UpdatedTimestamp: "2026-01-02T00:00:00.000Z",
		UpdatedBy:        "bob",
	}

	body, err := json.Marshal(entity.ToAuditDto())

	assert.NoError(t, err)
	assert.Equal(t, `{"createdTimestamp":"2026-01-01T00:00:00.000Z","createdBy":"alice","updatedTimestamp":"2026-01-02T00:00:00.000Z","updatedBy":"bob"}`, string(body))
}

func TestToAuditDto_OmitsUnsetUserFields(t *testing.T) {
	entity := NewBaseEntity()

	body, err := json.Marshal(entity.ToAuditDto())

	assert.NoError(t, err)
	assert.JSONEq(t, `{"createdTimestamp": "`+entity.CreatedTimestamp+`", "updatedTimestamp": "`+entity.UpdatedTimestamp+`"}`, string(body))
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
