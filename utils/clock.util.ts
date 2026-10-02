import { randomUUID } from 'node:crypto';

export const nowIso = (): string => new Date().toISOString();

export const newId = (): string => randomUUID();
