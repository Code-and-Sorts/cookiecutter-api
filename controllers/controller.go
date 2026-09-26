package controllers

import (
	"context"
	_ "embed"
	"encoding/json"
	"io"

	"kitties/models"
	"kitties/services"
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

type KittenClawsController interface {
	Get(ctx context.Context, id string) (*models.KittenClawsDto, error)
	GetList(ctx context.Context, limit int) ([]models.KittenClawsDto, error)
	Create(ctx context.Context, body io.Reader) (*models.KittenClawsDto, error)
	Update(ctx context.Context, id string, body io.Reader) (*models.KittenClawsDto, error)
	Delete(ctx context.Context, id string) error
}

type kittenClawsController struct {
	service         services.KittenClawsService
	schemaValidator services.SchemaValidator
}

func NewKittenClawsController(service services.KittenClawsService, schemaValidator services.SchemaValidator) KittenClawsController {
	return &kittenClawsController{service: service, schemaValidator: schemaValidator}
}

func (c *kittenClawsController) Get(ctx context.Context, id string) (*models.KittenClawsDto, error) {
	return c.service.Get(ctx, id)
}

func (c *kittenClawsController) GetList(ctx context.Context, limit int) ([]models.KittenClawsDto, error) {
	return c.service.GetList(ctx, CoerceLimit(limit))
}

func (c *kittenClawsController) Create(ctx context.Context, body io.Reader) (*models.KittenClawsDto, error) {
	var req models.CreateKittenClawsRequest
	if err := json.NewDecoder(body).Decode(&req); err != nil {
		return nil, &models.ValidationError{Message: "Invalid request body."}
	}

	if err := c.schemaValidator.Validate(req, "create_request"); err != nil {
		return nil, err
	}

	return c.service.Create(ctx, req)
}

func (c *kittenClawsController) Update(ctx context.Context, id string, body io.Reader) (*models.KittenClawsDto, error) {
	var req models.UpdateKittenClawsRequest
	if err := json.NewDecoder(body).Decode(&req); err != nil {
		return nil, &models.ValidationError{Message: "Invalid request body."}
	}

	req.Id = id

	if err := c.schemaValidator.Validate(req, "update_request"); err != nil {
		return nil, err
	}

	return c.service.Update(ctx, req)
}

func (c *kittenClawsController) Delete(ctx context.Context, id string) error {
	return c.service.Delete(ctx, id)
}
