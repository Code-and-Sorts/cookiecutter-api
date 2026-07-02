package services

import (
	"context"
	"time"

	"github.com/google/uuid"

	"{{project_endpoint}}/models"
	"{{project_endpoint}}/repositories"
)

type ItemService interface {
	Get(ctx context.Context, id string) (*models.ItemDto, error)
	GetList(ctx context.Context, limit int) ([]models.ItemDto, error)
	Create(ctx context.Context, req models.CreateItemRequest) (*models.ItemDto, error)
	Update(ctx context.Context, req models.UpdateItemRequest) (*models.ItemDto, error)
	Replace(ctx context.Context, req models.ReplaceItemRequest) (*models.ItemDto, error)
	Delete(ctx context.Context, id string) error
}

type itemService struct {
	repository repositories.ItemRepository
}

func NewItemService(repository repositories.ItemRepository) ItemService {
	return &itemService{repository: repository}
}

func (s *itemService) Get(ctx context.Context, id string) (*models.ItemDto, error) {
	return s.repository.Get(ctx, id)
}

func (s *itemService) GetList(ctx context.Context, limit int) ([]models.ItemDto, error) {
	return s.repository.GetList(ctx, limit)
}

func (s *itemService) Create(ctx context.Context, req models.CreateItemRequest) (*models.ItemDto, error) {
	now := time.Now().UTC()
	newItem := models.Item{
		BaseEntity: models.BaseEntity{
			Id:               uuid.New().String(),
			CreatedBy:        req.CreatedBy,
			UpdatedBy:        req.UpdatedBy,
			CreatedTimestamp: now,
			UpdatedTimestamp: now,
		},
		Name: req.Name,
	}
	return s.repository.Create(ctx, newItem)
}

func (s *itemService) Update(ctx context.Context, req models.UpdateItemRequest) (*models.ItemDto, error) {
	updatedItem := models.Item{
		BaseEntity: models.BaseEntity{
			Id:        req.Id,
			UpdatedBy: req.UpdatedBy,
		},
		Name: req.Name,
	}
	return s.repository.Update(ctx, updatedItem)
}

func (s *itemService) Replace(ctx context.Context, req models.ReplaceItemRequest) (*models.ItemDto, error) {
	replacementItem := models.Item{
		BaseEntity: models.BaseEntity{
			Id:        req.Id,
			UpdatedBy: req.UpdatedBy,
		},
		Name: req.Name,
	}
	return s.repository.Replace(ctx, replacementItem)
}

func (s *itemService) Delete(ctx context.Context, id string) error {
	return s.repository.Delete(ctx, id)
}
