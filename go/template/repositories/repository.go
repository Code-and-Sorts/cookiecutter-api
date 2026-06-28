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

type {{project_class_name}}Repository interface {
	Get(ctx context.Context, id string) (*models.{{project_class_name}}Dto, error)
	GetList(ctx context.Context, limit int) ([]models.{{project_class_name}}Dto, error)
	Create(ctx context.Context, item models.{{project_class_name}}) (*models.{{project_class_name}}Dto, error)
	Update(ctx context.Context, item models.{{project_class_name}}) (*models.{{project_class_name}}Dto, error)
	Delete(ctx context.Context, id string) error
}
{% if cloud_service == 'Azure Function App' %}
type {{project_lower_camel_name}}Repository struct {
	container *azcosmos.ContainerClient
}

func New{{project_class_name}}Repository(container *azcosmos.ContainerClient) {{project_class_name}}Repository {
	return &{{project_lower_camel_name}}Repository{container: container}
}

func (r *{{project_lower_camel_name}}Repository) getItem(ctx context.Context, id string) (*models.{{project_class_name}}, error) {
	pk := azcosmos.NewPartitionKeyString(id)
	resp, err := r.container.ReadItem(ctx, pk, id, nil)
	if err != nil {
		var respErr *azcore.ResponseError
		if errors.As(err, &respErr) && respErr.StatusCode == http.StatusNotFound {
			return nil, &models.NotFoundError{Message: fmt.Sprintf("Item with id %s not found", id)}
		}
		return nil, err
	}

	var item models.{{project_class_name}}
	if err := json.Unmarshal(resp.Value, &item); err != nil {
		return nil, err
	}

	if item.IsDeleted {
		return nil, &models.NotFoundError{Message: fmt.Sprintf("Item with id %s not found", id)}
	}

	return &item, nil
}

func (r *{{project_lower_camel_name}}Repository) Get(ctx context.Context, id string) (*models.{{project_class_name}}Dto, error) {
	item, err := r.getItem(ctx, id)
	if err != nil {
		return nil, err
	}

	return &models.{{project_class_name}}Dto{
		Id:   item.Id,
		Name: item.Name,
	}, nil
}

func (r *{{project_lower_camel_name}}Repository) GetList(ctx context.Context, limit int) ([]models.{{project_class_name}}Dto, error) {
	query := fmt.Sprintf("SELECT * FROM c WHERE c.isDeleted = false OFFSET 0 LIMIT %d", limit)
	pager := r.container.NewQueryItemsPager(query, azcosmos.NewPartitionKey(), nil)

	var results []models.{{project_class_name}}Dto
	for pager.More() {
		resp, err := pager.NextPage(ctx)
		if err != nil {
			return nil, err
		}

		for _, item := range resp.Items {
			var entity models.{{project_class_name}}
			if err := json.Unmarshal(item, &entity); err != nil {
				return nil, err
			}
			results = append(results, models.{{project_class_name}}Dto{
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

func (r *{{project_lower_camel_name}}Repository) Create(ctx context.Context, item models.{{project_class_name}}) (*models.{{project_class_name}}Dto, error) {
	pk := azcosmos.NewPartitionKeyString(item.Id)
	data, err := json.Marshal(item)
	if err != nil {
		return nil, err
	}

	resp, err := r.container.CreateItem(ctx, pk, data, nil)
	if err != nil {
		return nil, err
	}

	var created models.{{project_class_name}}
	if err := json.Unmarshal(resp.Value, &created); err != nil {
		return nil, err
	}

	return &models.{{project_class_name}}Dto{
		Id:   created.Id,
		Name: created.Name,
	}, nil
}

func (r *{{project_lower_camel_name}}Repository) Update(ctx context.Context, item models.{{project_class_name}}) (*models.{{project_class_name}}Dto, error) {
	currentItem, err := r.getItem(ctx, item.Id)
	if err != nil {
		return nil, err
	}

	updateItem := models.{{project_class_name}}{
		BaseEntity: models.BaseEntity{
			Id:               currentItem.Id,
			IsDeleted:        currentItem.IsDeleted,
			CreatedBy:        currentItem.CreatedBy,
			CreatedTimestamp:  currentItem.CreatedTimestamp,
			UpdatedTimestamp:  time.Now().UTC(),
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

	resp, err := r.container.ReplaceItem(ctx, pk, updateItem.Id, data, nil)
	if err != nil {
		return nil, err
	}

	var updated models.{{project_class_name}}
	if err := json.Unmarshal(resp.Value, &updated); err != nil {
		return nil, err
	}

	return &models.{{project_class_name}}Dto{
		Id:   updated.Id,
		Name: updated.Name,
	}, nil
}

func (r *{{project_lower_camel_name}}Repository) Delete(ctx context.Context, id string) error {
	item, err := r.getItem(ctx, id)
	if err != nil {
		return err
	}

	item.IsDeleted = true
	pk := azcosmos.NewPartitionKeyString(item.Id)
	data, err := json.Marshal(item)
	if err != nil {
		return err
	}

	_, err = r.container.ReplaceItem(ctx, pk, item.Id, data, nil)
	return err
}
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
type {{project_lower_camel_name}}Repository struct {
	collection *firestore.CollectionRef
}

func New{{project_class_name}}Repository(collection *firestore.CollectionRef) {{project_class_name}}Repository {
	return &{{project_lower_camel_name}}Repository{collection: collection}
}

func (r *{{project_lower_camel_name}}Repository) getItem(ctx context.Context, id string) (*models.{{project_class_name}}, error) {
	doc, err := r.collection.Doc(id).Get(ctx)
	if err != nil {
		if status.Code(err) == codes.NotFound {
			return nil, &models.NotFoundError{Message: fmt.Sprintf("Item with id %s not found", id)}
		}
		return nil, err
	}

	var item models.{{project_class_name}}
	if err := doc.DataTo(&item); err != nil {
		return nil, err
	}

	if item.IsDeleted {
		return nil, &models.NotFoundError{Message: fmt.Sprintf("Item with id %s not found", id)}
	}

	return &item, nil
}

func (r *{{project_lower_camel_name}}Repository) Get(ctx context.Context, id string) (*models.{{project_class_name}}Dto, error) {
	item, err := r.getItem(ctx, id)
	if err != nil {
		return nil, err
	}

	return &models.{{project_class_name}}Dto{
		Id:   item.Id,
		Name: item.Name,
	}, nil
}

func (r *{{project_lower_camel_name}}Repository) GetList(ctx context.Context, limit int) ([]models.{{project_class_name}}Dto, error) {
	iter := r.collection.Where("isDeleted", "==", false).Limit(limit).Documents(ctx)
	defer iter.Stop()

	var results []models.{{project_class_name}}Dto
	for {
		doc, err := iter.Next()
		if err == iterator.Done {
			break
		}
		if err != nil {
			return nil, err
		}

		var entity models.{{project_class_name}}
		if err := doc.DataTo(&entity); err != nil {
			return nil, err
		}
		results = append(results, models.{{project_class_name}}Dto{
			Id:   entity.Id,
			Name: entity.Name,
		})
	}

	return results, nil
}

func (r *{{project_lower_camel_name}}Repository) Create(ctx context.Context, item models.{{project_class_name}}) (*models.{{project_class_name}}Dto, error) {
	_, err := r.collection.Doc(item.Id).Set(ctx, item)
	if err != nil {
		return nil, err
	}

	return &models.{{project_class_name}}Dto{
		Id:   item.Id,
		Name: item.Name,
	}, nil
}

func (r *{{project_lower_camel_name}}Repository) Update(ctx context.Context, item models.{{project_class_name}}) (*models.{{project_class_name}}Dto, error) {
	currentItem, err := r.getItem(ctx, item.Id)
	if err != nil {
		return nil, err
	}

	updateItem := models.{{project_class_name}}{
		BaseEntity: models.BaseEntity{
			Id:               currentItem.Id,
			IsDeleted:        currentItem.IsDeleted,
			CreatedBy:        currentItem.CreatedBy,
			CreatedTimestamp:  currentItem.CreatedTimestamp,
			UpdatedTimestamp:  time.Now().UTC(),
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

	_, err = r.collection.Doc(updateItem.Id).Set(ctx, updateItem)
	if err != nil {
		return nil, err
	}

	return &models.{{project_class_name}}Dto{
		Id:   updateItem.Id,
		Name: updateItem.Name,
	}, nil
}

func (r *{{project_lower_camel_name}}Repository) Delete(ctx context.Context, id string) error {
	item, err := r.getItem(ctx, id)
	if err != nil {
		return err
	}

	item.IsDeleted = true
	_, err = r.collection.Doc(item.Id).Set(ctx, *item)
	return err
}
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
type {{project_lower_camel_name}}Repository struct {
	client    *dynamodb.Client
	tableName string
}

func New{{project_class_name}}Repository(client *dynamodb.Client, tableName string) {{project_class_name}}Repository {
	return &{{project_lower_camel_name}}Repository{client: client, tableName: tableName}
}

func (r *{{project_lower_camel_name}}Repository) getItem(ctx context.Context, id string) (*models.{{project_class_name}}, error) {
	key, err := attributevalue.MarshalMap(map[string]string{"id": id})
	if err != nil {
		return nil, err
	}

	resp, err := r.client.GetItem(ctx, &dynamodb.GetItemInput{
		TableName: aws.String(r.tableName),
		Key:       key,
	})
	if err != nil {
		return nil, err
	}

	if resp.Item == nil {
		return nil, &models.NotFoundError{Message: fmt.Sprintf("Item with id %s not found", id)}
	}

	var item models.{{project_class_name}}
	if err := attributevalue.UnmarshalMap(resp.Item, &item); err != nil {
		return nil, err
	}

	if item.IsDeleted {
		return nil, &models.NotFoundError{Message: fmt.Sprintf("Item with id %s not found", id)}
	}

	return &item, nil
}

func (r *{{project_lower_camel_name}}Repository) Get(ctx context.Context, id string) (*models.{{project_class_name}}Dto, error) {
	item, err := r.getItem(ctx, id)
	if err != nil {
		return nil, err
	}

	return &models.{{project_class_name}}Dto{
		Id:   item.Id,
		Name: item.Name,
	}, nil
}

func (r *{{project_lower_camel_name}}Repository) GetList(ctx context.Context, limit int) ([]models.{{project_class_name}}Dto, error) {
	filt := expression.Equal(expression.Name("isDeleted"), expression.Value(false))
	expr, err := expression.NewBuilder().WithFilter(filt).Build()
	if err != nil {
		return nil, err
	}

	var results []models.{{project_class_name}}Dto
	var lastKey map[string]types.AttributeValue

	for {
		input := &dynamodb.ScanInput{
			TableName:                 aws.String(r.tableName),
			FilterExpression:          expr.Filter(),
			ExpressionAttributeNames:  expr.Names(),
			ExpressionAttributeValues: expr.Values(),
			ExclusiveStartKey:         lastKey,
			Limit:                     aws.Int32(int32(limit)),
		}

		resp, err := r.client.Scan(ctx, input)
		if err != nil {
			return nil, err
		}

		for _, item := range resp.Items {
			var entity models.{{project_class_name}}
			if err := attributevalue.UnmarshalMap(item, &entity); err != nil {
				return nil, err
			}
			results = append(results, models.{{project_class_name}}Dto{
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

func (r *{{project_lower_camel_name}}Repository) Create(ctx context.Context, item models.{{project_class_name}}) (*models.{{project_class_name}}Dto, error) {
	av, err := attributevalue.MarshalMap(item)
	if err != nil {
		return nil, err
	}

	_, err = r.client.PutItem(ctx, &dynamodb.PutItemInput{
		TableName: aws.String(r.tableName),
		Item:      av,
	})
	if err != nil {
		return nil, err
	}

	return &models.{{project_class_name}}Dto{
		Id:   item.Id,
		Name: item.Name,
	}, nil
}

func (r *{{project_lower_camel_name}}Repository) Update(ctx context.Context, item models.{{project_class_name}}) (*models.{{project_class_name}}Dto, error) {
	currentItem, err := r.getItem(ctx, item.Id)
	if err != nil {
		return nil, err
	}

	updateItem := models.{{project_class_name}}{
		BaseEntity: models.BaseEntity{
			Id:               currentItem.Id,
			IsDeleted:        currentItem.IsDeleted,
			CreatedBy:        currentItem.CreatedBy,
			CreatedTimestamp:  currentItem.CreatedTimestamp,
			UpdatedTimestamp:  time.Now().UTC(),
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

	_, err = r.client.PutItem(ctx, &dynamodb.PutItemInput{
		TableName: aws.String(r.tableName),
		Item:      av,
	})
	if err != nil {
		return nil, err
	}

	return &models.{{project_class_name}}Dto{
		Id:   updateItem.Id,
		Name: updateItem.Name,
	}, nil
}

func (r *{{project_lower_camel_name}}Repository) Delete(ctx context.Context, id string) error {
	item, err := r.getItem(ctx, id)
	if err != nil {
		return err
	}

	item.IsDeleted = true

	av, err := attributevalue.MarshalMap(item)
	if err != nil {
		return err
	}

	_, err = r.client.PutItem(ctx, &dynamodb.PutItemInput{
		TableName: aws.String(r.tableName),
		Item:      av,
	})
	return err
}
{%- endif %}
