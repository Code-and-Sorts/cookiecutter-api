package repositories

import (
	"context"
	"fmt"
	"time"

	"cloud.google.com/go/firestore"
	"google.golang.org/api/iterator"
	"google.golang.org/grpc/codes"
	"google.golang.org/grpc/status"

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
	collection *firestore.CollectionRef
}

func NewKittenClawsRepository(collection *firestore.CollectionRef) KittenClawsRepository {
	return &kittenClawsRepository{collection: collection}
}

func (repo *kittenClawsRepository) getItem(ctx context.Context, id string) (*models.KittenClaws, error) {
	doc, err := repo.collection.Doc(id).Get(ctx)
	if err != nil {
		if status.Code(err) == codes.NotFound {
			return nil, &models.NotFoundError{Message: fmt.Sprintf("Item with id %s not found", id)}
		}
		return nil, err
	}

	var item models.KittenClaws
	if err := doc.DataTo(&item); err != nil {
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
	iter := repo.collection.Where("isDeleted", "==", false).Limit(limit).Documents(ctx)
	defer iter.Stop()

	var results []models.KittenClawsDto
	for {
		doc, err := iter.Next()
		if err == iterator.Done {
			break
		}
		if err != nil {
			return nil, err
		}

		var entity models.KittenClaws
		if err := doc.DataTo(&entity); err != nil {
			return nil, err
		}
		results = append(results, models.KittenClawsDto{
			Id:   entity.Id,
			Name: entity.Name,
		})
	}

	return results, nil
}

func (repo *kittenClawsRepository) Create(ctx context.Context, item models.KittenClaws) (*models.KittenClawsDto, error) {
	_, err := repo.collection.Doc(item.Id).Set(ctx, item)
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

	_, err = repo.collection.Doc(updateItem.Id).Set(ctx, updateItem)
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

	_, err = repo.collection.Doc(replacementItem.Id).Set(ctx, replacementItem)
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
	_, err = repo.collection.Doc(item.Id).Set(ctx, *item)
	return err
}
