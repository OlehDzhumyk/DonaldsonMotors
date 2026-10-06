import { describe, expect, it } from 'vitest';
import { screen } from '@testing-library/react';
import { Route, Routes } from 'react-router-dom';
import ProtectedRoute from './ProtectedRoute';
import { Roles } from '../utils/roles';
import { renderWithStore, userWithRole } from '../test/renderWithStore';

const routes = (
    <Routes>
        <Route element={<ProtectedRoute allowedRoles={[Roles.Manager]} />}>
            <Route path="/dashboard" element={<p>Dashboard</p>} />
        </Route>
        <Route path="/login" element={<p>Login page</p>} />
        <Route path="/unauthorized" element={<p>No access</p>} />
    </Routes>
);

describe('ProtectedRoute', () => {
    it('sends visitors who are not logged in to the login page', () => {
        renderWithStore(routes, { path: '/dashboard' });
        expect(screen.getByText('Login page')).toBeInTheDocument();
    });

    it('blocks users without the right role', () => {
        renderWithStore(routes, { path: '/dashboard', user: userWithRole('Customer') });
        expect(screen.getByText('No access')).toBeInTheDocument();
    });

    it('shows the page to users with the right role', () => {
        renderWithStore(routes, { path: '/dashboard', user: userWithRole('Manager') });
        expect(screen.getByText('Dashboard')).toBeInTheDocument();
    });
});
