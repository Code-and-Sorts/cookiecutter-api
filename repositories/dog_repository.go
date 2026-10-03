package repositories

import (
	"context"

	"kittenclaws/models"
)

type DogRepository interface {
	Get(ctx context.Context, id string) (*models.DogDto, error)
	GetList(ctx context.Context, limit int) ([]models.DogDto, error)
	Create(ctx context.Context, item models.Dog, userID string) (*models.DogDto, error)
	Replace(ctx context.Context, item models.Dog, userID string) (*models.DogDto, error)
	Delete(ctx context.Context, id, userID string) error
}

type dogRepository struct {
	store *store[models.Dog, *models.Dog]
}

func NewDogRepository(container Container) DogRepository {
	return &dogRepository{store: newStore[models.Dog]("Dog", container)}
}

func (repo *dogRepository) Get(ctx context.Context, id string) (*models.DogDto, error) {
	return toDto(models.ToDogDto)(repo.store.get(ctx, id))
}

func (repo *dogRepository) GetList(ctx context.Context, limit int) ([]models.DogDto, error) {
	return toDtos(models.ToDogDto)(repo.store.list(ctx, limit))
}

func (repo *dogRepository) Create(ctx context.Context, item models.Dog, userID string) (*models.DogDto, error) {
	return toDto(models.ToDogDto)(repo.store.create(ctx, &item, userID))
}

func (repo *dogRepository) Replace(ctx context.Context, item models.Dog, userID string) (*models.DogDto, error) {
	return toDto(models.ToDogDto)(repo.store.replace(ctx, &item, userID))
}

func (repo *dogRepository) Delete(ctx context.Context, id, userID string) error {
	return repo.store.softDelete(ctx, id, userID)
}
