import { z } from 'zod';

export const baseEnvSchema = z.object({
    COSMOS_DB_URL: z.url(),
    COSMOS_DB_KEY: z.string().min(1),
    COSMOS_DB_DATABASE_NAME: z.string().default('kittenclawss-sql-db'),
    COSMOS_DB_EMULATOR: z.stringbool().default(false),
    COSMOS_CONTAINER_KITTENCLAWS: z.string().default('kittenclaws'),
});
