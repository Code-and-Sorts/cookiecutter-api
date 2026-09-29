package models
{% for resource in resources %}
type {{ resource.name }}Dto struct {
	Id   string `json:"id"`
	Name string `json:"name"`
}
{% endfor %}