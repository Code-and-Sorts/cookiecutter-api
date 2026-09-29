export * from './schemaValidator.service';
{%- for resource in resources %}
export * from './{{ resource.name | to_lower_camel }}.service';
{%- endfor %}
