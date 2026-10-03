package controllers

import (
	"bytes"
	_ "embed"
	"encoding/json"
	"io"
	"math"
	"strconv"

	"kittenclaws/models"
	"kittenclaws/services"
)

//go:embed schemas/cat_create_request.json
var catCreateRequestSchema string

//go:embed schemas/cat_update_request.json
var catUpdateRequestSchema string

//go:embed schemas/cat_replace_request.json
var catReplaceRequestSchema string

//go:embed schemas/dog_create_request.json
var dogCreateRequestSchema string

//go:embed schemas/dog_replace_request.json
var dogReplaceRequestSchema string

//go:embed schemas/visit_create_request.json
var visitCreateRequestSchema string

//go:embed schemas/visit_update_request.json
var visitUpdateRequestSchema string

func RequestSchemas() map[string]string {
	schemas := make(map[string]string)
	schemas["cat_create_request"] = catCreateRequestSchema
	schemas["cat_update_request"] = catUpdateRequestSchema
	schemas["cat_replace_request"] = catReplaceRequestSchema
	schemas["dog_create_request"] = dogCreateRequestSchema
	schemas["dog_replace_request"] = dogReplaceRequestSchema
	schemas["visit_create_request"] = visitCreateRequestSchema
	schemas["visit_update_request"] = visitUpdateRequestSchema
	return schemas
}

// Returns the properties the body sent, so an update can tell an absent field from an explicit null.
func decodeRequest(validator services.SchemaValidator, body io.Reader, schemaName string, target any) (map[string]bool, error) {
	data, err := io.ReadAll(body)
	if err != nil {
		return nil, &models.ValidationError{Message: "Request body could not be read."}
	}

	if err := validator.Validate(data, schemaName); err != nil {
		return nil, err
	}

	// The schema reads 1.0 and 1e3 as integers, so whole numbers are rewritten before they reach int64 fields.
	decoder := json.NewDecoder(bytes.NewReader(data))
	decoder.UseNumber()
	var properties map[string]any
	if err := decoder.Decode(&properties); err != nil {
		return nil, err
	}
	normalized, err := json.Marshal(wholeNumbers(properties))
	if err != nil {
		return nil, err
	}
	if err := json.Unmarshal(normalized, target); err != nil {
		return nil, &models.ValidationError{Message: err.Error()}
	}

	sent := make(map[string]bool, len(properties))
	for name := range properties {
		sent[name] = true
	}
	return sent, nil
}

func wholeNumbers(value any) any {
	switch v := value.(type) {
	case map[string]any:
		for key, item := range v {
			v[key] = wholeNumbers(item)
		}
	case []any:
		for index, item := range v {
			v[index] = wholeNumbers(item)
		}
	case json.Number:
		if number, err := v.Float64(); err == nil && number == math.Trunc(number) && math.Abs(number) < math.MaxInt64 {
			return json.Number(strconv.FormatInt(int64(number), 10))
		}
	}
	return value
}
