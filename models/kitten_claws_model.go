package models

type KittenClaws struct {
	BaseEntity
	Name *string `json:"name,omitempty" dynamodbav:"name,omitempty" firestore:"name,omitempty"`
}

type KittenClawsDto struct {
	BaseResponse
	Name *string `json:"name"`
}

func ToKittenClawsDto(item KittenClaws) KittenClawsDto {
	dto := KittenClawsDto{BaseResponse: NewBaseResponse(item.BaseEntity)}
	dto.Name = item.Name
	return dto
}

type CreateKittenClawsRequest struct {
	BaseCreateRequest
	Name *string `json:"name"`
}

func NewCreateKittenClawsRequest() CreateKittenClawsRequest {
	req := CreateKittenClawsRequest{BaseCreateRequest: NewBaseCreateRequest()}
	return req
}

func (r CreateKittenClawsRequest) ToEntity() KittenClaws {
	item := KittenClaws{BaseEntity: r.BaseCreateRequest.ToEntity()}
	item.Name = r.Name
	return item
}

type UpdateKittenClawsRequest struct {
	BaseUpdateRequest
	Id   string  `json:"-"`
	Name *string `json:"name"`
}

func (r UpdateKittenClawsRequest) ApplyTo(item *KittenClaws) {
	r.BaseUpdateRequest.ApplyTo(&item.BaseEntity)
	if r.Sent["name"] {
		item.Name = r.Name
	}
}
