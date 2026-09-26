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
	collection *firestore.CollectionRef
}

func NewCatRepository(collection *firestore.CollectionRef) CatRepository {
	return &catRepository{collection: collection}
}

func (repo *catRepository) getItem(ctx context.Context, id string) (*models.Cat, error) {
	doc, err := repo.collection.Doc(id).Get(ctx)
	if err != nil {
		if status.Code(err) == codes.NotFound {
			return nil, &models.NotFoundError{Message: fmt.Sprintf("Item with id %s not found", id)}
		}
		return nil, err
	}

	var item models.Cat
	if err := doc.DataTo(&item); err != nil {
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
	iter := repo.collection.Where("isDeleted", "==", false).Limit(limit).Documents(ctx)
	defer iter.Stop()

	var results []models.CatDto
	for {
		doc, err := iter.Next()
		if err == iterator.Done {
			break
		}
		if err != nil {
			return nil, err
		}

		var entity models.Cat
		if err := doc.DataTo(&entity); err != nil {
			return nil, err
		}
		results = append(results, models.CatDto{
			Id:   entity.Id,
			Name: entity.Name,
		})
	}

	return results, nil
}

func (repo *catRepository) Create(ctx context.Context, item models.Cat) (*models.CatDto, error) {
	_, err := repo.collection.Doc(item.Id).Set(ctx, item)
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

	_, err = repo.collection.Doc(updateItem.Id).Set(ctx, updateItem)
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

	_, err = repo.collection.Doc(replacementItem.Id).Set(ctx, replacementItem)
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
	_, err = repo.collection.Doc(item.Id).Set(ctx, *item)
	return err
}

type dogRepository struct {
	collection *firestore.CollectionRef
}

func NewDogRepository(collection *firestore.CollectionRef) DogRepository {
	return &dogRepository{collection: collection}
}

func (repo *dogRepository) getItem(ctx context.Context, id string) (*models.Dog, error) {
	doc, err := repo.collection.Doc(id).Get(ctx)
	if err != nil {
		if status.Code(err) == codes.NotFound {
			return nil, &models.NotFoundError{Message: fmt.Sprintf("Item with id %s not found", id)}
		}
		return nil, err
	}

	var item models.Dog
	if err := doc.DataTo(&item); err != nil {
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
	iter := repo.collection.Where("isDeleted", "==", false).Limit(limit).Documents(ctx)
	defer iter.Stop()

	var results []models.DogDto
	for {
		doc, err := iter.Next()
		if err == iterator.Done {
			break
		}
		if err != nil {
			return nil, err
		}

		var entity models.Dog
		if err := doc.DataTo(&entity); err != nil {
			return nil, err
		}
		results = append(results, models.DogDto{
			Id:   entity.Id,
			Name: entity.Name,
		})
	}

	return results, nil
}

func (repo *dogRepository) Create(ctx context.Context, item models.Dog) (*models.DogDto, error) {
	_, err := repo.collection.Doc(item.Id).Set(ctx, item)
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

	_, err = repo.collection.Doc(updateItem.Id).Set(ctx, updateItem)
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

	_, err = repo.collection.Doc(replacementItem.Id).Set(ctx, replacementItem)
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
	_, err = repo.collection.Doc(item.Id).Set(ctx, *item)
	return err
}
