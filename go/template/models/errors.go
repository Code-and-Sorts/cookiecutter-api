package models

import "fmt"

// BaseError is the JSON body of every error response.
type BaseError struct {
	ErrorMessage string `json:"errorMessage"`
}

// NotFoundError maps to a 404 response.
type NotFoundError struct {
	Message string
}

func (e *NotFoundError) Error() string {
	return e.Message
}

// NewNotFoundError reports that the resource record with the given id does not
// exist, is soft-deleted, or that the id is not a valid id.
func NewNotFoundError(resourceName, id string) *NotFoundError {
	return &NotFoundError{Message: fmt.Sprintf("%s with id %s was not found.", resourceName, id)}
}

// ValidationError maps to a 400 response.
type ValidationError struct {
	Message string
}

func (e *ValidationError) Error() string {
	return e.Message
}
