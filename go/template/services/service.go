{%- set used_ops = resources | map(attribute='operations') | sum(start=[]) -%}
{%- set uses_body = ('create' in used_ops) or ('update' in used_ops) or ('replace' in used_ops) -%}
{%- set uses_models = uses_body or ('get_by_id' in used_ops) or ('list' in used_ops) -%}
package services

import (
	"context"
{%- if 'create' in used_ops %}
	"time"

	"github.com/google/uuid"
{%- endif %}
{%- if uses_models %}

	"{{project_endpoint}}/models"
{%- endif %}
	"{{project_endpoint}}/repositories"
)
{% for resource in resources %}
{%- set r = resource.name %}
{%- set lc = resource.name | to_lower_camel %}
type {{ r }}Service interface {
{%- if "get_by_id" in resource.operations %}
	Get(ctx context.Context, id string) (*models.{{ r }}Dto, error)
{%- endif %}
{%- if "list" in resource.operations %}
	GetList(ctx context.Context, limit int) ([]models.{{ r }}Dto, error)
{%- endif %}
{%- if "create" in resource.operations %}
	Create(ctx context.Context, req models.Create{{ r }}Request) (*models.{{ r }}Dto, error)
{%- endif %}
{%- if "update" in resource.operations %}
	Update(ctx context.Context, req models.Update{{ r }}Request) (*models.{{ r }}Dto, error)
{%- endif %}
{%- if "replace" in resource.operations %}
	Replace(ctx context.Context, req models.Replace{{ r }}Request) (*models.{{ r }}Dto, error)
{%- endif %}
{%- if "delete" in resource.operations %}
	Delete(ctx context.Context, id string) error
{%- endif %}
}

type {{ lc }}Service struct {
	repository repositories.{{ r }}Repository
}

func New{{ r }}Service(repository repositories.{{ r }}Repository) {{ r }}Service {
	return &{{ lc }}Service{repository: repository}
}
{%- if "get_by_id" in resource.operations %}

func (s *{{ lc }}Service) Get(ctx context.Context, id string) (*models.{{ r }}Dto, error) {
	return s.repository.Get(ctx, id)
}
{%- endif %}
{%- if "list" in resource.operations %}

func (s *{{ lc }}Service) GetList(ctx context.Context, limit int) ([]models.{{ r }}Dto, error) {
	return s.repository.GetList(ctx, limit)
}
{%- endif %}
{%- if "create" in resource.operations %}

func (s *{{ lc }}Service) Create(ctx context.Context, req models.Create{{ r }}Request) (*models.{{ r }}Dto, error) {
	now := time.Now().UTC()
	new{{ r }} := models.{{ r }}{
		BaseEntity: models.BaseEntity{
			Id:               uuid.New().String(),
			CreatedBy:        req.CreatedBy,
			UpdatedBy:        req.UpdatedBy,
			CreatedTimestamp: now,
			UpdatedTimestamp: now,
		},
		Name: req.Name,
	}
	return s.repository.Create(ctx, new{{ r }})
}
{%- endif %}
{%- if "update" in resource.operations %}

func (s *{{ lc }}Service) Update(ctx context.Context, req models.Update{{ r }}Request) (*models.{{ r }}Dto, error) {
	updated{{ r }} := models.{{ r }}{
		BaseEntity: models.BaseEntity{
			Id:        req.Id,
			UpdatedBy: req.UpdatedBy,
		},
		Name: req.Name,
	}
	return s.repository.Update(ctx, updated{{ r }})
}
{%- endif %}
{%- if "replace" in resource.operations %}

func (s *{{ lc }}Service) Replace(ctx context.Context, req models.Replace{{ r }}Request) (*models.{{ r }}Dto, error) {
	replacement{{ r }} := models.{{ r }}{
		BaseEntity: models.BaseEntity{
			Id:        req.Id,
			UpdatedBy: req.UpdatedBy,
		},
		Name: req.Name,
	}
	return s.repository.Replace(ctx, replacement{{ r }})
}
{%- endif %}
{%- if "delete" in resource.operations %}

func (s *{{ lc }}Service) Delete(ctx context.Context, id string) error {
	return s.repository.Delete(ctx, id)
}
{%- endif %}
{% endfor %}