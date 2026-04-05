package repositories

import (
	"context"
{%- if cookiecutter.cloud_service == 'Azure Function App' %}
	"encoding/json"
	"errors"
{%- endif %}
	"fmt"
{%- if cookiecutter.cloud_service == 'Azure Function App' %}
	"net/http"
{%- endif %}
	"time"
{% if cookiecutter.cloud_service == 'Azure Function App' %}
	"github.com/Azure/azure-sdk-for-go/sdk/azcore"
	"github.com/Azure/azure-sdk-for-go/sdk/data/azcosmos"
{%- endif %}
{%- if cookiecutter.cloud_service == 'AWS Lambda' %}
	"github.com/aws/aws-sdk-go-v2/aws"
	"github.com/aws/aws-sdk-go-v2/feature/dynamodb/attributevalue"
	"github.com/aws/aws-sdk-go-v2/feature/dynamodb/expression"
	"github.com/aws/aws-sdk-go-v2/service/dynamodb"
	"github.com/aws/aws-sdk-go-v2/service/dynamodb/types"
{%- endif %}

	"{{cookiecutter.project_endpoint}}/models"
)

type {{cookiecutter.project_class_name}}Repository interface {
	Get(ctx context.Context, id string) (*models.{{cookiecutter.project_class_name}}Dto, error)
	GetList(ctx context.Context) ([]models.{{cookiecutter.project_class_name}}Dto, error)
	Create(ctx context.Context, item models.{{cookiecutter.project_class_name}}) (*models.{{cookiecutter.project_class_name}}Dto, error)
	Update(ctx context.Context, item models.{{cookiecutter.project_class_name}}) (*models.{{cookiecutter.project_class_name}}Dto, error)
	Delete(ctx context.Context, id string) error
}
{% if cookiecutter.cloud_service == 'Azure Function App' %}
type {{cookiecutter.project_lower_camel_name}}Repository struct {
	container *azcosmos.ContainerClient
}

func New{{cookiecutter.project_class_name}}Repository(container *azcosmos.ContainerClient) {{cookiecutter.project_class_name}}Repository {
	return &{{cookiecutter.project_lower_camel_name}}Repository{container: container}
}

func (r *{{cookiecutter.project_lower_camel_name}}Repository) getItem(ctx context.Context, id string) (*models.{{cookiecutter.project_class_name}}, error) {
	pk := azcosmos.NewPartitionKeyString(id)
	resp, err := r.container.ReadItem(ctx, pk, id, nil)
	if err != nil {
		var respErr *azcore.ResponseError
		if errors.As(err, &respErr) && respErr.StatusCode == http.StatusNotFound {
			return nil, &models.NotFoundError{Message: fmt.Sprintf("Item with id %s not found", id)}
		}
		return nil, err
	}

	var item models.{{cookiecutter.project_class_name}}
	if err := json.Unmarshal(resp.Value, &item); err != nil {
		return nil, err
	}

	if item.IsDeleted {
		return nil, &models.NotFoundError{Message: fmt.Sprintf("Item with id %s not found", id)}
	}

	return &item, nil
}

func (r *{{cookiecutter.project_lower_camel_name}}Repository) Get(ctx context.Context, id string) (*models.{{cookiecutter.project_class_name}}Dto, error) {
	item, err := r.getItem(ctx, id)
	if err != nil {
		return nil, err
	}

	return &models.{{cookiecutter.project_class_name}}Dto{
		Id:   item.Id,
		Name: item.Name,
	}, nil
}

func (r *{{cookiecutter.project_lower_camel_name}}Repository) GetList(ctx context.Context) ([]models.{{cookiecutter.project_class_name}}Dto, error) {
	query := "SELECT * FROM c WHERE c.isDeleted = false"
	pager := r.container.NewQueryItemsPager(query, azcosmos.NewPartitionKey(), nil)

	var results []models.{{cookiecutter.project_class_name}}Dto
	for pager.More() {
		resp, err := pager.NextPage(ctx)
		if err != nil {
			return nil, err
		}

		for _, item := range resp.Items {
			var entity models.{{cookiecutter.project_class_name}}
			if err := json.Unmarshal(item, &entity); err != nil {
				return nil, err
			}
			results = append(results, models.{{cookiecutter.project_class_name}}Dto{
				Id:   entity.Id,
				Name: entity.Name,
			})
		}
	}

	return results, nil
}

func (r *{{cookiecutter.project_lower_camel_name}}Repository) Create(ctx context.Context, item models.{{cookiecutter.project_class_name}}) (*models.{{cookiecutter.project_class_name}}Dto, error) {
	pk := azcosmos.NewPartitionKeyString(item.Id)
	data, err := json.Marshal(item)
	if err != nil {
		return nil, err
	}

	resp, err := r.container.CreateItem(ctx, pk, data, nil)
	if err != nil {
		return nil, err
	}

	var created models.{{cookiecutter.project_class_name}}
	if err := json.Unmarshal(resp.Value, &created); err != nil {
		return nil, err
	}

	return &models.{{cookiecutter.project_class_name}}Dto{
		Id:   created.Id,
		Name: created.Name,
	}, nil
}

func (r *{{cookiecutter.project_lower_camel_name}}Repository) Update(ctx context.Context, item models.{{cookiecutter.project_class_name}}) (*models.{{cookiecutter.project_class_name}}Dto, error) {
	currentItem, err := r.getItem(ctx, item.Id)
	if err != nil {
		return nil, err
	}

	updateItem := models.{{cookiecutter.project_class_name}}{
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

	var updated models.{{cookiecutter.project_class_name}}
	if err := json.Unmarshal(resp.Value, &updated); err != nil {
		return nil, err
	}

	return &models.{{cookiecutter.project_class_name}}Dto{
		Id:   updated.Id,
		Name: updated.Name,
	}, nil
}

func (r *{{cookiecutter.project_lower_camel_name}}Repository) Delete(ctx context.Context, id string) error {
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
{%- if cookiecutter.cloud_service == 'AWS Lambda' %}
type {{cookiecutter.project_lower_camel_name}}Repository struct {
	client    *dynamodb.Client
	tableName string
}

func New{{cookiecutter.project_class_name}}Repository(client *dynamodb.Client, tableName string) {{cookiecutter.project_class_name}}Repository {
	return &{{cookiecutter.project_lower_camel_name}}Repository{client: client, tableName: tableName}
}

func (r *{{cookiecutter.project_lower_camel_name}}Repository) getItem(ctx context.Context, id string) (*models.{{cookiecutter.project_class_name}}, error) {
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

	var item models.{{cookiecutter.project_class_name}}
	if err := attributevalue.UnmarshalMap(resp.Item, &item); err != nil {
		return nil, err
	}

	if item.IsDeleted {
		return nil, &models.NotFoundError{Message: fmt.Sprintf("Item with id %s not found", id)}
	}

	return &item, nil
}

func (r *{{cookiecutter.project_lower_camel_name}}Repository) Get(ctx context.Context, id string) (*models.{{cookiecutter.project_class_name}}Dto, error) {
	item, err := r.getItem(ctx, id)
	if err != nil {
		return nil, err
	}

	return &models.{{cookiecutter.project_class_name}}Dto{
		Id:   item.Id,
		Name: item.Name,
	}, nil
}

func (r *{{cookiecutter.project_lower_camel_name}}Repository) GetList(ctx context.Context) ([]models.{{cookiecutter.project_class_name}}Dto, error) {
	filt := expression.Equal(expression.Name("isDeleted"), expression.Value(false))
	expr, err := expression.NewBuilder().WithFilter(filt).Build()
	if err != nil {
		return nil, err
	}

	var results []models.{{cookiecutter.project_class_name}}Dto
	var lastKey map[string]types.AttributeValue

	for {
		input := &dynamodb.ScanInput{
			TableName:                 aws.String(r.tableName),
			FilterExpression:          expr.Filter(),
			ExpressionAttributeNames:  expr.Names(),
			ExpressionAttributeValues: expr.Values(),
			ExclusiveStartKey:         lastKey,
		}

		resp, err := r.client.Scan(ctx, input)
		if err != nil {
			return nil, err
		}

		for _, item := range resp.Items {
			var entity models.{{cookiecutter.project_class_name}}
			if err := attributevalue.UnmarshalMap(item, &entity); err != nil {
				return nil, err
			}
			results = append(results, models.{{cookiecutter.project_class_name}}Dto{
				Id:   entity.Id,
				Name: entity.Name,
			})
		}

		if resp.LastEvaluatedKey == nil {
			break
		}
		lastKey = resp.LastEvaluatedKey
	}

	return results, nil
}

func (r *{{cookiecutter.project_lower_camel_name}}Repository) Create(ctx context.Context, item models.{{cookiecutter.project_class_name}}) (*models.{{cookiecutter.project_class_name}}Dto, error) {
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

	return &models.{{cookiecutter.project_class_name}}Dto{
		Id:   item.Id,
		Name: item.Name,
	}, nil
}

func (r *{{cookiecutter.project_lower_camel_name}}Repository) Update(ctx context.Context, item models.{{cookiecutter.project_class_name}}) (*models.{{cookiecutter.project_class_name}}Dto, error) {
	currentItem, err := r.getItem(ctx, item.Id)
	if err != nil {
		return nil, err
	}

	updateItem := models.{{cookiecutter.project_class_name}}{
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

	return &models.{{cookiecutter.project_class_name}}Dto{
		Id:   updateItem.Id,
		Name: updateItem.Name,
	}, nil
}

func (r *{{cookiecutter.project_lower_camel_name}}Repository) Delete(ctx context.Context, id string) error {
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
