package services

import (
	"context"
	"testing"

	"kittenclaws/models"

	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/mock"
)

const testCatID = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c"

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

func (m *mockCatRepository) Update(ctx context.Context, id, userID string, apply func(*models.Cat)) (*models.CatDto, error) {
	args := m.Called(ctx, id, userID, apply)
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

func testCatDto() *models.CatDto {
	return &models.CatDto{BaseResponse: models.BaseResponse{Id: testCatID}}
}

func TestGetCat_ShouldReturnCatDto(t *testing.T) {
	mockRepo, service := setupCatServiceTest()
	expected := testCatDto()
	mockRepo.On("Get", mock.Anything, testCatID).Return(expected, nil)

	result, err := service.Get(context.Background(), testCatID)

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockRepo.AssertExpectations(t)
}

func TestGetCatList_ShouldReturnListOfCatDto(t *testing.T) {
	mockRepo, service := setupCatServiceTest()
	expected := []models.CatDto{*testCatDto()}
	mockRepo.On("GetList", mock.Anything, 50).Return(expected, nil)

	result, err := service.GetList(context.Background(), 50)

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockRepo.AssertExpectations(t)
}

func TestCreateCat_StoresANewRecordWithTheRequestFields(t *testing.T) {
	mockRepo, service := setupCatServiceTest()
	req := models.NewCreateCatRequest()
	req.Name = new("sample")
	expected := testCatDto()
	mockRepo.On("Create", mock.Anything, mock.AnythingOfType("models.Cat"), "alice").Return(expected, nil)

	result, err := service.Create(context.Background(), req, "alice")

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockRepo.AssertExpectations(t)

	stored := mockRepo.Calls[0].Arguments.Get(1).(models.Cat)
	assert.Len(t, stored.Id, 36)
	assert.False(t, stored.IsDeleted)
	assert.Equal(t, stored.CreatedTimestamp, stored.UpdatedTimestamp)
	assert.Regexp(t, `^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3}Z$`, stored.CreatedTimestamp)
	assert.Equal(t, req.Name, stored.Name)
}

func TestUpdateCat_AppliesTheRequestToTheStoredRecord(t *testing.T) {
	mockRepo, service := setupCatServiceTest()
	req := models.UpdateCatRequest{Id: testCatID}
	req.Sent = map[string]bool{"name": true}
	req.Name = new("sample")
	expected := testCatDto()
	mockRepo.On("Update", mock.Anything, testCatID, "alice", mock.Anything).Return(expected, nil)

	result, err := service.Update(context.Background(), req, "alice")

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	apply := mockRepo.Calls[0].Arguments.Get(3).(func(*models.Cat))
	var stored models.Cat
	apply(&stored)
	assert.Equal(t, req.Name, stored.Name)
}

func TestDeleteCat_ShouldCallRepositoryDelete(t *testing.T) {
	mockRepo, service := setupCatServiceTest()
	mockRepo.On("Delete", mock.Anything, testCatID, "alice").Return(nil)

	err := service.Delete(context.Background(), testCatID, "alice")

	assert.NoError(t, err)
	mockRepo.AssertCalled(t, "Delete", mock.Anything, testCatID, "alice")
}
