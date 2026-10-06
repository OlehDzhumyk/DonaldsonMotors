import apiClient from './apiClient';
import type {Supplier, CreateSupplierPayload, UpdateSupplierPayload} from '../types/inventory';

const BASE_URL = '/suppliers'; // API base path for suppliers

/**
 * Fetches all suppliers.
 * GET /api/Suppliers
 */
export const getAllSuppliers = async (): Promise<Supplier[]> => {
    const response = await apiClient.get<Supplier[]>(BASE_URL);
    return response.data;
};

/**
 * Fetches a single supplier by its ID.
 * GET /api/Suppliers/{id}
 */
export const getSupplierById = async (supplierId: number): Promise<Supplier> => {
    const response = await apiClient.get<Supplier>(`${BASE_URL}/${supplierId}`);
    return response.data;
};

/**
 * Creates a new supplier.
 * POST /api/Suppliers
 */
export const createSupplier = async (payload: CreateSupplierPayload): Promise<Supplier> => {
    const response = await apiClient.post<Supplier>(BASE_URL, payload);
    return response.data; // API returns the created supplier object
};

/**
 * Updates an existing supplier.
 * PUT /api/Suppliers/{id}
 */
export const updateSupplier = async (supplierId: number, payload: UpdateSupplierPayload): Promise<Supplier> => {
    // Assuming API returns the updated supplier. If it returns 204 No Content, change Promise<void>.
    const response = await apiClient.put<Supplier>(`${BASE_URL}/${supplierId}`, payload);
    return response.data;
};

/**
 * Deletes a supplier by its ID.
 * DELETE /api/Suppliers/{id}
 */
export const deleteSupplier = async (supplierId: number): Promise<void> => {
    await apiClient.delete(`${BASE_URL}/${supplierId}`);
};