package services

import (
	"context"
	"testing"

	"kittenclaws/models"

	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/mock"
)

type mockDogRepository struct {
	mock.Mock
}

func (m *mockDogRepository) Get(ctx context.Context, id string) (*models.DogDto, error) {
	args := m.Called(ctx, id)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.DogDto), args.Error(1)
}

func (m *mockDogRepository) GetList(ctx context.Context, limit int) ([]models.DogDto, error) {
	args := m.Called(ctx, limit)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).([]models.DogDto), args.Error(1)
}

func (m *mockDogRepository) Create(ctx context.Context, item models.Dog, userID string) (*models.DogDto, error) {
	args := m.Called(ctx, item, userID)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.DogDto), args.Error(1)
}

func (m *mockDogRepository) Replace(ctx context.Context, item models.Dog, userID string) (*models.DogDto, error) {
	args := m.Called(ctx, item, userID)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.DogDto), args.Error(1)
}

func (m *mockDogRepository) Delete(ctx context.Context, id, userID string) error {
	args := m.Called(ctx, id, userID)
	return args.Error(0)
}

func setupDogServiceTest() (*mockDogRepository, DogService) {
	mockRepo := new(mockDogRepository)
	service := NewDogService(mockRepo)
	return mockRepo, service
}

func TestGetDog_ShouldReturnDogDto(t *testing.T) {
	mockRepo, service := setupDogServiceTest()
	id := "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c"
	expected := &models.DogDto{Id: id, Name: "mockDog"}
	mockRepo.On("Get", mock.Anything, id).Return(expected, nil)

	result, err := service.Get(context.Background(), id)

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockRepo.AssertExpectations(t)
}

func TestGetDogList_ShouldReturnListOfDogDto(t *testing.T) {
	mockRepo, service := setupDogServiceTest()
	expected := []models.DogDto{
		{Id: "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name: "mockDog1"},
		{Id: "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name: "mockDog2"},
	}
	mockRepo.On("GetList", mock.Anything, 50).Return(expected, nil)

	result, err := service.GetList(context.Background(), 50)

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockRepo.AssertExpectations(t)
}

func TestCreateDog_ShouldReturnCreatedDogDto(t *testing.T) {
	mockRepo, service := setupDogServiceTest()
	createRequest := models.CreateDogRequest{Name: "mockCreateDog"}
	expected := &models.DogDto{Id: "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name: "mockCreateDog"}
	mockRepo.On("Create", mock.Anything, mock.AnythingOfType("models.Dog"), "alice").Return(expected, nil)

	result, err := service.Create(context.Background(), createRequest, "alice")

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockRepo.AssertExpectations(t)

	stored := mockRepo.Calls[0].Arguments.Get(1).(models.Dog)
	assert.Len(t, stored.Id, 36)
	assert.Equal(t, "mockCreateDog", stored.Name)
	assert.False(t, stored.IsDeleted)
	assert.Equal(t, stored.CreatedTimestamp, stored.UpdatedTimestamp)
	assert.Regexp(t, `^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3}Z$`, stored.CreatedTimestamp)
}

func TestReplaceDog_ShouldReturnReplacedDogDto(t *testing.T) {
	mockRepo, service := setupDogServiceTest()
	replaceRequest := models.ReplaceDogRequest{Id: "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name: "mockReplaceDog"}
	expected := &models.DogDto{Id: "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name: "mockReplaceDog"}
	mockRepo.On("Replace", mock.Anything, mock.AnythingOfType("models.Dog"), "alice").Return(expected, nil)

	result, err := service.Replace(context.Background(), replaceRequest, "alice")

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockRepo.AssertExpectations(t)
}

func TestDeleteDog_ShouldCallRepositoryDelete(t *testing.T) {
	mockRepo, service := setupDogServiceTest()
	id := "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c"
	mockRepo.On("Delete", mock.Anything, id, "alice").Return(nil)

	err := service.Delete(context.Background(), id, "alice")

	assert.NoError(t, err)
	mockRepo.AssertCalled(t, "Delete", mock.Anything, id, "alice")
}
