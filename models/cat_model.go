package models

type Cat struct {
	BaseEntity
	Name string `json:"name" dynamodbav:"name" firestore:"name"`
}

type CatDto struct {
	Id   string `json:"id"`
	Name string `json:"name"`
	AuditDto
}

func ToCatDto(item Cat) CatDto {
	return CatDto{Id: item.Id, Name: item.Name, AuditDto: item.ToAuditDto()}
}

type CreateCatRequest struct {
	Name string `json:"name"`
}

// An empty Name leaves the stored name unchanged.
type UpdateCatRequest struct {
	Id   string `json:"-"`
	Name string `json:"name"`
}
