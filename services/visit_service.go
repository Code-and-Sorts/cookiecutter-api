package services

import (
	"context"

	"kittenclaws/models"
	"kittenclaws/repositories"
)

type VisitService interface {
	Get(ctx context.Context, id string) (*models.VisitDto, error)
	Create(ctx context.Context, req models.CreateVisitRequest, userID string) (*models.VisitDto, error)
	Update(ctx context.Context, req models.UpdateVisitRequest, userID string) (*models.VisitDto, error)
}

type visitService struct {
	repository repositories.VisitRepository
}

func NewVisitService(repository repositories.VisitRepository) VisitService {
	return &visitService{repository: repository}
}

func (s *visitService) Get(ctx context.Context, id string) (*models.VisitDto, error) {
	return s.repository.Get(ctx, id)
}

func (s *visitService) Create(ctx context.Context, req models.CreateVisitRequest, userID string) (*models.VisitDto, error) {
	return s.repository.Create(ctx, req.ToEntity(), userID)
}

func (s *visitService) Update(ctx context.Context, req models.UpdateVisitRequest, userID string) (*models.VisitDto, error) {
	return s.repository.Update(ctx, req.Id, userID, req.ApplyTo)
}
