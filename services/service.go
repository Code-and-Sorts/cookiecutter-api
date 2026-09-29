package services

import (
	"context"
	"time"

	"github.com/google/uuid"

	"kitties/models"
	"kitties/repositories"
)

type CatService interface {
	Get(ctx context.Context, id string) (*models.CatDto, error)
	GetList(ctx context.Context, limit int) ([]models.CatDto, error)
	Create(ctx context.Context, req models.CreateCatRequest) (*models.CatDto, error)
	Update(ctx context.Context, req models.UpdateCatRequest) (*models.CatDto, error)
	Delete(ctx context.Context, id string) error
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

func (s *catService) Create(ctx context.Context, req models.CreateCatRequest) (*models.CatDto, error) {
	now := time.Now().UTC()
	newCat := models.Cat{
		BaseEntity: models.BaseEntity{
			Id:               uuid.New().String(),
			CreatedBy:        req.CreatedBy,
			UpdatedBy:        req.UpdatedBy,
			CreatedTimestamp: now,
			UpdatedTimestamp: now,
		},
		Name: req.Name,
	}
	return s.repository.Create(ctx, newCat)
}

func (s *catService) Update(ctx context.Context, req models.UpdateCatRequest) (*models.CatDto, error) {
	updatedCat := models.Cat{
		BaseEntity: models.BaseEntity{
			Id:        req.Id,
			UpdatedBy: req.UpdatedBy,
		},
		Name: req.Name,
	}
	return s.repository.Update(ctx, updatedCat)
}

func (s *catService) Delete(ctx context.Context, id string) error {
	return s.repository.Delete(ctx, id)
}

type DogService interface {
	Get(ctx context.Context, id string) (*models.DogDto, error)
	GetList(ctx context.Context, limit int) ([]models.DogDto, error)
	Create(ctx context.Context, req models.CreateDogRequest) (*models.DogDto, error)
	Replace(ctx context.Context, req models.ReplaceDogRequest) (*models.DogDto, error)
	Delete(ctx context.Context, id string) error
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

func (s *dogService) Create(ctx context.Context, req models.CreateDogRequest) (*models.DogDto, error) {
	now := time.Now().UTC()
	newDog := models.Dog{
		BaseEntity: models.BaseEntity{
			Id:               uuid.New().String(),
			CreatedBy:        req.CreatedBy,
			UpdatedBy:        req.UpdatedBy,
			CreatedTimestamp: now,
			UpdatedTimestamp: now,
		},
		Name: req.Name,
	}
	return s.repository.Create(ctx, newDog)
}

func (s *dogService) Replace(ctx context.Context, req models.ReplaceDogRequest) (*models.DogDto, error) {
	replacementDog := models.Dog{
		BaseEntity: models.BaseEntity{
			Id:        req.Id,
			UpdatedBy: req.UpdatedBy,
		},
		Name: req.Name,
	}
	return s.repository.Replace(ctx, replacementDog)
}

func (s *dogService) Delete(ctx context.Context, id string) error {
	return s.repository.Delete(ctx, id)
}
