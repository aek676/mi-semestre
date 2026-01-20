import { Api } from './api';

const baseUrl = import.meta.env.API_BASE_URL || process.env.API_BASE_URL;

let instance: Api<unknown> | null = null;

export function getApi() {
  if (!instance) {
    instance = new Api({ baseUrl });
  }
  return instance;
}

export const api = getApi();
