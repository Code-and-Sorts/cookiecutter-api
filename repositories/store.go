package repositories

import (
	"context"
	"encoding/json"
	"fmt"

	"github.com/Azure/azure-sdk-for-go/sdk/azcore"
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

type version = azcore.ETag

func (s *store[T, P]) get(ctx context.Context, id string) (*T, error) {
	item, _, err := s.read(ctx, id)
	return item, err
}

func (s *store[T, P]) read(ctx context.Context, id string) (*T, version, error) {
	item := new(T)
	var read version
	resp, err := s.container.ReadItem(ctx, azcosmos.NewPartitionKeyString(id), id, nil)
	if IsItemNotFound(err) {
		return nil, read, models.NewNotFoundError(s.resource, id)
	}
	if err != nil {
		return nil, read, err
	}
	if err := json.Unmarshal(resp.Value, item); err != nil {
		return nil, read, err
	}
	read = resp.ETag

	if P(item).Base().IsDeleted {
		return nil, read, models.NewNotFoundError(s.resource, id)
	}
	return item, read, nil
}

// The SDK returns no body unless EnableContentResponseOnWrite is set, so callers respond with the item they wrote.
func (s *store[T, P]) write(ctx context.Context, item *T, read *version) error {
	data, err := json.Marshal(item)
	if err != nil {
		return err
	}

	id := P(item).Base().Id
	pk := azcosmos.NewPartitionKeyString(id)
	if read == nil {
		_, err = s.container.CreateItem(ctx, pk, data, nil)
	} else {
		_, err = s.container.ReplaceItem(ctx, pk, id, data, &azcosmos.ItemOptions{IfMatchEtag: read})
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
	if err := s.write(ctx, item, nil); err != nil {
		return nil, err
	}
	return item, nil
}

func (s *store[T, P]) update(ctx context.Context, id, userID string, merge func(stored *T)) (*T, error) {
	stored, read, err := s.read(ctx, id)
	if err != nil {
		return nil, err
	}

	merge(stored)
	P(stored).Base().StampWrite(userID)

	if err := s.write(ctx, stored, &read); err != nil {
		return nil, err
	}
	return stored, nil
}

func (s *store[T, P]) replace(ctx context.Context, replacement *T, userID string) (*T, error) {
	base := P(replacement).Base()
	current, read, err := s.read(ctx, base.Id)
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

	if err := s.write(ctx, replacement, &read); err != nil {
		return nil, err
	}
	return replacement, nil
}

func (s *store[T, P]) softDelete(ctx context.Context, id, userID string) error {
	item, read, err := s.read(ctx, id)
	if err != nil {
		return err
	}

	base := P(item).Base()
	base.IsDeleted = true
	base.StampWrite(userID)
	return s.write(ctx, item, &read)
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
