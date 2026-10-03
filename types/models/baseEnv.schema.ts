import { z } from 'zod';

export const baseEnvSchema = z.object({
    GCP_PROJECT_ID: z.string().min(1),
    FIRESTORE_DATABASE: z.string().default('(default)'),
    FIRESTORE_EMULATOR_HOST: z.string().optional(),
    FIRESTORE_COLLECTION_CATS: z.string().default('cats'),
    FIRESTORE_COLLECTION_DOGS: z.string().default('dogs'),
    FIRESTORE_COLLECTION_VISITS: z.string().default('visits'),
});
