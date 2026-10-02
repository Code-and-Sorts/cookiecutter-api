package repositories

import (
	"context"

	"kittenclaws/models"
)

type CatRepository interface {
	Get(ctx context.Context, id string) (*models.CatDto, error)
	GetList(ctx context.Context, limit int) ([]models.CatDto, error)
	Create(ctx context.Context, item models.Cat, userID string) (*models.CatDto, error)
	Update(ctx context.Context, item models.Cat, userID string) (*models.CatDto, error)
	Delete(ctx context.Context, id, userID string) error
}

type catRepository struct {
	store *store[models.Cat, *models.Cat]
}

func NewCatRepository(container Container) CatRepository {
	return &catRepository{store: newStore[models.Cat]("Cat", container)}
}

func (repo *catRepository) Get(ctx context.Context, id string) (*models.CatDto, error) {
	return toDto(models.ToCatDto)(repo.store.get(ctx, id))
}

func (repo *catRepository) GetList(ctx context.Context, limit int) ([]models.CatDto, error) {
	return toDtos(models.ToCatDto)(repo.store.list(ctx, limit))
}

func (repo *catRepository) Create(ctx context.Context, item models.Cat, userID string) (*models.CatDto, error) {
	return toDto(models.ToCatDto)(repo.store.create(ctx, &item, userID))
}

func (repo *catRepository) Update(ctx context.Context, item models.Cat, userID string) (*models.CatDto, error) {
	return toDto(models.ToCatDto)(repo.store.update(ctx, item.Id, userID, func(stored *models.Cat) {
		if item.Name != "" {
			stored.Name = item.Name
		}
	}))
}

func (repo *catRepository) Delete(ctx context.Context, id, userID string) error {
	return repo.store.softDelete(ctx, id, userID)
}
