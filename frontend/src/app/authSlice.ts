import { createSlice, createAsyncThunk, type PayloadAction } from '@reduxjs/toolkit';
import authService from '../api/authService';
import type {AuthState, LoginCredentials, RegisterPayload, RegisterStaffPayload, User} from '../types/auth';
import {Roles} from "../utils/roles.ts";

const storedUser = localStorage.getItem('user');
const user: User | null = storedUser ? JSON.parse(storedUser) : null;

const initialState: AuthState = {
    user: user,
    status: 'idle',
    error: null,
};

export const loginUser = createAsyncThunk<User, LoginCredentials, { rejectValue: string }>(
    'auth/login',
    async (credentials, { rejectWithValue }) => {
        try {
            const data = await authService.login(credentials);
            return data;
        } catch (error: any) {
            const message =
                error.response?.data?.message ||
                error.response?.data?.title ||
                (error.response?.data?.errors ? Object.values(error.response.data.errors).flat().join(' ') : null) ||
                error.message ||
                error.toString();
            return rejectWithValue(message);
        }
    }
);

export const registerUser = createAsyncThunk<User, Omit<RegisterPayload, 'confirmPassword'>, { rejectValue: string }>(
    'auth/register',
    async (payloadFromForm, { rejectWithValue }) => {

        const payloadToSendToService = {
            ...payloadFromForm,
            role:   Roles.Customer
        };

        try {
            const data = await authService.registerCustomer(payloadToSendToService);
            return data;
        } catch (error: any) {
            if (error.response?.status === 409) {
                return rejectWithValue(error.response.data.message || 'User with this email already exists.');
            }
            if (error.response?.data?.errors) {
                const validationErrors = Object.values(error.response.data.errors).flat().join(' ');
                return rejectWithValue(validationErrors);
            }
            const message =
                error.response?.data?.message ||
                error.response?.data?.title ||
                error.message ||
                error.toString();
            return rejectWithValue(message);
        }
    }
);


export const registerStaffMember = createAsyncThunk<User, Omit<RegisterStaffPayload, 'confirmPassword'>, { rejectValue: string }>(
    'auth/registerStaff',
    async (payload, { rejectWithValue }) => {
        try {
            // This call does not log in the new staff member for the manager.
            // It just creates the account. The response (User object) might be useful for display.
            const newStaff = await authService.registerStaff(payload);
            return newStaff; // Or void if you don't need the created staff details immediately
        } catch (error: any) {
            // Similar error handling as registerUser
            if (error.response?.status === 409) {
                return rejectWithValue(error.response.data.message || 'User with this email already exists.');
            }
            if (error.response?.data?.errors) {
                const validationErrors = Object.values(error.response.data.errors).flat().join(' ');
                return rejectWithValue(validationErrors);
            }
            const message =
                error.response?.data?.message || error.response?.data?.title ||
                error.message || error.toString();
            return rejectWithValue(message);
        }
    }
);


const authSlice = createSlice({
    name: 'auth',
    initialState,
    reducers: {
        logout: (state) => {
            authService.logout();
            state.user = null;
            state.status = 'idle';
            state.error = null;
        },
    },
    extraReducers: (builder) => {
        builder
            // Login cases
            .addCase(loginUser.pending, (state) => {
                state.status = 'loading';
                state.error = null;
            })
            .addCase(loginUser.fulfilled, (state, action: PayloadAction<User>) => {
                state.status = 'succeeded';
                state.user = action.payload;
            })
            .addCase(loginUser.rejected, (state, action) => {
                state.status = 'failed';
                // Ensure action.payload is treated as string or set to null
                state.error = typeof action.payload === 'string' ? action.payload : null;
                state.user = null;
            })
            // Register cases
            .addCase(registerUser.pending, (state) => {
                state.status = 'loading';
                state.error = null;
            })
            .addCase(registerUser.fulfilled, (state, action: PayloadAction<User>) => {
                state.status = 'succeeded';
                state.user = action.payload;
            })
            .addCase(registerUser.rejected, (state, action) => {
                state.status = 'failed';
                // Ensure action.payload is treated as string or set to null
                state.error = typeof action.payload === 'string' ? action.payload : null;
                state.user = null;
            }).addCase(registerStaffMember.pending, (state) => {
            state.status = 'loading'; // Or a specific status like 'registeringStaff'
            state.error = null;
        })
            .addCase(registerStaffMember.fulfilled, (state, action: PayloadAction<User>) => {
                state.status = 'succeeded'; // Or 'staffRegistered'
                // We don't set state.user here because the manager isn't logging in as the new staff.
                // We might want to add a success message to the state for the UI.
                console.log('New staff registered:', action.payload);
            })
            .addCase(registerStaffMember.rejected, (state, action) => {
                state.status = 'failed'; // Or 'staffRegistrationFailed'
                state.error = typeof action.payload === 'string' ? action.payload : null;
            });
    },
});





export const { logout } = authSlice.actions;
export default authSlice.reducer;