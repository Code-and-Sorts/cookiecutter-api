package models

import (
	"encoding/json"
	"testing"

	"github.com/stretchr/testify/assert"
)

func TestToCatDto_ReturnsOnlyTheResponseFields(t *testing.T) {
	item := Cat{
		BaseEntity: BaseEntity{
			Id:               "0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c",
			CreatedTimestamp: "2026-01-01T00:00:00.000Z",
			CreatedBy:        "alice",
			UpdatedTimestamp: "2026-01-02T00:00:00.000Z",
			UpdatedBy:        "bob",
		},
		Name: "Fake",
	}

	body, err := json.Marshal(ToCatDto(item))

	assert.NoError(t, err)
	assert.Equal(t, `{"id":"0f3a7ff7-a601-4d23-b33c-7f8f18b57a4c","name":"Fake","createdTimestamp":"2026-01-01T00:00:00.000Z","createdBy":"alice","updatedTimestamp":"2026-01-02T00:00:00.000Z","updatedBy":"bob"}`, string(body))
}
