{%- set client = (base_fields | rejectattr("system") | list) + (path_resources | map(attribute="fields") | sum(start=[])) -%}
{%- set read_defaults = client | rejectattr("hidden") | rejectattr("nullable") | selectattr("has_default") | rejectattr("dynamic") | rejectattr("default", "none") | list -%}
package models

import (
	"encoding/json"
	"testing"

	"github.com/stretchr/testify/assert"
)

func TestDateTime_StoresUTCWithMilliseconds(t *testing.T) {
	cases := [][2]string{
		{`"2026-01-31T09:30:00Z"`, "2026-01-31T09:30:00.000Z"},
		{`"2026-01-31T11:30:00.1239+02:00"`, "2026-01-31T09:30:00.123Z"},
		{`"2026-01-31T00:30:00.123456789-01:00"`, "2026-01-31T01:30:00.123Z"},
		{`"not a date-time"`, "not a date-time"},
	}
	for _, c := range cases {
		var value DateTime
		assert.NoError(t, json.Unmarshal([]byte(c[0]), &value))
		assert.Equal(t, DateTime(c[1]), value)
	}

	var value DateTime
	assert.Error(t, json.Unmarshal([]byte(`42`), &value))
}

func TestClock_FormatsUTC(t *testing.T) {
	assert.Regexp(t, `^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3}Z$`, Now())
	assert.Regexp(t, `^\d{4}-\d{2}-\d{2}$`, Today())
}
{%- if read_defaults %}

func TestOrDefault_FillsOnlyAMissingValue(t *testing.T) {
	assert.Equal(t, "fallback", *orDefault(nil, "fallback"))
	assert.Equal(t, "stored", *orDefault(new("stored"), "fallback"))
}
{%- endif %}
