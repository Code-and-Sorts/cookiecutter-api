package models

import (
	"encoding/json"
	"testing"

	"github.com/stretchr/testify/assert"
)

func TestDateTime_StoresUTCWithMilliseconds(t *testing.T) {
	cases := [][2]string{
		{`"2026-01-31T11:30:00.1239+02:00"`, "2026-01-31T09:30:00.123Z"},
		{`"2026-01-31T00:30:00.123456789+23:59"`, "2026-01-30T00:31:00.123Z"},
		{`"2026-01-31T23:30:00-23:59"`, "2026-02-01T23:29:00.000Z"},
		{`"0001-01-01T00:00:00-00:00"`, "0001-01-01T00:00:00.000Z"},
		{`"9999-12-31T23:59:59.999Z"`, "9999-12-31T23:59:59.999Z"},
	}
	for _, c := range cases {
		var value DateTime
		assert.NoError(t, json.Unmarshal([]byte(c[0]), &value))
		assert.Equal(t, DateTime(c[1]), value)
	}
}

func TestDateTime_RejectsInvalidValues(t *testing.T) {
	for _, raw := range []string{`42`, `"not a date-time"`, `"2026-01-31T09:30:00"`, `"2026-01-31T24:00:00Z"`, `"2026-01-31T23:59:60Z"`, `"2026-01-31T09:30:00+14:60"`, `"0000-12-31T23:00:00-01:00"`, `"0001-01-01T00:00:00+01:00"`, `"9999-12-31T23:59:59-01:00"`} {
		var value DateTime
		assert.Error(t, json.Unmarshal([]byte(raw), &value), raw)
	}
}

func TestUniqueDateTimes_ComparesInstantsInUTC(t *testing.T) {
	var values UniqueDateTimes
	assert.Error(t, json.Unmarshal([]byte(`["2026-01-31T11:30:00.1239+02:00", "2026-01-31T09:30:00.123Z"]`), &values))
	assert.NoError(t, json.Unmarshal([]byte(`["2026-01-31T11:30:00.1239+02:00", "2026-01-31T09:30:00Z"]`), &values))
	assert.Equal(t, UniqueDateTimes{"2026-01-31T09:30:00.123Z", "2026-01-31T09:30:00.000Z"}, values)
	assert.Error(t, json.Unmarshal([]byte(`[42]`), &values))
}

func TestClock_FormatsUTC(t *testing.T) {
	assert.Regexp(t, `^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3}Z$`, Now())
	assert.Regexp(t, `^\d{4}-\d{2}-\d{2}$`, Today())
}
