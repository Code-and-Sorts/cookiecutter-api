{%- set any_id = path_resources | selectattr('has_item') | list | length > 0 -%}
package controllers

import (
	"github.com/google/uuid"
{%- if any_id %}

	"{{project_endpoint}}/models"
{%- endif %}
)

// The API only issues UUIDs, so any other id is a 404 without a database read.
func IsValidID(id string) bool {
	if len(id) != 36 {
		return false
	}
	_, err := uuid.Parse(id)
	return err == nil
}
{%- if any_id %}

func requireID(resource, id string) error {
	if !IsValidID(id) {
		return models.NewNotFoundError(resource, id)
	}
	return nil
}
{%- endif %}
