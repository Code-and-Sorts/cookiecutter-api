{%- set gcp = cloud_service == 'GCP Cloud Function' -%}
import { describe, it, expect } from '@jest/globals';
{%- if gcp %}
import * as ff from '@google-cloud/functions-framework';
{%- else %}
import { APIGatewayProxyEvent } from 'aws-lambda';
{%- endif %}
import { ValidationError } from '@errors';
import { MAX_USER_ID_LENGTH, USER_ID_TOO_LONG_MESSAGE } from '@utils';
import { userIdFrom } from '../response';
{% if gcp %}
const request = (headers: Record<string, string | string[]> | null) => ({ headers }) as unknown as ff.Request;
{%- else %}
const request = (headers: Record<string, string> | null) => ({ headers }) as unknown as APIGatewayProxyEvent;
{%- endif %}
const longest = 'u'.repeat(MAX_USER_ID_LENGTH);

describe('userIdFrom', () => {
    it.each([
{%- if gcp %}
        { headers: { 'x-user-id': ' user-1 ' }, expected: 'user-1' },
        { headers: { 'x-user-id': [longest, 'other'] }, expected: longest },
        { headers: { 'x-user-id': '   ' }, expected: undefined },
        { headers: {}, expected: undefined },
{%- else %}
        { headers: { 'X-User-Id': ' user-1 ' }, expected: 'user-1' },
        { headers: { 'x-user-id': longest }, expected: longest },
        { headers: { 'X-USER-ID': '   ' }, expected: undefined },
        { headers: {}, expected: undefined },
        { headers: null, expected: undefined },
{%- endif %}
    ])('should read $headers as $expected', ({ headers, expected }) => {
        expect(userIdFrom(request(headers))).toEqual(expected);
    });

    it('should reject a user id over the maximum length', () => {
        const call = () => userIdFrom(request({ 'x-user-id': `${longest}u` }));
        expect(call).toThrow(ValidationError);
        expect(call).toThrow(USER_ID_TOO_LONG_MESSAGE);
    });
});
