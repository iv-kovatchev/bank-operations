const BASE_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:8080';

const getHeaders = (): HeadersInit => {
  const token = localStorage.getItem('accessToken');
  return {
    'Content-Type': 'application/json',
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
  };
};

const handleResponse = async <T>(response: Response): Promise<T> => {
  if (!response.ok) {
    const body = await response.json().catch(() => null);
    let message: string;
    if (body?.error) {
      message = body.error;
    } else if (body?.errors) {
      message = (Object.values(body.errors) as string[][]).flat().join(', ');
    } else if (body?.title) {
      message = body.title;
    } else {
      message = 'Something went wrong';
    }

    if (response.status === 401 && window.location.pathname !== '/login') {
      localStorage.clear();
      window.location.href = '/login';
    }

    throw new Error(message);
  }

  if (response.status === 204 || response.headers.get('content-length') === '0') {
    return undefined as T;
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

const patch = async <T>(url: string, body?: unknown): Promise<T> => {
  const response = await fetch(`${BASE_URL}${url}`, {
    method: 'PATCH',
    headers: getHeaders(),
    credentials: 'include',
    ...(body !== undefined ? { body: JSON.stringify(body) } : {}),
  });
  return handleResponse<T>(response);
};

export const http = { get, post, put, del, patch };
