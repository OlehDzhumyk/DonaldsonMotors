import React from 'react';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
import type { CreatePartPayload, Part, Supplier } from '../../types/inventory';
import { money, optionalText } from '../../utils/validation';

const schema = yup.object({
    name: yup.string().required('Name is required').min(3, 'At least 3 characters').max(100),
    supplierId: yup.number().typeError('Choose a supplier').required('Choose a supplier'),
    price: money('Selling price'),
    costPrice: money('Cost price'),
    initialStockLevel: yup.number().typeError('Enter a number').required().integer('Whole units only').min(0, 'Cannot be negative'),
    barcode: optionalText(50),
});

interface PartFormProps {
    /** The part being edited; omit to add a new one. */
    part?: Part;
    suppliers: Supplier[];
    onSubmit: (data: CreatePartPayload) => Promise<void>;
    onCancel: () => void;
    error: string | null;
}

const PartForm: React.FC<PartFormProps> = ({ part, suppliers, onSubmit, onCancel, error }) => {
    const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm<CreatePartPayload>({
        resolver: yupResolver(schema),
        defaultValues: part
            ? { ...part, initialStockLevel: part.currentStockLevel }
            : { initialStockLevel: 0, barcode: null, supplierId: suppliers[0]?.id },
    });

    return (
        <form onSubmit={(e) => void handleSubmit(onSubmit)(e)} noValidate>
            <div className="modal-body form-grid">
                <div className="field span-2">
                    <label htmlFor="name">Part name</label>
                    <input id="name" {...register('name')} />
                    {errors.name && <p className="field-error">{errors.name.message}</p>}
                </div>
                <div className="field span-2">
                    <label htmlFor="supplierId">Supplier</label>
                    <select id="supplierId" {...register('supplierId')}>
                        {suppliers.map(s => <option key={s.id} value={s.id}>{s.name}</option>)}
                    </select>
                    {errors.supplierId && <p className="field-error">{errors.supplierId.message}</p>}
                </div>
                <div className="field">
                    <label htmlFor="price">Selling price (£)</label>
                    <input id="price" type="number" step="0.01" {...register('price')} />
                    {errors.price && <p className="field-error">{errors.price.message}</p>}
                </div>
                <div className="field">
                    <label htmlFor="costPrice">Cost price (£)</label>
                    <input id="costPrice" type="number" step="0.01" {...register('costPrice')} />
                    {errors.costPrice && <p className="field-error">{errors.costPrice.message}</p>}
                </div>
                {!part && (
                    <div className="field">
                        <label htmlFor="initialStockLevel">Units in stock</label>
                        <input id="initialStockLevel" type="number" {...register('initialStockLevel')} />
                        {errors.initialStockLevel && <p className="field-error">{errors.initialStockLevel.message}</p>}
                    </div>
                )}
                <div className={`field ${part ? 'span-2' : ''}`}>
                    <label htmlFor="barcode">Barcode <span className="muted">(optional)</span></label>
                    <input id="barcode" className="mono" {...register('barcode')} />
                    {errors.barcode && <p className="field-error">{errors.barcode.message}</p>}
                </div>
                {error && <p className="alert alert-error span-2">{error}</p>}
            </div>
            <div className="modal-footer">
                <button type="button" className="btn btn-secondary" onClick={onCancel} disabled={isSubmitting}>Cancel</button>
                <button type="submit" className="btn btn-primary" disabled={isSubmitting}>
                    {isSubmitting ? 'Saving…' : part ? 'Save changes' : 'Add part'}
                </button>
            </div>
        </form>
    );
};

export default PartForm;
