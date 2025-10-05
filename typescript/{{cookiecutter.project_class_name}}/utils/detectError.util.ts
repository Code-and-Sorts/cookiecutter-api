{% if cookiecutter.cloud_service == 'Azure Function App' -%}
import { HttpResponseInit } from '@azure/functions';
{%- elif cookiecutter.cloud_service == 'GCP Cloud Function' -%}
interface ErrorResponse {
  status: number;
  body: string;
}
{%- endif %}
import { BaseError } from '../types/errors/base.error';

export const detectError = <T>(error: T){% if cookiecutter.cloud_service == 'Azure Function App' %}: HttpResponseInit{% elif cookiecutter.cloud_service == 'GCP Cloud Function' %}: ErrorResponse{% endif %} => {
  if (error instanceof BaseError && error.statusCode !== undefined) {
    return {
      status: error.statusCode,
      body: error.message,
    }{% if cookiecutter.cloud_service == 'Azure Function App' %} as HttpResponseInit{% endif %};
  }
  return {
    status: 500,
    body: 'Unknown error occurred.',
  }{% if cookiecutter.cloud_service == 'Azure Function App' %} as HttpResponseInit{% endif %};
};
