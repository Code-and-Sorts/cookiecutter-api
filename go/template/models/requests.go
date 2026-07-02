package models
{% for resource in resources %}
{%- if "create" in resource.operations %}
type Create{{ resource.name }}Request struct {
	Name      string `json:"name"`
	CreatedBy string `json:"createdBy"`
	UpdatedBy string `json:"updatedBy"`
}
{% endif %}
{%- if "update" in resource.operations %}
type Update{{ resource.name }}Request struct {
	Id        string `json:"-"`
	Name      string `json:"name"`
	UpdatedBy string `json:"updatedBy"`
}
{% endif %}
{%- if "replace" in resource.operations %}
type Replace{{ resource.name }}Request struct {
	Id        string `json:"-"`
	Name      string `json:"name"`
	UpdatedBy string `json:"updatedBy"`
}
{% endif %}
{%- endfor %}