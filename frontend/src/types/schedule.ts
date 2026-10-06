/** Mirrors MechanicAvailabilityDto in the API. */
export interface MechanicAvailability {
    mechanicId: number;
    mechanicName: string;
    isAvailable: boolean;
    reasonIfNotAvailable: string | null;
}
