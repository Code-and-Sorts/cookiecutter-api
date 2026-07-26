package services

import (
	"context"
	"testing"

	"kitties/models"

	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/mock"
)

type mockKittenClawsRepository struct {
	mock.Mock
}

func (m *mockKittenClawsRepository) Get(ctx context.Context, id string) (*models.KittenClawsDto, error) {
	args := m.Called(ctx, id)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.KittenClawsDto), args.Error(1)
}

func (m *mockKittenClawsRepository) GetList(ctx context.Context, limit int) ([]models.KittenClawsDto, error) {
	args := m.Called(ctx, limit)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).([]models.KittenClawsDto), args.Error(1)
}

func (m *mockKittenClawsRepository) Create(ctx context.Context, item models.KittenClaws) (*models.KittenClawsDto, error) {
	args := m.Called(ctx, item)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.KittenClawsDto), args.Error(1)
}

func (m *mockKittenClawsRepository) Update(ctx context.Context, item models.KittenClaws) (*models.KittenClawsDto, error) {
	args := m.Called(ctx, item)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.KittenClawsDto), args.Error(1)
}

func (m *mockKittenClawsRepository) Replace(ctx context.Context, item models.KittenClaws) (*models.KittenClawsDto, error) {
	args := m.Called(ctx, item)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.KittenClawsDto), args.Error(1)
}

func (m *mockKittenClawsRepository) Delete(ctx context.Context, id string) error {
	args := m.Called(ctx, id)
	return args.Error(0)
}

func setupKittenClawsServiceTest() (*mockKittenClawsRepository, KittenClawsService) {
	mockRepo := new(mockKittenClawsRepository)
	service := NewKittenClawsService(mockRepo)
	return mockRepo, service
}

func TestGetKittenClaws_ShouldReturnKittenClawsDto(t *testing.T) {
	mockRepo, service := setupKittenClawsServiceTest()
	id := "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c"
	expected := &models.KittenClawsDto{Id: id, Name: "mockKittenClaws"}
	mockRepo.On("Get", mock.Anything, id).Return(expected, nil)

	result, err := service.Get(context.Background(), id)

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockRepo.AssertExpectations(t)
}

func TestGetKittenClawsList_ShouldReturnListOfKittenClawsDto(t *testing.T) {
	mockRepo, service := setupKittenClawsServiceTest()
	expected := []models.KittenClawsDto{
		{Id: "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name: "mockKittenClaws1"},
		{Id: "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name: "mockKittenClaws2"},
	}
	mockRepo.On("GetList", mock.Anything, 50).Return(expected, nil)

	result, err := service.GetList(context.Background(), 50)

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockRepo.AssertExpectations(t)
}

func TestCreateKittenClaws_ShouldReturnCreatedKittenClawsDto(t *testing.T) {
	mockRepo, service := setupKittenClawsServiceTest()
	createRequest := models.CreateKittenClawsRequest{Name: "mockCreateKittenClaws"}
	expected := &models.KittenClawsDto{Id: "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name: "mockCreateKittenClaws"}
	mockRepo.On("Create", mock.Anything, mock.AnythingOfType("models.KittenClaws")).Return(expected, nil)

	result, err := service.Create(context.Background(), createRequest)

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockRepo.AssertExpectations(t)
}

func TestUpdateKittenClaws_ShouldReturnUpdatedKittenClawsDto(t *testing.T) {
	mockRepo, service := setupKittenClawsServiceTest()
	updateRequest := models.UpdateKittenClawsRequest{Id: "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name: "mockUpdateKittenClaws"}
	expected := &models.KittenClawsDto{Id: "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name: "mockUpdateKittenClaws"}
	mockRepo.On("Update", mock.Anything, mock.AnythingOfType("models.KittenClaws")).Return(expected, nil)

	result, err := service.Update(context.Background(), updateRequest)

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockRepo.AssertExpectations(t)
}

func TestDeleteKittenClaws_ShouldCallRepositoryDelete(t *testing.T) {
	mockRepo, service := setupKittenClawsServiceTest()
	id := "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c"
	mockRepo.On("Delete", mock.Anything, id).Return(nil)

	err := service.Delete(context.Background(), id)

	assert.NoError(t, err)
	mockRepo.AssertCalled(t, "Delete", mock.Anything, id)
}
