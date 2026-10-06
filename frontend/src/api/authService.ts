// src/api/authService.ts
import apiClient from './apiClient';
import type {LoginCredentials, RegisterStaffPayload, User} from '../types/auth';

const API_AUTH_URL = '/auth/';

const login = async (credentials: LoginCredentials): Promise<User> => {
    const response = await apiClient.post<User>(API_AUTH_URL + 'login', credentials);
    if (response.data.token) {
        localStorage.setItem('user', JSON.stringify(response.data));
    }
    return response.data;
};

// Define the type for the payload that registerCustomer actually sends to the API
interface RegisterCustomerPayloadAPI {
    fullName: string;
    email: string;
    password: string;
    role: string; // Role is now expected by the API according to your feedback
}

// Updated function to register a new customer
const registerCustomer = async (payload: RegisterCustomerPayloadAPI): Promise<User> => {
    const response = await apiClient.post<User>(API_AUTH_URL + 'register', payload);
    if (response.data.token) {
        localStorage.setItem('user', JSON.stringify(response.data));
    }
    return response.data;
};

const logout = () => {
    localStorage.removeItem('user');
};


export const registerStaff = async (payload: Omit<RegisterStaffPayload, 'confirmPassword'>): Promise<User> => {
    // The API is expected to return the created user object (with token, etc.)
    // If it doesn't return a token or you don't want to use it, adjust the Promise type.
    const response = await apiClient.post<User>(API_AUTH_URL + 'register/staff', payload);
    return response.data;
};

const authService = {
    login,
    registerCustomer,
    registerStaff,
    logout,
};

export default authService;