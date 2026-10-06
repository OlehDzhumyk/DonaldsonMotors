// src/pages/RegisterStaffPage.tsx
import React, { useState } from 'react';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
import type {RegisterStaffPayload} from '../types/auth';
import { useAppDispatch } from '../hooks/reduxHooks';
import { registerStaffMember } from '../app/authSlice';
import { Roles } from '../utils/roles';
import '../pages/RegisterPage.css';

// Схема валідації для форми реєстрації співробітника
const staffSchema = yup.object().shape({
    fullName: yup.string().required('Full name is required').min(3, 'Full name must be at least 3 characters'),
    email: yup.string().email('Must be a valid email').required('Email is required'),
    password: yup.string().required('Password is required').min(6, 'Password must be at least 6 characters'),
    confirmPassword: yup.string()
        .oneOf([yup.ref('password')], 'Passwords must match')
        .required('Confirm password is required'),
    role: yup.string()
        .oneOf([Roles.Mechanic, Roles.StockController, Roles.AccountsClerk], 'Invalid role selected')
        .required('Role is required'),
});

// Тип для значень форми, який yup виведе зі схеми
// type StaffFormData = yup.InferType<typeof staffSchema>;
// Або використовуємо RegisterStaffPayload, оскільки вони мають збігатися

const RegisterStaffPage: React.FC = () => {
    const dispatch = useAppDispatch();

    // Локальний стан для повідомлень форми
    const [submissionStatus, setSubmissionStatus] = useState<'idle' | 'loading' | 'succeeded' | 'failed'>('idle');
    const [submissionError, setSubmissionError] = useState<string | null>(null);
    const [successMessage, setSuccessMessage] = useState<string | null>(null);

    const {
        register,
        handleSubmit,
        formState: { errors },
        reset, // Для очищення форми після успішної реєстрації
    } = useForm<RegisterStaffPayload>({ // Використовуємо наш RegisterStaffPayload
        resolver: yupResolver(staffSchema),
        defaultValues: { // Важливо встановити defaultValues, особливо для select
            fullName: '',
            email: '',
            password: '',
            confirmPassword: '',
            role: undefined // Або перше значення зі списку
        }
    });

    const onSubmit = async (data: RegisterStaffPayload) => {
        // confirmPassword не надсилається на бекенд
        const { confirmPassword, ...payloadToSend } = data;

        setSubmissionStatus('loading');
        setSubmissionError(null);
        setSuccessMessage(null);

        try {
            // dispatch повертає Promise, unwrap() поверне resolved value або викине помилку
            const resultAction = await dispatch(registerStaffMember(payloadToSend)).unwrap();
            setSubmissionStatus('succeeded');
            setSuccessMessage(`Staff member '${resultAction.email}' registered successfully as ${resultAction.role}!`);
            reset(); // Очистити форму
        } catch (error: any) {
            setSubmissionStatus('failed');
            // 'error' тут - це те, що повернув rejectWithValue у thunk
            setSubmissionError(error as string || 'An unknown error occurred during staff registration.');
        }
    };

    // Ролі, які менеджер може призначати
    const assignableRoles = [
        { value: Roles.Mechanic, label: 'Mechanic' },
        { value: Roles.StockController, label: 'Stock Controller' },
        { value: Roles.AccountsClerk, label: 'Accounts Clerk' },
    ];

    return (
        <div className="register-page-container"> {/* Використовуємо клас зі сторінки реєстрації клієнта */}
            <div className="register-form-card" style={{maxWidth: '550px'}}> {/* Трохи ширше для поля ролі */}
                <h2>Register New Staff Member</h2>
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

                    <div className="form-group">
                        <label htmlFor="role">Assign Role</label>
                        <select id="role" {...register('role')} defaultValue="" className="filter-select" style={{width: '100%', padding: '10px', fontSize: '15px', borderRadius: '6px', border: '1px solid #ccc'}}>
                            <option value="" disabled hidden>-- Select a Role --</option>
                            {assignableRoles.map(r => (
                                <option key={r.value} value={r.value}>{r.label}</option>
                            ))}
                        </select>
                        {errors.role && <p className="error-message">{errors.role.message}</p>}
                    </div>

                    {submissionStatus === 'failed' && submissionError && (
                        <div className="register-error">{submissionError}</div>
                    )}
                    {submissionStatus === 'succeeded' && successMessage && (
                        <div style={{color: 'green', textAlign: 'center', marginBottom: '15px', backgroundColor: 'rgba(40,167,69,0.1)', padding: '10px', borderRadius: '6px'}}>
                            {successMessage}
                        </div>
                    )}

                    <button type="submit" className="submit-button" disabled={submissionStatus === 'loading'}>
                        {submissionStatus === 'loading' ? 'Registering...' : 'Register Staff'}
                    </button>
                </form>
            </div>
        </div>
    );
};

export default RegisterStaffPage;