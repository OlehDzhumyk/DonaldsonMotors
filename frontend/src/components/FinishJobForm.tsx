import React, { useEffect, useMemo, useState } from 'react';
import { getParts } from '../api/partService';
import type { Booking, FinishJobPayload } from '../types/booking';
import type { Part } from '../types/inventory';
import { formatMoney, getErrorMessage } from '../utils/format';

interface FinishJobFormProps {
    booking: Booking;
    onSubmit: (data: FinishJobPayload) => Promise<void>;
    onCancel: () => void;
    error: string | null;
}

interface UsedPart { part: Part; quantity: number }

/** What the mechanic did, the labour charge and the parts taken from stock. */
const FinishJobForm: React.FC<FinishJobFormProps> = ({ booking, onSubmit, onCancel, error }) => {
    const [parts, setParts] = useState<Part[]>([]);
    const [partsError, setPartsError] = useState<string | null>(null);
    const [description, setDescription] = useState('');
    const [labour, setLabour] = useState(String(booking.serviceTypePrice));
    const [used, setUsed] = useState<UsedPart[]>([]);
    const [partToAdd, setPartToAdd] = useState('');
    const [touched, setTouched] = useState(false);
    const [isSubmitting, setIsSubmitting] = useState(false);

    useEffect(() => {
        getParts()
            .then(result => { setParts(result.sort((a, b) => a.name.localeCompare(b.name))); })
            .catch((err: unknown) => { setPartsError(getErrorMessage(err, 'Could not load parts.')); });
    }, []);

    const labourCost = Number(labour);
    const partsCost = useMemo(() => used.reduce((sum, u) => sum + u.part.price * u.quantity, 0), [used]);
    const errors = {
        description: description.trim().length < 10 ? 'Describe the work in at least 10 characters.' : null,
        labour: !labour || Number.isNaN(labourCost) || labourCost < 0 ? 'Enter the labour cost.' : null,
        stock: used.find(u => u.quantity > u.part.currentStockLevel)?.part.name,
    };
    const isValid = !errors.description && !errors.labour && !errors.stock;

    const addPart = () => {
        const part = parts.find(p => p.id === Number(partToAdd));
        if (!part) return;
        setUsed(current => current.some(u => u.part.id === part.id)
            ? current.map(u => (u.part.id === part.id ? { ...u, quantity: u.quantity + 1 } : u))
            : [...current, { part, quantity: 1 }]);
        setPartToAdd('');
    };

    const setQuantity = (partId: number, quantity: number) => {
        setUsed(current => current.map(u => (u.part.id === partId ? { ...u, quantity: Math.max(1, quantity) } : u)));
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setTouched(true);
        if (!isValid) return;
        setIsSubmitting(true);
        await onSubmit({
            description: description.trim(),
            labourCost,
            usedParts: used.map(u => ({ partId: u.part.id, quantity: u.quantity })),
        });
        setIsSubmitting(false);
    };

    return (
        <form onSubmit={(e) => void handleSubmit(e)} noValidate>
            <div className="modal-body">
                <div className="field">
                    <label htmlFor="description">Work carried out</label>
                    <textarea id="description" value={description} placeholder="e.g. Replaced front brake pads and checked discs"
                              onChange={(e) => { setDescription(e.target.value); }} />
                    {touched && errors.description && <p className="field-error">{errors.description}</p>}
                </div>
                <div className="field">
                    <label htmlFor="labour">Labour cost (£)</label>
                    <input id="labour" type="number" step="0.01" min="0" value={labour} onChange={(e) => { setLabour(e.target.value); }} />
                    {touched && errors.labour && <p className="field-error">{errors.labour}</p>}
                </div>

                <div className="field">
                    <label htmlFor="part">Parts used</label>
                    <div className="row" style={{ flexWrap: 'nowrap' }}>
                        <select id="part" value={partToAdd} onChange={(e) => { setPartToAdd(e.target.value); }}>
                            <option value="">Choose a part…</option>
                            {parts.map(p => (
                                <option key={p.id} value={p.id} disabled={p.currentStockLevel === 0}>
                                    {p.name} · {formatMoney(p.price)} · {p.currentStockLevel} in stock
                                </option>
                            ))}
                        </select>
                        <button type="button" className="btn btn-secondary" onClick={addPart} disabled={!partToAdd}>Add</button>
                    </div>
                    {partsError && <p className="field-error">{partsError}</p>}
                </div>

                {used.length > 0 && (
                    <table className="table" style={{ marginBottom: 16 }}>
                        <tbody>
                            {used.map(u => (
                                <tr key={u.part.id}>
                                    <td>{u.part.name}</td>
                                    <td style={{ width: 90 }}>
                                        <input type="number" min="1" max={u.part.currentStockLevel} value={u.quantity} aria-label={`Quantity of ${u.part.name}`}
                                               onChange={(e) => { setQuantity(u.part.id, Number(e.target.value)); }} />
                                    </td>
                                    <td className="num">{formatMoney(u.part.price * u.quantity)}</td>
                                    <td className="actions">
                                        <button type="button" className="icon-btn" aria-label={`Remove ${u.part.name}`}
                                                onClick={() => { setUsed(current => current.filter(x => x.part.id !== u.part.id)); }}>&times;</button>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                )}
                {errors.stock && <p className="alert alert-error" style={{ marginBottom: 16 }}>Not enough {errors.stock} in stock.</p>}

                <dl className="meta">
                    <dt>Labour</dt><dd>{formatMoney(Number.isNaN(labourCost) ? 0 : labourCost)}</dd>
                    <dt>Parts</dt><dd>{formatMoney(partsCost)}</dd>
                    <dt><strong>Invoice total</strong></dt><dd><strong>{formatMoney((Number.isNaN(labourCost) ? 0 : labourCost) + partsCost)}</strong></dd>
                </dl>
                {error && <p className="alert alert-error" style={{ marginTop: 16 }}>{error}</p>}
            </div>
            <div className="modal-footer">
                <button type="button" className="btn btn-secondary" onClick={onCancel} disabled={isSubmitting}>Back</button>
                <button type="submit" className="btn btn-primary" disabled={isSubmitting}>
                    {isSubmitting ? 'Saving…' : 'Finish job and send invoice'}
                </button>
            </div>
        </form>
    );
};

export default FinishJobForm;
