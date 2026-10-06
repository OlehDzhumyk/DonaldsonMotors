
export interface Vehicle {
    registrationNumber: string;
    make: string;
    model: string;
    year: number;
    mileage: number;
}

export interface VehiclePayload {
    registrationNumber: string; // This will be the same for "update"
    make: string;
    model: string;
    year: number;
    mileage: number;
}

export interface UserProfile {
    id: number;
    fullName: string;
    email: string;
    address: string | null;
    phoneNumber: string | null;
    vehicles: Vehicle[];
}