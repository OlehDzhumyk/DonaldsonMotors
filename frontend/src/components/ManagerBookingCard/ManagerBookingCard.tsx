import React from 'react';
import type { Booking } from '../../types/booking';
import './ManagerBookingCard.css';

interface ManagerBookingCardProps {
    booking: Booking;
    onAssignMechanic: (booking: Booking) => void;
    onCancelBooking: (bookingId: number) => void;
    onMarkAsPaid: (bookingId: number) => void;
    isCardActionLoading?: boolean;
}

const ManagerBookingCard: React.FC<ManagerBookingCardProps> = ({
                                                                   booking,
                                                                   onAssignMechanic,
                                                                   onCancelBooking,
                                                                   onMarkAsPaid,
                                                                   isCardActionLoading // Зчитуємо новий проп
                                                               }) => {
    const canAssign = (booking.status === "Pending" || booking.status === "Booked");
    const canReassign = booking.mechanicId && (booking.status === "Assigned" || booking.status === "Pending" || booking.status === "Booked");

    const isCancellableByManager = !["Completed", "Paid", "Cancelled", "Archived"].includes(booking.status);
    const canBeMarkedPaid = booking.status === "WorkCompleted" || booking.status === "AwaitingPayment";

    const getStatusClass = (status: string | undefined) => {
        return `status-text status-${status?.toLowerCase().replace(/\s+/g, '-') || 'unknown'}`;
    };

    const formatDate = (dateString: string | undefined) => {
        if (!dateString) return 'N/A';
        return new Date(dateString).toLocaleString([], {
            year: 'numeric', month: 'short', day: 'numeric',
            hour: '2-digit', minute: '2-digit'
        });
    };

    return (
        <div className={`manager-booking-card status-${booking.status?.toLowerCase().replace(/\s+/g, '-')}`}>
            <div className="card-header">
                <span className={getStatusClass(booking.status)}>{booking.status}</span>
                <span className="booking-id">ID: {booking.id}</span>
            </div>

            <h4>{booking.serviceTypeName || 'Service Not Specified'}</h4>
            <p className="card-field">🗓️ <strong>Time:</strong> {formatDate(booking.slotStart)}</p>
            <p className="card-field">🚗 <strong>Vehicle:</strong> {booking.vehicleMake || 'N/A'} {booking.vehicleModel || 'N/A'} ({booking.vehicleYear || 'N/A'}) - [{booking.vehicleRegistrationNumber}]</p>
            <p className="card-field">👤 <strong>Client:</strong> {booking.customerFullName && booking.customerFullName !== "N/A" ? booking.customerFullName : `(ID: ${booking.customerId})`}</p>
            {booking.customerPhoneNumber && <p className="card-field">📞 <strong>Phone:</strong> {booking.customerPhoneNumber}</p>}
            {booking.customerEmail && <p className="card-field">📧 <strong>Email:</strong> {booking.customerEmail}</p>}

            {booking.mechanicName ? (
                <p className="card-field">🛠️ <strong>Mechanic:</strong> {booking.mechanicName} {booking.mechanicId ? `(ID: ${booking.mechanicId})` : ''}</p>
            ) : (
                (booking.status === "Pending" || booking.status === "Booked") &&
                <p className="card-field">🛠️ <strong>Mechanic:</strong> <span style={{color: 'orange', fontWeight: 'bold'}}>Needed</span></p>
            )}
            <p className="card-field">💰 <strong>Price:</strong> £{(booking.serviceTypePrice || 0).toFixed(2)} | ⏱️ ~{booking.serviceDurationHours} hr(s)</p>
            {booking.notes && <p className="card-field" style={{fontStyle: 'italic'}}>📝 <strong>Notes:</strong> {booking.notes}</p>}


            <div className="card-actions">
                {(canAssign || canReassign) && (
                    <button
                        onClick={() => onAssignMechanic(booking)}
                        className="assign-btn"
                        disabled={typeof booking.serviceTypeId !== 'number' || isCardActionLoading}
                    >
                        {isCardActionLoading && booking.mechanicId ? 'Re-assigning...' :
                            isCardActionLoading && !booking.mechanicId ? 'Assigning...' :
                                booking.mechanicId ? 'Re-assign Mechanic' : 'Assign Mechanic'}
                    </button>
                )}
                {isCancellableByManager && (
                    <button
                        onClick={() => onCancelBooking(booking.id)}
                        className="cancel-btn-manager"
                        disabled={isCardActionLoading}
                    >
                        {isCardActionLoading ? '...' : 'Cancel Booking'}
                    </button>
                )}
                {canBeMarkedPaid && (
                    <button
                        onClick={() => onMarkAsPaid(booking.id)}
                        className="mark-paid-btn"
                        disabled={isCardActionLoading}
                    >
                        {isCardActionLoading ? '...' : 'Mark as Paid'}
                    </button>
                )}
            </div>
        </div>
    );
};

export default ManagerBookingCard;