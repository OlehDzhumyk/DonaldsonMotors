import React, { useEffect, useState } from 'react';
import { getMechanicsForSlot } from '../api/scheduleService';
import type { Booking } from '../types/booking';
import type { MechanicAvailability } from '../types/schedule';
import { formatDateTime, getErrorMessage } from '../utils/format';

interface AssignMechanicFormProps {
    booking: Booking;
    onSubmit: (mechanicId: number) => Promise<void>;
    onCancel: () => void;
    error: string | null;
}

const AssignMechanicForm: React.FC<AssignMechanicFormProps> = ({ booking, onSubmit, onCancel, error }) => {
    const [mechanics, setMechanics] = useState<MechanicAvailability[] | null>(null);
    const [loadError, setLoadError] = useState<string | null>(null);
    const [selectedId, setSelectedId] = useState<number | null>(null);
    const [isSubmitting, setIsSubmitting] = useState(false);

    useEffect(() => {
        getMechanicsForSlot(booking.slotStart, booking.serviceTypeId)
            .then(result => {
                setMechanics(result);
                setSelectedId(result.find(m => m.isAvailable)?.mechanicId ?? null);
            })
            .catch((err: unknown) => { setLoadError(getErrorMessage(err, 'Could not load mechanics.')); });
    }, [booking.slotStart, booking.serviceTypeId]);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (selectedId === null) return;
        setIsSubmitting(true);
        await onSubmit(selectedId);
        setIsSubmitting(false);
    };

    return (
        <form onSubmit={(e) => void handleSubmit(e)}>
            <div className="modal-body">
                <p className="muted" style={{ marginBottom: 16 }}>
                    {booking.serviceTypeName} for <span className="plate">{booking.vehicleRegistrationNumber}</span> on {formatDateTime(booking.slotStart)}
                </p>
                {loadError && <p className="alert alert-error">{loadError}</p>}
                {mechanics === null && !loadError && <p className="loading">Checking who is free…</p>}
                {mechanics?.length === 0 && <p className="empty-state">There are no mechanics yet. Add one on the Staff page.</p>}
                <div className="role-options">
                    {mechanics?.map(m => (
                        <label key={m.mechanicId} className="role-option" style={m.isAvailable ? undefined : { opacity: 0.6 }}>
                            <input type="radio" name="mechanic" disabled={!m.isAvailable}
                                   checked={selectedId === m.mechanicId}
                                   onChange={() => { setSelectedId(m.mechanicId); }} />
                            <span>
                                <strong>{m.mechanicName}</strong>
                                <span className="muted small">{m.isAvailable ? 'Free at this time' : m.reasonIfNotAvailable}</span>
                            </span>
                        </label>
                    ))}
                </div>
                {error && <p className="alert alert-error" style={{ marginTop: 16 }}>{error}</p>}
            </div>
            <div className="modal-footer">
                <button type="button" className="btn btn-secondary" onClick={onCancel} disabled={isSubmitting}>Back</button>
                <button type="submit" className="btn btn-primary" disabled={selectedId === null || isSubmitting}>
                    {isSubmitting ? 'Assigning…' : 'Assign mechanic'}
                </button>
            </div>
        </form>
    );
};

export default AssignMechanicForm;
