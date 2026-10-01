import { {% if cloud_service == 'AWS Lambda' %}afterEach, {% endif %}describe, it, expect } from '@jest/globals';
{%- if cloud_service == 'AWS Lambda' %}
import { DynamoDBClient, ListTablesCommand } from '@aws-sdk/client-dynamodb';
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
{%- if cloud_service == 'AWS Lambda' %}

    describe('DynamoDB endpoint', () => {
        const saved = { ...process.env };
        afterEach(() => {
            process.env = { ...saved };
        });

        // The SDK resolves the endpoint per request, so capture the host the request is about to use.
        const requestHost = async (): Promise<string | undefined> => {
            const client = new DynamoDBClient({ ...dynamoDbClientConfig, credentials: { accessKeyId: 'test', secretAccessKey: 'test' } });
            let host: string | undefined;
            client.middlewareStack.add(
                () => async (args) => {
                    host = (args.request as { hostname: string }).hostname;
                    throw new Error('request captured');
                },
                { step: 'finalizeRequest' },
            );
            await client.send(new ListTablesCommand({})).catch(() => undefined);
            return host;
        };

        it.each([undefined, ''])('should use the AWS endpoint when the override is %p', async (override) => {
            delete process.env.AWS_ENDPOINT_URL;
            if (override === undefined) {
                delete process.env.AWS_ENDPOINT_URL_DYNAMODB;
            } else {
                process.env.AWS_ENDPOINT_URL_DYNAMODB = override;
            }

            expect(await requestHost()).toEqual('dynamodb.us-east-1.amazonaws.com');
        });

        it('should read the emulator endpoint from AWS_ENDPOINT_URL_DYNAMODB', async () => {
            process.env.AWS_ENDPOINT_URL_DYNAMODB = 'http://localhost:8000';

            expect(await requestHost()).toEqual('localhost');
        });
    });
{%- endif %}
});
