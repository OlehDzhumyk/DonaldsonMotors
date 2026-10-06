import React, { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
import type {Vehicle, VehiclePayload} from '../../types/user';
import '../AddVehicleForm/AddVehicleForm.css'; // Reusing styles from AddVehicleForm

// Re-use the same validation schema as for adding a vehicle
const vehicleSchema = yup.object().shape({
    registrationNumber: yup.string().required('Registration number is required'), // Still validated, but field will be read-only
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

interface EditVehicleFormProps {
    vehicleToEdit: Vehicle; // The vehicle data to pre-fill the form
    onSubmit: (data: VehiclePayload) => Promise<void>;
    onCancel: () => void;
    isSubmitting: boolean;
}

const EditVehicleForm: React.FC<EditVehicleFormProps> = ({ vehicleToEdit, onSubmit, onCancel, isSubmitting }) => {
    const {
        register,
        handleSubmit,
        formState: { errors },
        reset,
    } = useForm<VehiclePayload>({
        resolver: yupResolver(vehicleSchema),
        defaultValues: vehicleToEdit, // Pre-fill the form with vehicleToEdit data
    });

    // If vehicleToEdit changes, reset the form with new default values
    useEffect(() => {
        reset(vehicleToEdit);
    }, [vehicleToEdit, reset]);

    const handleFormSubmit = async (data: VehiclePayload) => {
        await onSubmit({ ...data, registrationNumber: vehicleToEdit.registrationNumber });
    };

    return (
        // Using 'add-vehicle-form' class for styling, assuming similarity
        <div className="add-vehicle-form">
            <h3>Edit Vehicle Details</h3>
            <form onSubmit={handleSubmit(handleFormSubmit)}>
                <div className="form-group">
                    <label htmlFor="registrationNumber">Registration Number</label>
                    <input
                        id="registrationNumber"
                        {...register('registrationNumber')}
                        readOnly // <-- MAKE THIS FIELD READ-ONLY
                        style={{ backgroundColor: '#e9ecef', cursor: 'not-allowed' }} // Visual cue
                    />
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
                        {isSubmitting ? 'Saving...' : 'Save Changes'}
                    </button>
                </div>
            </form>
        </div>
    );
};

export default EditVehicleForm;