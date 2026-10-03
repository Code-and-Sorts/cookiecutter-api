{%- from 'python/_model.jinja' import entity_field, imports, request_field, response_field -%}
{%- set user_fields = base_fields | rejectattr("system") | list -%}
{%- set system_names = base_fields | selectattr("system") | map(attribute="name") | list -%}
{%- set body %}

# Server-managed: set by the repository, never accepted in a request body.
SYSTEM_FIELDS = ({% for name in system_names %}"{{ name }}"{{ ", " if not loop.last }}{% endfor %})


class RequestModel(BaseModel):
    """Request bodies reject unknown and server-managed fields and never coerce types."""

    model_config = ConfigDict(extra="forbid", strict=True)


class BaseEntity(BaseModel):
    """A stored record; each default is what a create stores when the body leaves the field out."""

    id: str
    isDeleted: bool = False
    createdTimestamp: str
    updatedTimestamp: str
    createdBy: str | None = None
    updatedBy: str | None = None
{%- for f in user_fields %}
    {{ entity_field(f) }}
{%- endfor %}

    @classmethod
    def client_fields(cls) -> list[str]:
        return [name for name in cls.model_fields if name not in SYSTEM_FIELDS]

    @classmethod
    def client_defaults(cls) -> dict:
        """Evaluated per call, so dynamic defaults are fresh on every write."""
        return {name: cls.model_fields[name].get_default(call_default_factory=True) for name in cls.client_fields()}

{% for kind, op in [("Create", "create"), ("Replace", "replace"), ("Update", "update")] %}
class Base{{ kind }}Request(RequestModel):
{%- set fields = user_fields | selectattr("in_" ~ op) | list %}
{%- if op == "update" and fields %}
    # Defaults skip validation: an absent field stays unset, an explicit null is rejected unless nullable.
{%- endif %}
{%- for f in fields %}
    {{ request_field(f, op) }}
{%- else %}
    pass
{%- endfor %}

{% endfor %}
class BaseResponse(BaseModel):
    id: str
{%- for f in user_fields | rejectattr("hidden") %}
    {{ response_field(f) }}
{%- endfor %}
{%- endset -%}
{{ imports(body) }}{{ body }}
