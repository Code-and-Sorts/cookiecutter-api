package services

import (
	"context"
	"testing"

	"kittenclaws/models"

	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/mock"
)

const testVisitID = "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c"

type mockVisitRepository struct {
	mock.Mock
}

func (m *mockVisitRepository) Get(ctx context.Context, id string) (*models.VisitDto, error) {
	args := m.Called(ctx, id)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.VisitDto), args.Error(1)
}

func (m *mockVisitRepository) Create(ctx context.Context, item models.Visit, userID string) (*models.VisitDto, error) {
	args := m.Called(ctx, item, userID)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.VisitDto), args.Error(1)
}

func (m *mockVisitRepository) Update(ctx context.Context, id, userID string, apply func(*models.Visit)) (*models.VisitDto, error) {
	args := m.Called(ctx, id, userID, apply)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.VisitDto), args.Error(1)
}

func setupVisitServiceTest() (*mockVisitRepository, VisitService) {
	mockRepo := new(mockVisitRepository)
	service := NewVisitService(mockRepo)
	return mockRepo, service
}

func testVisitDto() *models.VisitDto {
	return &models.VisitDto{BaseResponse: models.BaseResponse{Id: testVisitID}}
}

func TestGetVisit_ShouldReturnVisitDto(t *testing.T) {
	mockRepo, service := setupVisitServiceTest()
	expected := testVisitDto()
	mockRepo.On("Get", mock.Anything, testVisitID).Return(expected, nil)

	result, err := service.Get(context.Background(), testVisitID)

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockRepo.AssertExpectations(t)
}

func TestCreateVisit_StoresANewRecordWithTheRequestFields(t *testing.T) {
	mockRepo, service := setupVisitServiceTest()
	req := models.NewCreateVisitRequest()
	req.TenantId = new("public")
	req.Region = new("eu")
	req.Priority = new(int64(1))
	req.Rank = new(float64(1.5))
	req.Labels = new([]string{})
	req.Reason = new("sample")
	req.VisitedOn = new("2026-01-01")
	req.Cost = new(float64(1.5))
	expected := testVisitDto()
	mockRepo.On("Create", mock.Anything, mock.AnythingOfType("models.Visit"), "alice").Return(expected, nil)

	result, err := service.Create(context.Background(), req, "alice")

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	mockRepo.AssertExpectations(t)

	stored := mockRepo.Calls[0].Arguments.Get(1).(models.Visit)
	assert.Len(t, stored.Id, 36)
	assert.False(t, stored.IsDeleted)
	assert.Equal(t, stored.CreatedTimestamp, stored.UpdatedTimestamp)
	assert.Regexp(t, `^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3}Z$`, stored.CreatedTimestamp)
	assert.Equal(t, req.TenantId, stored.TenantId)
	assert.Equal(t, req.Region, stored.Region)
	assert.Equal(t, req.Priority, stored.Priority)
	assert.Equal(t, req.Rank, stored.Rank)
	assert.Equal(t, req.Labels, stored.Labels)
	assert.Equal(t, req.Reason, stored.Reason)
	assert.Equal(t, req.VisitedOn, stored.VisitedOn)
	assert.Equal(t, req.Cost, stored.Cost)
}

func TestUpdateVisit_AppliesTheRequestToTheStoredRecord(t *testing.T) {
	mockRepo, service := setupVisitServiceTest()
	req := models.UpdateVisitRequest{Id: testVisitID}
	req.Sent = map[string]bool{"tenantId": true, "priority": true, "rank": true, "labels": true, "visitedOn": true, "cost": true, "paid": true, "checkedAt": true}
	req.TenantId = new("public")
	req.Priority = new(int64(1))
	req.Rank = new(float64(1.5))
	req.Labels = new([]string{})
	req.VisitedOn = new("2026-01-01")
	req.Cost = new(float64(1.5))
	req.Paid = new(false)
	req.CheckedAt = new([]models.DateTime{"2026-01-15T10:00:00.000Z"})
	expected := testVisitDto()
	mockRepo.On("Update", mock.Anything, testVisitID, "alice", mock.Anything).Return(expected, nil)

	result, err := service.Update(context.Background(), req, "alice")

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	apply := mockRepo.Calls[0].Arguments.Get(3).(func(*models.Visit))
	var stored models.Visit
	apply(&stored)
	assert.Equal(t, req.TenantId, stored.TenantId)
	assert.Equal(t, req.Priority, stored.Priority)
	assert.Equal(t, req.Rank, stored.Rank)
	assert.Equal(t, req.Labels, stored.Labels)
	assert.Equal(t, req.VisitedOn, stored.VisitedOn)
	assert.Equal(t, req.Cost, stored.Cost)
	assert.Equal(t, req.Paid, stored.Paid)
	assert.Equal(t, req.CheckedAt, stored.CheckedAt)
}
