// src/components/ProtectedRoute.tsx
import React from 'react';
import { Navigate, Outlet } from 'react-router-dom';
import { useAppSelector } from '../hooks/reduxHooks';
import type {Role} from '../utils/roles';

interface ProtectedRouteProps {
    allowedRoles?: Role[];
}

const ProtectedRoute: React.FC<ProtectedRouteProps> = ({ allowedRoles }) => {
    const { user } = useAppSelector((state) => state.auth);

    // 1. Check if user is logged in
    if (!user) {
        // Redirect to login page if not authenticated
        return <Navigate to="/login" replace />;
    }

    // 2. Check if the route requires specific roles and if the user has one
    // If allowedRoles is provided and the user's role is not in the list...
    if (allowedRoles && allowedRoles.length > 0 && !allowedRoles.includes(user.role)) {
        // ...redirect to an 'Unauthorized' page
        return <Navigate to="/unauthorized" replace />;
    }

    // If checks pass, render the child component (e.g., DashboardPage)
    return <Outlet />;
};

export default ProtectedRoute;