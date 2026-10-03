package models

import "github.com/google/uuid"

type Cat struct {
	BaseEntity
	Name        *string   `json:"name,omitempty" dynamodbav:"name,omitempty" firestore:"name,omitempty"`
	Breed       *string   `json:"breed,omitempty" dynamodbav:"breed,omitempty" firestore:"breed,omitempty"`
	AgeYears    *int64    `json:"ageYears,omitempty" dynamodbav:"ageYears,omitempty" firestore:"ageYears,omitempty"`
	WeightKg    *float64  `json:"weightKg,omitempty" dynamodbav:"weightKg,omitempty" firestore:"weightKg,omitempty"`
	Indoor      *bool     `json:"indoor,omitempty" dynamodbav:"indoor,omitempty" firestore:"indoor,omitempty"`
	BirthDate   *string   `json:"birthDate,omitempty" dynamodbav:"birthDate,omitempty" firestore:"birthDate,omitempty"`
	MicrochipId *string   `json:"microchipId,omitempty" dynamodbav:"microchipId,omitempty" firestore:"microchipId,omitempty"`
	OwnerEmail  *string   `json:"ownerEmail,omitempty" dynamodbav:"ownerEmail,omitempty" firestore:"ownerEmail,omitempty"`
	Website     *string   `json:"website,omitempty" dynamodbav:"website,omitempty" firestore:"website,omitempty"`
	TagCode     *string   `json:"tagCode,omitempty" dynamodbav:"tagCode,omitempty" firestore:"tagCode,omitempty"`
	Tags        *[]string `json:"tags,omitempty" dynamodbav:"tags,omitempty" firestore:"tags,omitempty"`
	Scores      *[]int64  `json:"scores,omitempty" dynamodbav:"scores,omitempty" firestore:"scores,omitempty"`
	AdoptedAt   *DateTime `json:"adoptedAt,omitempty" dynamodbav:"adoptedAt,omitempty" firestore:"adoptedAt,omitempty"`
	LastVisit   *DateTime `json:"lastVisit,omitempty" dynamodbav:"lastVisit,omitempty" firestore:"lastVisit,omitempty"`
	Notes       *string   `json:"notes,omitempty" dynamodbav:"notes,omitempty" firestore:"notes,omitempty"`
}

type CatDto struct {
	BaseResponse
	Name        *string   `json:"name"`
	Breed       *string   `json:"breed"`
	AgeYears    *int64    `json:"ageYears"`
	WeightKg    *float64  `json:"weightKg"`
	Indoor      *bool     `json:"indoor"`
	BirthDate   *string   `json:"birthDate"`
	MicrochipId *string   `json:"microchipId"`
	OwnerEmail  *string   `json:"ownerEmail"`
	Website     *string   `json:"website"`
	TagCode     *string   `json:"tagCode"`
	Tags        *[]string `json:"tags"`
	Scores      *[]int64  `json:"scores"`
	AdoptedAt   *DateTime `json:"adoptedAt"`
	LastVisit   *DateTime `json:"lastVisit"`
}

func ToCatDto(item Cat) CatDto {
	dto := CatDto{BaseResponse: NewBaseResponse(item.BaseEntity)}
	dto.Name = item.Name
	dto.Breed = orDefault(item.Breed, "tabby")
	dto.AgeYears = orDefault(item.AgeYears, int64(0))
	dto.WeightKg = item.WeightKg
	dto.Indoor = orDefault(item.Indoor, true)
	dto.BirthDate = item.BirthDate
	dto.MicrochipId = item.MicrochipId
	dto.OwnerEmail = orDefault(item.OwnerEmail, "unknown@example.com")
	dto.Website = item.Website
	dto.TagCode = item.TagCode
	dto.Tags = orDefault(item.Tags, []string{})
	dto.Scores = item.Scores
	dto.AdoptedAt = item.AdoptedAt
	dto.LastVisit = orDefault(item.LastVisit, DateTime("2026-01-01T00:00:00.000Z"))
	return dto
}

type CreateCatRequest struct {
	BaseCreateRequest
	Name        *string   `json:"name"`
	Breed       *string   `json:"breed"`
	AgeYears    *int64    `json:"ageYears"`
	WeightKg    *float64  `json:"weightKg"`
	Indoor      *bool     `json:"indoor"`
	BirthDate   *string   `json:"birthDate"`
	MicrochipId *string   `json:"microchipId"`
	OwnerEmail  *string   `json:"ownerEmail"`
	Website     *string   `json:"website"`
	TagCode     *string   `json:"tagCode"`
	Tags        *[]string `json:"tags"`
	Scores      *[]int64  `json:"scores"`
	AdoptedAt   *DateTime `json:"adoptedAt"`
	LastVisit   *DateTime `json:"lastVisit"`
	Notes       *string   `json:"notes"`
}

func NewCreateCatRequest() CreateCatRequest {
	req := CreateCatRequest{BaseCreateRequest: NewBaseCreateRequest()}
	req.Breed = new("tabby")
	req.AgeYears = new(int64(0))
	req.Indoor = new(true)
	req.BirthDate = new(Today())
	req.MicrochipId = new(uuid.NewString())
	req.OwnerEmail = new("unknown@example.com")
	req.Tags = new([]string{})
	req.AdoptedAt = new(DateTime(Now()))
	req.LastVisit = new(DateTime("2026-01-01T00:00:00.000Z"))
	req.Notes = new("$none")
	return req
}

func (r CreateCatRequest) ToEntity() Cat {
	item := Cat{BaseEntity: r.BaseCreateRequest.ToEntity()}
	item.Name = r.Name
	item.Breed = r.Breed
	item.AgeYears = r.AgeYears
	item.WeightKg = r.WeightKg
	item.Indoor = r.Indoor
	item.BirthDate = r.BirthDate
	item.MicrochipId = r.MicrochipId
	item.OwnerEmail = r.OwnerEmail
	item.Website = r.Website
	item.TagCode = r.TagCode
	item.Tags = r.Tags
	item.Scores = r.Scores
	item.AdoptedAt = r.AdoptedAt
	item.LastVisit = r.LastVisit
	item.Notes = r.Notes
	return item
}

type ReplaceCatRequest struct {
	BaseReplaceRequest
	Id         string    `json:"-"`
	Name       *string   `json:"name"`
	Breed      *string   `json:"breed"`
	AgeYears   *int64    `json:"ageYears"`
	WeightKg   *float64  `json:"weightKg"`
	Indoor     *bool     `json:"indoor"`
	BirthDate  *string   `json:"birthDate"`
	OwnerEmail *string   `json:"ownerEmail"`
	Website    *string   `json:"website"`
	TagCode    *string   `json:"tagCode"`
	Tags       *[]string `json:"tags"`
	Scores     *[]int64  `json:"scores"`
	AdoptedAt  *DateTime `json:"adoptedAt"`
	LastVisit  *DateTime `json:"lastVisit"`
	Notes      *string   `json:"notes"`
}

func NewReplaceCatRequest() ReplaceCatRequest {
	req := ReplaceCatRequest{BaseReplaceRequest: NewBaseReplaceRequest()}
	req.Breed = new("tabby")
	req.AgeYears = new(int64(0))
	req.Indoor = new(true)
	req.BirthDate = new(Today())
	req.OwnerEmail = new("unknown@example.com")
	req.Tags = new([]string{})
	req.AdoptedAt = new(DateTime(Now()))
	req.LastVisit = new(DateTime("2026-01-01T00:00:00.000Z"))
	req.Notes = new("$none")
	return req
}

func (r ReplaceCatRequest) ApplyTo(item *Cat) {
	r.BaseReplaceRequest.ApplyTo(&item.BaseEntity)
	item.Name = r.Name
	item.Breed = r.Breed
	item.AgeYears = r.AgeYears
	item.WeightKg = r.WeightKg
	item.Indoor = r.Indoor
	item.BirthDate = r.BirthDate
	item.OwnerEmail = r.OwnerEmail
	item.Website = r.Website
	item.TagCode = r.TagCode
	item.Tags = r.Tags
	item.Scores = r.Scores
	item.AdoptedAt = r.AdoptedAt
	item.LastVisit = r.LastVisit
	item.Notes = r.Notes
}

type UpdateCatRequest struct {
	BaseUpdateRequest
	Id         string    `json:"-"`
	Name       *string   `json:"name"`
	AgeYears   *int64    `json:"ageYears"`
	WeightKg   *float64  `json:"weightKg"`
	Indoor     *bool     `json:"indoor"`
	OwnerEmail *string   `json:"ownerEmail"`
	Website    *string   `json:"website"`
	Tags       *[]string `json:"tags"`
	AdoptedAt  *DateTime `json:"adoptedAt"`
	Notes      *string   `json:"notes"`
}

func (r UpdateCatRequest) ApplyTo(item *Cat) {
	r.BaseUpdateRequest.ApplyTo(&item.BaseEntity)
	if r.Sent["name"] {
		item.Name = r.Name
	}
	if r.Sent["ageYears"] {
		item.AgeYears = r.AgeYears
	}
	if r.Sent["weightKg"] {
		item.WeightKg = r.WeightKg
	}
	if r.Sent["indoor"] {
		item.Indoor = r.Indoor
	}
	if r.Sent["ownerEmail"] {
		item.OwnerEmail = r.OwnerEmail
	}
	if r.Sent["website"] {
		item.Website = r.Website
	}
	if r.Sent["tags"] {
		item.Tags = r.Tags
	}
	if r.Sent["adoptedAt"] {
		item.AdoptedAt = r.AdoptedAt
	}
	if r.Sent["notes"] {
		item.Notes = r.Notes
	}
}
