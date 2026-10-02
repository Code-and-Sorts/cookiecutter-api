package handlers

import (
	"kittenclaws/controllers"
)

type Controllers struct {
	Cat controllers.CatController
	Dog controllers.DogController
}

func RegisterRoutes(router Mux, c Controllers) {
	RegisterHealthRoute(router)
	RegisterCatRoutes(router, c.Cat)
	RegisterDogRoutes(router, c.Dog)
}
