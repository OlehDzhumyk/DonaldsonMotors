import type { Role } from '../utils/roles';

export interface LoginCredentials {
    email: string;
    password: string;
}

/** The logged-in user as returned by /api/auth/login and /api/auth/register. */
export interface User {
    token: string;
    email: string;
    role: Role;
    expiresAt: string;
}

export interface AuthState {
    user: User | null;
    status: 'idle' | 'loading' | 'succeeded' | 'failed';
    error: string | null;
}

export interface RegisterPayload {
    fullName: string;
    email: string;
    password: string;
}

export type StaffRole = 'Mechanic' | 'StockController' | 'AccountsClerk';

export interface RegisterStaffPayload extends RegisterPayload {
    role: StaffRole;
}
