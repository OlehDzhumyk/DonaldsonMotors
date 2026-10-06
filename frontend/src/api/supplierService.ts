import apiClient from './apiClient';
import type { Supplier, SupplierPayload } from '../types/inventory';

export const getSuppliers = async (): Promise<Supplier[]> => {
    const { data } = await apiClient.get<Supplier[]>('/suppliers');
    return data;
};

export const createSupplier = async (payload: SupplierPayload): Promise<Supplier> => {
    const { data } = await apiClient.post<Supplier>('/suppliers', payload);
    return data;
};

export const updateSupplier = async (id: number, payload: SupplierPayload): Promise<Supplier> => {
    const { data } = await apiClient.put<Supplier>(`/suppliers/${String(id)}`, payload);
    return data;
};

export const deleteSupplier = async (id: number): Promise<void> => {
    await apiClient.delete(`/suppliers/${String(id)}`);
};
