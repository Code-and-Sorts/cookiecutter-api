import { z } from 'zod';

export const baseEnvSchema = z.object({
    AWS_REGION: z.string().default('us-east-1'),
    AWS_ENDPOINT_URL_DYNAMODB: z.string().optional(),
    DYNAMODB_TABLE_NAME_KITTENCLAWS: z.string().min(1).default('kittenclaws'),
});
