// src/components/PartForm/PartForm.tsx
import React, { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
import type {CreatePartPayload, UpdatePartPayload, Part, Supplier} from '../../types/inventory';
import './PartForm.css';

// Yup schema for part validation
const partSchema = yup.object().shape({
    name: yup.string().required('Part name is required').min(3, 'Name too short'),
    price: yup.number().typeError('Price must be a number').required('Price is required').min(0, 'Price cannot be negative'),
    costPrice: yup.number().typeError('Cost price must be a number').optional().min(0, 'Cost price cannot be negative').nullable(),
    initialStockLevel: yup.number().typeError('Initial stock must be a number').required('Initial stock is required').integer('Stock must be an integer').min(0, 'Stock cannot be negative'),
    barcode: yup.string().optional().nullable(),
    supplierId: yup.number().typeError('Supplier ID must be a number').required('Supplier is required'),
});

interface PartFormProps {
    initialData?: Part | null; // For pre-filling in edit mode
    onSubmit: (data: CreatePartPayload | UpdatePartPayload) => Promise<void>;
    onCancel: () => void;
    isSubmitting: boolean;
    suppliers: Supplier[]; // List of available suppliers for dropdown
}

// For the form, we'll use CreatePartPayload as the base type for fields.
// UpdatePartPayload might have some fields optional or missing (like initialStockLevel).
type PartFormData = yup.InferType<typeof partSchema>;

const PartForm: React.FC<PartFormProps> = ({ initialData, onSubmit, onCancel, isSubmitting, suppliers }) => {
    const {
        register,
        handleSubmit,
        formState: { errors },
        reset,
    } = useForm({
        resolver: yupResolver(partSchema),
        defaultValues: initialData ?
            { // Map Part to PartFormData (CreatePartPayload fields)
                name: initialData.name,
                price: initialData.price,
                costPrice: initialData.costPrice,
                initialStockLevel: initialData.currentStockLevel, // Use currentStockLevel for initial value in edit
                barcode: initialData.barcode,
                supplierId: initialData.supplierId,
            } :
            { // Default for creating new
                name: '',
                price: undefined, // Let yup handle typeError for number
                costPrice: undefined,
                initialStockLevel: 0,
                barcode: '',
                supplierId: undefined,
            },
    });

    useEffect(() => {
        if (initialData) {
            reset({
                name: initialData.name,
                price: initialData.price,
                costPrice: initialData.costPrice,
                initialStockLevel: initialData.currentStockLevel,
                barcode: initialData.barcode,
                supplierId: initialData.supplierId,
            });
        } else {
            reset({ name: '', price: undefined, costPrice: undefined, initialStockLevel: 0, barcode: '', supplierId: undefined });
        }
    }, [initialData, reset]);

    const handleFormSubmit = async (data: PartFormData) => {
        await onSubmit(data);
    };

    return (
        <div className="part-form">
            <h3>{initialData ? 'Edit Part' : 'Add New Part'}</h3>
            <form onSubmit={handleSubmit(handleFormSubmit)}>
                <div className="form-group">
                    <label htmlFor="name">Part Name</label>
                    <input id="name" {...register('name')} />
                    {errors.name && <p className="error-message">{errors.name.message}</p>}
                </div>

                <div className="form-group">
                    <label htmlFor="price">Selling Price (£)</label>
                    <input id="price" type="number" step="0.01" {...register('price')} />
                    {errors.price && <p className="error-message">{errors.price.message}</p>}
                </div>

                <div className="form-group">
                    <label htmlFor="costPrice">Cost Price (£) (Optional)</label>
                    <input id="costPrice" type="number" step="0.01" {...register('costPrice')} />
                    {errors.costPrice && <p className="error-message">{errors.costPrice.message}</p>}
                </div>

                {!initialData && ( // Only show initialStockLevel for new parts
                    <div className="form-group">
                        <label htmlFor="initialStockLevel">Initial Stock Level</label>
                        <input id="initialStockLevel" type="number" step="1" {...register('initialStockLevel')} />
                        {errors.initialStockLevel && <p className="error-message">{errors.initialStockLevel.message}</p>}
                    </div>
                )}


                <div className="form-group">
                    <label htmlFor="barcode">Barcode (Optional)</label>
                    <input id="barcode" {...register('barcode')} />
                    {errors.barcode && <p className="error-message">{errors.barcode.message}</p>}
                </div>

                <div className="form-group">
                    <label htmlFor="supplierId">Supplier</label>
                    <select id="supplierId" {...register('supplierId')} defaultValue="">
                        <option value="" disabled>-- Select Supplier --</option>
                        {suppliers.map(supplier => (
                            <option key={supplier.id} value={supplier.id}>
                                {supplier.name} (ID: {supplier.id})
                            </option>
                        ))}
                    </select>
                    {errors.supplierId && <p className="error-message">{errors.supplierId.message}</p>}
                </div>

                <div className="form-actions">
                    <button type="button" onClick={onCancel} className="cancel-btn" disabled={isSubmitting}>
                        Cancel
                    </button>
                    <button type="submit" className="submit-btn" disabled={isSubmitting}>
                        {isSubmitting ? 'Saving...' : (initialData ? 'Save Changes' : 'Add Part')}
                    </button>
                </div>
            </form>
        </div>
    );
};

export default PartForm;