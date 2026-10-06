import apiClient from './apiClient';
import type {MechanicAvailability} from "../types/schedule.ts";

/**
 * Fetches available booking slots for a given date range.
 * Corresponds to GET /api/Schedule/availability
 * @param startDate - The start date of the range (e.g., 'YYYY-MM-DD').
 * @param endDate - The end date of the range (e.g., 'YYYY-MM-DD').
 * @returns A promise that resolves to an array of available slot date-time strings in ISO format (UTC).
 */
export const getAvailability = async (startDate: string, endDate: string): Promise<string[]> => {
    const response = await apiClient.get<string[]>('/schedule/availability', {
        params: {
            startDate,
            endDate,
        },
    });
    return response.data; // API returns IEnumerable<DateTime>, which Axios might parse as string[]
};

export const getAvailableMechanicsForSlot = async (
    slotStart: string, // UTC ISO string
    serviceTypeId: number
): Promise<MechanicAvailability[]> => {
    // Corresponds to GET /api/Schedule/available-mechanics
    const response = await apiClient.get<MechanicAvailability[]>('/schedule/available-mechanics', {
        params: {
            slotStart, // Backend expects DateTime
            serviceTypeId,
        },
    });
    return response.data;
};