{% for resource in resources -%}
export * from './{{ resource.name | to_lower_camel }}.controller';
{% endfor %}