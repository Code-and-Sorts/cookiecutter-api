package models

import (
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
