package models

type KittenClaws struct {
	BaseEntity
	Name string `json:"name" dynamodbav:"name" firestore:"name"`
}

type KittenClawsDto struct {
	Id   string `json:"id"`
	Name string `json:"name"`
}

func ToKittenClawsDto(item KittenClaws) KittenClawsDto {
	return KittenClawsDto{Id: item.Id, Name: item.Name}
}

type CreateKittenClawsRequest struct {
	Name string `json:"name"`
}

// An empty Name leaves the stored name unchanged.
type UpdateKittenClawsRequest struct {
	Id   string `json:"-"`
	Name string `json:"name"`
}
