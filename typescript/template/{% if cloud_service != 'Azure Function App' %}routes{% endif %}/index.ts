export * from './response';
{%- for resource in resources %}
export * from './{{ resource.name | to_lower_camel }}.routes';
{%- endfor %}
