package models

import (
	"encoding/json"
	"testing"

	"github.com/stretchr/testify/assert"
)

func sampleDog() Dog {
	item := Dog{BaseEntity: NewBaseEntity()}
	item.Name = new("sample")
	return item
}

func TestDogDto_HoldsTheIdAndEveryShownField(t *testing.T) {
	data, err := json.Marshal(ToDogDto(sampleDog()))

	assert.NoError(t, err)
	properties := map[string]json.RawMessage{}
	assert.NoError(t, json.Unmarshal(data, &properties))
	assert.Len(t, properties, 2)
	assert.JSONEq(t, "\"sample\"", string(properties["name"]))
}

func TestDogDto_ReadsAMissingFieldAsItsStaticDefault(t *testing.T) {
	data, err := json.Marshal(ToDogDto(Dog{}))

	assert.NoError(t, err)
	properties := map[string]json.RawMessage{}
	assert.NoError(t, json.Unmarshal(data, &properties))
	assert.JSONEq(t, "null", string(properties["name"]))
}

func TestCreateDogRequest_DefaultsFieldsTheBodyLeavesOut(t *testing.T) {
	item := NewCreateDogRequest().ToEntity()

	assert.Len(t, item.Id, 36)
	assert.Nil(t, item.Name)
}

func TestReplaceDogRequest_ResetsAcceptedFieldsAndKeepsTheRest(t *testing.T) {
	stored := sampleDog()
	created := stored.CreatedTimestamp

	NewReplaceDogRequest().ApplyTo(&stored)

	assert.Equal(t, created, stored.CreatedTimestamp)
	assert.Nil(t, stored.Name)
}
