const BASE_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:8080';

const getHeaders = (): HeadersInit => {
  const token = localStorage.getItem('accessToken');
  return {
    'Content-Type': 'application/json',
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
  };
};

const handleResponse = async <T>(response: Response): Promise<T> => {
  if (response.status === 401) {
    localStorage.clear();
    window.location.href = '/login';
    return Promise.reject(new Error('Unauthorized'));
  }

  if (!response.ok) {
    const error = await response.json().catch(() => ({ error: response.statusText }));
    throw new Error(error.error ?? response.statusText);
  }

  return response.json() as Promise<T>;
};

const get = async <T>(url: string): Promise<T> => {
  const response = await fetch(`${BASE_URL}${url}`, {
    method: 'GET',
    headers: getHeaders(),
    credentials: 'include',
  });
  return handleResponse<T>(response);
};

const post = async <T>(url: string, body: unknown): Promise<T> => {
  const response = await fetch(`${BASE_URL}${url}`, {
    method: 'POST',
    headers: getHeaders(),
    credentials: 'include',
    body: JSON.stringify(body),
  });
  return handleResponse<T>(response);
};

const put = async <T>(url: string, body: unknown): Promise<T> => {
  const response = await fetch(`${BASE_URL}${url}`, {
    method: 'PUT',
    headers: getHeaders(),
    credentials: 'include',
    body: JSON.stringify(body),
  });
  return handleResponse<T>(response);
};

const del = async <T>(url: string): Promise<T> => {
  const response = await fetch(`${BASE_URL}${url}`, {
    method: 'DELETE',
    headers: getHeaders(),
    credentials: 'include',
  });
  return handleResponse<T>(response);
};

export const http = { get, post, put, del };
