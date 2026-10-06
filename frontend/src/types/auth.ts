// src/types/auth.ts
export interface LoginCredentials {
    email: string;
    password: string;
}

export interface User {
    token: string;
    email: string;
    role: 'Manager' | 'Mechanic' | 'StockController' | 'AccountsClerk' | 'Customer';
    expiresAt?: string;
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
    confirmPassword: string;
}

export interface RegisterStaffPayload {
    fullName: string;
    email: string;
    password: string;
    confirmPassword: string;
    role: 'Mechanic' | 'StockController' | 'AccountsClerk';
}