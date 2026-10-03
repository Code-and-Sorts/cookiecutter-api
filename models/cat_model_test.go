package models

import (
	"encoding/json"
	"testing"

	"github.com/stretchr/testify/assert"
)

func sampleCat() Cat {
	item := Cat{BaseEntity: NewBaseEntity()}
	item.Name = new("sample")
	return item
}

func TestCatDto_HoldsTheIdAndEveryShownField(t *testing.T) {
	data, err := json.Marshal(ToCatDto(sampleCat()))

	assert.NoError(t, err)
	properties := map[string]json.RawMessage{}
	assert.NoError(t, json.Unmarshal(data, &properties))
	assert.Len(t, properties, 2)
	assert.JSONEq(t, "\"sample\"", string(properties["name"]))
}

func TestCatDto_ReadsAMissingFieldAsItsStaticDefault(t *testing.T) {
	data, err := json.Marshal(ToCatDto(Cat{}))

	assert.NoError(t, err)
	properties := map[string]json.RawMessage{}
	assert.NoError(t, json.Unmarshal(data, &properties))
	assert.JSONEq(t, "null", string(properties["name"]))
}

func TestCreateCatRequest_DefaultsFieldsTheBodyLeavesOut(t *testing.T) {
	item := NewCreateCatRequest().ToEntity()

	assert.Len(t, item.Id, 36)
	assert.Nil(t, item.Name)
}

func TestUpdateCatRequest_ChangesOnlyTheFieldsSent(t *testing.T) {
	stored := sampleCat()
	unchanged := stored

	UpdateCatRequest{BaseUpdateRequest: BaseUpdateRequest{Sent: map[string]bool{}}}.ApplyTo(&stored)
	assert.Equal(t, unchanged, stored)

	UpdateCatRequest{BaseUpdateRequest: BaseUpdateRequest{Sent: map[string]bool{"name": true}}}.ApplyTo(&stored)
	assert.Nil(t, stored.Name)
	stored = unchanged
}
