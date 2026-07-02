{% if cloud_service == 'Azure Function App' -%}
import { Container } from '@azure/cosmos';
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
import { CollectionReference } from '@google-cloud/firestore';
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
import { DynamoDBDocumentClient } from '@aws-sdk/lib-dynamodb';
{%- endif %}
import { ItemRecord } from '@models';
import { BaseRepository } from './base.repository';

export class ItemRepository extends BaseRepository<ItemRecord> {
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

  create = async (newItem: ItemRecord): Promise<ItemRecord> => this.addRecord(newItem);
  get = async (id: string): Promise<ItemRecord> => this.getRecord(id);
  list = async (limit?: number): Promise<ItemRecord[]> => this.getRecords(limit);
  update = async (item: Partial<ItemRecord>): Promise<ItemRecord> => this.updateRecord(item);
  replace = async (item: ItemRecord): Promise<ItemRecord> => this.replaceRecord(item);
  delete = async (id: string): Promise<void> => this.deleteRecord(id);
}
