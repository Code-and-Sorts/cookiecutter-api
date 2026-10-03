package services

import (
	"context"
	"testing"

	"kittenclaws/models"

	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/mock"
)

const testKittenClawsID = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c"

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

func (m *mockKittenClawsRepository) Create(ctx context.Context, item models.KittenClaws, userID string) (*models.KittenClawsDto, error) {
	args := m.Called(ctx, item, userID)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.KittenClawsDto), args.Error(1)
}

func (m *mockKittenClawsRepository) Update(ctx context.Context, id, userID string, apply func(*models.KittenClaws)) (*models.KittenClawsDto, error) {
	args := m.Called(ctx, id, userID, apply)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.KittenClawsDto), args.Error(1)
}

func (m *mockKittenClawsRepository) Delete(ctx context.Context, id, userID string) error {
	args := m.Called(ctx, id, userID)
	return args.Error(0)
}

func setupKittenClawsServiceTest() (*mockKittenClawsRepository, KittenClawsService) {
	mockRepo := new(mockKittenClawsRepository)
	service := NewKittenClawsService(mockRepo)
	return mockRepo, service
}

func testKittenClawsDto() *models.KittenClawsDto {
	return &models.KittenClawsDto{BaseResponse: models.BaseResponse{Id: testKittenClawsID}}
}

func TestGetKittenClaws_ShouldReturnKittenClawsDto(t *testing.T) {
	mockRepo, service := setupKittenClawsServiceTest()
	expected := testKittenClawsDto()
	mockRepo.On("Get", mock.Anything, testKittenClawsID).Return(expected, nil)

	result, err := service.Get(context.Background(), testKittenClawsID)

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockRepo.AssertExpectations(t)
}

func TestGetKittenClawsList_ShouldReturnListOfKittenClawsDto(t *testing.T) {
	mockRepo, service := setupKittenClawsServiceTest()
	expected := []models.KittenClawsDto{*testKittenClawsDto()}
	mockRepo.On("GetList", mock.Anything, 50).Return(expected, nil)

	result, err := service.GetList(context.Background(), 50)

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockRepo.AssertExpectations(t)
}

func TestCreateKittenClaws_StoresANewRecordWithTheRequestFields(t *testing.T) {
	mockRepo, service := setupKittenClawsServiceTest()
	req := models.NewCreateKittenClawsRequest()
	req.Name = new("sample")
	expected := testKittenClawsDto()
	mockRepo.On("Create", mock.Anything, mock.AnythingOfType("models.KittenClaws"), "alice").Return(expected, nil)

	result, err := service.Create(context.Background(), req, "alice")

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockRepo.AssertExpectations(t)

	stored := mockRepo.Calls[0].Arguments.Get(1).(models.KittenClaws)
	assert.Len(t, stored.Id, 36)
	assert.False(t, stored.IsDeleted)
	assert.Equal(t, stored.CreatedTimestamp, stored.UpdatedTimestamp)
	assert.Regexp(t, `^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3}Z$`, stored.CreatedTimestamp)
	assert.Equal(t, req.Name, stored.Name)
}

func TestUpdateKittenClaws_AppliesTheRequestToTheStoredRecord(t *testing.T) {
	mockRepo, service := setupKittenClawsServiceTest()
	req := models.UpdateKittenClawsRequest{Id: testKittenClawsID}
	req.Sent = map[string]bool{"name": true}
	req.Name = new("sample")
	expected := testKittenClawsDto()
	mockRepo.On("Update", mock.Anything, testKittenClawsID, "alice", mock.Anything).Return(expected, nil)

	result, err := service.Update(context.Background(), req, "alice")

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	apply := mockRepo.Calls[0].Arguments.Get(3).(func(*models.KittenClaws))
	var stored models.KittenClaws
	apply(&stored)
	assert.Equal(t, req.Name, stored.Name)
}

func TestDeleteKittenClaws_ShouldCallRepositoryDelete(t *testing.T) {
	mockRepo, service := setupKittenClawsServiceTest()
	mockRepo.On("Delete", mock.Anything, testKittenClawsID, "alice").Return(nil)

	err := service.Delete(context.Background(), testKittenClawsID, "alice")

	assert.NoError(t, err)
	mockRepo.AssertCalled(t, "Delete", mock.Anything, testKittenClawsID, "alice")
}
