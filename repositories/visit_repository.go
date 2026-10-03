package repositories

import (
	"context"

	"kittenclaws/models"
)

type VisitRepository interface {
	Get(ctx context.Context, id string) (*models.VisitDto, error)
	Create(ctx context.Context, item models.Visit, userID string) (*models.VisitDto, error)
	Update(ctx context.Context, id, userID string, apply func(*models.Visit)) (*models.VisitDto, error)
}

type visitRepository struct {
	store *store[models.Visit, *models.Visit]
}

func NewVisitRepository(container Container) VisitRepository {
	return &visitRepository{store: newStore[models.Visit]("Visit", container)}
}

func (repo *visitRepository) Get(ctx context.Context, id string) (*models.VisitDto, error) {
	return toDto(models.ToVisitDto)(repo.store.get(ctx, id))
}

func (repo *visitRepository) Create(ctx context.Context, item models.Visit, userID string) (*models.VisitDto, error) {
	return toDto(models.ToVisitDto)(repo.store.create(ctx, &item, userID))
}

func (repo *visitRepository) Update(ctx context.Context, id, userID string, apply func(*models.Visit)) (*models.VisitDto, error) {
	return toDto(models.ToVisitDto)(repo.store.update(ctx, id, userID, apply))
}
