package models

type CreateCatRequest struct {
	Name      string `json:"name"`
	CreatedBy string `json:"createdBy"`
	UpdatedBy string `json:"updatedBy"`
}

type UpdateCatRequest struct {
	Id        string `json:"-"`
	Name      string `json:"name"`
	UpdatedBy string `json:"updatedBy"`
}

type CreateDogRequest struct {
	Name      string `json:"name"`
	CreatedBy string `json:"createdBy"`
	UpdatedBy string `json:"updatedBy"`
}

type ReplaceDogRequest struct {
	Id        string `json:"-"`
	Name      string `json:"name"`
	UpdatedBy string `json:"updatedBy"`
}
