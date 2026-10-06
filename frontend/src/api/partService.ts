import apiClient from './apiClient';
import type {Part, CreatePartPayload, UpdatePartPayload, UpdateStockPayload} from '../types/inventory';

const BASE_URL = '/parts'; // API base path for parts

/**
 * Fetches all parts, optionally filtered by searchTerm or supplierId.
 * GET /api/Parts
 */
export const getAllParts = async (searchTerm?: string, supplierId?: number): Promise<Part[]> => {
    const response = await apiClient.get<Part[]>(BASE_URL, {
        params: { searchTerm, supplierId }
    });
    return response.data;
};

/**
 * Fetches a single part by its ID.
 * GET /api/Parts/{id}
 */
export const getPartById = async (partId: number): Promise<Part> => {
    const response = await apiClient.get<Part>(`${BASE_URL}/${partId}`);
    return response.data;
};

/**
 * Creates a new part.
 * POST /api/Parts (assuming this endpoint exists)
 */
export const createPart = async (payload: CreatePartPayload): Promise<Part> => {
    // Assuming API returns the created part object.
    const response = await apiClient.post<Part>(BASE_URL, payload);
    return response.data;
};

/**
 * Updates an existing part.
 * PUT /api/Parts/{id}
 */
export const updatePart = async (partId: number, payload: UpdatePartPayload): Promise<Part> => {
    // Assuming API returns the updated part. If it returns 204 No Content, change Promise<void>.
    const response = await apiClient.put<Part>(`${BASE_URL}/${partId}`, payload);
    return response.data;
};

/**
 * Updates the stock level of a specific part.
 * PATCH /api/Parts/{id}/stock
 */
export const updatePartStock = async (partId: number, payload: UpdateStockPayload): Promise<Part> => {
    // Assuming API returns the updated part with new stock level.
    const response = await apiClient.patch<Part>(`${BASE_URL}/${partId}/stock`, payload);
    return response.data;
};

/**
 * Deletes a part by its ID.
 * DELETE /api/Parts/{id}
 */
export const deletePart = async (partId: number): Promise<void> => {
    await apiClient.delete(`${BASE_URL}/${partId}`);
};

// searchParts can reuse getAllParts as it supports searchTerm
export const searchParts = getAllParts;