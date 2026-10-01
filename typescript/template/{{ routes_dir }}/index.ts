export * from './response';
{%- if health_endpoint %}
export * from './health.routes';
{%- endif %}
export * from './openapi.routes';
{%- for resource in resources %}
export * from './{{ resource.name | to_lower_camel }}.routes';
{%- endfor %}
