export const DEFAULT_LIST_LIMIT = 100;
export const MAX_LIST_LIMIT = 1000;

// Parses a list `limit` query value. Missing, non-integer or non-positive input falls back
// to the default, and values above the maximum are capped at it; neither is an error.
export const coerceLimit = (raw?: string | number | null): number => {
  const parsed = typeof raw === 'number' ? raw : /^\s*\d+\s*$/.test(raw ?? '') ? Number(raw) : NaN;
  if (!Number.isInteger(parsed) || parsed < 1) {
    return DEFAULT_LIST_LIMIT;
  }
  return Math.min(parsed, MAX_LIST_LIMIT);
};
