// src/pages/DashboardPage.tsx
import React, { useState, useEffect } from 'react';
import apiClient from '../api/apiClient'; // Using our central client

const DashboardPage: React.FC = () => {
    const [profile, setProfile] = useState<any>(null); // Use a proper type later
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');

    useEffect(() => {
        // This is a protected endpoint. It will only work if the auth token is sent correctly.
        apiClient.get('/users/me')
            .then(response => {
                setProfile(response.data);
            })
            .catch(err => {
                setError('Failed to fetch user profile. Your session might have expired.');
                console.error(err);
            })
            .finally(() => {
                setLoading(false);
            });
    }, []);

    if (loading) {
        return <div>Loading dashboard...</div>;
    }
    if (error) {
        return <div style={{ color: 'red' }}>{error}</div>;
    }

    return (
        <div>
            <h1>Dashboard</h1>
            <p>Welcome! Your profile has been loaded successfully.</p>
            <pre>{JSON.stringify(profile, null, 2)}</pre>
        </div>
    );
};

export default DashboardPage;