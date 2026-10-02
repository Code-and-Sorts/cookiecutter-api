package services

import (
	"context"

	"kittenclaws/models"
	"kittenclaws/repositories"
)

type CatService interface {
	Get(ctx context.Context, id string) (*models.CatDto, error)
	GetList(ctx context.Context, limit int) ([]models.CatDto, error)
	Create(ctx context.Context, req models.CreateCatRequest, userID string) (*models.CatDto, error)
	Update(ctx context.Context, req models.UpdateCatRequest, userID string) (*models.CatDto, error)
	Delete(ctx context.Context, id, userID string) error
}

type catService struct {
	repository repositories.CatRepository
}

func NewCatService(repository repositories.CatRepository) CatService {
	return &catService{repository: repository}
}

func (s *catService) Get(ctx context.Context, id string) (*models.CatDto, error) {
	return s.repository.Get(ctx, id)
}

func (s *catService) GetList(ctx context.Context, limit int) ([]models.CatDto, error) {
	return s.repository.GetList(ctx, limit)
}

func (s *catService) Create(ctx context.Context, req models.CreateCatRequest, userID string) (*models.CatDto, error) {
	return s.repository.Create(ctx, models.Cat{BaseEntity: models.NewBaseEntity(), Name: req.Name}, userID)
}

func (s *catService) Update(ctx context.Context, req models.UpdateCatRequest, userID string) (*models.CatDto, error) {
	updatedCat := models.Cat{
		BaseEntity: models.BaseEntity{Id: req.Id},
		Name:       req.Name,
	}
	return s.repository.Update(ctx, updatedCat, userID)
}

func (s *catService) Delete(ctx context.Context, id, userID string) error {
	return s.repository.Delete(ctx, id, userID)
}
