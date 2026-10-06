import React from 'react';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
import type { Vehicle, VehiclePayload } from '../../types/user';

const nextYear = new Date().getFullYear() + 1;

const schema = yup.object({
    registrationNumber: yup.string().required('Registration is required').min(3, 'Too short').max(10, 'Too long'),
    make: yup.string().required('Make is required').min(2, 'Too short'),
    model: yup.string().required('Model is required'),
    year: yup.number().typeError('Enter a year').required('Year is required')
        .min(1900, 'Year must be after 1900').max(nextYear, `Year cannot be after ${String(nextYear)}`),
    mileage: yup.number().typeError('Enter the mileage').required('Mileage is required').min(0, 'Mileage cannot be negative'),
});

interface VehicleFormProps {
    /** The vehicle being edited; omit to add a new one. */
    vehicle?: Vehicle;
    onSubmit: (data: VehiclePayload) => Promise<void>;
    onCancel: () => void;
    error: string | null;
}

const VehicleForm: React.FC<VehicleFormProps> = ({ vehicle, onSubmit, onCancel, error }) => {
    const isEdit = vehicle !== undefined;
    const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm<VehiclePayload>({
        resolver: yupResolver(schema),
        defaultValues: vehicle,
    });

    return (
        <form className="card" onSubmit={(e) => void handleSubmit(onSubmit)(e)} noValidate>
            <div className="card-header">
                <h3>{isEdit ? `Edit ${vehicle.registrationNumber}` : 'Add a vehicle'}</h3>
            </div>
            <div className="card-body form-grid">
                <div className="field">
                    <label htmlFor="registrationNumber">Registration</label>
                    <input id="registrationNumber" className="mono" readOnly={isEdit} {...register('registrationNumber')} />
                    {errors.registrationNumber && <p className="field-error">{errors.registrationNumber.message}</p>}
                </div>
                <div className="field">
                    <label htmlFor="year">Year</label>
                    <input id="year" type="number" {...register('year')} />
                    {errors.year && <p className="field-error">{errors.year.message}</p>}
                </div>
                <div className="field">
                    <label htmlFor="make">Make</label>
                    <input id="make" {...register('make')} />
                    {errors.make && <p className="field-error">{errors.make.message}</p>}
                </div>
                <div className="field">
                    <label htmlFor="model">Model</label>
                    <input id="model" {...register('model')} />
                    {errors.model && <p className="field-error">{errors.model.message}</p>}
                </div>
                <div className="field span-2">
                    <label htmlFor="mileage">Mileage</label>
                    <input id="mileage" type="number" {...register('mileage')} />
                    {errors.mileage && <p className="field-error">{errors.mileage.message}</p>}
                </div>
                {error && <p className="alert alert-error span-2">{error}</p>}
            </div>
            <div className="card-footer">
                <button type="button" className="btn btn-secondary" onClick={onCancel} disabled={isSubmitting}>Cancel</button>
                <button type="submit" className="btn btn-primary" disabled={isSubmitting}>
                    {isSubmitting ? 'Saving…' : 'Save vehicle'}
                </button>
            </div>
        </form>
    );
};

export default VehicleForm;
