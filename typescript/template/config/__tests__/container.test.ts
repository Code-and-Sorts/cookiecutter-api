import { describe, it, expect } from '@jest/globals';
{%- if cloud_service == 'Azure Function App' %}
import { DefaultAzureCredential } from '@azure/identity';
{%- endif %}
import {
{%- for resource in resources %}
    {{ resource.name | to_lower_camel }}Controller,
{%- endfor %}
{%- if cloud_service == 'Azure Function App' %}
    cosmosClientOptions,
    cosmosConnectionPolicy,
{%- elif cloud_service == 'GCP Cloud Function' %}
    firestoreRetryParams,
{%- else %}
    dynamoDbClientConfig,
{%- endif %}
} from '@config/container';
import {
{%- for resource in resources %}
    {{ resource.name }}Controller,
{%- endfor %}
} from '@controllers';
import { DATABASE_ATTEMPT_TIMEOUT_MS, DATABASE_DEADLINE_MS } from '@utils';

describe('container', () => {
{%- for resource in resources %}
    it('should resolve {{ resource.name }}Controller with its dependencies', () => {
        expect({{ resource.name | to_lower_camel }}Controller).toBeInstanceOf({{ resource.name }}Controller);
    });
{%- endfor %}

    it('should cap database client timeouts and retries', () => {
        expect(DATABASE_DEADLINE_MS).toBeLessThan(10000);
{%- if cloud_service == 'Azure Function App' %}
        expect(cosmosConnectionPolicy.requestTimeout).toEqual(DATABASE_ATTEMPT_TIMEOUT_MS);
        expect(cosmosConnectionPolicy.retryOptions.maxWaitTimeInSeconds * 1000).toBeLessThanOrEqual(DATABASE_DEADLINE_MS);
{%- elif cloud_service == 'GCP Cloud Function' %}
        expect(firestoreRetryParams.max_rpc_timeout_millis).toEqual(DATABASE_ATTEMPT_TIMEOUT_MS);
        expect(firestoreRetryParams.total_timeout_millis).toBeLessThanOrEqual(DATABASE_DEADLINE_MS);
{%- else %}
        expect(dynamoDbClientConfig.maxAttempts).toEqual(2);
        expect(dynamoDbClientConfig.requestHandler.throwOnRequestTimeout).toBe(true);
        expect(dynamoDbClientConfig.requestHandler.requestTimeout * dynamoDbClientConfig.maxAttempts).toBeLessThanOrEqual(DATABASE_DEADLINE_MS);
{%- endif %}
    });
{%- if cloud_service == 'Azure Function App' %}

    describe('cosmosClientOptions', () => {
        const settings = (url: string, emulator: boolean) => ({ COSMOS_DB_URL: url, COSMOS_DB_KEY: 'key', COSMOS_DB_EMULATOR: emulator });

        it('should configure the client as in production without the emulator flag', () => {
            const options = cosmosClientOptions(settings('https://account.documents.azure.com:443/', false));

            expect(options).toEqual({ endpoint: 'https://account.documents.azure.com:443/', key: 'key', connectionPolicy: cosmosConnectionPolicy });
        });

        it.each([undefined, ''])('should sign in with Microsoft Entra ID when the key is %p', (key) => {
            const options = cosmosClientOptions({ ...settings('https://account.documents.azure.com:443/', false), COSMOS_DB_KEY: key });

            expect(options).toEqual({
                endpoint: 'https://account.documents.azure.com:443/',
                aadCredentials: expect.any(DefaultAzureCredential),
                connectionPolicy: cosmosConnectionPolicy,
            });
            expect(options).not.toHaveProperty('key');
        });

        it('should keep the configured endpoint for the emulator', () => {
            const options = cosmosClientOptions(settings('http://localhost:8081/', true));

            expect(options.connectionPolicy).toEqual({ ...cosmosConnectionPolicy, enableEndpointDiscovery: false });
            expect(options.agent).toBeUndefined();
        });

        it('should skip certificate checks only for an https emulator', () => {
            const options = cosmosClientOptions(settings('https://localhost:8081/', true));

            expect((options.agent as unknown as { options: { rejectUnauthorized: boolean } }).options.rejectUnauthorized).toBe(false);
        });
    });
{%- endif %}
});
