import React, { useState } from 'react';

interface NoteFormProps {
    label: string;
    placeholder?: string;
    /** When set, the note is required and must be at least this long (the API's rule for cancellation reasons). */
    minLength?: number;
    submitLabel: string;
    danger?: boolean;
    onSubmit: (note: string | null) => Promise<void>;
    onCancel: () => void;
    error: string | null;
}

/** A dialog body with one text area, e.g. a cancellation reason or payment notes. */
const NoteForm: React.FC<NoteFormProps> = ({ label, placeholder, minLength, submitLabel, danger = false, onSubmit, onCancel, error }) => {
    const [note, setNote] = useState('');
    const [touched, setTouched] = useState(false);
    const [isSubmitting, setIsSubmitting] = useState(false);

    const trimmed = note.trim();
    const validationError = minLength !== undefined && trimmed.length < minLength
        ? `Please write at least ${String(minLength)} characters.`
        : null;

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setTouched(true);
        if (validationError) return;
        setIsSubmitting(true);
        await onSubmit(trimmed || null);
        setIsSubmitting(false);
    };

    return (
        <form onSubmit={(e) => void handleSubmit(e)} noValidate>
            <div className="modal-body">
                <div className="field">
                    <label htmlFor="note">{label}</label>
                    <textarea id="note" value={note} placeholder={placeholder} onChange={(e) => { setNote(e.target.value); }} />
                    {touched && validationError && <p className="field-error">{validationError}</p>}
                </div>
                {error && <p className="alert alert-error">{error}</p>}
            </div>
            <div className="modal-footer">
                <button type="button" className="btn btn-secondary" onClick={onCancel} disabled={isSubmitting}>Back</button>
                <button type="submit" className={`btn ${danger ? 'btn-danger' : 'btn-primary'}`} disabled={isSubmitting}>
                    {isSubmitting ? 'Saving…' : submitLabel}
                </button>
            </div>
        </form>
    );
};

export default NoteForm;
