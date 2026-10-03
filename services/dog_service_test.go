package services

import (
	"context"
	"testing"

	"kittenclaws/models"

	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/mock"
)

const testDogID = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c"

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

func (m *mockDogRepository) Replace(ctx context.Context, id, userID string, apply func(*models.Dog)) (*models.DogDto, error) {
	args := m.Called(ctx, id, userID, apply)
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

func testDogDto() *models.DogDto {
	return &models.DogDto{BaseResponse: models.BaseResponse{Id: testDogID}}
}

func TestGetDog_ShouldReturnDogDto(t *testing.T) {
	mockRepo, service := setupDogServiceTest()
	expected := testDogDto()
	mockRepo.On("Get", mock.Anything, testDogID).Return(expected, nil)

	result, err := service.Get(context.Background(), testDogID)

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockRepo.AssertExpectations(t)
}

func TestGetDogList_ShouldReturnListOfDogDto(t *testing.T) {
	mockRepo, service := setupDogServiceTest()
	expected := []models.DogDto{*testDogDto()}
	mockRepo.On("GetList", mock.Anything, 50).Return(expected, nil)

	result, err := service.GetList(context.Background(), 50)

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockRepo.AssertExpectations(t)
}

func TestCreateDog_StoresANewRecordWithTheRequestFields(t *testing.T) {
	mockRepo, service := setupDogServiceTest()
	req := models.NewCreateDogRequest()
	req.TenantId = new("public")
	req.Region = new("eu")
	req.Priority = new(int64(1))
	req.Rank = new(float64(1.5))
	req.Labels = new([]string{})
	req.Name = new("sample")
	expected := testDogDto()
	mockRepo.On("Create", mock.Anything, mock.AnythingOfType("models.Dog"), "alice").Return(expected, nil)

	result, err := service.Create(context.Background(), req, "alice")

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockRepo.AssertExpectations(t)

	stored := mockRepo.Calls[0].Arguments.Get(1).(models.Dog)
	assert.Len(t, stored.Id, 36)
	assert.False(t, stored.IsDeleted)
	assert.Equal(t, stored.CreatedTimestamp, stored.UpdatedTimestamp)
	assert.Regexp(t, `^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3}Z$`, stored.CreatedTimestamp)
	assert.Equal(t, req.TenantId, stored.TenantId)
	assert.Equal(t, req.Region, stored.Region)
	assert.Equal(t, req.Priority, stored.Priority)
	assert.Equal(t, req.Rank, stored.Rank)
	assert.Equal(t, req.Labels, stored.Labels)
	assert.Equal(t, req.Name, stored.Name)
}

func TestReplaceDog_AppliesTheRequestToTheStoredRecord(t *testing.T) {
	mockRepo, service := setupDogServiceTest()
	req := models.ReplaceDogRequest{Id: testDogID}
	req.TenantId = new("public")
	req.Priority = new(int64(1))
	req.Rank = new(float64(1.5))
	req.Labels = new([]string{})
	req.Name = new("sample")
	expected := testDogDto()
	mockRepo.On("Replace", mock.Anything, testDogID, "alice", mock.Anything).Return(expected, nil)

	result, err := service.Replace(context.Background(), req, "alice")

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	apply := mockRepo.Calls[0].Arguments.Get(3).(func(*models.Dog))
	var stored models.Dog
	apply(&stored)
	assert.Equal(t, req.TenantId, stored.TenantId)
	assert.Equal(t, req.Priority, stored.Priority)
	assert.Equal(t, req.Rank, stored.Rank)
	assert.Equal(t, req.Labels, stored.Labels)
	assert.Equal(t, req.Name, stored.Name)
}

func TestDeleteDog_ShouldCallRepositoryDelete(t *testing.T) {
	mockRepo, service := setupDogServiceTest()
	mockRepo.On("Delete", mock.Anything, testDogID, "alice").Return(nil)

	err := service.Delete(context.Background(), testDogID, "alice")

	assert.NoError(t, err)
	mockRepo.AssertCalled(t, "Delete", mock.Anything, testDogID, "alice")
}
