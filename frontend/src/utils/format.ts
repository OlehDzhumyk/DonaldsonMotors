import axios from 'axios';

export const formatDateTime = (iso: string): string =>
    new Date(iso).toLocaleString('en-GB', {
        weekday: 'short', day: 'numeric', month: 'short', hour: '2-digit', minute: '2-digit',
    });

export const formatDate = (iso: string): string =>
    new Date(iso).toLocaleDateString('en-GB', { weekday: 'long', day: 'numeric', month: 'long' });

export const formatTime = (iso: string): string =>
    new Date(iso).toLocaleTimeString('en-GB', { hour: '2-digit', minute: '2-digit' });

const money = new Intl.NumberFormat('en-GB', { style: 'currency', currency: 'GBP' });

export const formatMoney = (amount: number | null | undefined): string => money.format(amount ?? 0);

export const formatHours = (hours: number): string =>
    hours === 1 ? '1 hour' : hours < 1 ? `${String(hours * 60)} min` : `${String(hours)} hours`;

const STATUS_LABELS: Record<string, string> = {
    InProgress: 'In progress',
    AwaitingPayment: 'Awaiting payment',
};

export const statusLabel = (status: string): string => STATUS_LABELS[status] ?? status;

/** Shapes of error bodies the API returns: a plain string, { message }, or ASP.NET validation problems. */
/** The API answers errors with ProblemDetails (RFC 9457); validation errors add `errors`. */
interface ApiErrorBody {
    message?: string;
    title?: string;
    detail?: string;
    errors?: Record<string, string[]> | string[];
}

/** Turns an API or network error into a message that can be shown to the user. */
export const getErrorMessage = (err: unknown, fallback: string): string => {
    if (axios.isAxiosError<ApiErrorBody | string>(err) && err.response) {
        const data = err.response.data;
        if (typeof data === 'string') return data || fallback;
        if (data.message) return data.message;
        if (data.errors) return Object.values(data.errors).flat().join(' ');
        if (data.detail) return data.detail;
        if (data.title) return data.title;
    }
    if (err instanceof Error && err.message) return err.message;
    return fallback;
};
