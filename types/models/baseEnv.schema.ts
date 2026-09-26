import { z } from 'zod';

export const baseEnvSchema = z.object({
    GCP_PROJECT_ID: z.string().min(1),
    FIRESTORE_DATABASE: z.string().default('(default)'),
    FIRESTORE_COLLECTION_ANIMALS: z.string().default('animals'),
});
