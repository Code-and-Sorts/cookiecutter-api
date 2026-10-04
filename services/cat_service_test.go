package services

import (
	"context"
	"testing"

	"kittenclaws/models"

	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/mock"
)

type mockCatRepository struct {
	mock.Mock
}

func (m *mockCatRepository) Get(ctx context.Context, id string) (*models.CatDto, error) {
	args := m.Called(ctx, id)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.CatDto), args.Error(1)
}

func (m *mockCatRepository) GetList(ctx context.Context, limit int) ([]models.CatDto, error) {
	args := m.Called(ctx, limit)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).([]models.CatDto), args.Error(1)
}

func (m *mockCatRepository) Create(ctx context.Context, item models.Cat, userID string) (*models.CatDto, error) {
	args := m.Called(ctx, item, userID)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.CatDto), args.Error(1)
}

func (m *mockCatRepository) Update(ctx context.Context, item models.Cat, userID string) (*models.CatDto, error) {
	args := m.Called(ctx, item, userID)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.CatDto), args.Error(1)
}

func (m *mockCatRepository) Delete(ctx context.Context, id, userID string) error {
	args := m.Called(ctx, id, userID)
	return args.Error(0)
}

func setupCatServiceTest() (*mockCatRepository, CatService) {
	mockRepo := new(mockCatRepository)
	service := NewCatService(mockRepo)
	return mockRepo, service
}

func TestGetCat_ShouldReturnCatDto(t *testing.T) {
	mockRepo, service := setupCatServiceTest()
	id := "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c"
	expected := &models.CatDto{Id: id, Name: "mockCat"}
	mockRepo.On("Get", mock.Anything, id).Return(expected, nil)

	result, err := service.Get(context.Background(), id)

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockRepo.AssertExpectations(t)
}

func TestGetCatList_ShouldReturnListOfCatDto(t *testing.T) {
	mockRepo, service := setupCatServiceTest()
	expected := []models.CatDto{
		{Id: "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name: "mockCat1"},
		{Id: "5615ff05-3032-4459-88ad-b6a4c3e51ca0", Name: "mockCat2"},
	}
	mockRepo.On("GetList", mock.Anything, 50).Return(expected, nil)

	result, err := service.GetList(context.Background(), 50)

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockRepo.AssertExpectations(t)
}

func TestCreateCat_ShouldReturnCreatedCatDto(t *testing.T) {
	mockRepo, service := setupCatServiceTest()
	createRequest := models.CreateCatRequest{Name: "mockCreateCat"}
	expected := &models.CatDto{Id: "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name: "mockCreateCat"}
	mockRepo.On("Create", mock.Anything, mock.AnythingOfType("models.Cat"), "alice").Return(expected, nil)

	result, err := service.Create(context.Background(), createRequest, "alice")

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockRepo.AssertExpectations(t)

	stored := mockRepo.Calls[0].Arguments.Get(1).(models.Cat)
	assert.Len(t, stored.Id, 36)
	assert.Equal(t, "mockCreateCat", stored.Name)
	assert.False(t, stored.IsDeleted)
	assert.Equal(t, stored.CreatedTimestamp, stored.UpdatedTimestamp)
	assert.Regexp(t, `^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3}Z$`, stored.CreatedTimestamp)
}

func TestUpdateCat_ShouldReturnUpdatedCatDto(t *testing.T) {
	mockRepo, service := setupCatServiceTest()
	updateRequest := models.UpdateCatRequest{Id: "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name: "mockUpdateCat"}
	expected := &models.CatDto{Id: "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c", Name: "mockUpdateCat"}
	mockRepo.On("Update", mock.Anything, mock.AnythingOfType("models.Cat"), "alice").Return(expected, nil)

	result, err := service.Update(context.Background(), updateRequest, "alice")

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockRepo.AssertExpectations(t)
}

func TestDeleteCat_ShouldCallRepositoryDelete(t *testing.T) {
	mockRepo, service := setupCatServiceTest()
	id := "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c"
	mockRepo.On("Delete", mock.Anything, id, "alice").Return(nil)

	err := service.Delete(context.Background(), id, "alice")

	assert.NoError(t, err)
	mockRepo.AssertCalled(t, "Delete", mock.Anything, id, "alice")
}
