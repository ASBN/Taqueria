export const currency = (value: number | null): string =>
  value === null
    ? 'Sin precio'
    : new Intl.NumberFormat('es-MX', { style: 'currency', currency: 'MXN' }).format(value);

export const toDateTimeLocal = (iso: string): string => {
  const value = new Date(iso);
  const local = new Date(value.getTime() - value.getTimezoneOffset() * 60_000);
  return local.toISOString().slice(0, 16);
};

export const localInputToUtcIso = (value: string): string => new Date(value).toISOString();


export const todayLocalInputValue = (): string => {
  const now = new Date();
  const local = new Date(now.getTime() - now.getTimezoneOffset() * 60_000);
  return local.toISOString().slice(0, 10);
};

export const timeLocal = (iso: string): string =>
  new Date(iso).toLocaleTimeString('es-MX', { hour: '2-digit', minute: '2-digit' });
