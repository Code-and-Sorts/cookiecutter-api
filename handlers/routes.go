package handlers

import (
	"kittenclaws/controllers"
)

type Controllers struct {
	KittenClaws controllers.KittenClawsController
}

func RegisterRoutes(router *Router, c Controllers) {
	RegisterHealthRoute(router)
	RegisterKittenClawsRoutes(router, c.KittenClaws)
}
