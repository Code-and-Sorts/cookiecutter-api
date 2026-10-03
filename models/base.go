package models

import "github.com/google/uuid"

type BaseEntity struct {
	Id               string    `json:"id" dynamodbav:"id" firestore:"id"`
	IsDeleted        bool      `json:"isDeleted" dynamodbav:"isDeleted" firestore:"isDeleted"`
	CreatedTimestamp string    `json:"createdTimestamp" dynamodbav:"createdTimestamp" firestore:"createdTimestamp"`
	UpdatedTimestamp string    `json:"updatedTimestamp" dynamodbav:"updatedTimestamp" firestore:"updatedTimestamp"`
	CreatedBy        string    `json:"createdBy,omitempty" dynamodbav:"createdBy,omitempty" firestore:"createdBy,omitempty"`
	UpdatedBy        string    `json:"updatedBy,omitempty" dynamodbav:"updatedBy,omitempty" firestore:"updatedBy,omitempty"`
	TenantId         *string   `json:"tenantId,omitempty" dynamodbav:"tenantId,omitempty" firestore:"tenantId,omitempty"`
	Region           *string   `json:"region,omitempty" dynamodbav:"region,omitempty" firestore:"region,omitempty"`
	Priority         *int64    `json:"priority,omitempty" dynamodbav:"priority,omitempty" firestore:"priority,omitempty"`
	Rank             *float64  `json:"rank,omitempty" dynamodbav:"rank,omitempty" firestore:"rank,omitempty"`
	Labels           *[]string `json:"labels,omitempty" dynamodbav:"labels,omitempty" firestore:"labels,omitempty"`
}

func NewBaseEntity() BaseEntity {
	now := Now()
	return BaseEntity{Id: uuid.New().String(), CreatedTimestamp: now, UpdatedTimestamp: now}
}

// Promoted to every record type, so generic repository code can reach the shared fields.
func (b *BaseEntity) Base() *BaseEntity {
	return b
}

func (b *BaseEntity) StampCreate(userID string) {
	b.CreatedBy = userID
	b.UpdatedBy = userID
}

// An empty userID removes updatedBy, so it always describes the latest write.
func (b *BaseEntity) StampWrite(userID string) {
	b.UpdatedTimestamp = Now()
	b.UpdatedBy = userID
}

type BaseCreateRequest struct {
	TenantId *string   `json:"tenantId"`
	Region   *string   `json:"region"`
	Priority *int64    `json:"priority"`
	Rank     *float64  `json:"rank"`
	Labels   *[]string `json:"labels"`
}

func NewBaseCreateRequest() BaseCreateRequest {
	req := BaseCreateRequest{}
	req.TenantId = new("public")
	req.Region = new("eu")
	req.Labels = new([]string{})
	return req
}

func (r BaseCreateRequest) ToEntity() BaseEntity {
	item := NewBaseEntity()
	item.TenantId = r.TenantId
	item.Region = r.Region
	item.Priority = r.Priority
	item.Rank = r.Rank
	item.Labels = r.Labels
	return item
}

type BaseReplaceRequest struct {
	TenantId *string   `json:"tenantId"`
	Priority *int64    `json:"priority"`
	Rank     *float64  `json:"rank"`
	Labels   *[]string `json:"labels"`
}

func NewBaseReplaceRequest() BaseReplaceRequest {
	req := BaseReplaceRequest{}
	req.TenantId = new("public")
	req.Labels = new([]string{})
	return req
}

func (r BaseReplaceRequest) ApplyTo(item *BaseEntity) {
	item.TenantId = r.TenantId
	item.Priority = r.Priority
	item.Rank = r.Rank
	item.Labels = r.Labels
}

type BaseUpdateRequest struct {
	Sent     map[string]bool `json:"-"`
	TenantId *string         `json:"tenantId"`
	Priority *int64          `json:"priority"`
	Rank     *float64        `json:"rank"`
	Labels   *[]string       `json:"labels"`
}

func (r BaseUpdateRequest) ApplyTo(item *BaseEntity) {
	if r.Sent["tenantId"] {
		item.TenantId = r.TenantId
	}
	if r.Sent["priority"] {
		item.Priority = r.Priority
	}
	if r.Sent["rank"] {
		item.Rank = r.Rank
	}
	if r.Sent["labels"] {
		item.Labels = r.Labels
	}
}

type BaseResponse struct {
	Id       string    `json:"id"`
	TenantId *string   `json:"tenantId"`
	Region   *string   `json:"region"`
	Priority *int64    `json:"priority"`
	Rank     *float64  `json:"rank"`
	Labels   *[]string `json:"labels"`
}

func NewBaseResponse(entity BaseEntity) BaseResponse {
	response := BaseResponse{Id: entity.Id}
	response.TenantId = orDefault(entity.TenantId, "public")
	response.Region = orDefault(entity.Region, "eu")
	response.Priority = entity.Priority
	response.Rank = entity.Rank
	response.Labels = orDefault(entity.Labels, []string{})
	return response
}
