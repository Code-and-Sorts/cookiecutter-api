package repositories

import (
	"context"
	"fmt"
	"time"

	"github.com/aws/aws-sdk-go-v2/aws"
	"github.com/aws/aws-sdk-go-v2/feature/dynamodb/attributevalue"
	"github.com/aws/aws-sdk-go-v2/feature/dynamodb/expression"
	"github.com/aws/aws-sdk-go-v2/service/dynamodb"
	"github.com/aws/aws-sdk-go-v2/service/dynamodb/types"

	"kitties/models"
)

type KittenClawsRepository interface {
	Get(ctx context.Context, id string) (*models.KittenClawsDto, error)
	GetList(ctx context.Context, limit int) ([]models.KittenClawsDto, error)
	Create(ctx context.Context, item models.KittenClaws) (*models.KittenClawsDto, error)
	Update(ctx context.Context, item models.KittenClaws) (*models.KittenClawsDto, error)
	Replace(ctx context.Context, item models.KittenClaws) (*models.KittenClawsDto, error)
	Delete(ctx context.Context, id string) error
}

type kittenClawsRepository struct {
	client    *dynamodb.Client
	tableName string
}

func NewKittenClawsRepository(client *dynamodb.Client, tableName string) KittenClawsRepository {
	return &kittenClawsRepository{client: client, tableName: tableName}
}

func (repo *kittenClawsRepository) getItem(ctx context.Context, id string) (*models.KittenClaws, error) {
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

	var item models.KittenClaws
	if err := attributevalue.UnmarshalMap(resp.Item, &item); err != nil {
		return nil, err
	}

	if item.IsDeleted {
		return nil, &models.NotFoundError{Message: fmt.Sprintf("Item with id %s not found", id)}
	}

	return &item, nil
}

func (repo *kittenClawsRepository) Get(ctx context.Context, id string) (*models.KittenClawsDto, error) {
	item, err := repo.getItem(ctx, id)
	if err != nil {
		return nil, err
	}

	return &models.KittenClawsDto{
		Id:   item.Id,
		Name: item.Name,
	}, nil
}

func (repo *kittenClawsRepository) GetList(ctx context.Context, limit int) ([]models.KittenClawsDto, error) {
	filt := expression.Equal(expression.Name("isDeleted"), expression.Value(false))
	expr, err := expression.NewBuilder().WithFilter(filt).Build()
	if err != nil {
		return nil, err
	}

	var results []models.KittenClawsDto
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
			var entity models.KittenClaws
			if err := attributevalue.UnmarshalMap(item, &entity); err != nil {
				return nil, err
			}
			results = append(results, models.KittenClawsDto{
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

func (repo *kittenClawsRepository) Create(ctx context.Context, item models.KittenClaws) (*models.KittenClawsDto, error) {
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

	return &models.KittenClawsDto{
		Id:   item.Id,
		Name: item.Name,
	}, nil
}

func (repo *kittenClawsRepository) Update(ctx context.Context, item models.KittenClaws) (*models.KittenClawsDto, error) {
	currentItem, err := repo.getItem(ctx, item.Id)
	if err != nil {
		return nil, err
	}

	updateItem := models.KittenClaws{
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

	return &models.KittenClawsDto{
		Id:   updateItem.Id,
		Name: updateItem.Name,
	}, nil
}

func (repo *kittenClawsRepository) Replace(ctx context.Context, item models.KittenClaws) (*models.KittenClawsDto, error) {
	currentItem, err := repo.getItem(ctx, item.Id)
	if err != nil {
		return nil, err
	}

	replacementItem := models.KittenClaws{
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

	return &models.KittenClawsDto{
		Id:   replacementItem.Id,
		Name: replacementItem.Name,
	}, nil
}

func (repo *kittenClawsRepository) Delete(ctx context.Context, id string) error {
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
