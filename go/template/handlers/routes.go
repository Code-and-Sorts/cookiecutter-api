{%- set width = resources | map(attribute='name') | map('length') | max -%}
package handlers

import (
	"{{project_endpoint}}/controllers"
)

type Controllers struct {
{%- for resource in resources %}
	{{ resource.name ~ ' ' * (width - resource.name | length) }} controllers.{{ resource.name }}Controller
{%- endfor %}
}

func RegisterRoutes(router {% if cloud_service == 'AWS Lambda' %}*Router{% else %}Mux{% endif %}, c Controllers, openAPISpec []byte) {
{%- if health_endpoint %}
	RegisterHealthRoute(router)
{%- endif %}
	RegisterOpenAPIRoute(router, openAPISpec)
{%- for resource in resources %}
	Register{{ resource.name }}Routes(router, c.{{ resource.name }})
{%- endfor %}
}
