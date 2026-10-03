package models

type Cat struct {
	BaseEntity
	Name *string `json:"name,omitempty" dynamodbav:"name,omitempty" firestore:"name,omitempty"`
}

type CatDto struct {
	BaseResponse
	Name *string `json:"name"`
}

func ToCatDto(item Cat) CatDto {
	dto := CatDto{BaseResponse: NewBaseResponse(item.BaseEntity)}
	dto.Name = item.Name
	return dto
}

type CreateCatRequest struct {
	BaseCreateRequest
	Name *string `json:"name"`
}

func NewCreateCatRequest() CreateCatRequest {
	req := CreateCatRequest{BaseCreateRequest: NewBaseCreateRequest()}
	return req
}

func (r CreateCatRequest) ToEntity() Cat {
	item := Cat{BaseEntity: r.BaseCreateRequest.ToEntity()}
	item.Name = r.Name
	return item
}

type UpdateCatRequest struct {
	BaseUpdateRequest
	Id   string  `json:"-"`
	Name *string `json:"name"`
}

func (r UpdateCatRequest) ApplyTo(item *Cat) {
	r.BaseUpdateRequest.ApplyTo(&item.BaseEntity)
	if r.Sent["name"] {
		item.Name = r.Name
	}
}
