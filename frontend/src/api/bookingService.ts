import apiClient from './apiClient';
import type {
    Booking,
    AssignMechanicPayload,
    BookingSearchParameters,
    AdminCancelBookingPayload,
    MarkAsPaidPayload,
    CreateBookingPayload, FinishJobPayload
} from '../types/booking';

export const searchBookings = async (params?: BookingSearchParameters): Promise<Booking[]> => {
    const response = await apiClient.get<Booking[]>('/bookings/search', { params });
    return response.data;
};

// PATCH assign mechanic
export const assignMechanicToBooking = async (payload: AssignMechanicPayload): Promise<void> => {
    await apiClient.patch('/bookings/assign-mechanic', payload);
};

// PATCH admin cancel booking
export const adminCancelBooking = async (bookingId: number, payload: AdminCancelBookingPayload): Promise<void> => {
    await apiClient.patch(`/bookings/${bookingId}/admin-cancel`, payload);
};

// PATCH mark booking as paid
export const markBookingAsPaid = async (bookingId: number, payload: MarkAsPaidPayload): Promise<void> => {
    await apiClient.patch(`/bookings/${bookingId}/mark-as-paid`, payload);
};

export const createBooking = async (bookingData: CreateBookingPayload): Promise<Booking> => {
    const response = await apiClient.post<Booking>('/bookings', bookingData);
    return response.data;
};

/**
 * Marks a booking job as started by the mechanic.
 * Corresponds to POST /api/Bookings/{bookingId}/start-job
 * @param bookingId - The ID of the booking to start.
 */
export const startBookingJob = async (bookingId: number): Promise<void> => {
    await apiClient.post(`/bookings/${bookingId}/start-job`);
};

/**
 * Marks a booking job as finished by the mechanic, including work details.
 * Corresponds to POST /api/Bookings/{bookingId}/finish-job
 * @param bookingId - The ID of the booking to finish.
 * @param payload - The details of the finished job (description, labour cost, used parts).
 */
export const finishBookingJob = async (bookingId: number, payload: FinishJobPayload): Promise<void> => {
    // Assuming the API returns 200 OK or 204 No Content on success
    await apiClient.post(`/bookings/${bookingId}/finish-job`, payload);
};