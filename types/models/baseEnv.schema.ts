import { z } from 'zod';

export const baseEnvSchema = z.object({
    COSMOS_DB_URL: z.url(),
    COSMOS_DB_KEY: z.string().min(1),
    COSMOS_DB_DATABASE_NAME: z.string().default('kittenclawss-sql-db'),
    COSMOS_DB_EMULATOR: z.stringbool().default(false),
    COSMOS_CONTAINER_CATS: z.string().default('cats'),
    COSMOS_CONTAINER_DOGS: z.string().default('dogs'),
    COSMOS_CONTAINER_VISITS: z.string().default('visits'),
});
