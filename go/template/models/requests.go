package models

type CreateItemRequest struct {
	Name      string `json:"name"`
	CreatedBy string `json:"createdBy"`
	UpdatedBy string `json:"updatedBy"`
}

type UpdateItemRequest struct {
	Id        string `json:"-"`
	Name      string `json:"name"`
	UpdatedBy string `json:"updatedBy"`
}

type ReplaceItemRequest struct {
	Id        string `json:"-"`
	Name      string `json:"name"`
	UpdatedBy string `json:"updatedBy"`
}
