import { NotFoundError } from '../types/errors/notFound.error';
import { GuidSchema } from '../types/models/guid.schema';

// A non-UUID id can never exist, so it is a 404 rather than a 400.
export const assertUuid = (resource: string, id: string): void => {
  if (!GuidSchema.safeParse(id).success) {
    throw NotFoundError.forItem(resource, id);
  }
};
