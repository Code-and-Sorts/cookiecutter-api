import { CollectionReference, FieldValue, Firestore } from '@google-cloud/firestore';
import { BaseItemRecord } from '@models';
import { DocumentStore, StoreFactory } from './document.store';

export class FirestoreStore<T extends BaseItemRecord> implements DocumentStore<T> {
  constructor(private readonly collection: CollectionReference) {}

  read = async (id: string): Promise<T | undefined> => {
    const doc = await this.collection.doc(id).get();
    return doc.exists ? (doc.data() as T) : undefined;
  };

  query = async (limit: number): Promise<T[]> => {
    const snapshot = await this.collection.where('isDeleted', '==', false).limit(limit).get();
    return snapshot.docs.map((doc) => doc.data() as T);
  };

  create = async (item: T): Promise<void> => {
    await this.collection.doc(item.id).set(item);
  };

  write = async (item: T): Promise<boolean> => {
    await this.collection.doc(item.id).set(item);
    return true;
  };

  softDelete = async (id: string, updatedTimestamp: string, updatedBy?: string): Promise<boolean> => {
    const current = await this.read(id);
    if (!current || current.isDeleted) {
      return false;
    }
    await this.collection.doc(id).update({ isDeleted: true, updatedTimestamp, updatedBy: updatedBy ?? FieldValue.delete() });
    return true;
  };
}

export const firestoreStoreFactory = (client: Firestore): StoreFactory =>
  <T extends BaseItemRecord>(name: string) => new FirestoreStore<T>(client.collection(name));
