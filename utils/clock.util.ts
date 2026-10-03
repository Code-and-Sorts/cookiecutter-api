import { randomUUID } from 'node:crypto';

export const nowIso = (): string => new Date().toISOString();

export const todayIso = (): string => nowIso().slice(0, 10);

export const newId = (): string => randomUUID();
