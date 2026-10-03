package controllers

import (
	"context"
	"fmt"
	"io"

	"kittenclaws/models"
	"kittenclaws/services"
)

type DogController interface {
	Get(ctx context.Context, id string) (*models.DogDto, error)
	GetList(ctx context.Context, limit int) ([]models.DogDto, error)
	Create(ctx context.Context, userID string, body io.Reader) (*models.DogDto, error)
	Replace(ctx context.Context, id, userID string, body io.Reader) (*models.DogDto, error)
	Delete(ctx context.Context, id, userID string) (map[string]string, error)
}

type dogController struct {
	service         services.DogService
	schemaValidator services.SchemaValidator
}

func NewDogController(service services.DogService, schemaValidator services.SchemaValidator) DogController {
	return &dogController{service: service, schemaValidator: schemaValidator}
}

func (c *dogController) Get(ctx context.Context, id string) (*models.DogDto, error) {
	if err := requireID("Dog", id); err != nil {
		return nil, err
	}
	return c.service.Get(ctx, id)
}

// Never returns nil, so an empty list encodes as [] rather than null.
func (c *dogController) GetList(ctx context.Context, limit int) ([]models.DogDto, error) {
	items, err := c.service.GetList(ctx, CoerceLimit(limit))
	if err != nil {
		return nil, err
	}
	if items == nil {
		items = []models.DogDto{}
	}
	return items, nil
}

func (c *dogController) Create(ctx context.Context, userID string, body io.Reader) (*models.DogDto, error) {
	var req models.CreateDogRequest
	if err := decodeRequest(c.schemaValidator, body, "dog_create_request", &req); err != nil {
		return nil, err
	}

	return c.service.Create(ctx, req, userID)
}

func (c *dogController) Replace(ctx context.Context, id, userID string, body io.Reader) (*models.DogDto, error) {
	if err := requireID("Dog", id); err != nil {
		return nil, err
	}

	var req models.ReplaceDogRequest
	if err := decodeRequest(c.schemaValidator, body, "dog_replace_request", &req); err != nil {
		return nil, err
	}
	req.Id = id

	return c.service.Replace(ctx, req, userID)
}

func (c *dogController) Delete(ctx context.Context, id, userID string) (map[string]string, error) {
	if err := requireID("Dog", id); err != nil {
		return nil, err
	}
	if err := c.service.Delete(ctx, id, userID); err != nil {
		return nil, err
	}
	return map[string]string{"message": fmt.Sprintf("Dog with id %s was deleted successfully.", id)}, nil
}
