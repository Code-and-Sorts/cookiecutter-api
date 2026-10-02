package handlers

import (
	"kittenclaws/controllers"
)

type Controllers struct {
	KittenClaws controllers.KittenClawsController
}

func RegisterRoutes(router Mux, c Controllers) {
	RegisterHealthRoute(router)
	RegisterKittenClawsRoutes(router, c.KittenClaws)
}
