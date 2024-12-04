import { z } from 'zod';
import { ValidationError } from '@errors';
import { injectable } from 'inversify';
import 'reflect-metadata';

@injectable()
export class SchemaValidator {
  validate<T>(obj: any, schema: z.ZodType<T>): T {
    try {
      const validatedData = schema.parse(obj);
      return validatedData;
    } catch (error) {
      const errorInputs: Array<{ param: string; issue: string; data: any }> = [];
      if (error instanceof z.ZodError) {
        error.issues.map((issue) => {
          errorInputs.push({
            param: issue.path.join('.'),
            issue: issue.message,
            data: issue,
          });
        });
      }
      throw new ValidationError('Failed schema validation');
    }
  }
}
