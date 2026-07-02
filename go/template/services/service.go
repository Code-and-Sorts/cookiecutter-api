package services

import (
	"context"
	"time"

	"github.com/google/uuid"

	"{{project_endpoint}}/models"
	"{{project_endpoint}}/repositories"
)
{% for resource in resources %}
type {{ resource.name }}Service interface {
	Get(ctx context.Context, id string) (*models.{{ resource.name }}Dto, error)
	GetList(ctx context.Context, limit int) ([]models.{{ resource.name }}Dto, error)
	Create(ctx context.Context, req models.Create{{ resource.name }}Request) (*models.{{ resource.name }}Dto, error)
	Update(ctx context.Context, req models.Update{{ resource.name }}Request) (*models.{{ resource.name }}Dto, error)
	Replace(ctx context.Context, req models.Replace{{ resource.name }}Request) (*models.{{ resource.name }}Dto, error)
	Delete(ctx context.Context, id string) error
}

type {{ resource.name | to_lower_camel }}Service struct {
	repository repositories.{{ resource.name }}Repository
}

func New{{ resource.name }}Service(repository repositories.{{ resource.name }}Repository) {{ resource.name }}Service {
	return &{{ resource.name | to_lower_camel }}Service{repository: repository}
}

func (s *{{ resource.name | to_lower_camel }}Service) Get(ctx context.Context, id string) (*models.{{ resource.name }}Dto, error) {
	return s.repository.Get(ctx, id)
}

func (s *{{ resource.name | to_lower_camel }}Service) GetList(ctx context.Context, limit int) ([]models.{{ resource.name }}Dto, error) {
	return s.repository.GetList(ctx, limit)
}

func (s *{{ resource.name | to_lower_camel }}Service) Create(ctx context.Context, req models.Create{{ resource.name }}Request) (*models.{{ resource.name }}Dto, error) {
	now := time.Now().UTC()
	new{{ resource.name }} := models.{{ resource.name }}{
		BaseEntity: models.BaseEntity{
			Id:               uuid.New().String(),
			CreatedBy:        req.CreatedBy,
			UpdatedBy:        req.UpdatedBy,
			CreatedTimestamp: now,
			UpdatedTimestamp: now,
		},
		Name: req.Name,
	}
	return s.repository.Create(ctx, new{{ resource.name }})
}

func (s *{{ resource.name | to_lower_camel }}Service) Update(ctx context.Context, req models.Update{{ resource.name }}Request) (*models.{{ resource.name }}Dto, error) {
	updated{{ resource.name }} := models.{{ resource.name }}{
		BaseEntity: models.BaseEntity{
			Id:        req.Id,
			UpdatedBy: req.UpdatedBy,
		},
		Name: req.Name,
	}
	return s.repository.Update(ctx, updated{{ resource.name }})
}

func (s *{{ resource.name | to_lower_camel }}Service) Replace(ctx context.Context, req models.Replace{{ resource.name }}Request) (*models.{{ resource.name }}Dto, error) {
	replacement{{ resource.name }} := models.{{ resource.name }}{
		BaseEntity: models.BaseEntity{
			Id:        req.Id,
			UpdatedBy: req.UpdatedBy,
		},
		Name: req.Name,
	}
	return s.repository.Replace(ctx, replacement{{ resource.name }})
}

func (s *{{ resource.name | to_lower_camel }}Service) Delete(ctx context.Context, id string) error {
	return s.repository.Delete(ctx, id)
}
{% endfor %}