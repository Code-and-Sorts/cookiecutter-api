import { z } from 'zod';

export const baseEnvSchema = z.object({
    COSMOS_DB_URL: z.string().url(),
    COSMOS_DB_KEY: z.string().min(1),
    COSMOS_DB_DATABASE_NAME: z.string().default('kittiess-sql-db'),
    COSMOS_CONTAINER_KITTIES: z.string().default('kitties'),
});
