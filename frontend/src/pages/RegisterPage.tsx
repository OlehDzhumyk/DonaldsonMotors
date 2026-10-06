import React from 'react';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
import { useNavigate, Link } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '../hooks/reduxHooks';
import { registerUser } from '../app/authSlice';
import './AuthPage.css';

const schema = yup.object({
    fullName: yup.string().required('Full name is required').min(3, 'At least 3 characters'),
    email: yup.string().email('Enter a valid email').required('Email is required'),
    password: yup.string().required('Password is required').min(6, 'At least 6 characters'),
    confirmPassword: yup.string()
        .oneOf([yup.ref('password')], 'Passwords must match')
        .required('Please repeat the password'),
});

type RegisterForm = yup.InferType<typeof schema>;

const RegisterPage: React.FC = () => {
    const dispatch = useAppDispatch();
    const navigate = useNavigate();
    const { status, error } = useAppSelector((state) => state.auth);
    const { register, handleSubmit, formState: { errors } } = useForm<RegisterForm>({
        resolver: yupResolver(schema),
    });

    const onSubmit = async ({ fullName, email, password }: RegisterForm) => {
        const result = await dispatch(registerUser({ fullName, email, password }));
        if (registerUser.fulfilled.match(result)) {
            // New customers need a vehicle before they can book
            void navigate('/profile', { replace: true });
        }
    };

    return (
        <div className="auth-page">
            <div className="card card-body auth-card">
                <h1>Create an account</h1>
                <p className="subtitle">Book services and follow your car's progress online.</p>
                <form onSubmit={(e) => void handleSubmit(onSubmit)(e)} noValidate>
                    <div className="field">
                        <label htmlFor="fullName">Full name</label>
                        <input id="fullName" autoComplete="name" {...register('fullName')} />
                        {errors.fullName && <p className="field-error">{errors.fullName.message}</p>}
                    </div>
                    <div className="field">
                        <label htmlFor="email">Email</label>
                        <input id="email" type="email" autoComplete="email" {...register('email')} />
                        {errors.email && <p className="field-error">{errors.email.message}</p>}
                    </div>
                    <div className="field">
                        <label htmlFor="password">Password</label>
                        <input id="password" type="password" autoComplete="new-password" {...register('password')} />
                        {errors.password && <p className="field-error">{errors.password.message}</p>}
                    </div>
                    <div className="field">
                        <label htmlFor="confirmPassword">Repeat password</label>
                        <input id="confirmPassword" type="password" autoComplete="new-password" {...register('confirmPassword')} />
                        {errors.confirmPassword && <p className="field-error">{errors.confirmPassword.message}</p>}
                    </div>

                    {status === 'failed' && error && <p className="alert alert-error" style={{ marginBottom: 16 }}>{error}</p>}

                    <button type="submit" className="btn btn-primary btn-block btn-lg" disabled={status === 'loading'}>
                        {status === 'loading' ? 'Creating account…' : 'Create account'}
                    </button>
                </form>
                <p className="auth-switch">Already have an account? <Link to="/login">Log in</Link></p>
            </div>
        </div>
    );
};

export default RegisterPage;
