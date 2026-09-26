{%- set used_ops = resources | map(attribute='operations') | sum(start=[]) -%}
{%- set uses_body = ('create' in used_ops) or ('update' in used_ops) or ('replace' in used_ops) -%}
{%- set uses_models = uses_body or ('get_by_id' in used_ops) or ('list' in used_ops) -%}
package controllers

import (
	"context"
	_ "embed"
{%- if uses_body %}
	"encoding/json"
	"io"
{%- endif %}
{%- if uses_models %}

	"{{project_endpoint}}/models"
{%- endif %}
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
{%- set r = resource.name %}
{%- set lc = resource.name | to_lower_camel %}
type {{ r }}Controller interface {
{%- if "get_by_id" in resource.operations %}
	Get(ctx context.Context, id string) (*models.{{ r }}Dto, error)
{%- endif %}
{%- if "list" in resource.operations %}
	GetList(ctx context.Context, limit int) ([]models.{{ r }}Dto, error)
{%- endif %}
{%- if "create" in resource.operations %}
	Create(ctx context.Context, body io.Reader) (*models.{{ r }}Dto, error)
{%- endif %}
{%- if "update" in resource.operations %}
	Update(ctx context.Context, id string, body io.Reader) (*models.{{ r }}Dto, error)
{%- endif %}
{%- if "replace" in resource.operations %}
	Replace(ctx context.Context, id string, body io.Reader) (*models.{{ r }}Dto, error)
{%- endif %}
{%- if "delete" in resource.operations %}
	Delete(ctx context.Context, id string) error
{%- endif %}
}

type {{ lc }}Controller struct {
	service         services.{{ r }}Service
	schemaValidator services.SchemaValidator
}

func New{{ r }}Controller(service services.{{ r }}Service, schemaValidator services.SchemaValidator) {{ r }}Controller {
	return &{{ lc }}Controller{service: service, schemaValidator: schemaValidator}
}
{%- if "get_by_id" in resource.operations %}

func (c *{{ lc }}Controller) Get(ctx context.Context, id string) (*models.{{ r }}Dto, error) {
	return c.service.Get(ctx, id)
}
{%- endif %}
{%- if "list" in resource.operations %}

func (c *{{ lc }}Controller) GetList(ctx context.Context, limit int) ([]models.{{ r }}Dto, error) {
	return c.service.GetList(ctx, CoerceLimit(limit))
}
{%- endif %}
{%- if "create" in resource.operations %}

func (c *{{ lc }}Controller) Create(ctx context.Context, body io.Reader) (*models.{{ r }}Dto, error) {
	var req models.Create{{ r }}Request
	if err := json.NewDecoder(body).Decode(&req); err != nil {
		return nil, &models.ValidationError{Message: "Invalid request body."}
	}

	if err := c.schemaValidator.Validate(req, "create_request"); err != nil {
		return nil, err
	}

	return c.service.Create(ctx, req)
}
{%- endif %}
{%- if "update" in resource.operations %}

func (c *{{ lc }}Controller) Update(ctx context.Context, id string, body io.Reader) (*models.{{ r }}Dto, error) {
	var req models.Update{{ r }}Request
	if err := json.NewDecoder(body).Decode(&req); err != nil {
		return nil, &models.ValidationError{Message: "Invalid request body."}
	}

	req.Id = id

	if err := c.schemaValidator.Validate(req, "update_request"); err != nil {
		return nil, err
	}

	return c.service.Update(ctx, req)
}
{%- endif %}
{%- if "replace" in resource.operations %}

func (c *{{ lc }}Controller) Replace(ctx context.Context, id string, body io.Reader) (*models.{{ r }}Dto, error) {
	var req models.Replace{{ r }}Request
	if err := json.NewDecoder(body).Decode(&req); err != nil {
		return nil, &models.ValidationError{Message: "Invalid request body."}
	}

	req.Id = id

	if err := c.schemaValidator.Validate(req, "replace_request"); err != nil {
		return nil, err
	}

	return c.service.Replace(ctx, req)
}
{%- endif %}
{%- if "delete" in resource.operations %}

func (c *{{ lc }}Controller) Delete(ctx context.Context, id string) error {
	return c.service.Delete(ctx, id)
}
{%- endif %}
{% endfor %}