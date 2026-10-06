import React from 'react';
import { render } from '@testing-library/react';
import { configureStore } from '@reduxjs/toolkit';
import { Provider } from 'react-redux';
import { MemoryRouter } from 'react-router-dom';
import authReducer from '../app/authSlice';
import type { User } from '../types/auth';

/** Renders UI with a real auth store (optionally logged in) and an in-memory router. */
export const renderWithStore = (ui: React.ReactElement, { user = null, path = '/' }: { user?: User | null; path?: string } = {}) => {
    const store = configureStore({
        reducer: { auth: authReducer },
        preloadedState: { auth: { user, status: 'idle' as const, error: null } },
    });
    return render(
        <Provider store={store}>
            <MemoryRouter initialEntries={[path]}>{ui}</MemoryRouter>
        </Provider>
    );
};

export const userWithRole = (role: User['role']): User => ({
    token: 'test-token',
    email: `${role.toLowerCase()}@example.com`,
    role,
    expiresAt: new Date(Date.now() + 60_000).toISOString(),
});
