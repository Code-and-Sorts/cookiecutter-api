package services

import (
	"context"

	"kittenclaws/models"
	"kittenclaws/repositories"
)

type KittenClawsService interface {
	Get(ctx context.Context, id string) (*models.KittenClawsDto, error)
	GetList(ctx context.Context, limit int) ([]models.KittenClawsDto, error)
	Create(ctx context.Context, req models.CreateKittenClawsRequest, userID string) (*models.KittenClawsDto, error)
	Update(ctx context.Context, req models.UpdateKittenClawsRequest, userID string) (*models.KittenClawsDto, error)
	Delete(ctx context.Context, id, userID string) error
}

type kittenClawsService struct {
	repository repositories.KittenClawsRepository
}

func NewKittenClawsService(repository repositories.KittenClawsRepository) KittenClawsService {
	return &kittenClawsService{repository: repository}
}

func (s *kittenClawsService) Get(ctx context.Context, id string) (*models.KittenClawsDto, error) {
	return s.repository.Get(ctx, id)
}

func (s *kittenClawsService) GetList(ctx context.Context, limit int) ([]models.KittenClawsDto, error) {
	return s.repository.GetList(ctx, limit)
}

func (s *kittenClawsService) Create(ctx context.Context, req models.CreateKittenClawsRequest, userID string) (*models.KittenClawsDto, error) {
	return s.repository.Create(ctx, models.KittenClaws{BaseEntity: models.NewBaseEntity(), Name: req.Name}, userID)
}

func (s *kittenClawsService) Update(ctx context.Context, req models.UpdateKittenClawsRequest, userID string) (*models.KittenClawsDto, error) {
	updatedKittenClaws := models.KittenClaws{
		BaseEntity: models.BaseEntity{Id: req.Id},
		Name:       req.Name,
	}
	return s.repository.Update(ctx, updatedKittenClaws, userID)
}

func (s *kittenClawsService) Delete(ctx context.Context, id, userID string) error {
	return s.repository.Delete(ctx, id, userID)
}
