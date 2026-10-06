// src/pages/UnauthorizedPage.tsx
import React from 'react';
import { Link } from 'react-router-dom';

const UnauthorizedPage: React.FC = () => {
    return (
        <div style={{ textAlign: 'center', padding: '50px' }}>
            <h1>403 - Access Denied</h1>
            <p>You do not have permission to view this page.</p>
            <Link to="/dashboard">Back to Dashboard</Link>
        </div>
    );
};

export default UnauthorizedPage;