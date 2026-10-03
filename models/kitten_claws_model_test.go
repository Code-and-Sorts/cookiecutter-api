package models

import (
	"encoding/json"
	"testing"

	"github.com/stretchr/testify/assert"
)

func sampleKittenClaws() KittenClaws {
	item := KittenClaws{BaseEntity: NewBaseEntity()}
	item.Name = new("sample")
	return item
}

func TestKittenClawsDto_HoldsTheIdAndEveryShownField(t *testing.T) {
	data, err := json.Marshal(ToKittenClawsDto(sampleKittenClaws()))

	assert.NoError(t, err)
	properties := map[string]json.RawMessage{}
	assert.NoError(t, json.Unmarshal(data, &properties))
	assert.Len(t, properties, 2)
	assert.JSONEq(t, "\"sample\"", string(properties["name"]))
}

func TestKittenClawsDto_ReadsAMissingFieldAsItsStaticDefault(t *testing.T) {
	data, err := json.Marshal(ToKittenClawsDto(KittenClaws{}))

	assert.NoError(t, err)
	properties := map[string]json.RawMessage{}
	assert.NoError(t, json.Unmarshal(data, &properties))
	assert.JSONEq(t, "null", string(properties["name"]))
}

func TestCreateKittenClawsRequest_DefaultsFieldsTheBodyLeavesOut(t *testing.T) {
	item := NewCreateKittenClawsRequest().ToEntity()

	assert.Len(t, item.Id, 36)
	assert.Nil(t, item.Name)
}

func TestUpdateKittenClawsRequest_ChangesOnlyTheFieldsSent(t *testing.T) {
	stored := sampleKittenClaws()
	unchanged := stored

	UpdateKittenClawsRequest{BaseUpdateRequest: BaseUpdateRequest{Sent: map[string]bool{}}}.ApplyTo(&stored)
	assert.Equal(t, unchanged, stored)

	UpdateKittenClawsRequest{BaseUpdateRequest: BaseUpdateRequest{Sent: map[string]bool{"name": true}}}.ApplyTo(&stored)
	assert.Nil(t, stored.Name)
	stored = unchanged
}
