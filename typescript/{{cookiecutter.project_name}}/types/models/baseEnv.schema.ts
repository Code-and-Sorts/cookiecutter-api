import { z } from 'zod';

export const baseEnvSchema = z.object({
    COSMOS_DB_URL: z.string().url(),
    COSMOS_DB_KEY: z.string().min(1),
});
