package services

import (
	"context"
	"time"

	"github.com/google/uuid"

	"kitties/models"
	"kitties/repositories"
)

type KittenClawsService interface {
	Get(ctx context.Context, id string) (*models.KittenClawsDto, error)
	GetList(ctx context.Context, limit int) ([]models.KittenClawsDto, error)
	Create(ctx context.Context, req models.CreateKittenClawsRequest) (*models.KittenClawsDto, error)
	Update(ctx context.Context, req models.UpdateKittenClawsRequest) (*models.KittenClawsDto, error)
	Delete(ctx context.Context, id string) error
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

func (s *kittenClawsService) Create(ctx context.Context, req models.CreateKittenClawsRequest) (*models.KittenClawsDto, error) {
	now := time.Now().UTC()
	newKittenClaws := models.KittenClaws{
		BaseEntity: models.BaseEntity{
			Id:               uuid.New().String(),
			CreatedBy:        req.CreatedBy,
			UpdatedBy:        req.UpdatedBy,
			CreatedTimestamp: now,
			UpdatedTimestamp: now,
		},
		Name: req.Name,
	}
	return s.repository.Create(ctx, newKittenClaws)
}

func (s *kittenClawsService) Update(ctx context.Context, req models.UpdateKittenClawsRequest) (*models.KittenClawsDto, error) {
	updatedKittenClaws := models.KittenClaws{
		BaseEntity: models.BaseEntity{
			Id:        req.Id,
			UpdatedBy: req.UpdatedBy,
		},
		Name: req.Name,
	}
	return s.repository.Update(ctx, updatedKittenClaws)
}

func (s *kittenClawsService) Delete(ctx context.Context, id string) error {
	return s.repository.Delete(ctx, id)
}
