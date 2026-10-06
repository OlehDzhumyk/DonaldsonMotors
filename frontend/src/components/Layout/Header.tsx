// src/components/Layout/Header.tsx
import React, { useState } from 'react';
import { NavLink, useNavigate } from 'react-router-dom';
import { useAppSelector, useAppDispatch } from '../../hooks/reduxHooks';
import { logout } from '../../app/authSlice';
import { Roles } from '../../utils/roles'; // Make sure this file exists and is correct
import './Header.css';

const Header: React.FC = () => {
    const { user } = useAppSelector((state) => state.auth);
    const dispatch = useAppDispatch();
    const navigate = useNavigate();
    const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);

    const toggleMobileMenu = () => setIsMobileMenuOpen(!isMobileMenuOpen);
    const closeMobileMenu = () => setIsMobileMenuOpen(false);

    const handleLogout = () => {
        dispatch(logout());
        closeMobileMenu();
        navigate('/');
    };

    const renderNavLinks = () => {
        if (user) { // User is logged in
            return (
                <>
                    {/* Role-specific "dashboard" or primary link */}
                    {user.role === Roles.Manager && <NavLink to="/dashboard" onClick={closeMobileMenu}>Manager Dashboard</NavLink>}
                    {user.role === Roles.Customer && <NavLink to="/my-bookings" onClick={closeMobileMenu}>My Bookings</NavLink>}
                    {user.role === Roles.Mechanic && <NavLink to="/my-jobs" onClick={closeMobileMenu}>My Jobs</NavLink>}
                    {/* TODO: Add links for StockController & AccountsClerk dashboards if they exist */}
                    {/* Fallback for other logged-in roles if they don't have a specific dashboard */}
                    {![Roles.Manager, Roles.Customer, Roles.Mechanic].includes(user.role as any) && (
                        <NavLink to="/" onClick={closeMobileMenu}>Home</NavLink>
                    )}

                    {/* Additional role-specific operational links */}
                    {user.role === Roles.Manager && (
                        <>
                            <NavLink to="/manage/staff" onClick={closeMobileMenu}>Register Staff</NavLink>
                            <NavLink to="/manage/stock" onClick={closeMobileMenu}>Stock & Suppliers</NavLink>
                            {/* Add other manager-specific links like reports here */}
                        </>
                    )}
                    {(user.role === Roles.Manager || user.role === Roles.StockController) &&
                        user.role !== Roles.Manager && /* Avoid duplicate if manager already has it */
                        <NavLink to="/manage/stock" onClick={closeMobileMenu}>Stock & Suppliers</NavLink>
                    }
                    {/* TODO: Add links for AccountsClerk (e.g., /manage/customers, /reports/invoices) */}

                    {user.role === Roles.Customer && <NavLink to="/book" className="cta-button" onClick={closeMobileMenu}>Book Service</NavLink>}

                    {/* "My Profile" link ONLY for Customers */}
                    {user.role === Roles.Customer && <NavLink to="/profile" onClick={closeMobileMenu}>My Profile</NavLink>}

                    <button onClick={handleLogout} className="logout-button">Logout</button>
                </>
            );
        }

        // User is logged out (Guest)
        return (
            <>
                <NavLink to="/" end onClick={closeMobileMenu}>Home</NavLink>
                <NavLink to="/services" onClick={closeMobileMenu}>Services</NavLink>
                <NavLink to="/about" onClick={closeMobileMenu}>About Us</NavLink>
                <NavLink to="/login" onClick={closeMobileMenu}>Login</NavLink>
                <NavLink to="/register" onClick={closeMobileMenu}>Register</NavLink>
                <NavLink to="/book" className="cta-button" onClick={closeMobileMenu}>Book Now</NavLink>
            </>
        );
    };

    return (
        <header className="app-header">
            <div className="logo-container">
                <NavLink to="/">
                    <img src="/logo.png" alt="Donaldson Motors Logo" />
                </NavLink>
            </div>
            <div className="navigation">
                <nav>
                    {renderNavLinks()}
                </nav>
            </div>
            <button className="menu-icon" onClick={toggleMobileMenu}>
                &#9776;
            </button>
            {isMobileMenuOpen && (
                <div className="mobile-nav open">
                    {renderNavLinks()}
                </div>
            )}
        </header>
    );
};

export default Header;