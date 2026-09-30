package models

type Dog struct {
	BaseEntity
	Name string `json:"name" dynamodbav:"name" firestore:"name"`
}

type DogDto struct {
	Id   string `json:"id"`
	Name string `json:"name"`
}

func ToDogDto(item Dog) DogDto {
	return DogDto{Id: item.Id, Name: item.Name}
}

type CreateDogRequest struct {
	Name string `json:"name"`
}

type ReplaceDogRequest struct {
	Id   string `json:"-"`
	Name string `json:"name"`
}
