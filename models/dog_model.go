package models

type Dog struct {
	BaseEntity
	Name *string `json:"name,omitempty" dynamodbav:"name,omitempty" firestore:"name,omitempty"`
}

type DogDto struct {
	BaseResponse
	Name *string `json:"name"`
}

func ToDogDto(item Dog) DogDto {
	dto := DogDto{BaseResponse: NewBaseResponse(item.BaseEntity)}
	dto.Name = item.Name
	return dto
}

type CreateDogRequest struct {
	BaseCreateRequest
	Name *string `json:"name"`
}

func NewCreateDogRequest() CreateDogRequest {
	req := CreateDogRequest{BaseCreateRequest: NewBaseCreateRequest()}
	return req
}

func (r CreateDogRequest) ToEntity() Dog {
	item := Dog{BaseEntity: r.BaseCreateRequest.ToEntity()}
	item.Name = r.Name
	return item
}

type ReplaceDogRequest struct {
	BaseReplaceRequest
	Id   string  `json:"-"`
	Name *string `json:"name"`
}

func NewReplaceDogRequest() ReplaceDogRequest {
	req := ReplaceDogRequest{BaseReplaceRequest: NewBaseReplaceRequest()}
	return req
}

func (r ReplaceDogRequest) ApplyTo(item *Dog) {
	r.BaseReplaceRequest.ApplyTo(&item.BaseEntity)
	item.Name = r.Name
}
