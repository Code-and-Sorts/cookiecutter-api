{%- set any_id = path_resources | selectattr('has_item') | list | length > 0 -%}
package controllers

import (
	"testing"
{%- if any_id %}

	"{{project_endpoint}}/models"
{%- endif %}

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
{%- if any_id %}

func TestRequireID_ReturnsNotFoundForInvalidID(t *testing.T) {
	assert.NoError(t, requireID("Item", "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c"))

	var notFound *models.NotFoundError
	if assert.ErrorAs(t, requireID("Item", "abc"), &notFound) {
		assert.Equal(t, "Item with id abc was not found.", notFound.Message)
	}
}
{%- endif %}
