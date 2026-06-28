import { inject, injectable } from 'inversify';
{% if cloud_service == 'Azure Function App' -%}
import { CosmosClient } from '@azure/cosmos';
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
import { Firestore } from '@google-cloud/firestore';
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
import { DynamoDBDocumentClient } from '@aws-sdk/lib-dynamodb';
{%- endif %}
import { {{project_class_name}}ItemRecord } from '@models';
import { BaseRepository } from './base.repository';
import 'reflect-metadata';

@injectable()
export class {{project_class_name}}Repository extends BaseRepository<{{project_class_name}}ItemRecord> {
  constructor(
{%- if cloud_service == 'Azure Function App' %}
    @inject(CosmosClient) client: CosmosClient
  ) {
    const db = client.database('{{project_endpoint}}s-sql-db');
    const container = db.container('{{project_endpoint}}s-sql-container');
    super(container);
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
    @inject(Firestore) client: Firestore
  ) {
    const collection = client.collection('{{project_endpoint}}');
    super(collection);
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
    @inject(DynamoDBDocumentClient) client: DynamoDBDocumentClient
  ) {
    super(client, '{{project_endpoint}}');
{%- endif %}
  }

  create = async (new{{project_class_name}}: {{project_class_name}}ItemRecord): Promise<{{project_class_name}}ItemRecord> => this.addRecord(new{{project_class_name}});
  get = async (id: string): Promise<{{project_class_name}}ItemRecord> => this.getRecord(id);
  list = async (limit?: number): Promise<{{project_class_name}}ItemRecord[]> => this.getRecords(limit);
  update = async ({{project_lower_camel_name}}: Partial<{{project_class_name}}ItemRecord>): Promise<{{project_class_name}}ItemRecord> => this.updateRecord({{project_lower_camel_name}});
  delete = async (id: string): Promise<void> => this.deleteRecord(id);
}
