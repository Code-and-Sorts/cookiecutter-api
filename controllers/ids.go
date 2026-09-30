package controllers

import (
	"github.com/google/uuid"

	"kittenclaws/models"
)

// The API only issues UUIDs, so any other id is a 404 without a database read.
func IsValidID(id string) bool {
	if len(id) != 36 {
		return false
	}
	_, err := uuid.Parse(id)
	return err == nil
}

func requireID(resource, id string) error {
	if !IsValidID(id) {
		return models.NewNotFoundError(resource, id)
	}
	return nil
}
