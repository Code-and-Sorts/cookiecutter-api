{%- from 'shared/_fields.jinja' import REQUEST_KINDS -%}
{%- from 'typescript/_model.jinja' import field_imports, model_types -%}
{%- set body %}
export type SystemField = {% for f in base_fields | selectattr("system") %}'{{ f.name }}'{{ " | " if not loop.last }}{% endfor %};

{{ model_types("Base", "BaseEntity", base_fields, REQUEST_KINDS) }}
{%- endset -%}
import { z } from 'zod';
{{ field_imports(body) }}{{ body }}
