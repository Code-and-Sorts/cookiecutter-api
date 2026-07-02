package controllers

import (
	"context"
	_ "embed"
	"encoding/json"
	"io"

	"{{project_endpoint}}/models"
	"{{project_endpoint}}/services"
)

//go:embed schemas/create_request.json
var CreateRequestSchema string

//go:embed schemas/update_request.json
var UpdateRequestSchema string

//go:embed schemas/replace_request.json
var ReplaceRequestSchema string

// Bounds for list pagination, protecting the datastore from unbounded reads.
const (
	DefaultListLimit = 100
	MaxListLimit     = 1000
)

// CoerceLimit clamps a requested page size into the supported range,
// falling back to the default when the value is missing or invalid.
func CoerceLimit(limit int) int {
	if limit < 1 {
		return DefaultListLimit
	}
	if limit > MaxListLimit {
		return MaxListLimit
	}
	return limit
}
{% for resource in resources %}
type {{ resource.name }}Controller interface {
	Get(ctx context.Context, id string) (*models.{{ resource.name }}Dto, error)
	GetList(ctx context.Context, limit int) ([]models.{{ resource.name }}Dto, error)
	Create(ctx context.Context, body io.Reader) (*models.{{ resource.name }}Dto, error)
	Update(ctx context.Context, id string, body io.Reader) (*models.{{ resource.name }}Dto, error)
	Replace(ctx context.Context, id string, body io.Reader) (*models.{{ resource.name }}Dto, error)
	Delete(ctx context.Context, id string) error
}

type {{ resource.name | to_lower_camel }}Controller struct {
	service         services.{{ resource.name }}Service
	schemaValidator services.SchemaValidator
}

func New{{ resource.name }}Controller(service services.{{ resource.name }}Service, schemaValidator services.SchemaValidator) {{ resource.name }}Controller {
	return &{{ resource.name | to_lower_camel }}Controller{service: service, schemaValidator: schemaValidator}
}

func (c *{{ resource.name | to_lower_camel }}Controller) Get(ctx context.Context, id string) (*models.{{ resource.name }}Dto, error) {
	return c.service.Get(ctx, id)
}

func (c *{{ resource.name | to_lower_camel }}Controller) GetList(ctx context.Context, limit int) ([]models.{{ resource.name }}Dto, error) {
	return c.service.GetList(ctx, CoerceLimit(limit))
}

func (c *{{ resource.name | to_lower_camel }}Controller) Create(ctx context.Context, body io.Reader) (*models.{{ resource.name }}Dto, error) {
	var req models.Create{{ resource.name }}Request
	if err := json.NewDecoder(body).Decode(&req); err != nil {
		return nil, &models.ValidationError{Message: "Invalid request body."}
	}

	if err := c.schemaValidator.Validate(req, "create_request"); err != nil {
		return nil, err
	}

	return c.service.Create(ctx, req)
}

func (c *{{ resource.name | to_lower_camel }}Controller) Update(ctx context.Context, id string, body io.Reader) (*models.{{ resource.name }}Dto, error) {
	var req models.Update{{ resource.name }}Request
	if err := json.NewDecoder(body).Decode(&req); err != nil {
		return nil, &models.ValidationError{Message: "Invalid request body."}
	}

	req.Id = id

	if err := c.schemaValidator.Validate(req, "update_request"); err != nil {
		return nil, err
	}

	return c.service.Update(ctx, req)
}

func (c *{{ resource.name | to_lower_camel }}Controller) Replace(ctx context.Context, id string, body io.Reader) (*models.{{ resource.name }}Dto, error) {
	var req models.Replace{{ resource.name }}Request
	if err := json.NewDecoder(body).Decode(&req); err != nil {
		return nil, &models.ValidationError{Message: "Invalid request body."}
	}

	req.Id = id

	if err := c.schemaValidator.Validate(req, "replace_request"); err != nil {
		return nil, err
	}

	return c.service.Replace(ctx, req)
}

func (c *{{ resource.name | to_lower_camel }}Controller) Delete(ctx context.Context, id string) error {
	return c.service.Delete(ctx, id)
}
{% endfor %}