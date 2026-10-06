import React from 'react';
import type {Booking} from '../../types/booking';
import './MechanicBookingCard.css';

interface MechanicBookingCardProps {
    booking: Booking;
    onStartJob: (bookingId: number) => void;
    onFinishJob?: (bookingId: number) => void; // For later
    isProcessing: boolean;
}

const MechanicBookingCard: React.FC<MechanicBookingCardProps> = ({
                                                                     booking,
                                                                     onStartJob,
                                                                     onFinishJob,
                                                                     isProcessing
                                                                 }) => {
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

    const canStartJob = booking.status === "Assigned";
    const canFinishJob = booking.status === "InProgress";

    return (
        <div className={`mechanic-booking-card status-${booking.status?.toLowerCase().replace(/\s+/g, '-')}`}>
            <div className="card-header">
                <span className={getStatusClass(booking.status)}>{booking.status}</span>
                <span className="booking-id">ID: {booking.id}</span>
            </div>

            <h4>{booking.serviceTypeName || 'N/A'}</h4>
            <p className="card-field">🗓️ <strong>Time:</strong> {formatDate(booking.slotStart)}</p>
            <p className="card-field">🚗 <strong>Vehicle:</strong> {booking.vehicleMake || 'N/A'} {booking.vehicleModel || 'N/A'} ({booking.vehicleYear || 'N/A'}) - [{booking.vehicleRegistrationNumber}]</p>
            <p className="card-field">👤 <strong>Client:</strong> {booking.customerFullName && booking.customerFullName !== "N/A" ? booking.customerFullName : `(ID: ${booking.customerId})`}</p>
            {booking.customerPhoneNumber && <p className="card-field">📞 {booking.customerPhoneNumber}</p>}

            <div className="card-actions">
                {canStartJob && (
                    <button onClick={() => onStartJob(booking.id)} className="start-job-btn" disabled={isProcessing}>
                        {isProcessing ? 'Starting...' : 'Start Job'}
                    </button>
                )}
                {canFinishJob && onFinishJob && ( // onFinishJob will be implemented later
                    <button onClick={() => onFinishJob(booking.id)} className="finish-job-btn" disabled={isProcessing}>
                        {isProcessing ? '...' : 'Finish Job'}
                    </button>
                )}
            </div>
        </div>
    );
};

export default MechanicBookingCard;