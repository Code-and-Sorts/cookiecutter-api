package controllers

import "github.com/google/uuid"

// IsValidID reports whether id is a canonical UUID string, the only form of
// id the API issues. Anything else is answered with a 404 without touching
// the database.
func IsValidID(id string) bool {
	if len(id) != 36 {
		return false
	}
	_, err := uuid.Parse(id)
	return err == nil
}
