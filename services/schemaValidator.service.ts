import 'reflect-metadata';
import { injectable } from 'inversify';
import { z } from 'zod';
import { ValidationError } from '@errors';

type Issue = z.core.$ZodIssue;

const article = (word: string): string => (/^[aeiou]/i.test(word) ? 'an' : 'a');

const describeIssue = (issue: Issue): string => {
  const field = issue.path.join('.');
  switch (issue.code) {
    case 'unrecognized_keys':
      return `Unknown field${issue.keys.length > 1 ? 's' : ''}: ${issue.keys.join(', ')}.`;
    case 'invalid_type':
      if (!field) {
        return 'Request body must be a JSON object.';
      }
      if (!('input' in issue) || issue.input === undefined) {
        return `${field} is required.`;
      }
      return `${field} must be ${article(issue.expected)} ${issue.expected}.`;
    case 'too_small':
      if (issue.origin === 'string' && issue.minimum === 1) {
        return `${field} must not be empty.`;
      }
      break;
  }
  return field ? `${field}: ${issue.message}` : issue.message;
};

@injectable()
export class SchemaValidator {
  validate<T>(obj: unknown, schema: z.ZodType<T>): T {
    const result = schema.safeParse(obj, { reportInput: true });
    if (result.success) {
      return result.data;
    }
    throw new ValidationError(result.error.issues.map(describeIssue).join(' '));
  }
}
