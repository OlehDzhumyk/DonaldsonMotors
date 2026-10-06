import axios from 'axios';

const apiClient = axios.create({
    // Same origin: nginx (Docker) or the Vite dev server proxies /api to the backend
    baseURL: '/api',
    headers: {
        'Content-Type': 'application/json',
    },
});

interface ApiClientHooks {
    getToken: () => string | undefined;
    /** Called when the API rejects the saved token (expired or revoked). */
    onUnauthorized: () => void;
}

/** Wires the client to the auth state without importing the store (avoids a circular import). */
export const setupApiClient = ({ getToken, onUnauthorized }: ApiClientHooks) => {
    apiClient.interceptors.request.use((config) => {
        const token = getToken();
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    });

    apiClient.interceptors.response.use(undefined, (error: unknown) => {
        if (axios.isAxiosError(error) && error.response?.status === 401 && getToken()) {
            onUnauthorized();
        }
        return Promise.reject(error instanceof Error ? error : new Error(String(error)));
    });
};

export default apiClient;
