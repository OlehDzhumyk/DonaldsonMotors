import React from 'react';
import { Link } from 'react-router-dom';
import { useAppSelector } from '../hooks/reduxHooks';
import { HOME_BY_ROLE } from '../utils/roles';

const UnauthorizedPage: React.FC = () => {
    const { user } = useAppSelector((state) => state.auth);

    return (
        <div className="container page page-narrow">
            <div className="card card-body stack" style={{ alignItems: 'flex-start' }}>
                <h1>You don't have access to this page</h1>
                <p className="muted">Your account doesn't have the role this page needs.</p>
                <Link to={user ? HOME_BY_ROLE[user.role] : '/'} className="btn btn-dark">Go to my start page</Link>
            </div>
        </div>
    );
};

export default UnauthorizedPage;
