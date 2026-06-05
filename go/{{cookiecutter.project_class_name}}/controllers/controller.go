package controllers

import (
	"context"
	_ "embed"
	"encoding/json"
	"io"

	"{{cookiecutter.project_endpoint}}/models"
	"{{cookiecutter.project_endpoint}}/services"
)

//go:embed schemas/create_{{cookiecutter.project_endpoint}}_request.json
var CreateRequestSchema string

//go:embed schemas/update_{{cookiecutter.project_endpoint}}_request.json
var UpdateRequestSchema string

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

type {{cookiecutter.project_class_name}}Controller interface {
	Get(ctx context.Context, id string) (*models.{{cookiecutter.project_class_name}}Dto, error)
	GetList(ctx context.Context, limit int) ([]models.{{cookiecutter.project_class_name}}Dto, error)
	Create(ctx context.Context, body io.Reader) (*models.{{cookiecutter.project_class_name}}Dto, error)
	Update(ctx context.Context, id string, body io.Reader) (*models.{{cookiecutter.project_class_name}}Dto, error)
	Delete(ctx context.Context, id string) error
}

type {{cookiecutter.project_lower_camel_name}}Controller struct {
	service         services.{{cookiecutter.project_class_name}}Service
	schemaValidator services.SchemaValidator
}

func New{{cookiecutter.project_class_name}}Controller(service services.{{cookiecutter.project_class_name}}Service, schemaValidator services.SchemaValidator) {{cookiecutter.project_class_name}}Controller {
	return &{{cookiecutter.project_lower_camel_name}}Controller{service: service, schemaValidator: schemaValidator}
}

func (c *{{cookiecutter.project_lower_camel_name}}Controller) Get(ctx context.Context, id string) (*models.{{cookiecutter.project_class_name}}Dto, error) {
	return c.service.Get(ctx, id)
}

func (c *{{cookiecutter.project_lower_camel_name}}Controller) GetList(ctx context.Context, limit int) ([]models.{{cookiecutter.project_class_name}}Dto, error) {
	return c.service.GetList(ctx, CoerceLimit(limit))
}

func (c *{{cookiecutter.project_lower_camel_name}}Controller) Create(ctx context.Context, body io.Reader) (*models.{{cookiecutter.project_class_name}}Dto, error) {
	var req models.Create{{cookiecutter.project_class_name}}Request
	if err := json.NewDecoder(body).Decode(&req); err != nil {
		return nil, &models.ValidationError{Message: "Invalid request body."}
	}

	if err := c.schemaValidator.Validate(req, "create_request"); err != nil {
		return nil, err
	}

	return c.service.Create(ctx, req)
}

func (c *{{cookiecutter.project_lower_camel_name}}Controller) Update(ctx context.Context, id string, body io.Reader) (*models.{{cookiecutter.project_class_name}}Dto, error) {
	var req models.Update{{cookiecutter.project_class_name}}Request
	if err := json.NewDecoder(body).Decode(&req); err != nil {
		return nil, &models.ValidationError{Message: "Invalid request body."}
	}

	req.Id = id

	if err := c.schemaValidator.Validate(req, "update_request"); err != nil {
		return nil, err
	}

	return c.service.Update(ctx, req)
}

func (c *{{cookiecutter.project_lower_camel_name}}Controller) Delete(ctx context.Context, id string) error {
	return c.service.Delete(ctx, id)
}
