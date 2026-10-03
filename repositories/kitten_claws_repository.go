package repositories

import (
	"context"

	"kittenclaws/models"
)

type KittenClawsRepository interface {
	Get(ctx context.Context, id string) (*models.KittenClawsDto, error)
	GetList(ctx context.Context, limit int) ([]models.KittenClawsDto, error)
	Create(ctx context.Context, item models.KittenClaws, userID string) (*models.KittenClawsDto, error)
	Update(ctx context.Context, id, userID string, apply func(*models.KittenClaws)) (*models.KittenClawsDto, error)
	Delete(ctx context.Context, id, userID string) error
}

type kittenClawsRepository struct {
	store *store[models.KittenClaws, *models.KittenClaws]
}

func NewKittenClawsRepository(container Container) KittenClawsRepository {
	return &kittenClawsRepository{store: newStore[models.KittenClaws]("KittenClaws", container)}
}

func (repo *kittenClawsRepository) Get(ctx context.Context, id string) (*models.KittenClawsDto, error) {
	return toDto(models.ToKittenClawsDto)(repo.store.get(ctx, id))
}

func (repo *kittenClawsRepository) GetList(ctx context.Context, limit int) ([]models.KittenClawsDto, error) {
	return toDtos(models.ToKittenClawsDto)(repo.store.list(ctx, limit))
}

func (repo *kittenClawsRepository) Create(ctx context.Context, item models.KittenClaws, userID string) (*models.KittenClawsDto, error) {
	return toDto(models.ToKittenClawsDto)(repo.store.create(ctx, &item, userID))
}

func (repo *kittenClawsRepository) Update(ctx context.Context, id, userID string, apply func(*models.KittenClaws)) (*models.KittenClawsDto, error) {
	return toDto(models.ToKittenClawsDto)(repo.store.update(ctx, id, userID, apply))
}

func (repo *kittenClawsRepository) Delete(ctx context.Context, id, userID string) error {
	return repo.store.softDelete(ctx, id, userID)
}
