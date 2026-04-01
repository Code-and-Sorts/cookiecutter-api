package models

import "fmt"

type Create{{cookiecutter.project_class_name}}Request struct {
	Name      string `json:"name"`
	CreatedBy string `json:"createdBy"`
	UpdatedBy string `json:"updatedBy"`
}

func (r Create{{cookiecutter.project_class_name}}Request) Validate() error {
	if r.Name == "" {
		return &ValidationError{Message: fmt.Sprintf("%s is required.", "Name")}
	}
	return nil
}

type Update{{cookiecutter.project_class_name}}Request struct {
	Id        string `json:"-"`
	Name      string `json:"name"`
	UpdatedBy string `json:"updatedBy"`
}

func (r Update{{cookiecutter.project_class_name}}Request) Validate() error {
	if r.Name == "" {
		return &ValidationError{Message: fmt.Sprintf("%s is required.", "Name")}
	}
	return nil
}
