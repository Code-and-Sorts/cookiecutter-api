package models

type CreateKittenClawsRequest struct {
	Name      string `json:"name"`
	CreatedBy string `json:"createdBy"`
	UpdatedBy string `json:"updatedBy"`
}

type UpdateKittenClawsRequest struct {
	Id        string `json:"-"`
	Name      string `json:"name"`
	UpdatedBy string `json:"updatedBy"`
}
