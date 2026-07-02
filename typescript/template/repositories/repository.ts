{% if cloud_service == 'Azure Function App' -%}
import { Container } from '@azure/cosmos';
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
import { CollectionReference } from '@google-cloud/firestore';
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
import { DynamoDBDocumentClient } from '@aws-sdk/lib-dynamodb';
{%- endif %}
import {
{%- for resource in resources %}
    {{ resource.name }}Record,
{%- endfor %}
} from '@models';
import { BaseRepository } from './base.repository';
{% for resource in resources %}
{%- set r = resource.name %}
export class {{ r }}Repository extends BaseRepository<{{ r }}Record> {
{%- if cloud_service == 'Azure Function App' %}
  constructor(container: Container) {
    super(container);
  }
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
  constructor(collection: CollectionReference) {
    super(collection);
  }
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
  constructor(docClient: DynamoDBDocumentClient, tableName: string) {
    super(docClient, tableName);
  }
{%- endif %}

  create = async (newItem: {{ r }}Record): Promise<{{ r }}Record> => this.addRecord(newItem);
  get = async (id: string): Promise<{{ r }}Record> => this.getRecord(id);
  list = async (limit?: number): Promise<{{ r }}Record[]> => this.getRecords(limit);
  update = async (item: Partial<{{ r }}Record>): Promise<{{ r }}Record> => this.updateRecord(item);
  replace = async (item: {{ r }}Record): Promise<{{ r }}Record> => this.replaceRecord(item);
  delete = async (id: string): Promise<void> => this.deleteRecord(id);
}
{% endfor %}