package models

import "time"

const TimestampLayout = "2006-01-02T15:04:05.000Z"

type BaseEntity struct {
	Id               string `json:"id" dynamodbav:"id" firestore:"id"`
	IsDeleted        bool   `json:"isDeleted" dynamodbav:"isDeleted" firestore:"isDeleted"`
	CreatedTimestamp string `json:"createdTimestamp" dynamodbav:"createdTimestamp" firestore:"createdTimestamp"`
	UpdatedTimestamp string `json:"updatedTimestamp" dynamodbav:"updatedTimestamp" firestore:"updatedTimestamp"`
	CreatedBy        string `json:"createdBy,omitempty" dynamodbav:"createdBy,omitempty" firestore:"createdBy,omitempty"`
	UpdatedBy        string `json:"updatedBy,omitempty" dynamodbav:"updatedBy,omitempty" firestore:"updatedBy,omitempty"`
}

func Now() string {
	return time.Now().UTC().Format(TimestampLayout)
}
