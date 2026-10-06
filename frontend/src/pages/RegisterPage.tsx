// src/pages/RegisterPage.tsx
import React from 'react';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
import type { RegisterPayload } from '../types/auth';
import { useAppDispatch, useAppSelector } from '../hooks/reduxHooks';
import { registerUser } from '../app/authSlice';
import { useNavigate, Link } from 'react-router-dom';
import './RegisterPage.css';

// Оновлена, коротша схема валідації
const schema = yup.object().shape({
    fullName: yup.string().required('Full name is required').min(3, 'Full name must be at least 3 characters'),
    email: yup.string().email('Must be a valid email').required('Email is required'),
    password: yup.string().required('Password is required').min(6, 'Password must be at least 6 characters'),
    confirmPassword: yup.string()
        .oneOf([yup.ref('password')], 'Passwords must match')
        .required('Confirm password is required'),
});

const RegisterPage: React.FC = () => {
    const dispatch = useAppDispatch();
    const navigate = useNavigate();
    const { status, error } = useAppSelector((state) => state.auth);

    const {
        register,
        handleSubmit,
        formState: { errors },
    } = useForm<RegisterPayload>({
        resolver: yupResolver(schema),
    });

    const onSubmit = (data: RegisterPayload) => {
        const { confirmPassword, ...payloadToSend } = data;

        dispatch(registerUser(payloadToSend)) // Надсилаємо скорочений payload
            .unwrap()
            .then(() => {
                navigate('/dashboard', { replace: true }); // Redirect on success
            })
            .catch(() => {

            });
    };

    return (
        <div className="register-page-container">
            <div className="register-form-card">
                <h2>Create Your Account</h2>
                <form onSubmit={handleSubmit(onSubmit)}>
                    <div className="form-group">
                        <label htmlFor="fullName">Full Name</label>
                        <input id="fullName" {...register('fullName')} />
                        {errors.fullName && <p className="error-message">{errors.fullName.message}</p>}
                    </div>

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

                    <div className="form-group">
                        <label htmlFor="confirmPassword">Confirm Password</label>
                        <input id="confirmPassword" type="password" {...register('confirmPassword')} />
                        {errors.confirmPassword && <p className="error-message">{errors.confirmPassword.message}</p>}
                    </div>


                    {status === 'failed' && error && (
                        <div className="register-error">
                            {error as string}
                        </div>
                    )}

                    <button type="submit" className="submit-button" disabled={status === 'loading'}>
                        {status === 'loading' ? 'Registering...' : 'Create Account'}
                    </button>

                    <p className="login-link">
                        Already have an account? <Link to="/login">Login here</Link>
                    </p>
                </form>
            </div>
        </div>
    );
};

export default RegisterPage;