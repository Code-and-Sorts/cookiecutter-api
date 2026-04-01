package repositories

import (
	"context"
	"encoding/json"
	"fmt"
	"time"

	"github.com/Azure/azure-sdk-for-go/sdk/data/azcosmos"

	"{{cookiecutter.project_endpoint}}/models"
)

type {{cookiecutter.project_class_name}}Repository interface {
	Get(ctx context.Context, id string) (*models.{{cookiecutter.project_class_name}}Dto, error)
	GetList(ctx context.Context) ([]models.{{cookiecutter.project_class_name}}Dto, error)
	Create(ctx context.Context, item models.{{cookiecutter.project_class_name}}) (*models.{{cookiecutter.project_class_name}}Dto, error)
	Update(ctx context.Context, item models.{{cookiecutter.project_class_name}}) (*models.{{cookiecutter.project_class_name}}Dto, error)
	Delete(ctx context.Context, id string) error
}

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
		return nil, &models.NotFoundError{Message: fmt.Sprintf("Item with id %s not found", id)}
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
	pk := azcosmos.NewPartitionKeyString("")
	pager := r.container.NewQueryItemsPager(query, pk, nil)

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
