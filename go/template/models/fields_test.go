{%- set client = client_base_fields + (path_resources | map(attribute="fields") | sum(start=[])) -%}
{%- from 'shared/_fields.jinja' import DATE_TIME_CASES, DATE_TIME_EXAMPLE, INVALID_DATE_TIMES -%}
{%- set read_defaults = client | rejectattr("hidden") | selectattr("has_read_default") | list -%}
package models

import (
	"encoding/json"
	"testing"

	"github.com/stretchr/testify/assert"
)

func TestDateTime_StoresUTCWithMilliseconds(t *testing.T) {
	cases := [][2]string{
{%- for case in DATE_TIME_CASES %}
		{`"{{ case.sent }}"`, "{{ case.stored }}"},
{%- endfor %}
	}
	for _, c := range cases {
		var value DateTime
		assert.NoError(t, json.Unmarshal([]byte(c[0]), &value))
		assert.Equal(t, DateTime(c[1]), value)
	}
}

func TestDateTime_RejectsInvalidValues(t *testing.T) {
	for _, raw := range []string{`42`, `"not a date-time"`{% for value in INVALID_DATE_TIMES %}, `"{{ value }}"`{% endfor %}} {
		var value DateTime
		assert.Error(t, json.Unmarshal([]byte(raw), &value), raw)
	}
}

func TestUniqueDateTimes_ComparesInstantsInUTC(t *testing.T) {
	var values UniqueDateTimes
	assert.Error(t, json.Unmarshal([]byte(`["{{ DATE_TIME_EXAMPLE.sent }}", "{{ DATE_TIME_EXAMPLE.stored }}"]`), &values))
	assert.NoError(t, json.Unmarshal([]byte(`["{{ DATE_TIME_EXAMPLE.sent }}", "2026-01-31T09:30:00Z"]`), &values))
	assert.Equal(t, UniqueDateTimes{"{{ DATE_TIME_EXAMPLE.stored }}", "2026-01-31T09:30:00.000Z"}, values)
	assert.Error(t, json.Unmarshal([]byte(`[42]`), &values))
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
