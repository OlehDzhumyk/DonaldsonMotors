import apiClient from './apiClient';
import type { UserProfile, Vehicle, VehiclePayload } from '../types/user';

export const getMyProfile = async (): Promise<UserProfile> => {
    const { data } = await apiClient.get<UserProfile>('/users/me');
    return data;
};

export const addVehicle = async (vehicle: VehiclePayload): Promise<Vehicle> => {
    const { data } = await apiClient.post<Vehicle>('/users/me/vehicles', vehicle);
    return data;
};

/** The registration number is the vehicle's key, so it identifies the vehicle in the URL and cannot change. */
export const updateVehicle = async (registrationNumber: string, vehicle: VehiclePayload): Promise<void> => {
    await apiClient.put(`/users/me/vehicles/${encodeURIComponent(registrationNumber)}`, vehicle);
};

export const deleteVehicle = async (registrationNumber: string): Promise<void> => {
    await apiClient.delete(`/users/me/vehicles/${encodeURIComponent(registrationNumber)}`);
};
