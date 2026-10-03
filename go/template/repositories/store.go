{%- set all_ops = path_resources | map(attribute='operations') | sum(start=[]) | unique | list -%}
{%- set need_get = path_resources | selectattr('has_item') | list | length > 0 -%}
{%- set need_write = all_ops | select('in', ['create', 'update', 'replace', 'delete']) | list | length > 0 -%}
{%- set need_one = all_ops | select('in', ['get_by_id', 'create', 'update', 'replace']) | list | length > 0 -%}
package repositories

import (
	"context"
{%- if cloud_service == 'Azure Function App' %}
	"encoding/json"
{%- if 'list' in all_ops %}
	"fmt"
{%- endif %}
{%- endif %}
{% if cloud_service == 'Azure Function App' %}
	"github.com/Azure/azure-sdk-for-go/sdk/data/azcosmos"
{%- elif cloud_service == 'GCP Cloud Function' %}
	"cloud.google.com/go/firestore"
{%- if 'list' in all_ops %}
	"google.golang.org/api/iterator"
{%- endif %}
{%- if need_get %}
	"google.golang.org/grpc/codes"
	"google.golang.org/grpc/status"
{%- endif %}
{%- elif cloud_service == 'AWS Lambda' %}
	"github.com/aws/aws-sdk-go-v2/aws"
	"github.com/aws/aws-sdk-go-v2/feature/dynamodb/attributevalue"
{%- if 'list' in all_ops %}
	"github.com/aws/aws-sdk-go-v2/feature/dynamodb/expression"
{%- endif %}
	"github.com/aws/aws-sdk-go-v2/service/dynamodb"
{%- if 'list' in all_ops %}
	"github.com/aws/aws-sdk-go-v2/service/dynamodb/types"
{%- endif %}
{%- endif %}

	"{{project_endpoint}}/models"
)
{%- if cloud_service == 'Azure Function App' %}

type Container = *azcosmos.ContainerClient
{%- elif cloud_service == 'GCP Cloud Function' %}

type Container = *firestore.CollectionRef
{%- elif cloud_service == 'AWS Lambda' %}

type Container struct {
	Client    *dynamodb.Client
	TableName string
}
{%- endif %}

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
{%- if need_get %}

func (s *store[T, P]) get(ctx context.Context, id string) (*T, error) {
	item := new(T)
{%- if cloud_service == 'Azure Function App' %}
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
{%- elif cloud_service == 'GCP Cloud Function' %}
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
{%- elif cloud_service == 'AWS Lambda' %}
	key, err := attributevalue.MarshalMap(map[string]string{"id": id})
	if err != nil {
		return nil, err
	}
	resp, err := s.container.Client.GetItem(ctx, &dynamodb.GetItemInput{
		TableName: aws.String(s.container.TableName),
		Key:       key,
	})
	if err != nil {
		return nil, err
	}
	if resp.Item == nil {
		return nil, models.NewNotFoundError(s.resource, id)
	}
	if err := attributevalue.UnmarshalMap(resp.Item, item); err != nil {
		return nil, err
	}
{%- endif %}

	if P(item).Base().IsDeleted {
		return nil, models.NewNotFoundError(s.resource, id)
	}
	return item, nil
}
{%- endif %}
{%- if need_write %}
{%- if cloud_service == 'Azure Function App' %}

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
{%- elif cloud_service == 'GCP Cloud Function' %}

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
{%- elif cloud_service == 'AWS Lambda' %}

func (s *store[T, P]) write(ctx context.Context, item *T, create bool) error {
	av, err := attributevalue.MarshalMap(item)
	if err != nil {
		return err
	}

	input := &dynamodb.PutItemInput{
		TableName: aws.String(s.container.TableName),
		Item:      av,
	}
	if create {
		input.ConditionExpression = aws.String("attribute_not_exists(id)")
	}
	_, err = s.container.Client.PutItem(ctx, input)
	return err
}
{%- endif %}
{%- endif %}
{%- if 'list' in all_ops %}

func (s *store[T, P]) list(ctx context.Context, limit int) ([]T, error) {
	results := make([]T, 0)
{%- if cloud_service == 'Azure Function App' %}
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
{%- elif cloud_service == 'GCP Cloud Function' %}
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
{%- elif cloud_service == 'AWS Lambda' %}
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
{%- endif %}

	if len(results) > limit {
		results = results[:limit]
	}
	return results, nil
}
{%- endif %}
{%- if 'create' in all_ops %}

func (s *store[T, P]) create(ctx context.Context, item *T, userID string) (*T, error) {
	P(item).Base().StampCreate(userID)
	if err := s.write(ctx, item, true); err != nil {
		return nil, err
	}
	return item, nil
}
{%- endif %}
{%- if 'update' in all_ops or 'replace' in all_ops %}

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
{%- endif %}

{%- if 'delete' in all_ops %}

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
{%- endif %}
{%- if need_one %}

func toDto[T, D any](mapping func(T) D) func(*T, error) (*D, error) {
	return func(item *T, err error) (*D, error) {
		if err != nil {
			return nil, err
		}
		dto := mapping(*item)
		return &dto, nil
	}
}
{%- endif %}
{%- if 'list' in all_ops %}

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
{%- endif %}
