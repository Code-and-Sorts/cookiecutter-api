package services

import (
	"context"

	"kittenclaws/models"
	"kittenclaws/repositories"
)

type DogService interface {
	Get(ctx context.Context, id string) (*models.DogDto, error)
	GetList(ctx context.Context, limit int) ([]models.DogDto, error)
	Create(ctx context.Context, req models.CreateDogRequest, userID string) (*models.DogDto, error)
	Replace(ctx context.Context, req models.ReplaceDogRequest, userID string) (*models.DogDto, error)
	Delete(ctx context.Context, id, userID string) error
}

type dogService struct {
	repository repositories.DogRepository
}

func NewDogService(repository repositories.DogRepository) DogService {
	return &dogService{repository: repository}
}

func (s *dogService) Get(ctx context.Context, id string) (*models.DogDto, error) {
	return s.repository.Get(ctx, id)
}

func (s *dogService) GetList(ctx context.Context, limit int) ([]models.DogDto, error) {
	return s.repository.GetList(ctx, limit)
}

func (s *dogService) Create(ctx context.Context, req models.CreateDogRequest, userID string) (*models.DogDto, error) {
	return s.repository.Create(ctx, models.Dog{BaseEntity: models.NewBaseEntity(), Name: req.Name}, userID)
}

func (s *dogService) Replace(ctx context.Context, req models.ReplaceDogRequest, userID string) (*models.DogDto, error) {
	replacementDog := models.Dog{
		BaseEntity: models.BaseEntity{Id: req.Id},
		Name:       req.Name,
	}
	return s.repository.Replace(ctx, replacementDog, userID)
}

func (s *dogService) Delete(ctx context.Context, id, userID string) error {
	return s.repository.Delete(ctx, id, userID)
}
