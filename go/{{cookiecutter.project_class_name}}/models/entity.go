package models

import "time"

type BaseEntity struct {
	Id               string    `json:"id" dynamodbav:"id"`
	IsDeleted        bool      `json:"isDeleted" dynamodbav:"isDeleted"`
	CreatedTimestamp  time.Time `json:"createdTimestamp" dynamodbav:"createdTimestamp"`
	UpdatedTimestamp  time.Time `json:"updatedTimestamp" dynamodbav:"updatedTimestamp"`
	CreatedBy        string    `json:"createdBy" dynamodbav:"createdBy"`
	UpdatedBy        string    `json:"updatedBy" dynamodbav:"updatedBy"`
}

type {{cookiecutter.project_class_name}} struct {
	BaseEntity
	Name string `json:"name" dynamodbav:"name"`
}
