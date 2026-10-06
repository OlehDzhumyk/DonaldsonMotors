// src/pages/MyBookingsPage.tsx
import React, { useState, useEffect, useCallback, useMemo } from 'react';
import { searchBookings /*, cancelBookingByCustomer */ } from '../api/bookingService';
import type {Booking} from '../types/booking';
import './MyBookingsPage.css';
import { Link } from 'react-router-dom';

type ViewMode = 'active' | 'history';

// Define sets of statuses for filtering (can be same as manager's or simplified for customer)
const CUSTOMER_ACTIVE_STATUSES: string[] = ['Pending', 'Booked', 'Assigned', 'InProgress', 'AwaitingPayment', 'WorkCompleted'];
const CUSTOMER_HISTORICAL_STATUSES: string[] = ['Paid', 'Completed', 'Cancelled', 'CancelledByCustomer', 'CancelledByManager', 'Archived'];


const MyBookingsPage: React.FC = () => {
    const [allBookings, setAllBookings] = useState<Booking[]>([]);
    const [isLoading, setIsLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);
    // const [cancellingId, setCancellingId] = useState<number | null>(null); // REMOVE

    const [viewMode, setViewMode] = useState<ViewMode>('active');

    const fetchMyBookings = useCallback(async () => {
        setIsLoading(true);
        try {
            // For customer's "My Bookings", searchBookings is called without params.
            // Backend scopes results to the authenticated customer.
            const data = await searchBookings();
            data.sort((a, b) =>
                viewMode === 'active'
                    ? new Date(a.slotStart).getTime() - new Date(b.slotStart).getTime() // Active: oldest first
                    : new Date(b.slotStart).getTime() - new Date(a.slotStart).getTime() // History: newest first
            );
            setAllBookings(data);
            setError(null);
        } catch (err: any) {
            setError(err.response?.data?.message || err.message || 'Failed to fetch your bookings.');
            setAllBookings([]);
        } finally {
            setIsLoading(false);
        }
    }, [viewMode]);

    useEffect(() => {
        fetchMyBookings();
    }, [fetchMyBookings]); // Refetch if viewMode changes (due to sort order)

    // Filter bookings based on viewMode
    const bookingsToDisplay = useMemo(() => {
        if (viewMode === 'active') {
            return allBookings.filter(b => CUSTOMER_ACTIVE_STATUSES.includes(b.status));
        } else { // history
            return allBookings.filter(b => CUSTOMER_HISTORICAL_STATUSES.includes(b.status) ||
                (new Date(b.slotStart) < new Date() && !CUSTOMER_ACTIVE_STATUSES.includes(b.status)));
        }
    }, [allBookings, viewMode]);


    if (isLoading && allBookings.length === 0) {
        return <div className="loading-message">Loading your bookings...</div>;
    }

    if (error && allBookings.length === 0) {
        return <div className="error-message-bookings">{error}</div>;
    }

    return (
        <div className="my-bookings-page-container">
            <h1>My Bookings</h1>

            <div className="view-mode-toggle">
                <button
                    onClick={() => setViewMode('active')}
                    className={viewMode === 'active' ? 'active' : ''}
                    disabled={isLoading}
                >
                    Active/Upcoming
                </button>
                <button
                    onClick={() => setViewMode('history')}
                    className={viewMode === 'history' ? 'active' : ''}
                    disabled={isLoading}
                >
                    History
                </button>
            </div>

            {error && <p className="error-message-bookings" style={{marginBottom: '15px', textAlign:'center'}}>{error}</p>}
            {isLoading && <div className="loading-message">Refreshing bookings...</div>}


            {!isLoading && bookingsToDisplay.length === 0 ? (
                <p>
                    {viewMode === 'active'
                        ? <>You have no active or upcoming bookings. <Link to="/book">Book a service now!</Link></>
                        : 'You have no booking history yet.'
                    }
                </p>
            ) : (
                <div className="bookings-grid">
                    {bookingsToDisplay.map(booking => (
                        <div key={booking.id} className="booking-card"> {/* Customer specific card view */}
                            <h3>{booking.serviceTypeName}</h3>
                            <p><strong>Date & Time:</strong> {new Date(booking.slotStart).toLocaleString()}</p>
                            <p>
                                <strong>Status:</strong>
                                <span className={`status-badge status-${booking.status?.toLowerCase().replace(/\s+/g, '-') || 'unknown'}`}>
                    {booking.status}
                </span>
                            </p>
                            <p><strong>Vehicle:</strong> {booking.vehicleMake} {booking.vehicleModel} ({booking.vehicleRegistrationNumber})</p>
                            {booking.mechanicName && <p><strong>Assigned Mechanic:</strong> {booking.mechanicName}</p>}
                            <p><strong>Service Price:</strong> £{(booking.serviceTypePrice || 0).toFixed(2)}</p>
                            {booking.totalCost && booking.totalCost !== booking.serviceTypePrice &&
                                <p><strong>Total Cost:</strong> £{booking.totalCost.toFixed(2)}</p>
                            }
                            {/* NO CANCEL BUTTON FOR CUSTOMER */}
                        </div>
                    ))}
                </div>
            )}
        </div>
    );
};

export default MyBookingsPage;