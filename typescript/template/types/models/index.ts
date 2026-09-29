export * from './base.schema';
export * from './baseEnv.schema';
export * from './env.schema';
export * from './guid.schema';
{%- for resource in resources %}
export * from './{{ resource.name | to_lower_camel }}.schema';
{%- endfor %}
