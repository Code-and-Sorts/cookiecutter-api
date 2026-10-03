package repositories

import (
	"context"
	"reflect"
	"strings"

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

type document struct{}

func (s *store[T, P]) get(ctx context.Context, id string) (*T, error) {
	item, _, err := s.load(ctx, id)
	return item, err
}

func (s *store[T, P]) load(ctx context.Context, id string) (*T, document, error) {
	item := new(T)
	var doc document
	snapshot, err := s.container.Doc(id).Get(ctx)
	if status.Code(err) == codes.NotFound {
		return nil, doc, models.NewNotFoundError(s.resource, id)
	}
	if err != nil {
		return nil, doc, err
	}
	if err := snapshot.DataTo(item); err != nil {
		return nil, doc, err
	}
	keepEmptyLists(item)

	if P(item).Base().IsDeleted {
		return nil, doc, models.NewNotFoundError(s.resource, id)
	}
	return item, doc, nil
}

// Firestore reads a stored empty array into a nil slice, which would read as missing and be written back as null.
func keepEmptyLists(record any) {
	value := reflect.ValueOf(record).Elem()
	for i := range value.NumField() {
		field := value.Field(i)
		switch {
		case field.Kind() == reflect.Struct:
			keepEmptyLists(field.Addr().Interface())
		case field.Kind() == reflect.Pointer && !field.IsNil() && field.Elem().Kind() == reflect.Slice && field.Elem().IsNil():
			field.Elem().Set(reflect.MakeSlice(field.Elem().Type(), 0, 0))
		}
	}
}

func (s *store[T, P]) insert(ctx context.Context, item *T) error {
	_, err := s.container.Doc(P(item).Base().Id).Create(ctx, item)
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

// Resources sharing the collection may store fields this type does not know, so the write updates only its own.
func (s *store[T, P]) save(ctx context.Context, item *T, _ document) error {
	var updates []firestore.Update
	for _, field := range storedFields(reflect.ValueOf(item).Elem()) {
		value := field.value
		if value == nil {
			value = firestore.Delete
		}
		updates = append(updates, firestore.Update{Path: field.name, Value: value})
	}
	_, err := s.container.Doc(P(item).Base().Id).Update(ctx, updates)
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
		keepEmptyLists(&item)
		results = append(results, item)
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
