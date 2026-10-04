package controllers

import (
	_ "embed"
	"encoding/json"
	"io"

	"kittenclaws/models"
	"kittenclaws/services"
)

//go:embed schemas/cat_create_request.json
var catCreateRequestSchema string

//go:embed schemas/cat_update_request.json
var catUpdateRequestSchema string

//go:embed schemas/dog_create_request.json
var dogCreateRequestSchema string

//go:embed schemas/dog_replace_request.json
var dogReplaceRequestSchema string

func RequestSchemas() map[string]string {
	schemas := make(map[string]string)
	schemas["cat_create_request"] = catCreateRequestSchema
	schemas["cat_update_request"] = catUpdateRequestSchema
	schemas["dog_create_request"] = dogCreateRequestSchema
	schemas["dog_replace_request"] = dogReplaceRequestSchema
	return schemas
}

// Validating the raw body first rejects unknown fields and wrongly typed values.
func decodeRequest(validator services.SchemaValidator, body io.Reader, schemaName string, target any) error {
	data, err := io.ReadAll(body)
	if err != nil {
		return &models.ValidationError{Message: "Request body could not be read."}
	}

	if err := validator.Validate(data, schemaName); err != nil {
		return err
	}

	if err := json.Unmarshal(data, target); err != nil {
		return &models.ValidationError{Message: "Request body must be valid JSON."}
	}

	return nil
}
