import React from 'react';
import type { Booking } from '../../types/booking';
import StatusBadge from '../StatusBadge';
import { formatDateTime, formatHours, formatMoney } from '../../utils/format';
import './BookingCard.css';

interface BookingCardProps {
    booking: Booking;
    /** Show who the customer is (staff views). */
    showCustomer?: boolean;
    children?: React.ReactNode;
}

/** Compact summary of one booking; action buttons are passed as children. */
const BookingCard: React.FC<BookingCardProps> = ({ booking, showCustomer = false, children }) => (
    <article className="card booking-card">
        <div className="booking-card-top">
            <div>
                <h3>{booking.serviceTypeName}</h3>
                <p className="muted small">{formatDateTime(booking.slotStart)} · {formatHours(booking.serviceDurationHours)}</p>
            </div>
            <StatusBadge status={booking.status} />
        </div>

        <dl className="meta booking-card-meta">
            <dt>Vehicle</dt>
            <dd><span className="plate">{booking.vehicleRegistrationNumber}</span> {booking.vehicleMake} {booking.vehicleModel}</dd>
            {showCustomer && (
                <>
                    <dt>Customer</dt>
                    <dd>
                        {booking.customerFullName}
                        {booking.customerPhoneNumber && <span className="muted"> · {booking.customerPhoneNumber}</span>}
                    </dd>
                </>
            )}
            <dt>Mechanic</dt>
            <dd>{booking.mechanicName ?? <span className="muted">Not assigned yet</span>}</dd>
            <dt>Price</dt>
            <dd>{formatMoney(booking.serviceTypePrice)} <span className="muted small">+ parts</span></dd>
        </dl>

        {children && <div className="booking-card-actions">{children}</div>}
    </article>
);

export default BookingCard;
