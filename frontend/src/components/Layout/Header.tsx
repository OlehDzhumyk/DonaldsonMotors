import React, { useState } from 'react';
import { Link, NavLink, useNavigate } from 'react-router-dom';
import { useAppSelector, useAppDispatch } from '../../hooks/reduxHooks';
import { logout } from '../../app/authSlice';
import { Roles, type Role } from '../../utils/roles';
import BrandMark from './BrandMark';
import './Header.css';

interface NavItem { to: string; label: string; end?: boolean }

const NAV_BY_ROLE: Record<Role, NavItem[]> = {
    [Roles.Customer]: [
        { to: '/my-bookings', label: 'My bookings' },
        { to: '/book', label: 'Book a service' },
        { to: '/profile', label: 'Profile & vehicles' },
    ],
    [Roles.Manager]: [
        { to: '/dashboard', label: 'Bookings' },
        { to: '/manage/stock', label: 'Stock & suppliers' },
        { to: '/manage/staff', label: 'Staff' },
    ],
    [Roles.Mechanic]: [{ to: '/my-jobs', label: 'My jobs' }],
    [Roles.StockController]: [{ to: '/manage/stock', label: 'Stock & suppliers' }],
    [Roles.AccountsClerk]: [{ to: '/dashboard', label: 'Bookings & payments' }],
};

const GUEST_NAV: NavItem[] = [
    { to: '/', label: 'Home', end: true },
    { to: '/services', label: 'Services' },
    { to: '/about', label: 'About' },
];

const ROLE_LABELS: Record<Role, string> = {
    Customer: 'Customer',
    Manager: 'Manager',
    Mechanic: 'Mechanic',
    StockController: 'Stock controller',
    AccountsClerk: 'Accounts clerk',
};

const Header: React.FC = () => {
    const { user } = useAppSelector((state) => state.auth);
    const dispatch = useAppDispatch();
    const navigate = useNavigate();
    const [menuOpen, setMenuOpen] = useState(false);

    const navItems = user ? NAV_BY_ROLE[user.role] : GUEST_NAV;

    const handleLogout = () => {
        dispatch(logout());
        setMenuOpen(false);
        void navigate('/');
    };

    return (
        <header className="site-header">
            <div className="container site-header-inner">
                <Link to="/" className="brand" onClick={() => { setMenuOpen(false); }}>
                    <BrandMark />
                    <span>Donaldson Motors</span>
                </Link>

                <nav className={`site-nav ${menuOpen ? 'open' : ''}`}>
                    {navItems.map(item => (
                        <NavLink key={item.to} to={item.to} end={item.end} onClick={() => { setMenuOpen(false); }}>
                            {item.label}
                        </NavLink>
                    ))}
                    <div className="site-nav-account">
                        {user ? (
                            <>
                                <span className="account-chip">
                                    <span className="account-role">{ROLE_LABELS[user.role]}</span>
                                    <span className="account-email">{user.email}</span>
                                </span>
                                <button onClick={handleLogout} className="btn btn-sm header-btn-ghost">Log out</button>
                            </>
                        ) : (
                            <>
                                <NavLink to="/login" onClick={() => { setMenuOpen(false); }}>Log in</NavLink>
                                <Link to="/register" className="btn btn-sm btn-primary" onClick={() => { setMenuOpen(false); }}>Create account</Link>
                            </>
                        )}
                    </div>
                </nav>

                <button className="menu-toggle" onClick={() => { setMenuOpen(!menuOpen); }} aria-label="Toggle menu">
                    &#9776;
                </button>
            </div>
        </header>
    );
};

export default Header;
