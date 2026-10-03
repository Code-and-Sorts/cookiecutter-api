package models

import (
	"encoding/json"
	"testing"

	"github.com/stretchr/testify/assert"
)

func sampleCat() Cat {
	item := Cat{BaseEntity: NewBaseEntity()}
	item.TenantId = new("public")
	item.Region = new("eu")
	item.Priority = new(int64(1))
	item.Rank = new(float64(1.5))
	item.Labels = new([]string{})
	item.Name = new("sample")
	item.Breed = new("tabby")
	item.AgeYears = new(int64(0))
	item.WeightKg = new(float64(1.5))
	item.Indoor = new(true)
	item.BirthDate = new("2026-01-01")
	item.MicrochipId = new("6f1c2a3b-4d5e-4f60-8a7b-000000000000")
	item.OwnerEmail = new("unknown@example.com")
	item.Website = new("https://example.com/items/1")
	item.TagCode = new("ABC-123")
	item.Tags = new([]string{})
	item.Scores = new([]int64{1})
	item.AdoptedAt = new(DateTime("2026-01-15T10:00:00.000Z"))
	item.LastVisit = new(DateTime("2026-01-01T00:00:00.000Z"))
	item.Notes = new("$none")
	return item
}

func TestCatDto_HoldsTheIdAndEveryShownField(t *testing.T) {
	data, err := json.Marshal(ToCatDto(sampleCat()))

	assert.NoError(t, err)
	properties := map[string]json.RawMessage{}
	assert.NoError(t, json.Unmarshal(data, &properties))
	assert.Len(t, properties, 20)
	assert.JSONEq(t, "\"public\"", string(properties["tenantId"]))
	assert.JSONEq(t, "\"eu\"", string(properties["region"]))
	assert.JSONEq(t, "1", string(properties["priority"]))
	assert.JSONEq(t, "1.5", string(properties["rank"]))
	assert.JSONEq(t, "[]", string(properties["labels"]))
	assert.JSONEq(t, "\"sample\"", string(properties["name"]))
	assert.JSONEq(t, "\"tabby\"", string(properties["breed"]))
	assert.JSONEq(t, "0", string(properties["ageYears"]))
	assert.JSONEq(t, "1.5", string(properties["weightKg"]))
	assert.JSONEq(t, "true", string(properties["indoor"]))
	assert.JSONEq(t, "\"2026-01-01\"", string(properties["birthDate"]))
	assert.JSONEq(t, "\"6f1c2a3b-4d5e-4f60-8a7b-000000000000\"", string(properties["microchipId"]))
	assert.JSONEq(t, "\"unknown@example.com\"", string(properties["ownerEmail"]))
	assert.JSONEq(t, "\"https://example.com/items/1\"", string(properties["website"]))
	assert.JSONEq(t, "\"ABC-123\"", string(properties["tagCode"]))
	assert.JSONEq(t, "[]", string(properties["tags"]))
	assert.JSONEq(t, "[1]", string(properties["scores"]))
	assert.JSONEq(t, "\"2026-01-15T10:00:00.000Z\"", string(properties["adoptedAt"]))
	assert.JSONEq(t, "\"2026-01-01T00:00:00.000Z\"", string(properties["lastVisit"]))
}

func TestCatDto_ReadsAMissingFieldAsItsStaticDefault(t *testing.T) {
	data, err := json.Marshal(ToCatDto(Cat{}))

	assert.NoError(t, err)
	properties := map[string]json.RawMessage{}
	assert.NoError(t, json.Unmarshal(data, &properties))
	assert.JSONEq(t, "\"public\"", string(properties["tenantId"]))
	assert.JSONEq(t, "\"eu\"", string(properties["region"]))
	assert.JSONEq(t, "null", string(properties["priority"]))
	assert.JSONEq(t, "null", string(properties["rank"]))
	assert.JSONEq(t, "[]", string(properties["labels"]))
	assert.JSONEq(t, "null", string(properties["name"]))
	assert.JSONEq(t, "\"tabby\"", string(properties["breed"]))
	assert.JSONEq(t, "0", string(properties["ageYears"]))
	assert.JSONEq(t, "null", string(properties["weightKg"]))
	assert.JSONEq(t, "true", string(properties["indoor"]))
	assert.JSONEq(t, "null", string(properties["birthDate"]))
	assert.JSONEq(t, "null", string(properties["microchipId"]))
	assert.JSONEq(t, "\"unknown@example.com\"", string(properties["ownerEmail"]))
	assert.JSONEq(t, "null", string(properties["website"]))
	assert.JSONEq(t, "null", string(properties["tagCode"]))
	assert.JSONEq(t, "[]", string(properties["tags"]))
	assert.JSONEq(t, "null", string(properties["scores"]))
	assert.JSONEq(t, "null", string(properties["adoptedAt"]))
	assert.JSONEq(t, "\"2026-01-01T00:00:00.000Z\"", string(properties["lastVisit"]))
}

func TestCreateCatRequest_DefaultsFieldsTheBodyLeavesOut(t *testing.T) {
	item := NewCreateCatRequest().ToEntity()

	assert.Len(t, item.Id, 36)
	assert.Nil(t, item.Name)
	assert.Equal(t, new("tabby"), item.Breed)
	assert.Equal(t, new(int64(0)), item.AgeYears)
	assert.Nil(t, item.WeightKg)
	assert.Equal(t, new(true), item.Indoor)
	assert.NotNil(t, item.BirthDate)
	assert.NotNil(t, item.MicrochipId)
	assert.Equal(t, new("unknown@example.com"), item.OwnerEmail)
	assert.Nil(t, item.Website)
	assert.Nil(t, item.TagCode)
	assert.Equal(t, new([]string{}), item.Tags)
	assert.Nil(t, item.Scores)
	assert.NotNil(t, item.AdoptedAt)
	assert.Equal(t, new(DateTime("2026-01-01T00:00:00.000Z")), item.LastVisit)
	assert.Equal(t, new("$none"), item.Notes)
}

func TestReplaceCatRequest_ResetsAcceptedFieldsAndKeepsTheRest(t *testing.T) {
	stored := sampleCat()
	created := stored.CreatedTimestamp

	NewReplaceCatRequest().ApplyTo(&stored)

	assert.Equal(t, created, stored.CreatedTimestamp)
	assert.Nil(t, stored.Name)
	assert.Equal(t, new("tabby"), stored.Breed)
	assert.Equal(t, new(int64(0)), stored.AgeYears)
	assert.Nil(t, stored.WeightKg)
	assert.Equal(t, new(true), stored.Indoor)
	assert.NotNil(t, stored.BirthDate)
	assert.Equal(t, new("6f1c2a3b-4d5e-4f60-8a7b-000000000000"), stored.MicrochipId)
	assert.Equal(t, new("unknown@example.com"), stored.OwnerEmail)
	assert.Nil(t, stored.Website)
	assert.Nil(t, stored.TagCode)
	assert.Equal(t, new([]string{}), stored.Tags)
	assert.Nil(t, stored.Scores)
	assert.NotNil(t, stored.AdoptedAt)
	assert.Equal(t, new(DateTime("2026-01-01T00:00:00.000Z")), stored.LastVisit)
	assert.Equal(t, new("$none"), stored.Notes)
}

func TestUpdateCatRequest_ChangesOnlyTheFieldsSent(t *testing.T) {
	stored := sampleCat()
	unchanged := stored

	UpdateCatRequest{BaseUpdateRequest: BaseUpdateRequest{Sent: map[string]bool{}}}.ApplyTo(&stored)
	assert.Equal(t, unchanged, stored)

	UpdateCatRequest{BaseUpdateRequest: BaseUpdateRequest{Sent: map[string]bool{"name": true}}}.ApplyTo(&stored)
	assert.Nil(t, stored.Name)
	stored = unchanged

	UpdateCatRequest{BaseUpdateRequest: BaseUpdateRequest{Sent: map[string]bool{"ageYears": true}}}.ApplyTo(&stored)
	assert.Nil(t, stored.AgeYears)
	stored = unchanged

	UpdateCatRequest{BaseUpdateRequest: BaseUpdateRequest{Sent: map[string]bool{"weightKg": true}}}.ApplyTo(&stored)
	assert.Nil(t, stored.WeightKg)
	stored = unchanged

	UpdateCatRequest{BaseUpdateRequest: BaseUpdateRequest{Sent: map[string]bool{"indoor": true}}}.ApplyTo(&stored)
	assert.Nil(t, stored.Indoor)
	stored = unchanged

	UpdateCatRequest{BaseUpdateRequest: BaseUpdateRequest{Sent: map[string]bool{"ownerEmail": true}}}.ApplyTo(&stored)
	assert.Nil(t, stored.OwnerEmail)
	stored = unchanged

	UpdateCatRequest{BaseUpdateRequest: BaseUpdateRequest{Sent: map[string]bool{"website": true}}}.ApplyTo(&stored)
	assert.Nil(t, stored.Website)
	stored = unchanged

	UpdateCatRequest{BaseUpdateRequest: BaseUpdateRequest{Sent: map[string]bool{"tags": true}}}.ApplyTo(&stored)
	assert.Nil(t, stored.Tags)
	stored = unchanged

	UpdateCatRequest{BaseUpdateRequest: BaseUpdateRequest{Sent: map[string]bool{"adoptedAt": true}}}.ApplyTo(&stored)
	assert.Nil(t, stored.AdoptedAt)
	stored = unchanged

	UpdateCatRequest{BaseUpdateRequest: BaseUpdateRequest{Sent: map[string]bool{"notes": true}}}.ApplyTo(&stored)
	assert.Nil(t, stored.Notes)
	stored = unchanged
}
