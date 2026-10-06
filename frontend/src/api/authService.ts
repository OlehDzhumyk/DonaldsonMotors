import apiClient from './apiClient';
import type { LoginCredentials, RegisterPayload, RegisterStaffPayload, User } from '../types/auth';

const STORAGE_KEY = 'user';

const saveUser = (user: User) => { localStorage.setItem(STORAGE_KEY, JSON.stringify(user)); };

/** The saved session, or null when there is none or its token has expired. */
export const loadSavedUser = (): User | null => {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) return null;
    try {
        const user = JSON.parse(raw) as User;
        if (new Date(user.expiresAt).getTime() > Date.now()) return user;
    } catch {
        // Corrupt value: fall through and clear it
    }
    localStorage.removeItem(STORAGE_KEY);
    return null;
};

export const login = async (credentials: LoginCredentials): Promise<User> => {
    const { data } = await apiClient.post<User>('/auth/login', credentials);
    saveUser(data);
    return data;
};

/** Public sign-up; the API always creates a Customer. */
export const registerCustomer = async (payload: RegisterPayload): Promise<User> => {
    const { data } = await apiClient.post<User>('/auth/register', payload);
    saveUser(data);
    return data;
};

/** Manager-only: creates a staff account without logging in as it. */
export const registerStaff = async (payload: RegisterStaffPayload): Promise<void> => {
    await apiClient.post('/auth/register/staff', payload);
};

export const clearSavedUser = () => { localStorage.removeItem(STORAGE_KEY); };
