package controllers

import (
	"context"
	"fmt"
	"io"

	"kittenclaws/models"
	"kittenclaws/services"
)

type KittenClawsController interface {
	Get(ctx context.Context, id string) (*models.KittenClawsDto, error)
	GetList(ctx context.Context, limit int) ([]models.KittenClawsDto, error)
	Create(ctx context.Context, userID string, body io.Reader) (*models.KittenClawsDto, error)
	Update(ctx context.Context, id, userID string, body io.Reader) (*models.KittenClawsDto, error)
	Delete(ctx context.Context, id, userID string) (map[string]string, error)
}

type kittenClawsController struct {
	service         services.KittenClawsService
	schemaValidator services.SchemaValidator
}

func NewKittenClawsController(service services.KittenClawsService, schemaValidator services.SchemaValidator) KittenClawsController {
	return &kittenClawsController{service: service, schemaValidator: schemaValidator}
}

func (c *kittenClawsController) Get(ctx context.Context, id string) (*models.KittenClawsDto, error) {
	if err := requireID("KittenClaws", id); err != nil {
		return nil, err
	}
	return c.service.Get(ctx, id)
}

// Never returns nil, so an empty list encodes as [] rather than null.
func (c *kittenClawsController) GetList(ctx context.Context, limit int) ([]models.KittenClawsDto, error) {
	items, err := c.service.GetList(ctx, CoerceLimit(limit))
	if err != nil {
		return nil, err
	}
	if items == nil {
		items = []models.KittenClawsDto{}
	}
	return items, nil
}

func (c *kittenClawsController) Create(ctx context.Context, userID string, body io.Reader) (*models.KittenClawsDto, error) {
	req := models.NewCreateKittenClawsRequest()
	if _, err := decodeRequest(c.schemaValidator, body, "kitten_claws_create_request", &req); err != nil {
		return nil, err
	}

	return c.service.Create(ctx, req, userID)
}

func (c *kittenClawsController) Update(ctx context.Context, id, userID string, body io.Reader) (*models.KittenClawsDto, error) {
	if err := requireID("KittenClaws", id); err != nil {
		return nil, err
	}

	var req models.UpdateKittenClawsRequest
	sent, err := decodeRequest(c.schemaValidator, body, "kitten_claws_update_request", &req)
	if err != nil {
		return nil, err
	}
	req.Id = id
	req.Sent = sent

	return c.service.Update(ctx, req, userID)
}

func (c *kittenClawsController) Delete(ctx context.Context, id, userID string) (map[string]string, error) {
	if err := requireID("KittenClaws", id); err != nil {
		return nil, err
	}
	if err := c.service.Delete(ctx, id, userID); err != nil {
		return nil, err
	}
	return map[string]string{"message": fmt.Sprintf("KittenClaws with id %s was deleted successfully.", id)}, nil
}
