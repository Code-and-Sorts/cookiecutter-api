package repositories

import (
	"context"
{%- if cloud_service == 'Azure Function App' %}
	"encoding/json"
	"errors"
{%- endif %}
	"fmt"
{%- if cloud_service == 'Azure Function App' %}
	"net/http"
{%- endif %}
	"time"
{% if cloud_service == 'Azure Function App' %}
	"github.com/Azure/azure-sdk-for-go/sdk/azcore"
	"github.com/Azure/azure-sdk-for-go/sdk/data/azcosmos"
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
	"cloud.google.com/go/firestore"
	"google.golang.org/api/iterator"
	"google.golang.org/grpc/codes"
	"google.golang.org/grpc/status"
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
	"github.com/aws/aws-sdk-go-v2/aws"
	"github.com/aws/aws-sdk-go-v2/feature/dynamodb/attributevalue"
	"github.com/aws/aws-sdk-go-v2/feature/dynamodb/expression"
	"github.com/aws/aws-sdk-go-v2/service/dynamodb"
	"github.com/aws/aws-sdk-go-v2/service/dynamodb/types"
{%- endif %}

	"{{project_endpoint}}/models"
)
{%- for resource in resources %}

type {{ resource.name }}Repository interface {
	Get(ctx context.Context, id string) (*models.{{ resource.name }}Dto, error)
	GetList(ctx context.Context, limit int) ([]models.{{ resource.name }}Dto, error)
	Create(ctx context.Context, item models.{{ resource.name }}) (*models.{{ resource.name }}Dto, error)
	Update(ctx context.Context, item models.{{ resource.name }}) (*models.{{ resource.name }}Dto, error)
	Replace(ctx context.Context, item models.{{ resource.name }}) (*models.{{ resource.name }}Dto, error)
	Delete(ctx context.Context, id string) error
}
{%- endfor %}
{%- if cloud_service == 'Azure Function App' %}
{% for resource in resources %}
{%- set r = resource.name %}
{%- set lc = resource.name | to_lower_camel %}
type {{ lc }}Repository struct {
	container *azcosmos.ContainerClient
}

func New{{ r }}Repository(container *azcosmos.ContainerClient) {{ r }}Repository {
	return &{{ lc }}Repository{container: container}
}

func (repo *{{ lc }}Repository) getItem(ctx context.Context, id string) (*models.{{ r }}, error) {
	pk := azcosmos.NewPartitionKeyString(id)
	resp, err := repo.container.ReadItem(ctx, pk, id, nil)
	if err != nil {
		var respErr *azcore.ResponseError
		if errors.As(err, &respErr) && respErr.StatusCode == http.StatusNotFound {
			return nil, &models.NotFoundError{Message: fmt.Sprintf("Item with id %s not found", id)}
		}
		return nil, err
	}

	var item models.{{ r }}
	if err := json.Unmarshal(resp.Value, &item); err != nil {
		return nil, err
	}

	if item.IsDeleted {
		return nil, &models.NotFoundError{Message: fmt.Sprintf("Item with id %s not found", id)}
	}

	return &item, nil
}

func (repo *{{ lc }}Repository) Get(ctx context.Context, id string) (*models.{{ r }}Dto, error) {
	item, err := repo.getItem(ctx, id)
	if err != nil {
		return nil, err
	}

	return &models.{{ r }}Dto{
		Id:   item.Id,
		Name: item.Name,
	}, nil
}

func (repo *{{ lc }}Repository) GetList(ctx context.Context, limit int) ([]models.{{ r }}Dto, error) {
	query := fmt.Sprintf("SELECT * FROM c WHERE c.isDeleted = false OFFSET 0 LIMIT %d", limit)
	pager := repo.container.NewQueryItemsPager(query, azcosmos.NewPartitionKey(), nil)

	var results []models.{{ r }}Dto
	for pager.More() {
		resp, err := pager.NextPage(ctx)
		if err != nil {
			return nil, err
		}

		for _, item := range resp.Items {
			var entity models.{{ r }}
			if err := json.Unmarshal(item, &entity); err != nil {
				return nil, err
			}
			results = append(results, models.{{ r }}Dto{
				Id:   entity.Id,
				Name: entity.Name,
			})
		}

		if len(results) >= limit {
			break
		}
	}

	return results, nil
}

func (repo *{{ lc }}Repository) Create(ctx context.Context, item models.{{ r }}) (*models.{{ r }}Dto, error) {
	pk := azcosmos.NewPartitionKeyString(item.Id)
	data, err := json.Marshal(item)
	if err != nil {
		return nil, err
	}

	resp, err := repo.container.CreateItem(ctx, pk, data, nil)
	if err != nil {
		return nil, err
	}

	var created models.{{ r }}
	if err := json.Unmarshal(resp.Value, &created); err != nil {
		return nil, err
	}

	return &models.{{ r }}Dto{
		Id:   created.Id,
		Name: created.Name,
	}, nil
}

func (repo *{{ lc }}Repository) Update(ctx context.Context, item models.{{ r }}) (*models.{{ r }}Dto, error) {
	currentItem, err := repo.getItem(ctx, item.Id)
	if err != nil {
		return nil, err
	}

	updateItem := models.{{ r }}{
		BaseEntity: models.BaseEntity{
			Id:               currentItem.Id,
			IsDeleted:        currentItem.IsDeleted,
			CreatedBy:        currentItem.CreatedBy,
			CreatedTimestamp: currentItem.CreatedTimestamp,
			UpdatedTimestamp: time.Now().UTC(),
		},
	}

	if item.Name != "" {
		updateItem.Name = item.Name
	} else {
		updateItem.Name = currentItem.Name
	}

	if item.UpdatedBy != "" {
		updateItem.UpdatedBy = item.UpdatedBy
	} else {
		updateItem.UpdatedBy = currentItem.UpdatedBy
	}

	pk := azcosmos.NewPartitionKeyString(updateItem.Id)
	data, err := json.Marshal(updateItem)
	if err != nil {
		return nil, err
	}

	resp, err := repo.container.ReplaceItem(ctx, pk, updateItem.Id, data, nil)
	if err != nil {
		return nil, err
	}

	var updated models.{{ r }}
	if err := json.Unmarshal(resp.Value, &updated); err != nil {
		return nil, err
	}

	return &models.{{ r }}Dto{
		Id:   updated.Id,
		Name: updated.Name,
	}, nil
}

func (repo *{{ lc }}Repository) Replace(ctx context.Context, item models.{{ r }}) (*models.{{ r }}Dto, error) {
	currentItem, err := repo.getItem(ctx, item.Id)
	if err != nil {
		return nil, err
	}

	replacementItem := models.{{ r }}{
		BaseEntity: models.BaseEntity{
			Id:               currentItem.Id,
			IsDeleted:        false,
			CreatedBy:        currentItem.CreatedBy,
			CreatedTimestamp: currentItem.CreatedTimestamp,
			UpdatedBy:        item.UpdatedBy,
			UpdatedTimestamp: time.Now().UTC(),
		},
		Name: item.Name,
	}

	pk := azcosmos.NewPartitionKeyString(replacementItem.Id)
	data, err := json.Marshal(replacementItem)
	if err != nil {
		return nil, err
	}

	resp, err := repo.container.ReplaceItem(ctx, pk, replacementItem.Id, data, nil)
	if err != nil {
		return nil, err
	}

	var replaced models.{{ r }}
	if err := json.Unmarshal(resp.Value, &replaced); err != nil {
		return nil, err
	}

	return &models.{{ r }}Dto{
		Id:   replaced.Id,
		Name: replaced.Name,
	}, nil
}

func (repo *{{ lc }}Repository) Delete(ctx context.Context, id string) error {
	item, err := repo.getItem(ctx, id)
	if err != nil {
		return err
	}

	item.IsDeleted = true
	pk := azcosmos.NewPartitionKeyString(item.Id)
	data, err := json.Marshal(item)
	if err != nil {
		return err
	}

	_, err = repo.container.ReplaceItem(ctx, pk, item.Id, data, nil)
	return err
}
{% endfor %}
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
{% for resource in resources %}
{%- set r = resource.name %}
{%- set lc = resource.name | to_lower_camel %}
type {{ lc }}Repository struct {
	collection *firestore.CollectionRef
}

func New{{ r }}Repository(collection *firestore.CollectionRef) {{ r }}Repository {
	return &{{ lc }}Repository{collection: collection}
}

func (repo *{{ lc }}Repository) getItem(ctx context.Context, id string) (*models.{{ r }}, error) {
	doc, err := repo.collection.Doc(id).Get(ctx)
	if err != nil {
		if status.Code(err) == codes.NotFound {
			return nil, &models.NotFoundError{Message: fmt.Sprintf("Item with id %s not found", id)}
		}
		return nil, err
	}

	var item models.{{ r }}
	if err := doc.DataTo(&item); err != nil {
		return nil, err
	}

	if item.IsDeleted {
		return nil, &models.NotFoundError{Message: fmt.Sprintf("Item with id %s not found", id)}
	}

	return &item, nil
}

func (repo *{{ lc }}Repository) Get(ctx context.Context, id string) (*models.{{ r }}Dto, error) {
	item, err := repo.getItem(ctx, id)
	if err != nil {
		return nil, err
	}

	return &models.{{ r }}Dto{
		Id:   item.Id,
		Name: item.Name,
	}, nil
}

func (repo *{{ lc }}Repository) GetList(ctx context.Context, limit int) ([]models.{{ r }}Dto, error) {
	iter := repo.collection.Where("isDeleted", "==", false).Limit(limit).Documents(ctx)
	defer iter.Stop()

	var results []models.{{ r }}Dto
	for {
		doc, err := iter.Next()
		if err == iterator.Done {
			break
		}
		if err != nil {
			return nil, err
		}

		var entity models.{{ r }}
		if err := doc.DataTo(&entity); err != nil {
			return nil, err
		}
		results = append(results, models.{{ r }}Dto{
			Id:   entity.Id,
			Name: entity.Name,
		})
	}

	return results, nil
}

func (repo *{{ lc }}Repository) Create(ctx context.Context, item models.{{ r }}) (*models.{{ r }}Dto, error) {
	_, err := repo.collection.Doc(item.Id).Set(ctx, item)
	if err != nil {
		return nil, err
	}

	return &models.{{ r }}Dto{
		Id:   item.Id,
		Name: item.Name,
	}, nil
}

func (repo *{{ lc }}Repository) Update(ctx context.Context, item models.{{ r }}) (*models.{{ r }}Dto, error) {
	currentItem, err := repo.getItem(ctx, item.Id)
	if err != nil {
		return nil, err
	}

	updateItem := models.{{ r }}{
		BaseEntity: models.BaseEntity{
			Id:               currentItem.Id,
			IsDeleted:        currentItem.IsDeleted,
			CreatedBy:        currentItem.CreatedBy,
			CreatedTimestamp: currentItem.CreatedTimestamp,
			UpdatedTimestamp: time.Now().UTC(),
		},
	}

	if item.Name != "" {
		updateItem.Name = item.Name
	} else {
		updateItem.Name = currentItem.Name
	}

	if item.UpdatedBy != "" {
		updateItem.UpdatedBy = item.UpdatedBy
	} else {
		updateItem.UpdatedBy = currentItem.UpdatedBy
	}

	_, err = repo.collection.Doc(updateItem.Id).Set(ctx, updateItem)
	if err != nil {
		return nil, err
	}

	return &models.{{ r }}Dto{
		Id:   updateItem.Id,
		Name: updateItem.Name,
	}, nil
}

func (repo *{{ lc }}Repository) Replace(ctx context.Context, item models.{{ r }}) (*models.{{ r }}Dto, error) {
	currentItem, err := repo.getItem(ctx, item.Id)
	if err != nil {
		return nil, err
	}

	replacementItem := models.{{ r }}{
		BaseEntity: models.BaseEntity{
			Id:               currentItem.Id,
			IsDeleted:        false,
			CreatedBy:        currentItem.CreatedBy,
			CreatedTimestamp: currentItem.CreatedTimestamp,
			UpdatedBy:        item.UpdatedBy,
			UpdatedTimestamp: time.Now().UTC(),
		},
		Name: item.Name,
	}

	_, err = repo.collection.Doc(replacementItem.Id).Set(ctx, replacementItem)
	if err != nil {
		return nil, err
	}

	return &models.{{ r }}Dto{
		Id:   replacementItem.Id,
		Name: replacementItem.Name,
	}, nil
}

func (repo *{{ lc }}Repository) Delete(ctx context.Context, id string) error {
	item, err := repo.getItem(ctx, id)
	if err != nil {
		return err
	}

	item.IsDeleted = true
	_, err = repo.collection.Doc(item.Id).Set(ctx, *item)
	return err
}
{% endfor %}
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
{% for resource in resources %}
{%- set r = resource.name %}
{%- set lc = resource.name | to_lower_camel %}
type {{ lc }}Repository struct {
	client    *dynamodb.Client
	tableName string
}

func New{{ r }}Repository(client *dynamodb.Client, tableName string) {{ r }}Repository {
	return &{{ lc }}Repository{client: client, tableName: tableName}
}

func (repo *{{ lc }}Repository) getItem(ctx context.Context, id string) (*models.{{ r }}, error) {
	key, err := attributevalue.MarshalMap(map[string]string{"id": id})
	if err != nil {
		return nil, err
	}

	resp, err := repo.client.GetItem(ctx, &dynamodb.GetItemInput{
		TableName: aws.String(repo.tableName),
		Key:       key,
	})
	if err != nil {
		return nil, err
	}

	if resp.Item == nil {
		return nil, &models.NotFoundError{Message: fmt.Sprintf("Item with id %s not found", id)}
	}

	var item models.{{ r }}
	if err := attributevalue.UnmarshalMap(resp.Item, &item); err != nil {
		return nil, err
	}

	if item.IsDeleted {
		return nil, &models.NotFoundError{Message: fmt.Sprintf("Item with id %s not found", id)}
	}

	return &item, nil
}

func (repo *{{ lc }}Repository) Get(ctx context.Context, id string) (*models.{{ r }}Dto, error) {
	item, err := repo.getItem(ctx, id)
	if err != nil {
		return nil, err
	}

	return &models.{{ r }}Dto{
		Id:   item.Id,
		Name: item.Name,
	}, nil
}

func (repo *{{ lc }}Repository) GetList(ctx context.Context, limit int) ([]models.{{ r }}Dto, error) {
	filt := expression.Equal(expression.Name("isDeleted"), expression.Value(false))
	expr, err := expression.NewBuilder().WithFilter(filt).Build()
	if err != nil {
		return nil, err
	}

	var results []models.{{ r }}Dto
	var lastKey map[string]types.AttributeValue

	for {
		input := &dynamodb.ScanInput{
			TableName:                 aws.String(repo.tableName),
			FilterExpression:          expr.Filter(),
			ExpressionAttributeNames:  expr.Names(),
			ExpressionAttributeValues: expr.Values(),
			ExclusiveStartKey:         lastKey,
			Limit:                     aws.Int32(int32(limit)),
		}

		resp, err := repo.client.Scan(ctx, input)
		if err != nil {
			return nil, err
		}

		for _, item := range resp.Items {
			var entity models.{{ r }}
			if err := attributevalue.UnmarshalMap(item, &entity); err != nil {
				return nil, err
			}
			results = append(results, models.{{ r }}Dto{
				Id:   entity.Id,
				Name: entity.Name,
			})
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

func (repo *{{ lc }}Repository) Create(ctx context.Context, item models.{{ r }}) (*models.{{ r }}Dto, error) {
	av, err := attributevalue.MarshalMap(item)
	if err != nil {
		return nil, err
	}

	_, err = repo.client.PutItem(ctx, &dynamodb.PutItemInput{
		TableName: aws.String(repo.tableName),
		Item:      av,
	})
	if err != nil {
		return nil, err
	}

	return &models.{{ r }}Dto{
		Id:   item.Id,
		Name: item.Name,
	}, nil
}

func (repo *{{ lc }}Repository) Update(ctx context.Context, item models.{{ r }}) (*models.{{ r }}Dto, error) {
	currentItem, err := repo.getItem(ctx, item.Id)
	if err != nil {
		return nil, err
	}

	updateItem := models.{{ r }}{
		BaseEntity: models.BaseEntity{
			Id:               currentItem.Id,
			IsDeleted:        currentItem.IsDeleted,
			CreatedBy:        currentItem.CreatedBy,
			CreatedTimestamp: currentItem.CreatedTimestamp,
			UpdatedTimestamp: time.Now().UTC(),
		},
	}

	if item.Name != "" {
		updateItem.Name = item.Name
	} else {
		updateItem.Name = currentItem.Name
	}

	if item.UpdatedBy != "" {
		updateItem.UpdatedBy = item.UpdatedBy
	} else {
		updateItem.UpdatedBy = currentItem.UpdatedBy
	}

	av, err := attributevalue.MarshalMap(updateItem)
	if err != nil {
		return nil, err
	}

	_, err = repo.client.PutItem(ctx, &dynamodb.PutItemInput{
		TableName: aws.String(repo.tableName),
		Item:      av,
	})
	if err != nil {
		return nil, err
	}

	return &models.{{ r }}Dto{
		Id:   updateItem.Id,
		Name: updateItem.Name,
	}, nil
}

func (repo *{{ lc }}Repository) Replace(ctx context.Context, item models.{{ r }}) (*models.{{ r }}Dto, error) {
	currentItem, err := repo.getItem(ctx, item.Id)
	if err != nil {
		return nil, err
	}

	replacementItem := models.{{ r }}{
		BaseEntity: models.BaseEntity{
			Id:               currentItem.Id,
			IsDeleted:        false,
			CreatedBy:        currentItem.CreatedBy,
			CreatedTimestamp: currentItem.CreatedTimestamp,
			UpdatedBy:        item.UpdatedBy,
			UpdatedTimestamp: time.Now().UTC(),
		},
		Name: item.Name,
	}

	av, err := attributevalue.MarshalMap(replacementItem)
	if err != nil {
		return nil, err
	}

	_, err = repo.client.PutItem(ctx, &dynamodb.PutItemInput{
		TableName: aws.String(repo.tableName),
		Item:      av,
	})
	if err != nil {
		return nil, err
	}

	return &models.{{ r }}Dto{
		Id:   replacementItem.Id,
		Name: replacementItem.Name,
	}, nil
}

func (repo *{{ lc }}Repository) Delete(ctx context.Context, id string) error {
	item, err := repo.getItem(ctx, id)
	if err != nil {
		return err
	}

	item.IsDeleted = true

	av, err := attributevalue.MarshalMap(item)
	if err != nil {
		return err
	}

	_, err = repo.client.PutItem(ctx, &dynamodb.PutItemInput{
		TableName: aws.String(repo.tableName),
		Item:      av,
	})
	return err
}
{% endfor %}
{%- endif %}