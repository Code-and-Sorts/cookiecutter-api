package services

import (
	"context"
	"testing"

	"{{project_endpoint}}/models"

	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/mock"
)
{% for resource in resources %}
{%- set r = resource.name %}
type mock{{ r }}Repository struct {
	mock.Mock
}

func (m *mock{{ r }}Repository) Get(ctx context.Context, id string) (*models.{{ r }}Dto, error) {
	args := m.Called(ctx, id)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.{{ r }}Dto), args.Error(1)
}

func (m *mock{{ r }}Repository) GetList(ctx context.Context, limit int) ([]models.{{ r }}Dto, error) {
	args := m.Called(ctx, limit)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).([]models.{{ r }}Dto), args.Error(1)
}

func (m *mock{{ r }}Repository) Create(ctx context.Context, item models.{{ r }}) (*models.{{ r }}Dto, error) {
	args := m.Called(ctx, item)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.{{ r }}Dto), args.Error(1)
}

func (m *mock{{ r }}Repository) Update(ctx context.Context, item models.{{ r }}) (*models.{{ r }}Dto, error) {
	args := m.Called(ctx, item)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.{{ r }}Dto), args.Error(1)
}

func (m *mock{{ r }}Repository) Replace(ctx context.Context, item models.{{ r }}) (*models.{{ r }}Dto, error) {
	args := m.Called(ctx, item)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.{{ r }}Dto), args.Error(1)
}

func (m *mock{{ r }}Repository) Delete(ctx context.Context, id string) error {
	args := m.Called(ctx, id)
	return args.Error(0)
}

func setup{{ r }}ServiceTest() (*mock{{ r }}Repository, {{ r }}Service) {
	mockRepo := new(mock{{ r }}Repository)
	service := New{{ r }}Service(mockRepo)
	return mockRepo, service
}
{%- if "get_by_id" in resource.operations %}

func TestGet{{ r }}_ShouldReturn{{ r }}Dto(t *testing.T) {
	mockRepo, service := setup{{ r }}ServiceTest()
	id := "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c"
	expected := &models.{{ r }}Dto{Id: id, Name: "mock{{ r }}"}
	mockRepo.On("Get", mock.Anything, id).Return(expected, nil)

	result, err := service.Get(context.Background(), id)

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockRepo.AssertExpectations(t)
}
{%- endif %}
{%- if "list" in resource.operations %}

func TestGet{{ r }}List_ShouldReturnListOf{{ r }}Dto(t *testing.T) {
	mockRepo, service := setup{{ r }}ServiceTest()
	expected := []models.{{ r }}Dto{
		{Id: "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name: "mock{{ r }}1"},
		{Id: "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name: "mock{{ r }}2"},
	}
	mockRepo.On("GetList", mock.Anything, 50).Return(expected, nil)

	result, err := service.GetList(context.Background(), 50)

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockRepo.AssertExpectations(t)
}
{%- endif %}
{%- if "create" in resource.operations %}

func TestCreate{{ r }}_ShouldReturnCreated{{ r }}Dto(t *testing.T) {
	mockRepo, service := setup{{ r }}ServiceTest()
	createRequest := models.Create{{ r }}Request{Name: "mockCreate{{ r }}"}
	expected := &models.{{ r }}Dto{Id: "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name: "mockCreate{{ r }}"}
	mockRepo.On("Create", mock.Anything, mock.AnythingOfType("models.{{ r }}")).Return(expected, nil)

	result, err := service.Create(context.Background(), createRequest)

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockRepo.AssertExpectations(t)
}
{%- endif %}
{%- if "update" in resource.operations %}

func TestUpdate{{ r }}_ShouldReturnUpdated{{ r }}Dto(t *testing.T) {
	mockRepo, service := setup{{ r }}ServiceTest()
	updateRequest := models.Update{{ r }}Request{Id: "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name: "mockUpdate{{ r }}"}
	expected := &models.{{ r }}Dto{Id: "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name: "mockUpdate{{ r }}"}
	mockRepo.On("Update", mock.Anything, mock.AnythingOfType("models.{{ r }}")).Return(expected, nil)

	result, err := service.Update(context.Background(), updateRequest)

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockRepo.AssertExpectations(t)
}
{%- endif %}
{%- if "replace" in resource.operations %}

func TestReplace{{ r }}_ShouldReturnReplaced{{ r }}Dto(t *testing.T) {
	mockRepo, service := setup{{ r }}ServiceTest()
	replaceRequest := models.Replace{{ r }}Request{Id: "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name: "mockReplace{{ r }}"}
	expected := &models.{{ r }}Dto{Id: "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name: "mockReplace{{ r }}"}
	mockRepo.On("Replace", mock.Anything, mock.AnythingOfType("models.{{ r }}")).Return(expected, nil)

	result, err := service.Replace(context.Background(), replaceRequest)

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockRepo.AssertExpectations(t)
}
{%- endif %}
{%- if "delete" in resource.operations %}

func TestDelete{{ r }}_ShouldCallRepositoryDelete(t *testing.T) {
	mockRepo, service := setup{{ r }}ServiceTest()
	id := "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c"
	mockRepo.On("Delete", mock.Anything, id).Return(nil)

	err := service.Delete(context.Background(), id)

	assert.NoError(t, err)
	mockRepo.AssertCalled(t, "Delete", mock.Anything, id)
}
{%- endif %}
{% endfor %}