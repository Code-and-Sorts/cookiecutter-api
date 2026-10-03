package controllers

import (
	"context"
	"io"

	"kittenclaws/models"
	"kittenclaws/services"
)

type VisitController interface {
	Get(ctx context.Context, id string) (*models.VisitDto, error)
	Create(ctx context.Context, userID string, body io.Reader) (*models.VisitDto, error)
	Update(ctx context.Context, id, userID string, body io.Reader) (*models.VisitDto, error)
}

type visitController struct {
	service         services.VisitService
	schemaValidator services.SchemaValidator
}

func NewVisitController(service services.VisitService, schemaValidator services.SchemaValidator) VisitController {
	return &visitController{service: service, schemaValidator: schemaValidator}
}

func (c *visitController) Get(ctx context.Context, id string) (*models.VisitDto, error) {
	if err := requireID("Visit", id); err != nil {
		return nil, err
	}
	return c.service.Get(ctx, id)
}

func (c *visitController) Create(ctx context.Context, userID string, body io.Reader) (*models.VisitDto, error) {
	req := models.NewCreateVisitRequest()
	if _, err := decodeRequest(c.schemaValidator, body, "visit_create_request", &req); err != nil {
		return nil, err
	}

	return c.service.Create(ctx, req, userID)
}

func (c *visitController) Update(ctx context.Context, id, userID string, body io.Reader) (*models.VisitDto, error) {
	if err := requireID("Visit", id); err != nil {
		return nil, err
	}

	var req models.UpdateVisitRequest
	sent, err := decodeRequest(c.schemaValidator, body, "visit_update_request", &req)
	if err != nil {
		return nil, err
	}
	req.Id = id
	req.Sent = sent

	return c.service.Update(ctx, req, userID)
}
