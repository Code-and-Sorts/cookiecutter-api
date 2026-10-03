package models

type Visit struct {
	BaseEntity
	Reason    *string     `json:"reason,omitempty" dynamodbav:"reason,omitempty" firestore:"reason,omitempty"`
	VisitedOn *string     `json:"visitedOn,omitempty" dynamodbav:"visitedOn,omitempty" firestore:"visitedOn,omitempty"`
	Cost      *float64    `json:"cost,omitempty" dynamodbav:"cost,omitempty" firestore:"cost,omitempty"`
	Paid      *bool       `json:"paid,omitempty" dynamodbav:"paid,omitempty" firestore:"paid,omitempty"`
	CheckedAt *[]DateTime `json:"checkedAt,omitempty" dynamodbav:"checkedAt,omitempty" firestore:"checkedAt,omitempty"`
}

type VisitDto struct {
	BaseResponse
	Reason    *string     `json:"reason"`
	VisitedOn *string     `json:"visitedOn"`
	Cost      *float64    `json:"cost"`
	Paid      *bool       `json:"paid"`
	CheckedAt *[]DateTime `json:"checkedAt"`
}

func ToVisitDto(item Visit) VisitDto {
	dto := VisitDto{BaseResponse: NewBaseResponse(item.BaseEntity)}
	dto.Reason = item.Reason
	dto.VisitedOn = item.VisitedOn
	dto.Cost = item.Cost
	dto.Paid = orDefault(item.Paid, false)
	dto.CheckedAt = item.CheckedAt
	return dto
}

type CreateVisitRequest struct {
	BaseCreateRequest
	Reason    *string  `json:"reason"`
	VisitedOn *string  `json:"visitedOn"`
	Cost      *float64 `json:"cost"`
}

func NewCreateVisitRequest() CreateVisitRequest {
	req := CreateVisitRequest{BaseCreateRequest: NewBaseCreateRequest()}
	return req
}

func (r CreateVisitRequest) ToEntity() Visit {
	item := Visit{BaseEntity: r.BaseCreateRequest.ToEntity()}
	item.Reason = r.Reason
	item.VisitedOn = r.VisitedOn
	item.Cost = r.Cost
	item.Paid = new(false)
	return item
}

type UpdateVisitRequest struct {
	BaseUpdateRequest
	Id        string      `json:"-"`
	VisitedOn *string     `json:"visitedOn"`
	Cost      *float64    `json:"cost"`
	Paid      *bool       `json:"paid"`
	CheckedAt *[]DateTime `json:"checkedAt"`
}

func (r UpdateVisitRequest) ApplyTo(item *Visit) {
	r.BaseUpdateRequest.ApplyTo(&item.BaseEntity)
	if r.Sent["visitedOn"] {
		item.VisitedOn = r.VisitedOn
	}
	if r.Sent["cost"] {
		item.Cost = r.Cost
	}
	if r.Sent["paid"] {
		item.Paid = r.Paid
	}
	if r.Sent["checkedAt"] {
		item.CheckedAt = r.CheckedAt
	}
}
