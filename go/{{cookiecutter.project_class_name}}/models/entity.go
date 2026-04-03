package models

import "time"

type BaseEntity struct {
	Id               string    `json:"id"`
	IsDeleted        bool      `json:"isDeleted"`
	CreatedTimestamp  time.Time `json:"createdTimestamp"`
	UpdatedTimestamp  time.Time `json:"updatedTimestamp"`
	CreatedBy        string    `json:"createdBy"`
	UpdatedBy        string    `json:"updatedBy"`
}

type {{cookiecutter.project_class_name}} struct {
	BaseEntity
	Name string `json:"name"`
}
