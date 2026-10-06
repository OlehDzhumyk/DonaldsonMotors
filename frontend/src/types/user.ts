export interface Vehicle {
    registrationNumber: string;
    make: string;
    model: string;
    year: number;
    mileage: number;
}

export type VehiclePayload = Vehicle;

export interface UserProfile {
    id: number;
    fullName: string;
    email: string;
    address: string | null;
    phoneNumber: string | null;
    vehicles: Vehicle[];
}
