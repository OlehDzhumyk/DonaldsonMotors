import { describe, expect, it } from 'vitest';
import { AxiosError, AxiosHeaders, type AxiosResponse } from 'axios';
import { formatHours, formatMoney, getErrorMessage, statusLabel } from './format';

const axiosErrorWith = (status: number, data: unknown) => {
    const response = { status, data, statusText: '', headers: {}, config: { headers: new AxiosHeaders() } } as AxiosResponse;
    return new AxiosError('Request failed', 'ERR_BAD_RESPONSE', undefined, undefined, response);
};

describe('formatting', () => {
    it('formats money in pounds with two decimals', () => {
        expect(formatMoney(80)).toBe('£80.00');
        expect(formatMoney(null)).toBe('£0.00');
        expect(formatMoney(3485)).toBe('£3,485.00');
    });

    it('describes durations in words', () => {
        expect(formatHours(1)).toBe('1 hour');
        expect(formatHours(0.5)).toBe('30 min');
        expect(formatHours(3)).toBe('3 hours');
    });

    it('spells out multi-word statuses', () => {
        expect(statusLabel('AwaitingPayment')).toBe('Awaiting payment');
        expect(statusLabel('Pending')).toBe('Pending');
    });
});

describe('getErrorMessage', () => {
    it('uses a plain-text error body from the API', () => {
        expect(getErrorMessage(axiosErrorWith(409, 'This time slot has just been booked.'), 'fallback'))
            .toBe('This time slot has just been booked.');
    });

    it('uses the message field of a JSON error', () => {
        expect(getErrorMessage(axiosErrorWith(401, { message: 'Invalid credentials.' }), 'fallback')).toBe('Invalid credentials.');
    });

    it('joins ASP.NET validation errors', () => {
        const body = { title: 'One or more validation errors occurred.', errors: { Reason: ['Too short.'], Email: ['Required.'] } };
        expect(getErrorMessage(axiosErrorWith(400, body), 'fallback')).toBe('Too short. Required.');
    });

    it('uses the detail of a ProblemDetails body', () => {
        const body = { title: 'Conflict', status: 409, detail: 'This time slot has just been booked.' };
        expect(getErrorMessage(axiosErrorWith(409, body), 'fallback')).toBe('This time slot has just been booked.');
    });

    it('joins a plain list of errors, such as password rules', () => {
        const body = { title: 'Bad Request', detail: 'Registration failed', errors: ['Too short.', 'Needs a digit.'] };
        expect(getErrorMessage(axiosErrorWith(400, body), 'fallback')).toBe('Too short. Needs a digit.');
    });

    it('falls back when there is nothing useful', () => {
        expect(getErrorMessage(axiosErrorWith(500, ''), 'Could not save.')).toBe('Could not save.');
        expect(getErrorMessage('weird', 'Could not save.')).toBe('Could not save.');
    });
});
