package repositories

import (
	"bytes"
	"context"
	"encoding/json"
	"fmt"
	"reflect"
	"strings"

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

// What writing a record back needs from reading it: the whole document, which may hold fields other resources in
// the container store, and its ETag, so a write that races another fails instead of losing it.
type document struct {
	fields map[string]any
	etag   azcore.ETag
}

func (s *store[T, P]) get(ctx context.Context, id string) (*T, error) {
	item, _, err := s.load(ctx, id)
	return item, err
}

func (s *store[T, P]) load(ctx context.Context, id string) (*T, document, error) {
	item := new(T)
	var doc document
	resp, err := s.container.ReadItem(ctx, azcosmos.NewPartitionKeyString(id), id, nil)
	if IsItemNotFound(err) {
		return nil, doc, models.NewNotFoundError(s.resource, id)
	}
	if err != nil {
		return nil, doc, err
	}
	if err := json.Unmarshal(resp.Value, item); err != nil {
		return nil, doc, err
	}
	decoder := json.NewDecoder(bytes.NewReader(resp.Value))
	decoder.UseNumber()
	if err := decoder.Decode(&doc.fields); err != nil {
		return nil, doc, err
	}
	doc.etag = resp.ETag

	if P(item).Base().IsDeleted {
		return nil, doc, models.NewNotFoundError(s.resource, id)
	}
	return item, doc, nil
}

// The SDK returns no body unless EnableContentResponseOnWrite is set, so callers respond with the item they wrote.
func (s *store[T, P]) insert(ctx context.Context, item *T) error {
	data, err := json.Marshal(item)
	if err != nil {
		return err
	}
	_, err = s.container.CreateItem(ctx, azcosmos.NewPartitionKeyString(P(item).Base().Id), data, nil)
	return err
}

type storedField struct {
	name  string
	value any
}

// A record's stored fields by name; a field that omitempty leaves out has a nil value, so a write removes it.
func storedFields(record reflect.Value) []storedField {
	var fields []storedField
	for i := range record.NumField() {
		field, info := record.Field(i), record.Type().Field(i)
		if info.Anonymous {
			fields = append(fields, storedFields(field)...)
			continue
		}
		name, options, _ := strings.Cut(info.Tag.Get("json"), ",")
		if name == "" || name == "-" {
			continue
		}
		var value any
		if !strings.Contains(options, "omitempty") || !field.IsZero() {
			value = field.Interface()
		}
		fields = append(fields, storedField{name: name, value: value})
	}
	return fields
}

func (s *store[T, P]) save(ctx context.Context, item *T, doc document) error {
	for _, field := range storedFields(reflect.ValueOf(item).Elem()) {
		if field.value == nil {
			delete(doc.fields, field.name)
		} else {
			doc.fields[field.name] = field.value
		}
	}
	data, err := json.Marshal(doc.fields)
	if err != nil {
		return err
	}
	id := P(item).Base().Id
	_, err = s.container.ReplaceItem(ctx, azcosmos.NewPartitionKeyString(id), id, data, &azcosmos.ItemOptions{IfMatchEtag: &doc.etag})
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
	if err := s.insert(ctx, item); err != nil {
		return nil, err
	}
	return item, nil
}

func (s *store[T, P]) update(ctx context.Context, id, userID string, merge func(stored *T)) (*T, error) {
	stored, doc, err := s.load(ctx, id)
	if err != nil {
		return nil, err
	}

	merge(stored)
	P(stored).Base().StampWrite(userID)

	if err := s.save(ctx, stored, doc); err != nil {
		return nil, err
	}
	return stored, nil
}

func (s *store[T, P]) softDelete(ctx context.Context, id, userID string) error {
	item, doc, err := s.load(ctx, id)
	if err != nil {
		return err
	}

	base := P(item).Base()
	base.IsDeleted = true
	base.StampWrite(userID)
	return s.save(ctx, item, doc)
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
