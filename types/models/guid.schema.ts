import { z } from 'zod';

export const GuidSchema = z.string().refine(value => {
    const guidRegex = /^[0-9a-f]{8}-[0-9a-f]{4}-4[0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i;
    return guidRegex.test(value);
}, {
    message: 'Invalid ID format',
});
