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
