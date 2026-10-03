{%- set client = client_base_fields + (path_resources | map(attribute="fields") | sum(start=[])) -%}
{%- set read_defaults = client | rejectattr("hidden") | selectattr("has_read_default") | list -%}
package models

import (
	"encoding/json"
	"fmt"
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
	parsed, err := time.Parse(time.RFC3339Nano, value)
	if err != nil {
		return fmt.Errorf("%q is not a date-time with a time zone", value)
	}
	utc := parsed.UTC()
	if utc.Year() < 1 || utc.Year() > 9999 {
		return fmt.Errorf("%q falls outside the years 0001 to 9999 in UTC", value)
	}
	*d = DateTime(utc.Format(TimestampLayout))
	return nil
}

// UniqueDateTimes rejects two values that are one instant in UTC, which the schema's uniqueItems compares as text.
type UniqueDateTimes []DateTime

func (d *UniqueDateTimes) UnmarshalJSON(data []byte) error {
	var values []DateTime
	if err := json.Unmarshal(data, &values); err != nil {
		return err
	}
	seen := make(map[DateTime]bool, len(values))
	for _, value := range values {
		if seen[value] {
			return fmt.Errorf("%q is repeated in a list that must not repeat an item", value)
		}
		seen[value] = true
	}
	*d = values
	return nil
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
