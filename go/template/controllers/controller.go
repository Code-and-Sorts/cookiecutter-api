package controllers

import (
	"context"
	_ "embed"
	"encoding/json"
	"io"

	"{{project_endpoint}}/models"
	"{{project_endpoint}}/services"
)

//go:embed schemas/create_item_request.json
var CreateRequestSchema string

//go:embed schemas/update_item_request.json
var UpdateRequestSchema string

//go:embed schemas/replace_item_request.json
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

type ItemController interface {
	Get(ctx context.Context, id string) (*models.ItemDto, error)
	GetList(ctx context.Context, limit int) ([]models.ItemDto, error)
	Create(ctx context.Context, body io.Reader) (*models.ItemDto, error)
	Update(ctx context.Context, id string, body io.Reader) (*models.ItemDto, error)
	Replace(ctx context.Context, id string, body io.Reader) (*models.ItemDto, error)
	Delete(ctx context.Context, id string) error
}

type itemController struct {
	service         services.ItemService
	schemaValidator services.SchemaValidator
}

func NewItemController(service services.ItemService, schemaValidator services.SchemaValidator) ItemController {
	return &itemController{service: service, schemaValidator: schemaValidator}
}

func (c *itemController) Get(ctx context.Context, id string) (*models.ItemDto, error) {
	return c.service.Get(ctx, id)
}

func (c *itemController) GetList(ctx context.Context, limit int) ([]models.ItemDto, error) {
	return c.service.GetList(ctx, CoerceLimit(limit))
}

func (c *itemController) Create(ctx context.Context, body io.Reader) (*models.ItemDto, error) {
	var req models.CreateItemRequest
	if err := json.NewDecoder(body).Decode(&req); err != nil {
		return nil, &models.ValidationError{Message: "Invalid request body."}
	}

	if err := c.schemaValidator.Validate(req, "create_request"); err != nil {
		return nil, err
	}

	return c.service.Create(ctx, req)
}

func (c *itemController) Update(ctx context.Context, id string, body io.Reader) (*models.ItemDto, error) {
	var req models.UpdateItemRequest
	if err := json.NewDecoder(body).Decode(&req); err != nil {
		return nil, &models.ValidationError{Message: "Invalid request body."}
	}

	req.Id = id

	if err := c.schemaValidator.Validate(req, "update_request"); err != nil {
		return nil, err
	}

	return c.service.Update(ctx, req)
}

func (c *itemController) Replace(ctx context.Context, id string, body io.Reader) (*models.ItemDto, error) {
	var req models.ReplaceItemRequest
	if err := json.NewDecoder(body).Decode(&req); err != nil {
		return nil, &models.ValidationError{Message: "Invalid request body."}
	}

	req.Id = id

	if err := c.schemaValidator.Validate(req, "replace_request"); err != nil {
		return nil, err
	}

	return c.service.Replace(ctx, req)
}

func (c *itemController) Delete(ctx context.Context, id string) error {
	return c.service.Delete(ctx, id)
}
