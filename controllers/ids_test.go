package controllers

import (
	"testing"

	"kittenclaws/models"

	"github.com/stretchr/testify/assert"
)

func TestIsValidID_AcceptsOnlyCanonicalUUIDs(t *testing.T) {
	assert.True(t, IsValidID("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c"))
	assert.False(t, IsValidID(""))
	assert.False(t, IsValidID("not-a-uuid"))
	assert.False(t, IsValidID("0f3a7ff7a6014d23b33c7f8f18b57a4c"))
	assert.False(t, IsValidID("{0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c}"))
	assert.False(t, IsValidID("0f3a7ff7-a601-4d23-b33c-7f8f18b57a4z"))
}

func TestRequireID_ReturnsNotFoundForInvalidID(t *testing.T) {
	assert.NoError(t, requireID("Item", "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c"))

	var notFound *models.NotFoundError
	if assert.ErrorAs(t, requireID("Item", "abc"), &notFound) {
		assert.Equal(t, "Item with id abc was not found.", notFound.Message)
	}
}
