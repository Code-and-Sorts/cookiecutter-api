package services

import (
	"context"
	"time"

	"github.com/google/uuid"

	"{{cookiecutter.project_endpoint}}/models"
	"{{cookiecutter.project_endpoint}}/repositories"
)

type {{cookiecutter.project_class_name}}Service interface {
	Get(ctx context.Context, id string) (*models.{{cookiecutter.project_class_name}}Dto, error)
	GetList(ctx context.Context) ([]models.{{cookiecutter.project_class_name}}Dto, error)
	Create(ctx context.Context, req models.Create{{cookiecutter.project_class_name}}Request) (*models.{{cookiecutter.project_class_name}}Dto, error)
	Update(ctx context.Context, req models.Update{{cookiecutter.project_class_name}}Request) (*models.{{cookiecutter.project_class_name}}Dto, error)
	Delete(ctx context.Context, id string) error
}

type {{cookiecutter.project_lower_camel_name}}Service struct {
	repository repositories.{{cookiecutter.project_class_name}}Repository
}

func New{{cookiecutter.project_class_name}}Service(repository repositories.{{cookiecutter.project_class_name}}Repository) {{cookiecutter.project_class_name}}Service {
	return &{{cookiecutter.project_lower_camel_name}}Service{repository: repository}
}

func (s *{{cookiecutter.project_lower_camel_name}}Service) Get(ctx context.Context, id string) (*models.{{cookiecutter.project_class_name}}Dto, error) {
	return s.repository.Get(ctx, id)
}

func (s *{{cookiecutter.project_lower_camel_name}}Service) GetList(ctx context.Context) ([]models.{{cookiecutter.project_class_name}}Dto, error) {
	return s.repository.GetList(ctx)
}

func (s *{{cookiecutter.project_lower_camel_name}}Service) Create(ctx context.Context, req models.Create{{cookiecutter.project_class_name}}Request) (*models.{{cookiecutter.project_class_name}}Dto, error) {
	now := time.Now().UTC()
	new{{cookiecutter.project_class_name}} := models.{{cookiecutter.project_class_name}}{
		BaseEntity: models.BaseEntity{
			Id:              uuid.New().String(),
			CreatedBy:       req.CreatedBy,
			UpdatedBy:       req.UpdatedBy,
			CreatedTimestamp: now,
			UpdatedTimestamp: now,
		},
		Name: req.Name,
	}
	return s.repository.Create(ctx, new{{cookiecutter.project_class_name}})
}

func (s *{{cookiecutter.project_lower_camel_name}}Service) Update(ctx context.Context, req models.Update{{cookiecutter.project_class_name}}Request) (*models.{{cookiecutter.project_class_name}}Dto, error) {
	updated{{cookiecutter.project_class_name}} := models.{{cookiecutter.project_class_name}}{
		BaseEntity: models.BaseEntity{
			Id:        req.Id,
			UpdatedBy: req.UpdatedBy,
		},
		Name: req.Name,
	}
	return s.repository.Update(ctx, updated{{cookiecutter.project_class_name}})
}

func (s *{{cookiecutter.project_lower_camel_name}}Service) Delete(ctx context.Context, id string) error {
	return s.repository.Delete(ctx, id)
}
