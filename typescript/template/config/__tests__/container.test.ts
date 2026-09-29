import { describe, it, expect } from '@jest/globals';
import {
{%- for resource in resources %}
    {{ resource.name | to_lower_camel }}Controller,
{%- endfor %}
} from '@config/container';
import {
{%- for resource in resources %}
    {{ resource.name }}Controller,
{%- endfor %}
} from '@controllers';

describe('container', () => {
{%- for resource in resources %}
    it('should resolve {{ resource.name }}Controller with its dependencies', () => {
        expect({{ resource.name | to_lower_camel }}Controller).toBeInstanceOf({{ resource.name }}Controller);
    });
{%- endfor %}
});
