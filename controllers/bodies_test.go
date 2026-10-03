package controllers

import (
	"encoding/json"
	"testing"

	"github.com/stretchr/testify/assert"
)

func withProperty(body, name, value string) string {
	properties := map[string]json.RawMessage{}
	_ = json.Unmarshal([]byte(body), &properties)
	if value == "" {
		delete(properties, name)
	} else {
		properties[name] = json.RawMessage(value)
	}
	data, _ := json.Marshal(properties)
	return string(data)
}

func requestProperties(t *testing.T, req any) map[string]json.RawMessage {
	t.Helper()
	data, err := json.Marshal(req)
	assert.NoError(t, err)
	properties := map[string]json.RawMessage{}
	assert.NoError(t, json.Unmarshal(data, &properties))
	return properties
}

// Bodies an operation must reject: server-set, unknown or refused properties, rejected values, missing required ones.
func invalidBodies(valid string, required, refused []string, rejected [][2]string) []string {
	bodies := []string{`[]`, `null`, `{not json`, withProperty(valid, "not_a_field", `1`)}
	for _, property := range [][2]string{
		{"id", "\"6f1c2a3b-4d5e-4f60-8a7b-000000000000\""},
		{"isDeleted", "false"},
		{"createdTimestamp", "\"2026-01-15T10:00:00.000Z\""},
		{"updatedTimestamp", "\"2026-01-15T10:00:00.000Z\""},
		{"createdBy", "\"sample\""},
		{"updatedBy", "\"sample\""},
	} {
		bodies = append(bodies, withProperty(valid, property[0], property[1]))
	}
	for _, name := range refused {
		bodies = append(bodies, withProperty(valid, name, `"x"`))
	}
	properties := map[string]json.RawMessage{}
	_ = json.Unmarshal([]byte(valid), &properties)
	for _, value := range rejected {
		if _, ok := properties[value[0]]; ok {
			bodies = append(bodies, withProperty(valid, value[0], value[1]))
		}
	}
	for _, name := range required {
		bodies = append(bodies, withProperty(valid, name, ""))
	}
	return bodies
}
