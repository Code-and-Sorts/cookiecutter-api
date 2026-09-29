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

type CatController interface {
	Get(ctx context.Context, id string) (*models.CatDto, error)
	GetList(ctx context.Context, limit int) ([]models.CatDto, error)
	Create(ctx context.Context, body io.Reader) (*models.CatDto, error)
	Update(ctx context.Context, id string, body io.Reader) (*models.CatDto, error)
	Delete(ctx context.Context, id string) error
}

type catController struct {
	service         services.CatService
	schemaValidator services.SchemaValidator
}

func NewCatController(service services.CatService, schemaValidator services.SchemaValidator) CatController {
	return &catController{service: service, schemaValidator: schemaValidator}
}

func (c *catController) Get(ctx context.Context, id string) (*models.CatDto, error) {
	return c.service.Get(ctx, id)
}

func (c *catController) GetList(ctx context.Context, limit int) ([]models.CatDto, error) {
	return c.service.GetList(ctx, CoerceLimit(limit))
}

func (c *catController) Create(ctx context.Context, body io.Reader) (*models.CatDto, error) {
	var req models.CreateCatRequest
	if err := json.NewDecoder(body).Decode(&req); err != nil {
		return nil, &models.ValidationError{Message: "Invalid request body."}
	}

	if err := c.schemaValidator.Validate(req, "create_request"); err != nil {
		return nil, err
	}

	return c.service.Create(ctx, req)
}

func (c *catController) Update(ctx context.Context, id string, body io.Reader) (*models.CatDto, error) {
	var req models.UpdateCatRequest
	if err := json.NewDecoder(body).Decode(&req); err != nil {
		return nil, &models.ValidationError{Message: "Invalid request body."}
	}

	req.Id = id

	if err := c.schemaValidator.Validate(req, "update_request"); err != nil {
		return nil, err
	}

	return c.service.Update(ctx, req)
}

func (c *catController) Delete(ctx context.Context, id string) error {
	return c.service.Delete(ctx, id)
}

type DogController interface {
	Get(ctx context.Context, id string) (*models.DogDto, error)
	GetList(ctx context.Context, limit int) ([]models.DogDto, error)
	Create(ctx context.Context, body io.Reader) (*models.DogDto, error)
	Replace(ctx context.Context, id string, body io.Reader) (*models.DogDto, error)
	Delete(ctx context.Context, id string) error
}

type dogController struct {
	service         services.DogService
	schemaValidator services.SchemaValidator
}

func NewDogController(service services.DogService, schemaValidator services.SchemaValidator) DogController {
	return &dogController{service: service, schemaValidator: schemaValidator}
}

func (c *dogController) Get(ctx context.Context, id string) (*models.DogDto, error) {
	return c.service.Get(ctx, id)
}

func (c *dogController) GetList(ctx context.Context, limit int) ([]models.DogDto, error) {
	return c.service.GetList(ctx, CoerceLimit(limit))
}

func (c *dogController) Create(ctx context.Context, body io.Reader) (*models.DogDto, error) {
	var req models.CreateDogRequest
	if err := json.NewDecoder(body).Decode(&req); err != nil {
		return nil, &models.ValidationError{Message: "Invalid request body."}
	}

	if err := c.schemaValidator.Validate(req, "create_request"); err != nil {
		return nil, err
	}

	return c.service.Create(ctx, req)
}

func (c *dogController) Replace(ctx context.Context, id string, body io.Reader) (*models.DogDto, error) {
	var req models.ReplaceDogRequest
	if err := json.NewDecoder(body).Decode(&req); err != nil {
		return nil, &models.ValidationError{Message: "Invalid request body."}
	}

	req.Id = id

	if err := c.schemaValidator.Validate(req, "replace_request"); err != nil {
		return nil, err
	}

	return c.service.Replace(ctx, req)
}

func (c *dogController) Delete(ctx context.Context, id string) error {
	return c.service.Delete(ctx, id)
}
