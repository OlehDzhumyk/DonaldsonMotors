// src/pages/LoginPage.tsx
import React from 'react';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
import type {LoginCredentials, User} from '../types/auth';
import { useAppDispatch, useAppSelector } from '../hooks/reduxHooks';
import { loginUser } from '../app/authSlice';
import { useNavigate, Link } from 'react-router-dom';
import './LoginPage.css';
import {Roles} from "../utils/roles.ts";

const schema = yup.object().shape({
    email: yup.string().email('Must be a valid email').required('Email is required'),
    password: yup.string().required('Password is required'),
});

const LoginPage: React.FC = () => {
    const dispatch = useAppDispatch();
    const navigate = useNavigate();
    const { status, error } = useAppSelector((state) => state.auth); // error is now string | null

    const {
        register,
        handleSubmit,
        formState: { errors },
    } = useForm<LoginCredentials>({
        resolver: yupResolver(schema),
    });

    const onSubmit = (data: LoginCredentials) => {
        dispatch(loginUser(data))
            .unwrap()
            .then((loggedInUser: User) => { // loginUser thunk повертає об'єкт User
                // Redirect based on role
                switch (loggedInUser.role) {
                    case Roles.Manager:
                        navigate('/dashboard', { replace: true });
                        break;
                    case Roles.Customer:
                        navigate('/my-bookings', { replace: true });
                        break;
                    case Roles.Mechanic:
                        navigate('/my-jobs', { replace: true });
                        break;
                    // Add cases for StockController, AccountsClerk if they have specific pages
                    default:
                        navigate('/profile', { replace: true }); // Default for other roles or if no specific page
                }
            })
            .catch(() => {
                // Помилка вже обробляється в authSlice і відображається через стан 'error'
                console.error("Login attempt failed on page!");
            });
    };

    return (
        <div className="login-page-container">
            <div className="login-form-card">
                <h2>Welcome Back!</h2>
                <form onSubmit={handleSubmit(onSubmit)}>
                    <div className="form-group">
                        <label htmlFor="email">Email</label>
                        <input id="email" type="email" {...register('email')} />
                        {errors.email && <p className="error-message">{errors.email.message}</p>}
                    </div>

                    <div className="form-group">
                        <label htmlFor="password">Password</label>
                        <input id="password" type="password" {...register('password')} />
                        {errors.password && <p className="error-message">{errors.password.message}</p>}
                    </div>

                    {/* Simplified error display */}
                    {status === 'failed' && error && (
                        <div className="login-error">
                            {error} {/* error is now guaranteed to be a string if this block renders */}
                        </div>
                    )}

                    <button type="submit" className="submit-button" disabled={status === 'loading'}>
                        {status === 'loading' ? 'Logging in...' : 'Login'}
                    </button>

                    <p className="register-link">
                        Don't have an account? <Link to="/register">Register here</Link>
                    </p>
                </form>
            </div>
        </div>
    );
};

export default LoginPage;