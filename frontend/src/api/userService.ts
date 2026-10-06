import apiClient from './apiClient';
import type { UserProfile, Vehicle, VehiclePayload } from '../types/user';

export const getMyProfile = async (): Promise<UserProfile> => {
    // Endpoint for getting user profile
    // GET /api/Users/me
    const response = await apiClient.get<UserProfile>('/users/me');
    return response.data;
};

// --- VEHICLE API FUNCTIONS ---

/**
 * Adds a new vehicle for the currently authenticated user.
 * POST /api/users/me/vehicles
 */
export const addVehicle = async (vehicleData: VehiclePayload): Promise<Vehicle> => {
    // Endpoint path matches your controller: /users/me/vehicles
    // Your controller returns CreatedAtAction with the vehicle DTO, so <Vehicle> is correct.
    const response = await apiClient.post<Vehicle>('/users/me/vehicles', vehicleData);
    return response.data;
};

/**
 * Deletes a vehicle for the currently authenticated user.
 * DELETE /api/users/me/vehicles/{registrationNumber}
 */
export const deleteVehicle = async (registrationNumber: string): Promise<void> => {
    // Endpoint path matches your controller
    // Your controller returns NoContent(), so Promise<void> is correct.
    await apiClient.delete(`/users/me/vehicles/${registrationNumber}`);
};

/**
 * Updates an existing vehicle for the authenticated user.
 * PUT /api/users/me/vehicles/{registrationNumber}
 */
export const updateVehicle = async (
    currentRegistrationNumber: string, // Used in the URL to identify the vehicle
    vehicleData: VehiclePayload      // Contains all fields for the update
): Promise<void> => {

    // Ensure the registrationNumber in the payload is consistent if needed,
    // though the primary identifier is in the URL.
    // Your controller uses [FromBody] VehicleRequestDto, which will bind vehicleData.
    if (currentRegistrationNumber !== vehicleData.registrationNumber) {
        // This check is good practice on the frontend, even if the backend might also validate.
        // It ensures we're conceptually "updating" the same identified vehicle.
        console.warn("Registration number in payload differs from URL parameter during update. Using URL parameter.");
    }

    // Endpoint path and method matches your controller.
    // Your controller returns NoContent(), so we don't expect a Vehicle object back.
    await apiClient.put(`/users/me/vehicles/${currentRegistrationNumber}`, vehicleData);
};