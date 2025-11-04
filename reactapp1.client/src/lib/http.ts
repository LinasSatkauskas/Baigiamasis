// Simple wrapper around fetch to include cookies and XSRF token on unsafe methods
const API_PREFIX = '/api';

let cachedXsrf: string | null = null;

async function ensureXsrf(): Promise<string> {
  if (cachedXsrf) return cachedXsrf;

  const res = await fetch(`${API_PREFIX}/antiforgery/token`, {
    method: 'GET',
    credentials: 'include'
  });
  if (!res.ok) throw new Error('Failed to get anti-forgery token');
  const data = await res.json();
  cachedXsrf = data.token ?? null;
  return cachedXsrf!;
}

export async function apiFetch(input: string, init: RequestInit = {}) {
  const url = input.startsWith('/api') ? input : `${API_PREFIX}${input}`;
  const method = (init.method ?? 'GET').toUpperCase();
  const isUnsafe = method !== 'GET' && method !== 'HEAD' && method !== 'OPTIONS' && method !== 'TRACE';

  const headers = new Headers(init.headers || {});
  let xsrfHeader = {};
  if (isUnsafe) {
    const token = await ensureXsrf();
    headers.set('X-XSRF-TOKEN', token);
    xsrfHeader = { 'X-XSRF-TOKEN': token };
  }

  return fetch(url, {
    ...init,
    method,
    headers,
    credentials: 'include' // important for cookie-based auth
  });
}