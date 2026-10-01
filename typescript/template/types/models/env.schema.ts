import { config } from 'dotenv';
import { baseEnvSchema } from './baseEnv.schema';

const envSchema = baseEnvSchema;

// quiet: dotenv otherwise prints an "injecting env" banner on every cold start.
config({ quiet: true });

export const env = envSchema.parse(process.env);
