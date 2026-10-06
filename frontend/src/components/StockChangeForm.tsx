import React from 'react';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
import type { Part, UpdateStockPayload } from '../types/inventory';
import { optionalText } from '../utils/validation';

const schema = yup.object({
    changeInQuantity: yup.number().typeError('Enter a whole number').required('Enter a whole number')
        .integer('Whole units only').notOneOf([0], 'Enter a change other than 0'),
    reason: optionalText(200),
});

interface StockChangeFormProps {
    part: Part;
    onSubmit: (data: UpdateStockPayload) => Promise<void>;
    onCancel: () => void;
    error: string | null;
}

const StockChangeForm: React.FC<StockChangeFormProps> = ({ part, onSubmit, onCancel, error }) => {
    const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm<UpdateStockPayload>({
        resolver: yupResolver(schema),
        defaultValues: { reason: null },
    });

    return (
        <form onSubmit={(e) => void handleSubmit(onSubmit)(e)} noValidate>
            <div className="modal-body">
                <p className="muted" style={{ marginBottom: 16 }}>
                    {part.name} currently has <strong>{part.currentStockLevel}</strong> units in stock.
                </p>
                <div className="field">
                    <label htmlFor="changeInQuantity">Change in units</label>
                    <input id="changeInQuantity" type="number" placeholder="e.g. 10 for a delivery, -2 for damaged stock" {...register('changeInQuantity')} />
                    {errors.changeInQuantity && <p className="field-error">{errors.changeInQuantity.message}</p>}
                </div>
                <div className="field">
                    <label htmlFor="reason">Reason <span className="muted">(optional)</span></label>
                    <input id="reason" placeholder="e.g. Delivery INV-1042" {...register('reason')} />
                    {errors.reason && <p className="field-error">{errors.reason.message}</p>}
                </div>
                {error && <p className="alert alert-error">{error}</p>}
            </div>
            <div className="modal-footer">
                <button type="button" className="btn btn-secondary" onClick={onCancel} disabled={isSubmitting}>Cancel</button>
                <button type="submit" className="btn btn-primary" disabled={isSubmitting}>
                    {isSubmitting ? 'Saving…' : 'Update stock'}
                </button>
            </div>
        </form>
    );
};

export default StockChangeForm;
