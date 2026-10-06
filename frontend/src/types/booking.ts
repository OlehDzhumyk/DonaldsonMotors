export type BookingStatus = 'Pending' | 'Assigned' | 'InProgress' | 'AwaitingPayment' | 'Paid' | 'Cancelled' | 'Archived';

export const ACTIVE_STATUSES: BookingStatus[] = ['Pending', 'Assigned', 'InProgress', 'AwaitingPayment'];

/** Mirrors BookingResponseDto in the API. */
export interface Booking {
    id: number;
    slotStart: string;
    status: BookingStatus;

    serviceTypeId: number;
    serviceTypeName: string;
    serviceTypePrice: number;
    serviceDurationHours: number;

    vehicleRegistrationNumber: string;
    vehicleMake: string;
    vehicleModel: string;
    vehicleYear: number;

    customerId: number;
    customerFullName: string;
    customerPhoneNumber: string | null;
    customerEmail: string | null;

    mechanicId: number | null;
    mechanicName: string | null;
}

export interface CreateBookingPayload {
    vehicleRegistrationNumber: string;
    serviceTypeId: number;
    slotStart: string;
}

export interface UsedPartPayload {
    partId: number;
    quantity: number;
}

export interface FinishJobPayload {
    description: string;
    labourCost: number;
    usedParts: UsedPartPayload[];
}

export interface BookingSearchParameters {
    vehicleRegistrationNumber?: string;
    dateFrom?: string;
    dateTo?: string;
    status?: BookingStatus;
}
