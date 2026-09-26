import { config } from 'dotenv';
import { baseEnvSchema } from './baseEnv.schema';

const envSchema = baseEnvSchema;

config();

export const env = envSchema.parse(process.env);
