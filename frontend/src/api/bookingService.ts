import apiClient from './apiClient';
import type { Booking, BookingSearchParameters, CreateBookingPayload, FinishJobPayload } from '../types/booking';

/** The API limits results to the caller: customers see their own bookings, mechanics their own jobs. */
export const searchBookings = async (params: BookingSearchParameters = {}): Promise<Booking[]> => {
    const { data } = await apiClient.get<Booking[]>('/bookings/search', { params });
    return data;
};

export const createBooking = async (payload: CreateBookingPayload): Promise<Booking> => {
    const { data } = await apiClient.post<Booking>('/bookings', payload);
    return data;
};

export const cancelMyBooking = async (bookingId: number, reason: string): Promise<void> => {
    await apiClient.patch(`/bookings/${String(bookingId)}/cancel-by-customer`, { reason });
};

export const assignMechanic = async (bookingId: number, mechanicId: number): Promise<void> => {
    await apiClient.patch('/bookings/assign-mechanic', { bookingId, mechanicId });
};

export const cancelBookingAsManager = async (bookingId: number, reason: string): Promise<void> => {
    await apiClient.patch(`/bookings/${String(bookingId)}/admin-cancel`, { reason });
};

export const markBookingAsPaid = async (bookingId: number, paymentNotes: string | null): Promise<void> => {
    await apiClient.patch(`/bookings/${String(bookingId)}/mark-as-paid`, { paymentNotes });
};

export const startJob = async (bookingId: number): Promise<void> => {
    await apiClient.post(`/bookings/${String(bookingId)}/start-job`);
};

export const finishJob = async (bookingId: number, payload: FinishJobPayload): Promise<void> => {
    await apiClient.post(`/bookings/${String(bookingId)}/finish-job`, payload);
};
