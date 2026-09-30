export const DEFAULT_LIST_LIMIT = 100;
export const MAX_LIST_LIMIT = 1000;

export const coerceLimit = (raw?: string | number | null): number => {
  const parsed = typeof raw === 'number' ? raw : /^\s*\d+\s*$/.test(raw ?? '') ? Number(raw) : NaN;
  if (!Number.isInteger(parsed) || parsed < 1) {
    return DEFAULT_LIST_LIMIT;
  }
  return Math.min(parsed, MAX_LIST_LIMIT);
};
