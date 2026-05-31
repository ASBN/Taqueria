import { authStorage } from './authStorage';

interface ApiError {
  mensaje?: string;
  codigo?: string;
}

async function request<T>(url: string, options: RequestInit = {}): Promise<T> {
  const session = authStorage.get();
  const headers = new Headers(options.headers);
  headers.set('Content-Type', 'application/json');

  if (session?.token) {
    headers.set('Authorization', `Bearer ${session.token}`);
  }

  const response = await fetch(url, { ...options, headers });
  await asegurarRespuestaExitosa(response);

  if (response.status === 204) {
    return undefined as T;
  }

  return response.json() as Promise<T>;
}

async function asegurarRespuestaExitosa(response: Response): Promise<void> {
  if (response.ok) {
    return;
  }

  const error = (await response.json().catch(() => ({}))) as ApiError;

  if (response.status === 401) {
    authStorage.clear();
    if (window.location.pathname !== '/login') {
      window.location.assign('/login');
    }
  }

  if (response.status === 403 && error.codigo === 'PASSWORD_CHANGE_REQUIRED') {
    const session = authStorage.get();
    if (session) {
      authStorage.set({
        ...session,
        usuario: { ...session.usuario, debeCambiarPassword: true }
      });
    }
    if (window.location.pathname !== '/cambiar-password') {
      window.location.assign('/cambiar-password');
    }
  }

  throw new Error(error.mensaje ?? 'No fue posible completar la operación.');
}

async function download(url: string): Promise<{ blob: Blob; filename: string }> {
  const session = authStorage.get();
  const headers = new Headers();
  if (session?.token) {
    headers.set('Authorization', `Bearer ${session.token}`);
  }

  const response = await fetch(url, { headers });
  await asegurarRespuestaExitosa(response);

  const disposition = response.headers.get('Content-Disposition') ?? '';
  const filename = disposition.match(/filename\*?=(?:UTF-8''|\")?([^";]+)/i)?.[1]
    ?.replace(/"/g, '')
    ?.trim() ?? 'Reporte.xlsx';

  return { blob: await response.blob(), filename: decodeURIComponent(filename) };
}

export const apiClient = {
  get<T>(url: string): Promise<T> {
    return request<T>(url);
  },

  post<T>(url: string, body: unknown): Promise<T> {
    return request<T>(url, { method: 'POST', body: JSON.stringify(body) });
  },

  put<T>(url: string, body: unknown): Promise<T> {
    return request<T>(url, { method: 'PUT', body: JSON.stringify(body) });
  },

  delete<T = void>(url: string): Promise<T> {
    return request<T>(url, { method: 'DELETE' });
  },

  download(url: string): Promise<{ blob: Blob; filename: string }> {
    return download(url);
  }
};
