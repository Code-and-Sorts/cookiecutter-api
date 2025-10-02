import { inject, injectable } from 'inversify';
{% if cookiecutter.cloud_service == 'Azure Function App' -%}
import { CosmosClient } from '@azure/cosmos';
{%- elif cookiecutter.cloud_service == 'GCP Cloud Function' -%}
import { Firestore } from '@google-cloud/firestore';
{%- endif %}
import { {{cookiecutter.project_class_name}}ItemRecord } from '@models';
import { BaseRepository } from './base.repository';
import 'reflect-metadata';

@injectable()
export class {{cookiecutter.project_class_name}}Repository extends BaseRepository<{{cookiecutter.project_class_name}}ItemRecord> {
  {% if cookiecutter.cloud_service == 'Azure Function App' -%}
  constructor(
    @inject(CosmosClient) client: CosmosClient
  ) {
    const db = client.database('{{cookiecutter.project_endpoint}}s-sql-db');
    const container = db.container('{{cookiecutter.project_endpoint}}s-sql-container');
    super(container);
  }
  {%- elif cookiecutter.cloud_service == 'GCP Cloud Function' -%}
  constructor(
    @inject(Firestore) firestore: Firestore
  ) {
    super(firestore, '{{cookiecutter.project_endpoint}}s');
  }
  {%- endif %}

  create = async (new{{cookiecutter.project_class_name}}: {{cookiecutter.project_class_name}}ItemRecord): Promise<{{cookiecutter.project_class_name}}ItemRecord> => this.addRecord(new{{cookiecutter.project_class_name}});
  get = async (id: string): Promise<{{cookiecutter.project_class_name}}ItemRecord> => this.getRecord(id);
  list = async (): Promise<{{cookiecutter.project_class_name}}ItemRecord[]> => this.getRecords();
  update = async ({{cookiecutter.project_lower_camel_name}}: Partial<{{cookiecutter.project_class_name}}ItemRecord>): Promise<{{cookiecutter.project_class_name}}ItemRecord> => this.updateRecord({{cookiecutter.project_lower_camel_name}});
  delete = async (id: string): Promise<void> => this.deleteRecord(id);
}
