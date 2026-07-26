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

type CatRepository interface {
	Get(ctx context.Context, id string) (*models.CatDto, error)
	GetList(ctx context.Context, limit int) ([]models.CatDto, error)
	Create(ctx context.Context, item models.Cat) (*models.CatDto, error)
	Update(ctx context.Context, item models.Cat) (*models.CatDto, error)
	Replace(ctx context.Context, item models.Cat) (*models.CatDto, error)
	Delete(ctx context.Context, id string) error
}

type DogRepository interface {
	Get(ctx context.Context, id string) (*models.DogDto, error)
	GetList(ctx context.Context, limit int) ([]models.DogDto, error)
	Create(ctx context.Context, item models.Dog) (*models.DogDto, error)
	Update(ctx context.Context, item models.Dog) (*models.DogDto, error)
	Replace(ctx context.Context, item models.Dog) (*models.DogDto, error)
	Delete(ctx context.Context, id string) error
}

type catRepository struct {
	client    *dynamodb.Client
	tableName string
}

func NewCatRepository(client *dynamodb.Client, tableName string) CatRepository {
	return &catRepository{client: client, tableName: tableName}
}

func (repo *catRepository) getItem(ctx context.Context, id string) (*models.Cat, error) {
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

	var item models.Cat
	if err := attributevalue.UnmarshalMap(resp.Item, &item); err != nil {
		return nil, err
	}

	if item.IsDeleted {
		return nil, &models.NotFoundError{Message: fmt.Sprintf("Item with id %s not found", id)}
	}

	return &item, nil
}

func (repo *catRepository) Get(ctx context.Context, id string) (*models.CatDto, error) {
	item, err := repo.getItem(ctx, id)
	if err != nil {
		return nil, err
	}

	return &models.CatDto{
		Id:   item.Id,
		Name: item.Name,
	}, nil
}

func (repo *catRepository) GetList(ctx context.Context, limit int) ([]models.CatDto, error) {
	filt := expression.Equal(expression.Name("isDeleted"), expression.Value(false))
	expr, err := expression.NewBuilder().WithFilter(filt).Build()
	if err != nil {
		return nil, err
	}

	var results []models.CatDto
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
			var entity models.Cat
			if err := attributevalue.UnmarshalMap(item, &entity); err != nil {
				return nil, err
			}
			results = append(results, models.CatDto{
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

func (repo *catRepository) Create(ctx context.Context, item models.Cat) (*models.CatDto, error) {
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

	return &models.CatDto{
		Id:   item.Id,
		Name: item.Name,
	}, nil
}

func (repo *catRepository) Update(ctx context.Context, item models.Cat) (*models.CatDto, error) {
	currentItem, err := repo.getItem(ctx, item.Id)
	if err != nil {
		return nil, err
	}

	updateItem := models.Cat{
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

	return &models.CatDto{
		Id:   updateItem.Id,
		Name: updateItem.Name,
	}, nil
}

func (repo *catRepository) Replace(ctx context.Context, item models.Cat) (*models.CatDto, error) {
	currentItem, err := repo.getItem(ctx, item.Id)
	if err != nil {
		return nil, err
	}

	replacementItem := models.Cat{
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

	return &models.CatDto{
		Id:   replacementItem.Id,
		Name: replacementItem.Name,
	}, nil
}

func (repo *catRepository) Delete(ctx context.Context, id string) error {
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

type dogRepository struct {
	client    *dynamodb.Client
	tableName string
}

func NewDogRepository(client *dynamodb.Client, tableName string) DogRepository {
	return &dogRepository{client: client, tableName: tableName}
}

func (repo *dogRepository) getItem(ctx context.Context, id string) (*models.Dog, error) {
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

	var item models.Dog
	if err := attributevalue.UnmarshalMap(resp.Item, &item); err != nil {
		return nil, err
	}

	if item.IsDeleted {
		return nil, &models.NotFoundError{Message: fmt.Sprintf("Item with id %s not found", id)}
	}

	return &item, nil
}

func (repo *dogRepository) Get(ctx context.Context, id string) (*models.DogDto, error) {
	item, err := repo.getItem(ctx, id)
	if err != nil {
		return nil, err
	}

	return &models.DogDto{
		Id:   item.Id,
		Name: item.Name,
	}, nil
}

func (repo *dogRepository) GetList(ctx context.Context, limit int) ([]models.DogDto, error) {
	filt := expression.Equal(expression.Name("isDeleted"), expression.Value(false))
	expr, err := expression.NewBuilder().WithFilter(filt).Build()
	if err != nil {
		return nil, err
	}

	var results []models.DogDto
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
			var entity models.Dog
			if err := attributevalue.UnmarshalMap(item, &entity); err != nil {
				return nil, err
			}
			results = append(results, models.DogDto{
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

func (repo *dogRepository) Create(ctx context.Context, item models.Dog) (*models.DogDto, error) {
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

	return &models.DogDto{
		Id:   item.Id,
		Name: item.Name,
	}, nil
}

func (repo *dogRepository) Update(ctx context.Context, item models.Dog) (*models.DogDto, error) {
	currentItem, err := repo.getItem(ctx, item.Id)
	if err != nil {
		return nil, err
	}

	updateItem := models.Dog{
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

	return &models.DogDto{
		Id:   updateItem.Id,
		Name: updateItem.Name,
	}, nil
}

func (repo *dogRepository) Replace(ctx context.Context, item models.Dog) (*models.DogDto, error) {
	currentItem, err := repo.getItem(ctx, item.Id)
	if err != nil {
		return nil, err
	}

	replacementItem := models.Dog{
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

	return &models.DogDto{
		Id:   replacementItem.Id,
		Name: replacementItem.Name,
	}, nil
}

func (repo *dogRepository) Delete(ctx context.Context, id string) error {
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
