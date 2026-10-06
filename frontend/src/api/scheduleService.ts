import apiClient from './apiClient';
import type { MechanicAvailability } from '../types/schedule';

/** Free slot start times (UTC ISO strings) between two dates (YYYY-MM-DD). Public endpoint. */
export const getAvailability = async (startDate: string, endDate: string): Promise<string[]> => {
    const { data } = await apiClient.get<string[]>('/schedule/availability', { params: { startDate, endDate } });
    return data;
};

/** Every mechanic, with whether they are free for this booking's slot. */
export const getMechanicsForSlot = async (slotStart: string, serviceTypeId: number): Promise<MechanicAvailability[]> => {
    const { data } = await apiClient.get<MechanicAvailability[]>('/schedule/available-mechanics', {
        params: { slotStart, serviceTypeId },
    });
    return data;
};
