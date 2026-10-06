// src/types/booking.ts

export interface CreateBookingPayload {
    vehicleRegistrationNumber: string;
    serviceTypeId: number;
    slotStart: string;
}

export interface AssignMechanicPayload {
    bookingId: number;
    mechanicId: number;
}

export interface PartUsed {
    partName: string;
    quantity: number;
    pricePerUnit: number;
}

// --- UPDATED Booking Interface ---
export interface Booking {
    id: number;
    slotStart: string;
    status: string;

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

    jobStartTime?: string | null;
    jobEndTime?: string | null;
    notes?: string | null;
    partsUsed?: PartUsed[];
    totalCost?: number;
}


export interface AdminCancelBookingPayload {
    reason: string;
}

export interface MarkAsPaidPayload {
    paymentNotes?: string;
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
    customerId?: number;
    vehicleRegistrationNumber?: string;
    mechanicId?: number;
    dateFrom?: string;   // ISO date string, e.g., "YYYY-MM-DD"
    dateTo?: string;     // ISO date string
    status?: string;     // This will be one of your BookingStatus enum string values
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

