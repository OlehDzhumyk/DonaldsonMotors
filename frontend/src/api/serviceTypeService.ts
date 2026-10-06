// src/api/serviceTypeService.ts
import apiClient from './apiClient';
import type {ServiceType} from '../types/serviceType'; // Import our new ServiceType
// Import our new ServiceType

/**
 * Fetches all available service types.
 * Corresponds to GET /api/ServiceTypes
 * @returns A promise that resolves to an array of ServiceType objects.
 */
export const getAllServiceTypes = async (): Promise<ServiceType[]> => {
    // This endpoint can be accessed by authenticated customers (and other roles).
    // The apiClient will automatically send the auth token if available.
    const response = await apiClient.get<ServiceType[]>('/servicetypes'); // Endpoint path is case-insensitive usually
    return response.data;
};

// Later, if needed for an admin panel, we can add:
// addServiceType, updateServiceType, deleteServiceType functions here.