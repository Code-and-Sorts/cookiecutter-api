import { describe, it, expect } from '@jest/globals';
import {
{%- for resource in resources %}
    {{ resource.name | to_lower_camel }}Controller,
{%- endfor %}
{%- if cloud_service == 'Azure Function App' %}
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

    // A failing database must end in a 500 within 10 seconds: the client gives up early too.
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
});
