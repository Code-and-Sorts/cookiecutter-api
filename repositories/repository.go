package repositories

import (
	"context"
	"encoding/json"
	"errors"
	"fmt"
	"net/http"
	"time"

	"github.com/Azure/azure-sdk-for-go/sdk/azcore"
	"github.com/Azure/azure-sdk-for-go/sdk/data/azcosmos"

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
	container *azcosmos.ContainerClient
}

func NewCatRepository(container *azcosmos.ContainerClient) CatRepository {
	return &catRepository{container: container}
}

func (repo *catRepository) getItem(ctx context.Context, id string) (*models.Cat, error) {
	pk := azcosmos.NewPartitionKeyString(id)
	resp, err := repo.container.ReadItem(ctx, pk, id, nil)
	if err != nil {
		var respErr *azcore.ResponseError
		if errors.As(err, &respErr) && respErr.StatusCode == http.StatusNotFound {
			return nil, &models.NotFoundError{Message: fmt.Sprintf("Item with id %s not found", id)}
		}
		return nil, err
	}

	var item models.Cat
	if err := json.Unmarshal(resp.Value, &item); err != nil {
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
	query := fmt.Sprintf("SELECT * FROM c WHERE c.isDeleted = false OFFSET 0 LIMIT %d", limit)
	pager := repo.container.NewQueryItemsPager(query, azcosmos.NewPartitionKey(), nil)

	var results []models.CatDto
	for pager.More() {
		resp, err := pager.NextPage(ctx)
		if err != nil {
			return nil, err
		}

		for _, item := range resp.Items {
			var entity models.Cat
			if err := json.Unmarshal(item, &entity); err != nil {
				return nil, err
			}
			results = append(results, models.CatDto{
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

func (repo *catRepository) Create(ctx context.Context, item models.Cat) (*models.CatDto, error) {
	pk := azcosmos.NewPartitionKeyString(item.Id)
	data, err := json.Marshal(item)
	if err != nil {
		return nil, err
	}

	resp, err := repo.container.CreateItem(ctx, pk, data, nil)
	if err != nil {
		return nil, err
	}

	var created models.Cat
	if err := json.Unmarshal(resp.Value, &created); err != nil {
		return nil, err
	}

	return &models.CatDto{
		Id:   created.Id,
		Name: created.Name,
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

	pk := azcosmos.NewPartitionKeyString(updateItem.Id)
	data, err := json.Marshal(updateItem)
	if err != nil {
		return nil, err
	}

	resp, err := repo.container.ReplaceItem(ctx, pk, updateItem.Id, data, nil)
	if err != nil {
		return nil, err
	}

	var updated models.Cat
	if err := json.Unmarshal(resp.Value, &updated); err != nil {
		return nil, err
	}

	return &models.CatDto{
		Id:   updated.Id,
		Name: updated.Name,
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

	pk := azcosmos.NewPartitionKeyString(replacementItem.Id)
	data, err := json.Marshal(replacementItem)
	if err != nil {
		return nil, err
	}

	resp, err := repo.container.ReplaceItem(ctx, pk, replacementItem.Id, data, nil)
	if err != nil {
		return nil, err
	}

	var replaced models.Cat
	if err := json.Unmarshal(resp.Value, &replaced); err != nil {
		return nil, err
	}

	return &models.CatDto{
		Id:   replaced.Id,
		Name: replaced.Name,
	}, nil
}

func (repo *catRepository) Delete(ctx context.Context, id string) error {
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

type dogRepository struct {
	container *azcosmos.ContainerClient
}

func NewDogRepository(container *azcosmos.ContainerClient) DogRepository {
	return &dogRepository{container: container}
}

func (repo *dogRepository) getItem(ctx context.Context, id string) (*models.Dog, error) {
	pk := azcosmos.NewPartitionKeyString(id)
	resp, err := repo.container.ReadItem(ctx, pk, id, nil)
	if err != nil {
		var respErr *azcore.ResponseError
		if errors.As(err, &respErr) && respErr.StatusCode == http.StatusNotFound {
			return nil, &models.NotFoundError{Message: fmt.Sprintf("Item with id %s not found", id)}
		}
		return nil, err
	}

	var item models.Dog
	if err := json.Unmarshal(resp.Value, &item); err != nil {
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
	query := fmt.Sprintf("SELECT * FROM c WHERE c.isDeleted = false OFFSET 0 LIMIT %d", limit)
	pager := repo.container.NewQueryItemsPager(query, azcosmos.NewPartitionKey(), nil)

	var results []models.DogDto
	for pager.More() {
		resp, err := pager.NextPage(ctx)
		if err != nil {
			return nil, err
		}

		for _, item := range resp.Items {
			var entity models.Dog
			if err := json.Unmarshal(item, &entity); err != nil {
				return nil, err
			}
			results = append(results, models.DogDto{
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

func (repo *dogRepository) Create(ctx context.Context, item models.Dog) (*models.DogDto, error) {
	pk := azcosmos.NewPartitionKeyString(item.Id)
	data, err := json.Marshal(item)
	if err != nil {
		return nil, err
	}

	resp, err := repo.container.CreateItem(ctx, pk, data, nil)
	if err != nil {
		return nil, err
	}

	var created models.Dog
	if err := json.Unmarshal(resp.Value, &created); err != nil {
		return nil, err
	}

	return &models.DogDto{
		Id:   created.Id,
		Name: created.Name,
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

	pk := azcosmos.NewPartitionKeyString(updateItem.Id)
	data, err := json.Marshal(updateItem)
	if err != nil {
		return nil, err
	}

	resp, err := repo.container.ReplaceItem(ctx, pk, updateItem.Id, data, nil)
	if err != nil {
		return nil, err
	}

	var updated models.Dog
	if err := json.Unmarshal(resp.Value, &updated); err != nil {
		return nil, err
	}

	return &models.DogDto{
		Id:   updated.Id,
		Name: updated.Name,
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

	pk := azcosmos.NewPartitionKeyString(replacementItem.Id)
	data, err := json.Marshal(replacementItem)
	if err != nil {
		return nil, err
	}

	resp, err := repo.container.ReplaceItem(ctx, pk, replacementItem.Id, data, nil)
	if err != nil {
		return nil, err
	}

	var replaced models.Dog
	if err := json.Unmarshal(resp.Value, &replaced); err != nil {
		return nil, err
	}

	return &models.DogDto{
		Id:   replaced.Id,
		Name: replaced.Name,
	}, nil
}

func (repo *dogRepository) Delete(ctx context.Context, id string) error {
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
