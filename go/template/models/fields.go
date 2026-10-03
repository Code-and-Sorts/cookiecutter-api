{%- set client = (base_fields | rejectattr("system") | list) + (path_resources | map(attribute="fields") | sum(start=[])) -%}
{%- set read_defaults = client | rejectattr("hidden") | rejectattr("nullable") | selectattr("has_default") | rejectattr("dynamic") | rejectattr("default", "none") | list -%}
package models

import (
	"encoding/json"
	"time"
)

const TimestampLayout = "2006-01-02T15:04:05.000Z"

// DateTime accepts any RFC 3339 offset and holds the value in UTC with milliseconds, like the system timestamps.
type DateTime string

func (d *DateTime) UnmarshalJSON(data []byte) error {
	var value string
	if err := json.Unmarshal(data, &value); err != nil {
		return err
	}
	*d = DateTime(normalizeDateTime(value))
	return nil
}

// Request schemas check the format first, so a value that does not parse is kept as stored.
func normalizeDateTime(value string) string {
	parsed, err := time.Parse(time.RFC3339Nano, value)
	if err != nil {
		return value
	}
	return parsed.UTC().Format(TimestampLayout)
}

func Now() string {
	return time.Now().UTC().Format(TimestampLayout)
}

func Today() string {
	return time.Now().UTC().Format(time.DateOnly)
}
{%- if read_defaults %}

// A stored record without the field reads as its static default.
func orDefault[T any](value *T, fallback T) *T {
	if value == nil {
		return &fallback
	}
	return value
}
{%- endif %}
