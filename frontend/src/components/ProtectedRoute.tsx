import React from 'react';
import { Navigate, Outlet } from 'react-router-dom';
import { useAppSelector } from '../hooks/reduxHooks';
import type { Role } from '../utils/roles';

interface ProtectedRouteProps {
    allowedRoles: Role[];
}

/** Renders the child routes only for logged-in users with one of the allowed roles. */
const ProtectedRoute: React.FC<ProtectedRouteProps> = ({ allowedRoles }) => {
    const { user } = useAppSelector((state) => state.auth);

    if (!user) {
        return <Navigate to="/login" replace />;
    }
    if (!allowedRoles.includes(user.role)) {
        return <Navigate to="/unauthorized" replace />;
    }
    return <Outlet />;
};

export default ProtectedRoute;
