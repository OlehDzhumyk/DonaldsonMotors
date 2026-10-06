import React from 'react';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
import type { Supplier, SupplierPayload } from '../../types/inventory';
import { optionalText } from '../../utils/validation';

const schema = yup.object({
    name: yup.string().required('Name is required').min(2, 'At least 2 characters').max(100),
    addressLine1: optionalText(100),
    addressLine2: optionalText(100),
    postcode: yup.string().required('Postcode is required').max(10, 'At most 10 characters'),
    telephone: optionalText(20),
    email: optionalText(100).email('Enter a valid email'),
});

interface SupplierFormProps {
    /** The supplier being edited; omit to add a new one. */
    supplier?: Supplier;
    onSubmit: (data: SupplierPayload) => Promise<void>;
    onCancel: () => void;
    error: string | null;
}

const SupplierForm: React.FC<SupplierFormProps> = ({ supplier, onSubmit, onCancel, error }) => {
    const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm<SupplierPayload>({
        resolver: yupResolver(schema),
        defaultValues: supplier,
    });

    return (
        <form onSubmit={(e) => void handleSubmit(onSubmit)(e)} noValidate>
            <div className="modal-body form-grid">
                <div className="field span-2">
                    <label htmlFor="name">Company name</label>
                    <input id="name" {...register('name')} />
                    {errors.name && <p className="field-error">{errors.name.message}</p>}
                </div>
                <div className="field">
                    <label htmlFor="email">Email</label>
                    <input id="email" type="email" {...register('email')} />
                    {errors.email && <p className="field-error">{errors.email.message}</p>}
                </div>
                <div className="field">
                    <label htmlFor="telephone">Telephone</label>
                    <input id="telephone" {...register('telephone')} />
                    {errors.telephone && <p className="field-error">{errors.telephone.message}</p>}
                </div>
                <div className="field span-2">
                    <label htmlFor="addressLine1">Address</label>
                    <input id="addressLine1" {...register('addressLine1')} />
                    {errors.addressLine1 && <p className="field-error">{errors.addressLine1.message}</p>}
                </div>
                <div className="field">
                    <label htmlFor="addressLine2">Address line 2 <span className="muted">(optional)</span></label>
                    <input id="addressLine2" {...register('addressLine2')} />
                    {errors.addressLine2 && <p className="field-error">{errors.addressLine2.message}</p>}
                </div>
                <div className="field">
                    <label htmlFor="postcode">Postcode</label>
                    <input id="postcode" {...register('postcode')} />
                    {errors.postcode && <p className="field-error">{errors.postcode.message}</p>}
                </div>
                {error && <p className="alert alert-error span-2">{error}</p>}
            </div>
            <div className="modal-footer">
                <button type="button" className="btn btn-secondary" onClick={onCancel} disabled={isSubmitting}>Cancel</button>
                <button type="submit" className="btn btn-primary" disabled={isSubmitting}>
                    {isSubmitting ? 'Saving…' : supplier ? 'Save changes' : 'Add supplier'}
                </button>
            </div>
        </form>
    );
};

export default SupplierForm;
