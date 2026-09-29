package models

import "time"

// TimestampLayout is the ISO-8601 UTC layout, with millisecond precision, used
// for every stored timestamp (for example 2026-09-29T22:49:26.625Z).
const TimestampLayout = "2006-01-02T15:04:05.000Z"

// BaseEntity holds the fields every stored record carries. CreatedBy and
// UpdatedBy are omitted from the stored record when they are not set.
type BaseEntity struct {
	Id               string `json:"id" dynamodbav:"id" firestore:"id"`
	IsDeleted        bool   `json:"isDeleted" dynamodbav:"isDeleted" firestore:"isDeleted"`
	CreatedTimestamp string `json:"createdTimestamp" dynamodbav:"createdTimestamp" firestore:"createdTimestamp"`
	UpdatedTimestamp string `json:"updatedTimestamp" dynamodbav:"updatedTimestamp" firestore:"updatedTimestamp"`
	CreatedBy        string `json:"createdBy,omitempty" dynamodbav:"createdBy,omitempty" firestore:"createdBy,omitempty"`
	UpdatedBy        string `json:"updatedBy,omitempty" dynamodbav:"updatedBy,omitempty" firestore:"updatedBy,omitempty"`
}

// Now returns the current time formatted with TimestampLayout.
func Now() string {
	return time.Now().UTC().Format(TimestampLayout)
}
