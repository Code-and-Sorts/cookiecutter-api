package models

import (
	"encoding/json"
	"testing"

	"github.com/stretchr/testify/assert"
)

func sampleVisit() Visit {
	item := Visit{BaseEntity: NewBaseEntity()}
	item.TenantId = new("public")
	item.Region = new("eu")
	item.Priority = new(int64(1))
	item.Rank = new(float64(1.5))
	item.Labels = new([]string{})
	item.Reason = new("sample")
	item.VisitedOn = new("2026-01-01")
	item.Cost = new(float64(1.5))
	item.Paid = new(false)
	item.CheckedAt = new([]DateTime{"2026-01-15T10:00:00.000Z"})
	return item
}

func TestVisitDto_HoldsTheIdAndEveryShownField(t *testing.T) {
	data, err := json.Marshal(ToVisitDto(sampleVisit()))

	assert.NoError(t, err)
	properties := map[string]json.RawMessage{}
	assert.NoError(t, json.Unmarshal(data, &properties))
	assert.Len(t, properties, 11)
	assert.JSONEq(t, "\"public\"", string(properties["tenantId"]))
	assert.JSONEq(t, "\"eu\"", string(properties["region"]))
	assert.JSONEq(t, "1", string(properties["priority"]))
	assert.JSONEq(t, "1.5", string(properties["rank"]))
	assert.JSONEq(t, "[]", string(properties["labels"]))
	assert.JSONEq(t, "\"sample\"", string(properties["reason"]))
	assert.JSONEq(t, "\"2026-01-01\"", string(properties["visitedOn"]))
	assert.JSONEq(t, "1.5", string(properties["cost"]))
	assert.JSONEq(t, "false", string(properties["paid"]))
	assert.JSONEq(t, "[\"2026-01-15T10:00:00.000Z\"]", string(properties["checkedAt"]))
}

func TestVisitDto_ReadsAMissingFieldAsItsStaticDefault(t *testing.T) {
	data, err := json.Marshal(ToVisitDto(Visit{}))

	assert.NoError(t, err)
	properties := map[string]json.RawMessage{}
	assert.NoError(t, json.Unmarshal(data, &properties))
	assert.JSONEq(t, "\"public\"", string(properties["tenantId"]))
	assert.JSONEq(t, "\"eu\"", string(properties["region"]))
	assert.JSONEq(t, "null", string(properties["priority"]))
	assert.JSONEq(t, "null", string(properties["rank"]))
	assert.JSONEq(t, "[]", string(properties["labels"]))
	assert.JSONEq(t, "null", string(properties["reason"]))
	assert.JSONEq(t, "null", string(properties["visitedOn"]))
	assert.JSONEq(t, "null", string(properties["cost"]))
	assert.JSONEq(t, "false", string(properties["paid"]))
	assert.JSONEq(t, "null", string(properties["checkedAt"]))
}

func TestCreateVisitRequest_DefaultsFieldsTheBodyLeavesOut(t *testing.T) {
	item := NewCreateVisitRequest().ToEntity()

	assert.Len(t, item.Id, 36)
	assert.Nil(t, item.Reason)
	assert.Nil(t, item.VisitedOn)
	assert.Nil(t, item.Cost)
	assert.Equal(t, new(false), item.Paid)
	assert.Nil(t, item.CheckedAt)
}

func TestUpdateVisitRequest_ChangesOnlyTheFieldsSent(t *testing.T) {
	stored := sampleVisit()
	unchanged := stored

	UpdateVisitRequest{BaseUpdateRequest: BaseUpdateRequest{Sent: map[string]bool{}}}.ApplyTo(&stored)
	assert.Equal(t, unchanged, stored)

	UpdateVisitRequest{BaseUpdateRequest: BaseUpdateRequest{Sent: map[string]bool{"visitedOn": true}}}.ApplyTo(&stored)
	assert.Nil(t, stored.VisitedOn)
	stored = unchanged

	UpdateVisitRequest{BaseUpdateRequest: BaseUpdateRequest{Sent: map[string]bool{"cost": true}}}.ApplyTo(&stored)
	assert.Nil(t, stored.Cost)
	stored = unchanged

	UpdateVisitRequest{BaseUpdateRequest: BaseUpdateRequest{Sent: map[string]bool{"paid": true}}}.ApplyTo(&stored)
	assert.Nil(t, stored.Paid)
	stored = unchanged

	UpdateVisitRequest{BaseUpdateRequest: BaseUpdateRequest{Sent: map[string]bool{"checkedAt": true}}}.ApplyTo(&stored)
	assert.Nil(t, stored.CheckedAt)
	stored = unchanged
}
