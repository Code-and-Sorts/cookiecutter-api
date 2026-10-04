import 'reflect-metadata';
import { inject, injectable } from 'inversify';
import { KittenClawsRepository } from '@repositories';
import {
    KittenClaws,
    KittenClawsResponse,
    KittenClawsUpdate,
    toKittenClawsResponse,
} from '@models';

@injectable()
export class KittenClawsService {
  private _repo: KittenClawsRepository;

  constructor(@inject(KittenClawsRepository) repo: KittenClawsRepository) {
    this._repo = repo;
  }

  create = async (item: KittenClaws, userId?: string): Promise<KittenClawsResponse> =>
    toKittenClawsResponse(await this._repo.create(item, userId));

  get = async (id: string): Promise<KittenClawsResponse> => toKittenClawsResponse(await this._repo.get(id));

  list = async (limit?: number): Promise<KittenClawsResponse[]> => (await this._repo.list(limit)).map(toKittenClawsResponse);

  update = async (id: string, fields: KittenClawsUpdate, userId?: string): Promise<KittenClawsResponse> =>
    toKittenClawsResponse(await this._repo.update(id, fields, userId));

  delete = (id: string, userId?: string): Promise<void> => this._repo.delete(id, userId);
}
