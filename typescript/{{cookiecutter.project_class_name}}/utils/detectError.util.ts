{% if cookiecutter.cloud_service == 'Azure Function App' -%}
import { HttpResponseInit } from '@azure/functions';
{%- elif cookiecutter.cloud_service == 'GCP Cloud Function' -%}
interface ErrorResponse {
  status: number;
  body: string;
}
{%- endif %}
import { BaseError } from '../types/errors/base.error';

{% if cookiecutter.cloud_service == 'Azure Function App' -%}
export const detectError = <T>(error: T) => {
  if (error instanceof BaseError && error.statusCode !== undefined) {
    return {
      status: error.statusCode,
      body: error.message,
    } as HttpResponseInit;
  }
  return {
    status: 500,
    body: 'Unknown error occurred.',
  } as HttpResponseInit;
};
{%- elif cookiecutter.cloud_service == 'GCP Cloud Function' -%}
export const detectError = <T>(error: T): ErrorResponse => {
  if (error instanceof BaseError && error.statusCode !== undefined) {
    return {
      status: error.statusCode,
      body: error.message,
    };
  }
  return {
    status: 500,
    body: 'Unknown error occurred.',
  };
};
{%- endif %}
