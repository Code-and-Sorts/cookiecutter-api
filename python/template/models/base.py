{%- from 'shared/_fields.jinja' import REQUEST_KINDS -%}
{%- from 'python/_model.jinja' import imports, model_classes -%}
{%- set body %}

SYSTEM_FIELDS = ({% for f in base_fields | selectattr("system") %}"{{ f.name }}"{{ ", " if not loop.last }}{% endfor %})


class RequestModel(BaseModel):
    """Request bodies reject unknown and server-managed fields and never coerce types."""

    model_config = ConfigDict(extra="forbid", strict=True)


{% call model_classes("Base", base_fields, REQUEST_KINDS) %}
    @classmethod
    def client_fields(cls) -> list[str]:
        return [name for name in cls.model_fields if name not in SYSTEM_FIELDS]

    @classmethod
    def client_defaults(cls) -> dict:
        """Evaluated per call, so dynamic defaults are fresh on every write."""
        return {name: cls.model_fields[name].get_default(call_default_factory=True) for name in cls.client_fields()}
{%- endcall %}
{%- endset -%}
{{ imports(body) }}{{ body }}
