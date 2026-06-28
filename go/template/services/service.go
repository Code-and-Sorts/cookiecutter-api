package services

import (
	"context"
	"time"

	"github.com/google/uuid"

	"{{project_endpoint}}/models"
	"{{project_endpoint}}/repositories"
)

type {{project_class_name}}Service interface {
	Get(ctx context.Context, id string) (*models.{{project_class_name}}Dto, error)
	GetList(ctx context.Context, limit int) ([]models.{{project_class_name}}Dto, error)
	Create(ctx context.Context, req models.Create{{project_class_name}}Request) (*models.{{project_class_name}}Dto, error)
	Update(ctx context.Context, req models.Update{{project_class_name}}Request) (*models.{{project_class_name}}Dto, error)
	Delete(ctx context.Context, id string) error
}

type {{project_lower_camel_name}}Service struct {
	repository repositories.{{project_class_name}}Repository
}

func New{{project_class_name}}Service(repository repositories.{{project_class_name}}Repository) {{project_class_name}}Service {
	return &{{project_lower_camel_name}}Service{repository: repository}
}

func (s *{{project_lower_camel_name}}Service) Get(ctx context.Context, id string) (*models.{{project_class_name}}Dto, error) {
	return s.repository.Get(ctx, id)
}

func (s *{{project_lower_camel_name}}Service) GetList(ctx context.Context, limit int) ([]models.{{project_class_name}}Dto, error) {
	return s.repository.GetList(ctx, limit)
}

func (s *{{project_lower_camel_name}}Service) Create(ctx context.Context, req models.Create{{project_class_name}}Request) (*models.{{project_class_name}}Dto, error) {
	now := time.Now().UTC()
	new{{project_class_name}} := models.{{project_class_name}}{
		BaseEntity: models.BaseEntity{
			Id:              uuid.New().String(),
			CreatedBy:       req.CreatedBy,
			UpdatedBy:       req.UpdatedBy,
			CreatedTimestamp: now,
			UpdatedTimestamp: now,
		},
		Name: req.Name,
	}
	return s.repository.Create(ctx, new{{project_class_name}})
}

func (s *{{project_lower_camel_name}}Service) Update(ctx context.Context, req models.Update{{project_class_name}}Request) (*models.{{project_class_name}}Dto, error) {
	updated{{project_class_name}} := models.{{project_class_name}}{
		BaseEntity: models.BaseEntity{
			Id:        req.Id,
			UpdatedBy: req.UpdatedBy,
		},
		Name: req.Name,
	}
	return s.repository.Update(ctx, updated{{project_class_name}})
}

func (s *{{project_lower_camel_name}}Service) Delete(ctx context.Context, id string) error {
	return s.repository.Delete(ctx, id)
}
