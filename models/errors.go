package models

import "fmt"

type BaseError struct {
	ErrorMessage string `json:"errorMessage"`
}

type NotFoundError struct {
	Message string
}

func (e *NotFoundError) Error() string {
	return e.Message
}

func NewNotFoundError(resourceName, id string) *NotFoundError {
	return &NotFoundError{Message: fmt.Sprintf("%s with id %s was not found.", resourceName, id)}
}

type ValidationError struct {
	Message string
}

func (e *ValidationError) Error() string {
	return e.Message
}
