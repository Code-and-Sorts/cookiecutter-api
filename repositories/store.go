package repositories

import (
	"context"

	"cloud.google.com/go/firestore"
	"google.golang.org/api/iterator"
	"google.golang.org/grpc/codes"
	"google.golang.org/grpc/status"

	"kittenclaws/models"
)

type Container = *firestore.CollectionRef

type record[T any] interface {
	*T
	Base() *models.BaseEntity
}

type store[T any, P record[T]] struct {
	resource  string
	container Container
}

func newStore[T any, P record[T]](resource string, container Container) *store[T, P] {
	return &store[T, P]{resource: resource, container: container}
}

func (s *store[T, P]) get(ctx context.Context, id string) (*T, error) {
	item := new(T)
	doc, err := s.container.Doc(id).Get(ctx)
	if status.Code(err) == codes.NotFound {
		return nil, models.NewNotFoundError(s.resource, id)
	}
	if err != nil {
		return nil, err
	}
	if err := doc.DataTo(item); err != nil {
		return nil, err
	}

	if P(item).Base().IsDeleted {
		return nil, models.NewNotFoundError(s.resource, id)
	}
	return item, nil
}

func (s *store[T, P]) write(ctx context.Context, item *T, create bool) error {
	doc := s.container.Doc(P(item).Base().Id)
	var err error
	if create {
		_, err = doc.Create(ctx, item)
	} else {
		_, err = doc.Set(ctx, item)
	}
	return err
}

func (s *store[T, P]) list(ctx context.Context, limit int) ([]T, error) {
	results := make([]T, 0)
	iter := s.container.Where("isDeleted", "==", false).Limit(limit).Documents(ctx)
	defer iter.Stop()

	for {
		doc, err := iter.Next()
		if err == iterator.Done {
			break
		}
		if err != nil {
			return nil, err
		}

		var item T
		if err := doc.DataTo(&item); err != nil {
			return nil, err
		}
		results = append(results, item)
	}

	if len(results) > limit {
		results = results[:limit]
	}
	return results, nil
}

func (s *store[T, P]) create(ctx context.Context, item *T, userID string) (*T, error) {
	P(item).Base().StampCreate(userID)
	if err := s.write(ctx, item, true); err != nil {
		return nil, err
	}
	return item, nil
}

func (s *store[T, P]) update(ctx context.Context, id, userID string, merge func(stored *T)) (*T, error) {
	stored, err := s.get(ctx, id)
	if err != nil {
		return nil, err
	}

	merge(stored)
	P(stored).Base().StampWrite(userID)

	if err := s.write(ctx, stored, false); err != nil {
		return nil, err
	}
	return stored, nil
}

func (s *store[T, P]) replace(ctx context.Context, replacement *T, userID string) (*T, error) {
	base := P(replacement).Base()
	current, err := s.get(ctx, base.Id)
	if err != nil {
		return nil, err
	}

	kept := P(current).Base()
	*base = models.BaseEntity{
		Id:               kept.Id,
		CreatedTimestamp: kept.CreatedTimestamp,
		CreatedBy:        kept.CreatedBy,
	}
	base.StampWrite(userID)

	if err := s.write(ctx, replacement, false); err != nil {
		return nil, err
	}
	return replacement, nil
}

func (s *store[T, P]) softDelete(ctx context.Context, id, userID string) error {
	item, err := s.get(ctx, id)
	if err != nil {
		return err
	}

	base := P(item).Base()
	base.IsDeleted = true
	base.StampWrite(userID)
	return s.write(ctx, item, false)
}

func toDto[T, D any](mapping func(T) D) func(*T, error) (*D, error) {
	return func(item *T, err error) (*D, error) {
		if err != nil {
			return nil, err
		}
		dto := mapping(*item)
		return &dto, nil
	}
}

func toDtos[T, D any](mapping func(T) D) func([]T, error) ([]D, error) {
	return func(items []T, err error) ([]D, error) {
		if err != nil {
			return nil, err
		}
		dtos := make([]D, len(items))
		for i, item := range items {
			dtos[i] = mapping(item)
		}
		return dtos, nil
	}
}
