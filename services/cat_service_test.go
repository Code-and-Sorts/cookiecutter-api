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

func (m *mockCatRepository) Replace(ctx context.Context, id, userID string, apply func(*models.Cat)) (*models.CatDto, error) {
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
	req.TenantId = new("public")
	req.Region = new("eu")
	req.Priority = new(int64(1))
	req.Rank = new(float64(1.5))
	req.Labels = new([]string{})
	req.Name = new("sample")
	req.Breed = new("tabby")
	req.AgeYears = new(int64(0))
	req.WeightKg = new(float64(1.5))
	req.Indoor = new(true)
	req.BirthDate = new("2026-01-01")
	req.MicrochipId = new("6f1c2a3b-4d5e-4f60-8a7b-000000000000")
	req.OwnerEmail = new("unknown@example.com")
	req.Website = new("https://example.com/items/1")
	req.TagCode = new("ABC-123")
	req.Tags = new([]string{})
	req.Scores = new([]int64{1})
	req.AdoptedAt = new(models.DateTime("2026-01-15T10:00:00.000Z"))
	req.LastVisit = new(models.DateTime("2026-01-01T00:00:00.000Z"))
	req.Notes = new("$none")
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
	assert.Equal(t, req.TenantId, stored.TenantId)
	assert.Equal(t, req.Region, stored.Region)
	assert.Equal(t, req.Priority, stored.Priority)
	assert.Equal(t, req.Rank, stored.Rank)
	assert.Equal(t, req.Labels, stored.Labels)
	assert.Equal(t, req.Name, stored.Name)
	assert.Equal(t, req.Breed, stored.Breed)
	assert.Equal(t, req.AgeYears, stored.AgeYears)
	assert.Equal(t, req.WeightKg, stored.WeightKg)
	assert.Equal(t, req.Indoor, stored.Indoor)
	assert.Equal(t, req.BirthDate, stored.BirthDate)
	assert.Equal(t, req.MicrochipId, stored.MicrochipId)
	assert.Equal(t, req.OwnerEmail, stored.OwnerEmail)
	assert.Equal(t, req.Website, stored.Website)
	assert.Equal(t, req.TagCode, stored.TagCode)
	assert.Equal(t, req.Tags, stored.Tags)
	assert.Equal(t, req.Scores, stored.Scores)
	assert.Equal(t, req.AdoptedAt, stored.AdoptedAt)
	assert.Equal(t, req.LastVisit, stored.LastVisit)
	assert.Equal(t, req.Notes, stored.Notes)
}

func TestUpdateCat_AppliesTheRequestToTheStoredRecord(t *testing.T) {
	mockRepo, service := setupCatServiceTest()
	req := models.UpdateCatRequest{Id: testCatID}
	req.Sent = map[string]bool{"tenantId": true, "priority": true, "rank": true, "labels": true, "name": true, "ageYears": true, "weightKg": true, "indoor": true, "ownerEmail": true, "website": true, "tags": true, "adoptedAt": true, "notes": true}
	req.TenantId = new("public")
	req.Priority = new(int64(1))
	req.Rank = new(float64(1.5))
	req.Labels = new([]string{})
	req.Name = new("sample")
	req.AgeYears = new(int64(0))
	req.WeightKg = new(float64(1.5))
	req.Indoor = new(true)
	req.OwnerEmail = new("unknown@example.com")
	req.Website = new("https://example.com/items/1")
	req.Tags = new([]string{})
	req.AdoptedAt = new(models.DateTime("2026-01-15T10:00:00.000Z"))
	req.Notes = new("$none")
	expected := testCatDto()
	mockRepo.On("Update", mock.Anything, testCatID, "alice", mock.Anything).Return(expected, nil)

	result, err := service.Update(context.Background(), req, "alice")

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	apply := mockRepo.Calls[0].Arguments.Get(3).(func(*models.Cat))
	var stored models.Cat
	apply(&stored)
	assert.Equal(t, req.TenantId, stored.TenantId)
	assert.Equal(t, req.Priority, stored.Priority)
	assert.Equal(t, req.Rank, stored.Rank)
	assert.Equal(t, req.Labels, stored.Labels)
	assert.Equal(t, req.Name, stored.Name)
	assert.Equal(t, req.AgeYears, stored.AgeYears)
	assert.Equal(t, req.WeightKg, stored.WeightKg)
	assert.Equal(t, req.Indoor, stored.Indoor)
	assert.Equal(t, req.OwnerEmail, stored.OwnerEmail)
	assert.Equal(t, req.Website, stored.Website)
	assert.Equal(t, req.Tags, stored.Tags)
	assert.Equal(t, req.AdoptedAt, stored.AdoptedAt)
	assert.Equal(t, req.Notes, stored.Notes)
}

func TestReplaceCat_AppliesTheRequestToTheStoredRecord(t *testing.T) {
	mockRepo, service := setupCatServiceTest()
	req := models.ReplaceCatRequest{Id: testCatID}
	req.TenantId = new("public")
	req.Priority = new(int64(1))
	req.Rank = new(float64(1.5))
	req.Labels = new([]string{})
	req.Name = new("sample")
	req.Breed = new("tabby")
	req.AgeYears = new(int64(0))
	req.WeightKg = new(float64(1.5))
	req.Indoor = new(true)
	req.BirthDate = new("2026-01-01")
	req.OwnerEmail = new("unknown@example.com")
	req.Website = new("https://example.com/items/1")
	req.TagCode = new("ABC-123")
	req.Tags = new([]string{})
	req.Scores = new([]int64{1})
	req.AdoptedAt = new(models.DateTime("2026-01-15T10:00:00.000Z"))
	req.LastVisit = new(models.DateTime("2026-01-01T00:00:00.000Z"))
	req.Notes = new("$none")
	expected := testCatDto()
	mockRepo.On("Replace", mock.Anything, testCatID, "alice", mock.Anything).Return(expected, nil)

	result, err := service.Replace(context.Background(), req, "alice")

	assert.NoError(t, err)
	assert.Equal(t, expected, result)
	apply := mockRepo.Calls[0].Arguments.Get(3).(func(*models.Cat))
	var stored models.Cat
	apply(&stored)
	assert.Equal(t, req.TenantId, stored.TenantId)
	assert.Equal(t, req.Priority, stored.Priority)
	assert.Equal(t, req.Rank, stored.Rank)
	assert.Equal(t, req.Labels, stored.Labels)
	assert.Equal(t, req.Name, stored.Name)
	assert.Equal(t, req.Breed, stored.Breed)
	assert.Equal(t, req.AgeYears, stored.AgeYears)
	assert.Equal(t, req.WeightKg, stored.WeightKg)
	assert.Equal(t, req.Indoor, stored.Indoor)
	assert.Equal(t, req.BirthDate, stored.BirthDate)
	assert.Equal(t, req.OwnerEmail, stored.OwnerEmail)
	assert.Equal(t, req.Website, stored.Website)
	assert.Equal(t, req.TagCode, stored.TagCode)
	assert.Equal(t, req.Tags, stored.Tags)
	assert.Equal(t, req.Scores, stored.Scores)
	assert.Equal(t, req.AdoptedAt, stored.AdoptedAt)
	assert.Equal(t, req.LastVisit, stored.LastVisit)
	assert.Equal(t, req.Notes, stored.Notes)
}

func TestDeleteCat_ShouldCallRepositoryDelete(t *testing.T) {
	mockRepo, service := setupCatServiceTest()
	mockRepo.On("Delete", mock.Anything, testCatID, "alice").Return(nil)

	err := service.Delete(context.Background(), testCatID, "alice")

	assert.NoError(t, err)
	mockRepo.AssertCalled(t, "Delete", mock.Anything, testCatID, "alice")
}
