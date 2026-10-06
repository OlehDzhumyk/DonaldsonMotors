import apiClient from './apiClient';
import type { ServiceType } from '../types/serviceType';

export const getAllServiceTypes = async (): Promise<ServiceType[]> => {
    const { data } = await apiClient.get<ServiceType[]>('/servicetypes');
    return data;
};
