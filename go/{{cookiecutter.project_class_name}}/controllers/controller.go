package controllers

import (
	"context"
	"encoding/json"
	"io"

	"{{cookiecutter.project_endpoint}}/models"
	"{{cookiecutter.project_endpoint}}/services"
)

type {{cookiecutter.project_class_name}}Controller interface {
	Get(ctx context.Context, id string) (*models.{{cookiecutter.project_class_name}}Dto, error)
	GetList(ctx context.Context) ([]models.{{cookiecutter.project_class_name}}Dto, error)
	Create(ctx context.Context, body io.Reader) (*models.{{cookiecutter.project_class_name}}Dto, error)
	Update(ctx context.Context, id string, body io.Reader) (*models.{{cookiecutter.project_class_name}}Dto, error)
	Delete(ctx context.Context, id string) error
}

type {{cookiecutter.project_lower_camel_name}}Controller struct {
	service services.{{cookiecutter.project_class_name}}Service
}

func New{{cookiecutter.project_class_name}}Controller(service services.{{cookiecutter.project_class_name}}Service) {{cookiecutter.project_class_name}}Controller {
	return &{{cookiecutter.project_lower_camel_name}}Controller{service: service}
}

func (c *{{cookiecutter.project_lower_camel_name}}Controller) Get(ctx context.Context, id string) (*models.{{cookiecutter.project_class_name}}Dto, error) {
	return c.service.Get(ctx, id)
}

func (c *{{cookiecutter.project_lower_camel_name}}Controller) GetList(ctx context.Context) ([]models.{{cookiecutter.project_class_name}}Dto, error) {
	return c.service.GetList(ctx)
}

func (c *{{cookiecutter.project_lower_camel_name}}Controller) Create(ctx context.Context, body io.Reader) (*models.{{cookiecutter.project_class_name}}Dto, error) {
	var req models.Create{{cookiecutter.project_class_name}}Request
	if err := json.NewDecoder(body).Decode(&req); err != nil {
		return nil, &models.ValidationError{Message: "Invalid request body."}
	}

	if err := req.Validate(); err != nil {
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

	if err := req.Validate(); err != nil {
		return nil, err
	}

	return c.service.Update(ctx, req)
}

func (c *{{cookiecutter.project_lower_camel_name}}Controller) Delete(ctx context.Context, id string) error {
	return c.service.Delete(ctx, id)
}
