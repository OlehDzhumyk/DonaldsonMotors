// src/components/FinishJobModal/FinishJobModal.tsx
import React, { useState, useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
// import type { TypeOf } from 'yup'; // <-- ВИДАЛІТЬ АБО ЗАМІНІТЬ ЦЕЙ РЯДОК
import type { InferType } from 'yup'; // <-- ЗАМІНІТЬ НА InferType
import type { Booking, FinishJobPayload, UsedPartPayload } from '../../types/booking';
import type { Part } from '../../types/inventory';
import { searchParts } from '../../api/partService';
import './FinishJobModal.css';

interface FinishJobModalProps {
    isOpen: boolean;
    onClose: () => void;
    booking: Booking | null;
    onSubmit: (bookingId: number, data: FinishJobPayload) => Promise<void>;
    isSubmitting: boolean;
}

interface SelectedPartForJob extends UsedPartPayload {
    name: string;
    price: number;
}

const finishJobSchema = yup.object().shape({
    description: yup.string().required('Description of work done is required').min(10, 'Description too short'),
    labourCost: yup.number()
        .typeError('Labour cost must be a number (e.g., 50.00)')
        .transform((value, originalValue) => {
            return originalValue === "" ? undefined : value;
        })
        .required('Labour cost is required')
        .min(0, 'Labour cost cannot be negative'),
});

// ОНОВЛЕНО: Використовуємо InferType замість TypeOf
type FinishJobFormValues = InferType<typeof finishJobSchema>;

const FinishJobModal: React.FC<FinishJobModalProps> = ({
                                                           isOpen,
                                                           onClose,
                                                           booking,
                                                           onSubmit,
                                                           isSubmitting
                                                       }) => {
    // ... решта коду компонента залишається без змін ...
    const [partSearchTerm, setPartSearchTerm] = useState('');
    const [searchedParts, setSearchedParts] = useState<Part[]>([]);
    const [isLoadingParts, setIsLoadingParts] = useState(false);
    const [selectedPartsForJob, setSelectedPartsForJob] = useState<SelectedPartForJob[]>([]);

    const {
        register,
        handleSubmit,
        formState: { errors },
        reset,
    } = useForm<FinishJobFormValues>({ // Тепер тут правильний тип
        resolver: yupResolver(finishJobSchema),
        defaultValues: {
            description: booking?.notes || '',
            labourCost: undefined
        }
    });

    useEffect(() => {
        if (isOpen && booking) {
            reset({
                description: booking.notes || '',
                labourCost: undefined
            });
            setSelectedPartsForJob([]);
            setSearchedParts([]);
            setPartSearchTerm('');
        }
    }, [isOpen, booking, reset]);

    const handlePartSearch = async () => {
        if (!partSearchTerm.trim()) {
            setSearchedParts([]);
            return;
        }
        setIsLoadingParts(true);
        try {
            const parts = await searchParts(partSearchTerm);
            setSearchedParts(parts);
        } catch (error) {
            console.error("Failed to search parts:", error);
        } finally {
            setIsLoadingParts(false);
        }
    };

    const handleAddPartToJob = (part: Part) => {
        setSelectedPartsForJob(prev => {
            const existingPart = prev.find(p => p.partId === part.id);
            if (existingPart) {
                return prev.map(p => p.partId === part.id ? { ...p, quantity: p.quantity + 1 } : p);
            } else {
                return [...prev, { partId: part.id, quantity: 1, name: part.name, price: part.price }];
            }
        });
    };

    const handleUpdatePartQuantity = (partId: number, quantity: number) => {
        setSelectedPartsForJob(prev =>
            prev.map(p => p.partId === partId ? { ...p, quantity: Math.max(1, quantity) } : p) // Quantity should be at least 1 if part is selected
                .filter(p => p.quantity > 0) // This filter might be redundant if min quantity is 1
        );
    };

    const handleRemovePartFromJob = (partId: number) => {
        setSelectedPartsForJob(prev => prev.filter(p => p.partId !== partId));
    };

    const onFinalSubmit = (data: FinishJobFormValues) => { // data тепер типу FinishJobFormValues
        if (!booking) return;

        const payload: FinishJobPayload = {
            description: data.description,
            labourCost: data.labourCost, // data.labourCost тепер коректно number
            usedParts: selectedPartsForJob.map(p => ({ partId: p.partId, quantity: p.quantity })),
        };
        onSubmit(booking.id, payload);
    };

    if (!isOpen || !booking) {
        return null;
    }

    // ... решта JSX розмітки залишається без змін ...
    return (
        <>
            <div className="modal-overlay" onClick={onClose}></div>
            <div className="finish-job-modal">
                <div className="modal-header">
                    <h3>Finish Job for Booking ID: {booking.id}</h3>
                    <button onClick={onClose} className="close-modal-btn" disabled={isSubmitting}>&times;</button>
                </div>

                <form onSubmit={handleSubmit(onFinalSubmit)}>
                    <div className="form-group">
                        <label htmlFor="description">Work Description / Notes:</label>
                        <textarea id="description" {...register('description')} />
                        {errors.description && <p className="error-message">{errors.description.message}</p>}
                    </div>

                    <div className="form-group">
                        <label htmlFor="labourCost">Labour Cost (£):</label>
                        <input id="labourCost" type="number" step="0.01" {...register('labourCost')} />
                        {errors.labourCost && <p className="error-message">{errors.labourCost.message}</p>}
                    </div>

                    <div className="part-search-section">
                        <h4>Add Parts Used:</h4>
                        <div className="part-search-input-group">
                            <input
                                type="text"
                                placeholder="Search parts by name or code..."
                                value={partSearchTerm}
                                onChange={(e) => setPartSearchTerm(e.target.value)}
                            />
                            <button type="button" onClick={handlePartSearch} disabled={isLoadingParts}>
                                {isLoadingParts ? 'Searching...' : 'Search'}
                            </button>
                        </div>
                        {isLoadingParts && <p>Searching parts...</p>}
                        {!isLoadingParts && searchedParts.length > 0 && (
                            <ul className="searched-parts-list">
                                {searchedParts.map(part => (
                                    <li key={part.id}>
                                        <span>{part.name} (£{part.price.toFixed(2)}) - Stock: {part.currentStockLevel}</span>
                                        <button type="button" onClick={() => handleAddPartToJob(part)} disabled={part.currentStockLevel <= 0}>
                                            Add
                                        </button>
                                    </li>
                                ))}
                            </ul>
                        )}
                        {!isLoadingParts && searchedParts.length === 0 && partSearchTerm && (
                            <p>No parts found for "{partSearchTerm}".</p>
                        )}

                        {selectedPartsForJob.length > 0 && (
                            <>
                                <h4>Selected Parts:</h4>
                                <ul className="used-parts-list">
                                    {selectedPartsForJob.map(part => (
                                        <li key={part.partId}>
                                            <span className="part-info">{part.name} (£{part.price.toFixed(2)})</span>
                                            <div className="quantity-control">
                                                Qty:
                                                <input
                                                    type="number"
                                                    value={part.quantity}
                                                    min="1"
                                                    onChange={(e) => handleUpdatePartQuantity(part.partId, parseInt(e.target.value) || 1)}
                                                />
                                                <button type="button" onClick={() => handleRemovePartFromJob(part.partId)}>X</button>
                                            </div>
                                        </li>
                                    ))}
                                </ul>
                            </>
                        )}
                    </div>


                    <div className="form-actions">
                        <button type="button" onClick={onClose} className="cancel-btn" disabled={isSubmitting}>
                            Cancel
                        </button>
                        <button type="submit" className="submit-booking-btn" disabled={isSubmitting}>
                            {isSubmitting ? 'Submitting...' : 'Complete Job'}
                        </button>
                    </div>
                </form>
            </div>
        </>
    );
};

export default FinishJobModal;