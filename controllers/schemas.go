package controllers

import (
	_ "embed"
	"encoding/json"
	"io"

	"kittenclaws/models"
	"kittenclaws/services"
)

//go:embed schemas/kitten_claws_create_request.json
var kittenClawsCreateRequestSchema string

//go:embed schemas/kitten_claws_update_request.json
var kittenClawsUpdateRequestSchema string

func RequestSchemas() map[string]string {
	schemas := make(map[string]string)
	schemas["kitten_claws_create_request"] = kittenClawsCreateRequestSchema
	schemas["kitten_claws_update_request"] = kittenClawsUpdateRequestSchema
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
