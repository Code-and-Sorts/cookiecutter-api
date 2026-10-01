{%- set all_ops = path_resources | map(attribute='operations') | sum(start=[]) | unique | list -%}
package models

import (
	"time"

	"github.com/google/uuid"
)

const TimestampLayout = "2006-01-02T15:04:05.000Z"

type BaseEntity struct {
	Id               string `json:"id" dynamodbav:"id" firestore:"id"`
	IsDeleted        bool   `json:"isDeleted" dynamodbav:"isDeleted" firestore:"isDeleted"`
	CreatedTimestamp string `json:"createdTimestamp" dynamodbav:"createdTimestamp" firestore:"createdTimestamp"`
	UpdatedTimestamp string `json:"updatedTimestamp" dynamodbav:"updatedTimestamp" firestore:"updatedTimestamp"`
	CreatedBy        string `json:"createdBy,omitempty" dynamodbav:"createdBy,omitempty" firestore:"createdBy,omitempty"`
	UpdatedBy        string `json:"updatedBy,omitempty" dynamodbav:"updatedBy,omitempty" firestore:"updatedBy,omitempty"`
}

func NewBaseEntity() BaseEntity {
	now := Now()
	return BaseEntity{Id: uuid.New().String(), CreatedTimestamp: now, UpdatedTimestamp: now}
}

// Promoted to every record type, so generic repository code can reach the shared fields.
func (b *BaseEntity) Base() *BaseEntity {
	return b
}

{%- if 'create' in all_ops %}

func (b *BaseEntity) StampCreate(userID string) {
	b.CreatedBy = userID
	b.UpdatedBy = userID
}
{%- endif %}
{%- if all_ops | select('in', ['update', 'replace', 'delete']) | list %}

// An empty userID removes updatedBy, so it always describes the latest write.
func (b *BaseEntity) StampWrite(userID string) {
	b.UpdatedTimestamp = Now()
	b.UpdatedBy = userID
}
{%- endif %}

func Now() string {
	return time.Now().UTC().Format(TimestampLayout)
}
