import React, { useCallback, useEffect, useState } from 'react';
import { Link, useLocation } from 'react-router-dom';
import { searchBookings, cancelMyBooking } from '../api/bookingService';
import type { Booking, BookingStatus } from '../types/booking';
import { ACTIVE_STATUSES } from '../types/booking';
import BookingCard from '../components/BookingCard/BookingCard';
import Modal from '../components/Modal';
import NoteForm from '../components/NoteForm';
import { getErrorMessage } from '../utils/format';

const CANCELLABLE: BookingStatus[] = ['Pending', 'Assigned', 'InProgress'];

const MyBookingsPage: React.FC = () => {
    const location = useLocation();
    const justBooked = (location.state as { justBooked?: string } | null)?.justBooked;

    const [view, setView] = useState<'upcoming' | 'past'>('upcoming');
    const [bookings, setBookings] = useState<Booking[] | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [toCancel, setToCancel] = useState<Booking | null>(null);
    const [cancelError, setCancelError] = useState<string | null>(null);

    const load = useCallback(async () => {
        try {
            setBookings(await searchBookings());
        } catch (err) {
            setError(getErrorMessage(err, 'Could not load your bookings.'));
        }
    }, []);

    useEffect(() => { void load(); }, [load]);

    const handleCancel = async (reason: string | null) => {
        if (!toCancel) return;
        try {
            await cancelMyBooking(toCancel.id, reason ?? '');
            setToCancel(null);
            await load();
        } catch (err) {
            setCancelError(getErrorMessage(err, 'Could not cancel the booking.'));
        }
    };

    const upcoming = (bookings ?? []).filter(b => ACTIVE_STATUSES.includes(b.status))
        .sort((a, b) => a.slotStart.localeCompare(b.slotStart));
    const past = (bookings ?? []).filter(b => !ACTIVE_STATUSES.includes(b.status))
        .sort((a, b) => b.slotStart.localeCompare(a.slotStart));
    const shown = view === 'upcoming' ? upcoming : past;

    return (
        <div className="container page">
            <div className="page-header">
                <div>
                    <h1>My bookings</h1>
                    <p className="subtitle">Follow each service from booking to invoice. We email you at every step.</p>
                </div>
                <Link to="/book" className="btn btn-primary">Book a service</Link>
            </div>

            {justBooked && <p className="alert alert-success" style={{ marginBottom: 16 }}>Booked: {justBooked}. A confirmation email is on its way.</p>}
            {error && <p className="alert alert-error" style={{ marginBottom: 16 }}>{error}</p>}

            <div className="tabs" style={{ marginBottom: 16 }}>
                <button className={view === 'upcoming' ? 'active' : ''} onClick={() => { setView('upcoming'); }}>Upcoming ({upcoming.length})</button>
                <button className={view === 'past' ? 'active' : ''} onClick={() => { setView('past'); }}>Past ({past.length})</button>
            </div>

            {bookings === null && !error && <p className="loading">Loading your bookings…</p>}
            {bookings && shown.length === 0 && (
                <p className="empty-state">
                    {view === 'upcoming' ? <>No upcoming bookings. <Link to="/book">Book a service</Link></> : 'No past bookings yet.'}
                </p>
            )}

            <div className="grid grid-2">
                {shown.map(b => (
                    <BookingCard key={b.id} booking={b}>
                        {CANCELLABLE.includes(b.status) && (
                            <button className="btn btn-sm btn-danger" onClick={() => { setToCancel(b); setCancelError(null); }}>Cancel booking</button>
                        )}
                    </BookingCard>
                ))}
            </div>

            {toCancel && (
                <Modal title="Cancel this booking?" onClose={() => { setToCancel(null); }}>
                    <NoteForm label="Why are you cancelling?" placeholder="e.g. I need to rebook for next week"
                              minLength={10} submitLabel="Cancel booking" danger
                              error={cancelError} onCancel={() => { setToCancel(null); }} onSubmit={handleCancel} />
                </Modal>
            )}
        </div>
    );
};

export default MyBookingsPage;
