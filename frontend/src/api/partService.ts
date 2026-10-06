import apiClient from './apiClient';
import type { Part, CreatePartPayload, UpdatePartPayload, UpdateStockPayload } from '../types/inventory';

export const getParts = async (searchTerm?: string): Promise<Part[]> => {
    const { data } = await apiClient.get<Part[]>('/parts', { params: { searchTerm } });
    return data;
};

export const createPart = async (payload: CreatePartPayload): Promise<Part> => {
    const { data } = await apiClient.post<Part>('/parts', payload);
    return data;
};

export const updatePart = async (id: number, payload: UpdatePartPayload): Promise<Part> => {
    const { data } = await apiClient.put<Part>(`/parts/${String(id)}`, payload);
    return data;
};

export const updatePartStock = async (id: number, payload: UpdateStockPayload): Promise<Part> => {
    const { data } = await apiClient.patch<Part>(`/parts/${String(id)}/stock`, payload);
    return data;
};

export const deletePart = async (id: number): Promise<void> => {
    await apiClient.delete(`/parts/${String(id)}`);
};
