package controllers

import (
	"context"
	"fmt"
	"io"

	"kittenclaws/models"
	"kittenclaws/services"
)

type CatController interface {
	Get(ctx context.Context, id string) (*models.CatDto, error)
	GetList(ctx context.Context, limit int) ([]models.CatDto, error)
	Create(ctx context.Context, userID string, body io.Reader) (*models.CatDto, error)
	Update(ctx context.Context, id, userID string, body io.Reader) (*models.CatDto, error)
	Delete(ctx context.Context, id, userID string) (map[string]string, error)
}

type catController struct {
	service         services.CatService
	schemaValidator services.SchemaValidator
}

func NewCatController(service services.CatService, schemaValidator services.SchemaValidator) CatController {
	return &catController{service: service, schemaValidator: schemaValidator}
}

func (c *catController) Get(ctx context.Context, id string) (*models.CatDto, error) {
	if err := requireID("Cat", id); err != nil {
		return nil, err
	}
	return c.service.Get(ctx, id)
}

// Never returns nil, so an empty list encodes as [] rather than null.
func (c *catController) GetList(ctx context.Context, limit int) ([]models.CatDto, error) {
	items, err := c.service.GetList(ctx, CoerceLimit(limit))
	if err != nil {
		return nil, err
	}
	if items == nil {
		items = []models.CatDto{}
	}
	return items, nil
}

func (c *catController) Create(ctx context.Context, userID string, body io.Reader) (*models.CatDto, error) {
	var req models.CreateCatRequest
	if err := decodeRequest(c.schemaValidator, body, "cat_create_request", &req); err != nil {
		return nil, err
	}

	return c.service.Create(ctx, req, userID)
}

func (c *catController) Update(ctx context.Context, id, userID string, body io.Reader) (*models.CatDto, error) {
	if err := requireID("Cat", id); err != nil {
		return nil, err
	}

	var req models.UpdateCatRequest
	if err := decodeRequest(c.schemaValidator, body, "cat_update_request", &req); err != nil {
		return nil, err
	}
	req.Id = id

	return c.service.Update(ctx, req, userID)
}

func (c *catController) Delete(ctx context.Context, id, userID string) (map[string]string, error) {
	if err := requireID("Cat", id); err != nil {
		return nil, err
	}
	if err := c.service.Delete(ctx, id, userID); err != nil {
		return nil, err
	}
	return map[string]string{"message": fmt.Sprintf("Cat with id %s was deleted successfully.", id)}, nil
}
