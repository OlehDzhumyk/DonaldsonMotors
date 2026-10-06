// src/components/SupplierForm/SupplierForm.tsx
import React, { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
import type {CreateSupplierPayload, UpdateSupplierPayload, Supplier} from '../../types/inventory';
import './SupplierForm.css';

// Yup schema for supplier validation
const supplierSchema = yup.object().shape({
    name: yup.string().required('Supplier name is required').min(3, 'Name too short'),
    addressLine1: yup.string().required('Address Line 1 is required').min(5, 'Address too short'),
    addressLine2: yup.string().nullable(),
    postcode: yup.string().required('Postcode is required').matches(/^[A-Z]{1,2}[0-9R][0-9A-Z]? [0-9][ABD-HJLNP-UW-Z]{2}$/i, 'Invalid UK postcode format'),
    telephone: yup.string().required('Telephone is required').matches(/^[0-9+\-() ]+$/, 'Invalid phone number format'),
    email: yup.string().email('Invalid email format').required('Email is required'),
});

// Props for the form
interface SupplierFormProps {
    initialData?: Supplier | null; // For pre-filling the form in edit mode
    onSubmit: (data: CreateSupplierPayload | UpdateSupplierPayload) => Promise<void>;
    onCancel: () => void;
    isSubmitting: boolean;
}

type SupplierFormData = yup.InferType<typeof supplierSchema>;


const SupplierForm: React.FC<SupplierFormProps> = ({ initialData, onSubmit, onCancel, isSubmitting }) => {
    const {
        register,
        handleSubmit,
        formState: { errors },
        reset,
    } = useForm({
        resolver: yupResolver(supplierSchema),
        defaultValues: initialData ?
            { // Map Supplier to SupplierFormData (which is CreateSupplierPayload)
                name: initialData.name,
                addressLine1: initialData.addressLine1,
                addressLine2: initialData.addressLine2 || '',
                postcode: initialData.postcode,
                telephone: initialData.telephone || '', // Handle null telephone from Supplier type
                email: initialData.email,
            } :
            { // Default values for creating
                name: '',
                addressLine1: '',
                addressLine2: '',
                postcode: '',
                telephone: '',
                email: '',
            },
    });

    // Reset form if initialData changes (for edit mode primarily)
    useEffect(() => {
        if (initialData) {
            reset({
                name: initialData.name,
                addressLine1: initialData.addressLine1,
                addressLine2: initialData.addressLine2 || '',
                postcode: initialData.postcode,
                telephone: initialData.telephone || '',
                email: initialData.email,
            });
        } else {
            // Reset to empty for add mode if form is re-shown
            reset({ name: '', addressLine1: '', addressLine2: '', postcode: '', telephone: '', email: '' });
        }
    }, [initialData, reset]);

    const handleFormSubmit = async (data: SupplierFormData) => {
        await onSubmit(data);
    };

    return (
        <div className="supplier-form">
            <h3>{initialData ? 'Edit Supplier' : 'Add New Supplier'}</h3>
            <form onSubmit={handleSubmit(handleFormSubmit)}>
                <div className="form-group">
                    <label htmlFor="name">Supplier Name</label>
                    <input id="name" {...register('name')} />
                    {errors.name && <p className="error-message">{errors.name.message}</p>}
                </div>

                <div className="form-group">
                    <label htmlFor="email">Email</label>
                    <input id="email" type="email" {...register('email')} />
                    {errors.email && <p className="error-message">{errors.email.message}</p>}
                </div>

                <div className="form-group">
                    <label htmlFor="telephone">Telephone</label>
                    <input id="telephone" type="tel" {...register('telephone')} />
                    {errors.telephone && <p className="error-message">{errors.telephone.message}</p>}
                </div>

                <div className="form-group">
                    <label htmlFor="addressLine1">Address Line 1</label>
                    <input id="addressLine1" {...register('addressLine1')} />
                    {errors.addressLine1 && <p className="error-message">{errors.addressLine1.message}</p>}
                </div>

                <div className="form-group">
                    <label htmlFor="addressLine2">Address Line 2 (Optional)</label>
                    <input id="addressLine2" {...register('addressLine2')} />
                    {errors.addressLine2 && <p className="error-message">{errors.addressLine2.message}</p>}
                </div>

                <div className="form-group">
                    <label htmlFor="postcode">Postcode</label>
                    <input id="postcode" {...register('postcode')} />
                    {errors.postcode && <p className="error-message">{errors.postcode.message}</p>}
                </div>

                <div className="form-actions">
                    <button type="button" onClick={onCancel} className="cancel-btn" disabled={isSubmitting}>
                        Cancel
                    </button>
                    <button type="submit" className="submit-btn" disabled={isSubmitting}>
                        {isSubmitting ? 'Saving...' : (initialData ? 'Save Changes' : 'Add Supplier')}
                    </button>
                </div>
            </form>
        </div>
    );
};

export default SupplierForm;
