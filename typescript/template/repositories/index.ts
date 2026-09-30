export * from './document.store';
export * from './base.repository';
{%- if cloud_service == 'Azure Function App' %}
export * from './cosmos.store';
{%- elif cloud_service == 'GCP Cloud Function' %}
export * from './firestore.store';
{%- else %}
export * from './dynamo.store';
{%- endif %}
{%- for resource in resources %}
export * from './{{ resource.name | to_lower_camel }}.repository';
{%- endfor %}
