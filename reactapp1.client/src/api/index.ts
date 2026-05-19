// Centralized API helper for the SPA.
// - Absolute URLs (/api/...)
// - credentials: 'include' for cookies
// - Adds XSRF header for unsafe methods (POST/PUT/PATCH/DELETE)
// - Safe JSON parsing only when content-type is JSON
// - postApiRaw returns status + body for detailed error handling

let xsrfToken: string | null = null;

function fullUrl(endpoint: string) {
  const cleaned = String(endpoint ?? '').replace(/^\/+/, '');
  return `/api/${cleaned}`;
}

function isJsonResponse(res: Response) {
  const ct = res.headers.get('content-type') || '';
  return ct.toLowerCase().includes('application/json');
}

async function parseJsonSafe<T>(res: Response): Promise<T | undefined> {
  if (!isJsonResponse(res)) return undefined;
  try {
    return (await res.json()) as T;
  } catch {
    return undefined;
  }
}

async function ensureXsrf(force = false): Promise<string | null> {
  if (!force && xsrfToken !== null) return xsrfToken;
  try {
    const res = await fetch('/api/antiforgery/token', {
      method: 'GET',
      credentials: 'include',
      headers: { 'X-Requested-With': 'XMLHttpRequest' },
    });
    const data = await parseJsonSafe<{ token: string }>(res);
    xsrfToken = data?.token ?? null;
  } catch {
    xsrfToken = null;
  }
  return xsrfToken;
}

function baseHeaders(extra: Record<string, string> = {}) {
  return {
    'X-Requested-With': 'XMLHttpRequest',
    Accept: 'application/json',
    ...extra,
  };
}

function looksLikeXsrfFailure(status: number, body?: any, text?: string) {
  if (status === 400 || status === 401 || status === 403) {
    const payload = (typeof body === 'string' ? body : JSON.stringify(body || '')).toLowerCase();
    const msg = (text || '').toLowerCase();
    return payload.includes('antiforgery') || msg.includes('antiforgery') || status === 400;
  }
  return false;
}

export async function getApi<T>(endpoint: string): Promise<T | undefined> {
  const url = fullUrl(endpoint);
  try {
    const res = await fetch(url, {
      method: 'GET',
      credentials: 'include',
      headers: baseHeaders(),
    });
    if (!res.ok) {
      const text = await res.text().catch(() => '');
      console.error(`GET ${url} failed: ${res.status} ${text}`);
      return undefined;
    }
    if (res.status === 204) return undefined;
    return await parseJsonSafe<T>(res);
  } catch (e: any) {
    console.error(`GET ${url} error:`, e?.message ?? e);
    return undefined;
  }
}

async function unsafeFetch(
  method: 'POST' | 'PUT' | 'DELETE' | 'PATCH',
  endpoint: string,
  data: Record<string, any>,
  retry = true
): Promise<{ ok: boolean; status: number; bodyJson?: any; bodyText?: string }> {
  const url = fullUrl(endpoint);

  // Always ensure token before unsafe calls
  await ensureXsrf(xsrfToken === null);

  const headers: Record<string, string> = { 'Content-Type': 'application/json' };
  const token = xsrfToken;
  if (token) headers['X-XSRF-TOKEN'] = token;

  const res = await fetch(url, {
    method,
    credentials: 'include',
    headers: baseHeaders(headers),
    body: JSON.stringify(data ?? {}),
  });

  const bodyJson = await parseJsonSafe<any>(res);
  const bodyText = bodyJson ? undefined : await res.text().catch(() => undefined);

  if (!res.ok && retry && looksLikeXsrfFailure(res.status, bodyJson, bodyText)) {
    // Reset and re-fetch token, then retry once
    xsrfToken = null;
    await ensureXsrf(true);

    const retryHeaders: Record<string, string> = { 'Content-Type': 'application/json' };
    if (xsrfToken) retryHeaders['X-XSRF-TOKEN'] = xsrfToken;

    const res2 = await fetch(url, {
      method,
      credentials: 'include',
      headers: baseHeaders(retryHeaders),
      body: JSON.stringify(data ?? {}),
    });
    const bodyJson2 = await parseJsonSafe<any>(res2);
    const bodyText2 = bodyJson2 ? undefined : await res2.text().catch(() => undefined);
    return { ok: res2.ok, status: res2.status, bodyJson: bodyJson2, bodyText: bodyText2 };
  }

  return { ok: res.ok, status: res.status, bodyJson, bodyText };
}

export async function postApi(
  endpoint: string,
  data: Record<string, any>,
  methodType: 'POST' | 'PUT' | 'DELETE' | 'PATCH' | '' = '',
) {
  const method: 'POST' | 'PUT' | 'DELETE' | 'PATCH' =
    methodType === 'PUT' ? 'PUT' :
    methodType === 'DELETE' ? 'DELETE' :
    methodType === 'PATCH' ? 'PATCH' : 'POST';

  try {
    const res = await unsafeFetch(method, endpoint, data);
    if (!res.ok) {
      console.error(`${method} ${fullUrl(endpoint)} failed: ${res.status} ${res.bodyText ?? JSON.stringify(res.bodyJson ?? '')}`);
      return undefined;
    }
    return res.bodyJson ?? undefined;
  } catch (e: any) {
    console.error(`${method} ${fullUrl(endpoint)} error:`, e?.message ?? e);
    return undefined;
  }
}

export async function putApi(endpoint: string, data: Record<string, any>) {
  return postApi(endpoint, data, 'PUT');
}

export async function deleteApi(endpoint: string, data: Record<string, any>) {
  return postApi(endpoint, data, 'DELETE');
}

// Raw variant to expose non-2xx responses and payloads (for Identity errors, etc.)
export async function postApiRaw(
  endpoint: string,
  data: Record<string, any>,
  methodType: 'POST' | 'PUT' | 'DELETE' | 'PATCH' | '' = '',
): Promise<{ ok: boolean; status: number; bodyJson?: any; bodyText?: string }> {
  const method: 'POST' | 'PUT' | 'DELETE' | 'PATCH' =
    methodType === 'PUT' ? 'PUT' :
    methodType === 'DELETE' ? 'DELETE' :
    methodType === 'PATCH' ? 'PATCH' : 'POST';

  return unsafeFetch(method, endpoint, data);
}

export function resetXsrf() {
  xsrfToken = null;
}