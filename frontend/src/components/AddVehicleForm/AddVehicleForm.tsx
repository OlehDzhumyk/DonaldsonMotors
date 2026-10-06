import React from 'react';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
import type {VehiclePayload} from '../../types/user';
import './AddVehicleForm.css';

// Validation schema for the vehicle form
const vehicleSchema = yup.object().shape({
    registrationNumber: yup.string().required('Registration number is required').min(3, 'Too short').max(10, 'Too long'),
    make: yup.string().required('Make is required').min(2, 'Too short'),
    model: yup.string().required('Model is required').min(1, 'Too short'),
    year: yup.number()
        .typeError('Year must be a number')
        .required('Year is required')
        .min(1900, 'Year must be after 1900')
        .max(new Date().getFullYear() + 1, `Year cannot be in the future beyond ${new Date().getFullYear() + 1}`),
    mileage: yup.number()
        .typeError('Mileage must be a number')
        .required('Mileage is required')
        .min(0, 'Mileage cannot be negative'),
});

interface AddVehicleFormProps {
    onSubmit: (data: VehiclePayload) => Promise<void>; // onSubmit now returns a Promise
    onCancel: () => void;
    isSubmitting: boolean; // To disable button during submission
}

const AddVehicleForm: React.FC<AddVehicleFormProps> = ({ onSubmit, onCancel, isSubmitting }) => {
    const {
        register,
        handleSubmit,
        formState: { errors },
    } = useForm<VehiclePayload>({
        resolver: yupResolver(vehicleSchema),
    });

    const handleFormSubmit = async (data: VehiclePayload) => {
        await onSubmit(data); // Wait for the onSubmit prop to complete
    };

    return (
        <div className="add-vehicle-form">
            <h3>Add New Vehicle</h3>
            <form onSubmit={handleSubmit(handleFormSubmit)}>
                <div className="form-group">
                    <label htmlFor="registrationNumber">Registration Number</label>
                    <input id="registrationNumber" {...register('registrationNumber')} />
                    {errors.registrationNumber && <p className="error-message">{errors.registrationNumber.message}</p>}
                </div>

                <div className="form-group">
                    <label htmlFor="make">Make</label>
                    <input id="make" {...register('make')} />
                    {errors.make && <p className="error-message">{errors.make.message}</p>}
                </div>

                <div className="form-group">
                    <label htmlFor="model">Model</label>
                    <input id="model" {...register('model')} />
                    {errors.model && <p className="error-message">{errors.model.message}</p>}
                </div>

                <div className="form-group">
                    <label htmlFor="year">Year</label>
                    <input id="year" type="number" {...register('year')} />
                    {errors.year && <p className="error-message">{errors.year.message}</p>}
                </div>

                <div className="form-group">
                    <label htmlFor="mileage">Mileage</label>
                    <input id="mileage" type="number" {...register('mileage')} />
                    {errors.mileage && <p className="error-message">{errors.mileage.message}</p>}
                </div>

                <div className="form-actions">
                    <button type="button" onClick={onCancel} className="cancel-btn" disabled={isSubmitting}>
                        Cancel
                    </button>
                    <button type="submit" className="submit-vehicle-btn" disabled={isSubmitting}>
                        {isSubmitting ? 'Saving...' : 'Save Vehicle'}
                    </button>
                </div>
            </form>
        </div>
    );
};

export default AddVehicleForm;