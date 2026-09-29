import 'reflect-metadata';
import { inject, injectable } from 'inversify';
{% if cloud_service == 'Azure Function App' -%}
import { Database } from '@azure/cosmos';
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
import { Firestore } from '@google-cloud/firestore';
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
import { DynamoDBDocumentClient } from '@aws-sdk/lib-dynamodb';
{%- endif %}
import {
    env,
{%- for resource in resources %}
    {{ resource.name }}Record,
{%- endfor %}
} from '@models';
import { BaseRepository{% if cloud_service == 'AWS Lambda' %}, DocumentClient{% endif %} } from './base.repository';
{% for resource in resources %}
{%- set r = resource.name %}
{%- set key = resource.container | upper | replace('-', '_') %}
@injectable()
export class {{ r }}Repository extends BaseRepository<{{ r }}Record> {
{%- if cloud_service == 'Azure Function App' %}
  constructor(@inject(Database) database: Database) {
    super(database.container(env.COSMOS_CONTAINER_{{ key }}));
  }
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
  constructor(@inject(Firestore) client: Firestore) {
    super(client.collection(env.FIRESTORE_COLLECTION_{{ key }}));
  }
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
  constructor(@inject(DocumentClient) client: DynamoDBDocumentClient) {
    super(client, env.DYNAMODB_TABLE_NAME_{{ key }});
  }
{%- endif %}

  create = async (newItem: {{ r }}Record): Promise<{{ r }}Record> => this.addRecord(newItem);
  get = async (id: string): Promise<{{ r }}Record> => this.getRecord(id);
  list = async (limit?: number): Promise<{{ r }}Record[]> => this.getRecords(limit);
  update = async (item: Partial<{{ r }}Record> & { id: string }): Promise<{{ r }}Record> => this.updateRecord(item);
  replace = async (item: {{ r }}Record): Promise<{{ r }}Record> => this.replaceRecord(item);
  delete = async (id: string): Promise<void> => this.deleteRecord(id);
}
{% endfor %}
