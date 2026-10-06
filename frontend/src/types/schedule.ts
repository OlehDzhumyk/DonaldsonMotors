// src/types/schedule.ts

export interface MechanicAvailability {
    mechanicId: number;
    fullName: string;
    // Add any other relevant fields if your backend DTO has them,
    // e.g., isAvailable (though endpoint name implies they are all available), specializations
}