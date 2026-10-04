package repositories

import (
	"context"

	"github.com/aws/aws-sdk-go-v2/aws"
	"github.com/aws/aws-sdk-go-v2/feature/dynamodb/attributevalue"
	"github.com/aws/aws-sdk-go-v2/feature/dynamodb/expression"
	"github.com/aws/aws-sdk-go-v2/service/dynamodb"
	"github.com/aws/aws-sdk-go-v2/service/dynamodb/types"

	"kittenclaws/models"
)

type Container struct {
	Client    *dynamodb.Client
	TableName string
}

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

type version = struct{}

func (s *store[T, P]) get(ctx context.Context, id string) (*T, error) {
	item, _, err := s.read(ctx, id)
	return item, err
}

func (s *store[T, P]) read(ctx context.Context, id string) (*T, version, error) {
	item := new(T)
	var read version
	key, err := attributevalue.MarshalMap(map[string]string{"id": id})
	if err != nil {
		return nil, read, err
	}
	resp, err := s.container.Client.GetItem(ctx, &dynamodb.GetItemInput{
		TableName: aws.String(s.container.TableName),
		Key:       key,
	})
	if err != nil {
		return nil, read, err
	}
	if resp.Item == nil {
		return nil, read, models.NewNotFoundError(s.resource, id)
	}
	if err := attributevalue.UnmarshalMap(resp.Item, item); err != nil {
		return nil, read, err
	}

	if P(item).Base().IsDeleted {
		return nil, read, models.NewNotFoundError(s.resource, id)
	}
	return item, read, nil
}

func (s *store[T, P]) write(ctx context.Context, item *T, read *version) error {
	av, err := attributevalue.MarshalMap(item)
	if err != nil {
		return err
	}

	input := &dynamodb.PutItemInput{
		TableName: aws.String(s.container.TableName),
		Item:      av,
	}
	if read == nil {
		input.ConditionExpression = aws.String("attribute_not_exists(id)")
	}
	_, err = s.container.Client.PutItem(ctx, input)
	return err
}

func (s *store[T, P]) list(ctx context.Context, limit int) ([]T, error) {
	results := make([]T, 0)
	filter := expression.Equal(expression.Name("isDeleted"), expression.Value(false))
	expr, err := expression.NewBuilder().WithFilter(filter).Build()
	if err != nil {
		return nil, err
	}

	var lastKey map[string]types.AttributeValue
	for {
		resp, err := s.container.Client.Scan(ctx, &dynamodb.ScanInput{
			TableName:                 aws.String(s.container.TableName),
			FilterExpression:          expr.Filter(),
			ExpressionAttributeNames:  expr.Names(),
			ExpressionAttributeValues: expr.Values(),
			ExclusiveStartKey:         lastKey,
			Limit:                     aws.Int32(int32(limit)),
		})
		if err != nil {
			return nil, err
		}

		for _, data := range resp.Items {
			var item T
			if err := attributevalue.UnmarshalMap(data, &item); err != nil {
				return nil, err
			}
			results = append(results, item)
		}

		if resp.LastEvaluatedKey == nil || len(results) >= limit {
			break
		}
		lastKey = resp.LastEvaluatedKey
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
