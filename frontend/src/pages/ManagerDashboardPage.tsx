import React, { useCallback, useEffect, useState } from 'react';
import { searchBookings, assignMechanic, cancelBookingAsManager, markBookingAsPaid } from '../api/bookingService';
import type { Booking, BookingSearchParameters, BookingStatus } from '../types/booking';
import { ACTIVE_STATUSES } from '../types/booking';
import BookingCard from '../components/BookingCard/BookingCard';
import StatusBadge from '../components/StatusBadge';
import Modal from '../components/Modal';
import NoteForm from '../components/NoteForm';
import AssignMechanicForm from '../components/AssignMechanicForm';
import { useAppSelector } from '../hooks/reduxHooks';
import { Roles } from '../utils/roles';
import { formatDateTime, formatMoney, getErrorMessage, statusLabel } from '../utils/format';
import './ManagerDashboardPage.css';

const COLUMNS: { title: string; hint: string; statuses: BookingStatus[] }[] = [
    { title: 'Needs a mechanic', hint: 'New bookings waiting to be assigned', statuses: ['Pending'] },
    { title: 'Scheduled & in progress', hint: 'Assigned or being worked on', statuses: ['Assigned', 'InProgress'] },
    { title: 'Awaiting payment', hint: 'Job finished, invoice sent', statuses: ['AwaitingPayment'] },
];

const HISTORY_STATUSES: BookingStatus[] = ['Paid', 'Cancelled', 'Archived'];

interface Dialog { kind: 'assign' | 'cancel' | 'pay'; booking: Booking }

const ManagerDashboardPage: React.FC = () => {
    const role = useAppSelector((state) => state.auth.user?.role);
    const isManager = role === Roles.Manager;

    const [view, setView] = useState<'board' | 'history'>('board');
    const [bookings, setBookings] = useState<Booking[] | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [filters, setFilters] = useState<BookingSearchParameters>({});
    const [dialog, setDialog] = useState<Dialog | null>(null);
    const [dialogError, setDialogError] = useState<string | null>(null);

    const load = useCallback(async (params: BookingSearchParameters) => {
        try {
            const result = await searchBookings(params);
            setBookings(result.sort((a, b) => a.slotStart.localeCompare(b.slotStart)));
            setError(null);
        } catch (err) {
            setError(getErrorMessage(err, 'Could not load bookings.'));
        }
    }, []);

    useEffect(() => { void load({}); }, [load]);

    const updateFilter = (patch: Partial<BookingSearchParameters>) => {
        setFilters(current => ({ ...current, ...patch }));
    };

    const applyFilters = (e: React.FormEvent) => {
        e.preventDefault();
        void load(filters);
    };

    const clearFilters = () => {
        setFilters({});
        void load({});
    };

    const runDialogAction = async (action: () => Promise<void>, fallback: string) => {
        try {
            await action();
            setDialog(null);
            await load(filters);
        } catch (err) {
            setDialogError(getErrorMessage(err, fallback));
        }
    };

    const openDialog = (kind: Dialog['kind'], booking: Booking) => {
        setDialog({ kind, booking });
        setDialogError(null);
    };

    const all = bookings ?? [];
    const count = (statuses: BookingStatus[]) => all.filter(b => statuses.includes(b.status)).length;
    const history = all.filter(b => HISTORY_STATUSES.includes(b.status)).reverse();
    const awaitingTotal = all.filter(b => b.status === 'AwaitingPayment').reduce((sum, b) => sum + b.serviceTypePrice, 0);

    const actionsFor = (booking: Booking) => (
        <>
            {isManager && booking.status === 'Pending' && (
                <button className="btn btn-sm btn-dark" onClick={() => { openDialog('assign', booking); }}>Assign mechanic</button>
            )}
            {booking.status === 'AwaitingPayment' && (
                <button className="btn btn-sm btn-success" onClick={() => { openDialog('pay', booking); }}>Mark as paid</button>
            )}
            {isManager && ['Pending', 'Assigned', 'InProgress'].includes(booking.status) && (
                <button className="btn btn-sm btn-danger" onClick={() => { openDialog('cancel', booking); }}>Cancel</button>
            )}
        </>
    );

    return (
        <div className="container page">
            <div className="page-header">
                <div>
                    <h1>{isManager ? 'Bookings' : 'Bookings & payments'}</h1>
                    <p className="subtitle">
                        {isManager ? 'Assign mechanics, follow jobs and take payments.' : 'Mark finished jobs as paid once the customer has paid.'}
                    </p>
                </div>
                <div className="tabs">
                    <button className={view === 'board' ? 'active' : ''} onClick={() => { setView('board'); }}>Active</button>
                    <button className={view === 'history' ? 'active' : ''} onClick={() => { setView('history'); }}>History</button>
                </div>
            </div>

            <div className="stats">
                <div className="card stat"><span className="stat-label">Needs a mechanic</span><span className={`stat-value ${count(['Pending']) > 0 ? 'stat-warning' : ''}`}>{count(['Pending'])}</span></div>
                <div className="card stat"><span className="stat-label">Scheduled & in progress</span><span className="stat-value">{count(['Assigned', 'InProgress'])}</span></div>
                <div className="card stat"><span className="stat-label">Awaiting payment</span><span className="stat-value">{count(['AwaitingPayment'])}</span></div>
                <div className="card stat"><span className="stat-label">Paid jobs</span><span className="stat-value">{count(['Paid'])}</span></div>
            </div>

            <form className="card filter-bar" onSubmit={applyFilters}>
                <input placeholder="Registration" value={filters.vehicleRegistrationNumber ?? ''}
                       onChange={(e) => { updateFilter({ vehicleRegistrationNumber: e.target.value || undefined }); }} />
                <select value={filters.status ?? ''}
                        onChange={(e) => { updateFilter({ status: (e.target.value || undefined) as BookingStatus | undefined }); }}>
                    <option value="">Any status</option>
                    {[...ACTIVE_STATUSES, ...HISTORY_STATUSES].map(s => <option key={s} value={s}>{statusLabel(s)}</option>)}
                </select>
                <input type="date" aria-label="From date" value={filters.dateFrom ?? ''}
                       onChange={(e) => { updateFilter({ dateFrom: e.target.value || undefined }); }} />
                <input type="date" aria-label="To date" value={filters.dateTo ?? ''}
                       onChange={(e) => { updateFilter({ dateTo: e.target.value || undefined }); }} />
                <button type="submit" className="btn btn-dark">Search</button>
                <button type="button" className="btn btn-secondary" onClick={clearFilters}>Clear</button>
            </form>

            {error && <p className="alert alert-error" style={{ marginBottom: 16 }}>{error}</p>}
            {bookings === null && !error && <p className="loading">Loading bookings…</p>}

            {bookings && view === 'board' && (
                <div className="board">
                    {COLUMNS.map(column => {
                        const items = all.filter(b => column.statuses.includes(b.status));
                        return (
                            <section key={column.title} className="board-column">
                                <header>
                                    <h2>{column.title} <span className="count">{items.length}</span></h2>
                                    <p className="muted small">{column.hint}</p>
                                </header>
                                {items.length === 0 && <p className="empty-state">Nothing here.</p>}
                                {items.map(b => (
                                    <BookingCard key={b.id} booking={b} showCustomer>{actionsFor(b)}</BookingCard>
                                ))}
                                {column.statuses.includes('AwaitingPayment') && items.length > 0 && (
                                    <p className="muted small">Service total {formatMoney(awaitingTotal)} (parts are added on the invoice)</p>
                                )}
                            </section>
                        );
                    })}
                </div>
            )}

            {bookings && view === 'history' && (
                <div className="card table-wrap">
                    <table className="table">
                        <thead>
                            <tr><th>Date</th><th>Service</th><th>Vehicle</th><th>Customer</th><th>Mechanic</th><th>Status</th></tr>
                        </thead>
                        <tbody>
                            {history.map(b => (
                                <tr key={b.id}>
                                    <td>{formatDateTime(b.slotStart)}</td>
                                    <td>{b.serviceTypeName}</td>
                                    <td><span className="plate">{b.vehicleRegistrationNumber}</span></td>
                                    <td>{b.customerFullName}</td>
                                    <td>{b.mechanicName ?? '—'}</td>
                                    <td><StatusBadge status={b.status} /></td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                    {history.length === 0 && <div className="card-body"><p className="empty-state">No finished or cancelled bookings yet.</p></div>}
                </div>
            )}

            {dialog?.kind === 'assign' && (
                <Modal title="Assign a mechanic" onClose={() => { setDialog(null); }}>
                    <AssignMechanicForm booking={dialog.booking} error={dialogError} onCancel={() => { setDialog(null); }}
                                        onSubmit={(mechanicId) => runDialogAction(() => assignMechanic(dialog.booking.id, mechanicId), 'Could not assign the mechanic.')} />
                </Modal>
            )}
            {dialog?.kind === 'cancel' && (
                <Modal title={`Cancel booking #${String(dialog.booking.id)}`} onClose={() => { setDialog(null); }}>
                    <NoteForm label="Reason (sent to the customer)" minLength={10} submitLabel="Cancel booking" danger
                              error={dialogError} onCancel={() => { setDialog(null); }}
                              onSubmit={(reason) => runDialogAction(() => cancelBookingAsManager(dialog.booking.id, reason ?? ''), 'Could not cancel the booking.')} />
                </Modal>
            )}
            {dialog?.kind === 'pay' && (
                <Modal title={`Mark booking #${String(dialog.booking.id)} as paid`} onClose={() => { setDialog(null); }}>
                    <NoteForm label="Payment notes (optional)" placeholder="e.g. Paid by card at collection" submitLabel="Mark as paid"
                              error={dialogError} onCancel={() => { setDialog(null); }}
                              onSubmit={(notes) => runDialogAction(() => markBookingAsPaid(dialog.booking.id, notes), 'Could not record the payment.')} />
                </Modal>
            )}
        </div>
    );
};

export default ManagerDashboardPage;
