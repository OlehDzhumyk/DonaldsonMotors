import React, { useState } from 'react';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
import { registerStaff } from '../api/authService';
import type { StaffRole } from '../types/auth';
import { getErrorMessage } from '../utils/format';

const STAFF_ROLES: { value: StaffRole; label: string; description: string }[] = [
    { value: 'Mechanic', label: 'Mechanic', description: 'Sees assigned jobs, starts and finishes them.' },
    { value: 'StockController', label: 'Stock controller', description: 'Manages parts, stock levels and suppliers.' },
    { value: 'AccountsClerk', label: 'Accounts clerk', description: 'Marks completed jobs as paid.' },
];

const schema = yup.object({
    fullName: yup.string().required('Full name is required').min(3, 'At least 3 characters'),
    email: yup.string().email('Enter a valid email').required('Email is required'),
    password: yup.string().required('Password is required').min(6, 'At least 6 characters'),
    role: yup.mixed<StaffRole>()
        .oneOf(STAFF_ROLES.map(r => r.value), 'Choose a role')
        .required('Choose a role'),
});

type StaffForm = yup.InferType<typeof schema>;

const RegisterStaffPage: React.FC = () => {
    const [error, setError] = useState<string | null>(null);
    const [success, setSuccess] = useState<string | null>(null);
    const { register, handleSubmit, reset, formState: { errors, isSubmitting } } = useForm<StaffForm>({
        resolver: yupResolver(schema),
        defaultValues: { role: 'Mechanic' },
    });

    const onSubmit = async (data: StaffForm) => {
        setError(null);
        setSuccess(null);
        try {
            await registerStaff(data);
            setSuccess(`${data.fullName} can now log in as ${data.email}.`);
            reset();
        } catch (err) {
            setError(getErrorMessage(err, 'Could not create the account.'));
        }
    };

    return (
        <div className="container page page-narrow">
            <div className="page-header">
                <div>
                    <h1>Add a staff member</h1>
                    <p className="subtitle">Create a login for a new mechanic, stock controller or accounts clerk.</p>
                </div>
            </div>

            <form className="card" onSubmit={(e) => void handleSubmit(onSubmit)(e)} noValidate>
                <div className="card-body">
                    <div className="form-grid">
                        <div className="field">
                            <label htmlFor="fullName">Full name</label>
                            <input id="fullName" {...register('fullName')} />
                            {errors.fullName && <p className="field-error">{errors.fullName.message}</p>}
                        </div>
                        <div className="field">
                            <label htmlFor="email">Email</label>
                            <input id="email" type="email" {...register('email')} />
                            {errors.email && <p className="field-error">{errors.email.message}</p>}
                        </div>
                        <div className="field span-2">
                            <label htmlFor="password">Temporary password</label>
                            <input id="password" type="password" autoComplete="new-password" {...register('password')} />
                            {errors.password && <p className="field-error">{errors.password.message}</p>}
                        </div>
                    </div>

                    <fieldset className="role-options">
                        <legend>Role</legend>
                        {STAFF_ROLES.map(role => (
                            <label key={role.value} className="role-option">
                                <input type="radio" value={role.value} {...register('role')} />
                                <span>
                                    <strong>{role.label}</strong>
                                    <span className="muted small">{role.description}</span>
                                </span>
                            </label>
                        ))}
                    </fieldset>
                    {errors.role && <p className="field-error">{errors.role.message}</p>}

                    {error && <p className="alert alert-error" style={{ marginTop: 16 }}>{error}</p>}
                    {success && <p className="alert alert-success" style={{ marginTop: 16 }}>{success}</p>}
                </div>
                <div className="card-footer">
                    <button type="submit" className="btn btn-primary" disabled={isSubmitting}>
                        {isSubmitting ? 'Creating…' : 'Create staff account'}
                    </button>
                </div>
            </form>
        </div>
    );
};

export default RegisterStaffPage;
