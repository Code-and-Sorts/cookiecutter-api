export const DEFAULT_LIST_LIMIT = 100;
export const MAX_LIST_LIMIT = 1000;

// Parses a list `limit` query value, falling back to the default for missing or
// invalid input and capping it at the maximum.
export const coerceLimit = (raw?: string | number): number => {
  const parsed = typeof raw === 'number' ? raw : parseInt(raw ?? '', 10);
  if (!Number.isFinite(parsed) || parsed < 1) {
    return DEFAULT_LIST_LIMIT;
  }
  return Math.min(parsed, MAX_LIST_LIMIT);
};
