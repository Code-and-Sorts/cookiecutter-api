package models

import "time"

type BaseEntity struct {
	Id               string    `json:"id" dynamodbav:"id" firestore:"id"`
	IsDeleted        bool      `json:"isDeleted" dynamodbav:"isDeleted" firestore:"isDeleted"`
	CreatedTimestamp  time.Time `json:"createdTimestamp" dynamodbav:"createdTimestamp" firestore:"createdTimestamp"`
	UpdatedTimestamp  time.Time `json:"updatedTimestamp" dynamodbav:"updatedTimestamp" firestore:"updatedTimestamp"`
	CreatedBy        string    `json:"createdBy" dynamodbav:"createdBy" firestore:"createdBy"`
	UpdatedBy        string    `json:"updatedBy" dynamodbav:"updatedBy" firestore:"updatedBy"`
}

type {{project_class_name}} struct {
	BaseEntity
	Name string `json:"name" dynamodbav:"name" firestore:"name"`
}
