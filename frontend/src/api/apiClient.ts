import axios from 'axios';
import type { EnhancedStore } from '@reduxjs/toolkit';

const apiClient = axios.create({
    baseURL: 'http://localhost:5000/api',
    headers: {
        'Content-Type': 'application/json',
    },
});

export const setupStoreForApiClient = (store: EnhancedStore) => {
    apiClient.interceptors.request.use(
        (config) => {
            const user = store.getState().auth.user;
            if (user && user.token) {
                config.headers.Authorization = `Bearer ${user.token}`;
            }
            return config;
        },
        (error) => {
            return Promise.reject(error);
        }
    );
};

export default apiClient;