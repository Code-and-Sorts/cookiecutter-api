import { z } from 'zod';

export const baseEnvSchema = z.object({
    AWS_REGION: z.string().default('us-east-1'),
    DYNAMODB_TABLE_NAME_KITTIES: z.string().min(1).default('kitties'),
});
