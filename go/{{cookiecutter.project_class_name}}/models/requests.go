package models

type Create{{cookiecutter.project_class_name}}Request struct {
	Name      string `json:"name"`
	CreatedBy string `json:"createdBy"`
	UpdatedBy string `json:"updatedBy"`
}

type Update{{cookiecutter.project_class_name}}Request struct {
	Id        string `json:"-"`
	Name      string `json:"name"`
	UpdatedBy string `json:"updatedBy"`
}
