package repositories

import (
	"context"
	"encoding/json"
	"fmt"

	"github.com/Azure/azure-sdk-for-go/sdk/data/azcosmos"

	"kittenclaws/models"
)

type Container = *azcosmos.ContainerClient

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
	resp, err := s.container.ReadItem(ctx, azcosmos.NewPartitionKeyString(id), id, nil)
	if IsItemNotFound(err) {
		return nil, models.NewNotFoundError(s.resource, id)
	}
	if err != nil {
		return nil, err
	}
	if err := json.Unmarshal(resp.Value, item); err != nil {
		return nil, err
	}

	if P(item).Base().IsDeleted {
		return nil, models.NewNotFoundError(s.resource, id)
	}
	return item, nil
}

// The SDK returns no body unless EnableContentResponseOnWrite is set, so callers respond with the item they wrote.
func (s *store[T, P]) write(ctx context.Context, item *T, create bool) error {
	data, err := json.Marshal(item)
	if err != nil {
		return err
	}

	id := P(item).Base().Id
	pk := azcosmos.NewPartitionKeyString(id)
	if create {
		_, err = s.container.CreateItem(ctx, pk, data, nil)
	} else {
		_, err = s.container.ReplaceItem(ctx, pk, id, data, nil)
	}
	return err
}

func (s *store[T, P]) list(ctx context.Context, limit int) ([]T, error) {
	results := make([]T, 0)
	query := fmt.Sprintf("SELECT * FROM c WHERE c.isDeleted = false OFFSET 0 LIMIT %d", limit)
	pager := s.container.NewQueryItemsPager(query, azcosmos.NewPartitionKey(), nil)

	for pager.More() && len(results) < limit {
		resp, err := pager.NextPage(ctx)
		if err != nil {
			return nil, err
		}

		for _, data := range resp.Items {
			var item T
			if err := json.Unmarshal(data, &item); err != nil {
				return nil, err
			}
			results = append(results, item)
		}
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
