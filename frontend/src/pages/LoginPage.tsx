import React from 'react';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
import { useNavigate, Link } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '../hooks/reduxHooks';
import { loginUser } from '../app/authSlice';
import { HOME_BY_ROLE } from '../utils/roles';
import './AuthPage.css';

const schema = yup.object({
    email: yup.string().email('Enter a valid email').required('Email is required'),
    password: yup.string().required('Password is required'),
});

type LoginForm = yup.InferType<typeof schema>;

// Accounts created by the API when SeedDemoData is on (the default in docker-compose)
const DEMO_ACCOUNTS = [
    { role: 'Customer', email: 'customer@donaldson.com' },
    { role: 'Manager', email: 'manager@donaldson.com' },
    { role: 'Mechanic', email: 'mechanic@donaldson.com' },
    { role: 'Stock controller', email: 'stock@donaldson.com' },
    { role: 'Accounts clerk', email: 'accounts@donaldson.com' },
];
const DEMO_PASSWORD = 'Password123!';

const LoginPage: React.FC = () => {
    const dispatch = useAppDispatch();
    const navigate = useNavigate();
    const { status, error } = useAppSelector((state) => state.auth);
    const { register, handleSubmit, setValue, formState: { errors } } = useForm<LoginForm>({
        resolver: yupResolver(schema),
    });

    const onSubmit = async (data: LoginForm) => {
        const result = await dispatch(loginUser(data));
        if (loginUser.fulfilled.match(result)) {
            void navigate(HOME_BY_ROLE[result.payload.role], { replace: true });
        }
    };

    const fillDemo = (email: string) => {
        setValue('email', email);
        setValue('password', DEMO_PASSWORD);
    };

    return (
        <div className="auth-page">
            <div className="card card-body auth-card">
                <h1>Log in</h1>
                <p className="subtitle">Welcome back to Donaldson Motors.</p>
                <form onSubmit={(e) => void handleSubmit(onSubmit)(e)} noValidate>
                    <div className="field">
                        <label htmlFor="email">Email</label>
                        <input id="email" type="email" autoComplete="email" {...register('email')} />
                        {errors.email && <p className="field-error">{errors.email.message}</p>}
                    </div>
                    <div className="field">
                        <label htmlFor="password">Password</label>
                        <input id="password" type="password" autoComplete="current-password" {...register('password')} />
                        {errors.password && <p className="field-error">{errors.password.message}</p>}
                    </div>

                    {status === 'failed' && error && <p className="alert alert-error" style={{ marginBottom: 16 }}>{error}</p>}

                    <button type="submit" className="btn btn-primary btn-block btn-lg" disabled={status === 'loading'}>
                        {status === 'loading' ? 'Logging in…' : 'Log in'}
                    </button>
                </form>
                <p className="auth-switch">No account yet? <Link to="/register">Create one</Link></p>
            </div>

            <div className="card demo-card">
                <div className="card-header">
                    <div>
                        <h2>Demo accounts</h2>
                        <p className="muted small">Click one to fill the form. Password: {DEMO_PASSWORD}</p>
                    </div>
                </div>
                <ul>
                    {DEMO_ACCOUNTS.map(account => (
                        <li key={account.email}>
                            <button type="button" onClick={() => { fillDemo(account.email); }}>
                                <span>
                                    <span className="demo-role">{account.role}</span><br />
                                    <span className="demo-email">{account.email}</span>
                                </span>
                                <span className="muted">→</span>
                            </button>
                        </li>
                    ))}
                </ul>
            </div>
        </div>
    );
};

export default LoginPage;
