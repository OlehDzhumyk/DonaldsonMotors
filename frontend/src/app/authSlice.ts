import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import * as authService from '../api/authService';
import type { AuthState, LoginCredentials, RegisterPayload, User } from '../types/auth';
import { getErrorMessage } from '../utils/format';

const initialState: AuthState = {
    user: authService.loadSavedUser(),
    status: 'idle',
    error: null,
};

export const loginUser = createAsyncThunk<User, LoginCredentials, { rejectValue: string }>(
    'auth/login',
    async (credentials, { rejectWithValue }) => {
        try {
            return await authService.login(credentials);
        } catch (err) {
            return rejectWithValue(getErrorMessage(err, 'Login failed.'));
        }
    }
);

export const registerUser = createAsyncThunk<User, RegisterPayload, { rejectValue: string }>(
    'auth/register',
    async (payload, { rejectWithValue }) => {
        try {
            return await authService.registerCustomer(payload);
        } catch (err) {
            return rejectWithValue(getErrorMessage(err, 'Registration failed.'));
        }
    }
);

const authSlice = createSlice({
    name: 'auth',
    initialState,
    reducers: {
        logout: (state) => {
            authService.clearSavedUser();
            state.user = null;
            state.status = 'idle';
            state.error = null;
        },
    },
    extraReducers: (builder) => {
        for (const thunk of [loginUser, registerUser]) {
            builder
                .addCase(thunk.pending, (state) => {
                    state.status = 'loading';
                    state.error = null;
                })
                .addCase(thunk.fulfilled, (state, action) => {
                    state.status = 'succeeded';
                    state.user = action.payload;
                })
                .addCase(thunk.rejected, (state, action) => {
                    state.status = 'failed';
                    state.error = action.payload ?? 'Something went wrong.';
                    state.user = null;
                });
        }
    },
});

export const { logout } = authSlice.actions;
export default authSlice.reducer;
