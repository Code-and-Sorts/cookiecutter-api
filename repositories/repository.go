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

type KittenClawsRepository interface {
	Get(ctx context.Context, id string) (*models.KittenClawsDto, error)
	GetList(ctx context.Context, limit int) ([]models.KittenClawsDto, error)
	Create(ctx context.Context, item models.KittenClaws) (*models.KittenClawsDto, error)
	Update(ctx context.Context, item models.KittenClaws) (*models.KittenClawsDto, error)
	Replace(ctx context.Context, item models.KittenClaws) (*models.KittenClawsDto, error)
	Delete(ctx context.Context, id string) error
}

type kittenClawsRepository struct {
	container *azcosmos.ContainerClient
}

func NewKittenClawsRepository(container *azcosmos.ContainerClient) KittenClawsRepository {
	return &kittenClawsRepository{container: container}
}

func (repo *kittenClawsRepository) getItem(ctx context.Context, id string) (*models.KittenClaws, error) {
	pk := azcosmos.NewPartitionKeyString(id)
	resp, err := repo.container.ReadItem(ctx, pk, id, nil)
	if err != nil {
		var respErr *azcore.ResponseError
		if errors.As(err, &respErr) && respErr.StatusCode == http.StatusNotFound {
			return nil, &models.NotFoundError{Message: fmt.Sprintf("Item with id %s not found", id)}
		}
		return nil, err
	}

	var item models.KittenClaws
	if err := json.Unmarshal(resp.Value, &item); err != nil {
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
	query := fmt.Sprintf("SELECT * FROM c WHERE c.isDeleted = false OFFSET 0 LIMIT %d", limit)
	pager := repo.container.NewQueryItemsPager(query, azcosmos.NewPartitionKey(), nil)

	var results []models.KittenClawsDto
	for pager.More() {
		resp, err := pager.NextPage(ctx)
		if err != nil {
			return nil, err
		}

		for _, item := range resp.Items {
			var entity models.KittenClaws
			if err := json.Unmarshal(item, &entity); err != nil {
				return nil, err
			}
			results = append(results, models.KittenClawsDto{
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

func (repo *kittenClawsRepository) Create(ctx context.Context, item models.KittenClaws) (*models.KittenClawsDto, error) {
	pk := azcosmos.NewPartitionKeyString(item.Id)
	data, err := json.Marshal(item)
	if err != nil {
		return nil, err
	}

	resp, err := repo.container.CreateItem(ctx, pk, data, nil)
	if err != nil {
		return nil, err
	}

	var created models.KittenClaws
	if err := json.Unmarshal(resp.Value, &created); err != nil {
		return nil, err
	}

	return &models.KittenClawsDto{
		Id:   created.Id,
		Name: created.Name,
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

	pk := azcosmos.NewPartitionKeyString(updateItem.Id)
	data, err := json.Marshal(updateItem)
	if err != nil {
		return nil, err
	}

	resp, err := repo.container.ReplaceItem(ctx, pk, updateItem.Id, data, nil)
	if err != nil {
		return nil, err
	}

	var updated models.KittenClaws
	if err := json.Unmarshal(resp.Value, &updated); err != nil {
		return nil, err
	}

	return &models.KittenClawsDto{
		Id:   updated.Id,
		Name: updated.Name,
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

	pk := azcosmos.NewPartitionKeyString(replacementItem.Id)
	data, err := json.Marshal(replacementItem)
	if err != nil {
		return nil, err
	}

	resp, err := repo.container.ReplaceItem(ctx, pk, replacementItem.Id, data, nil)
	if err != nil {
		return nil, err
	}

	var replaced models.KittenClaws
	if err := json.Unmarshal(resp.Value, &replaced); err != nil {
		return nil, err
	}

	return &models.KittenClawsDto{
		Id:   replaced.Id,
		Name: replaced.Name,
	}, nil
}

func (repo *kittenClawsRepository) Delete(ctx context.Context, id string) error {
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
