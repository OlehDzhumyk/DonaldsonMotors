import React from 'react';
import type {Booking} from '../../types/booking';
import ManagerBookingCard from '../ManagerBookingCard/ManagerBookingCard';

interface BookingColumnProps {
    title: string;
    bookings: Booking[];
    onAssignMechanic: (booking: Booking) => void;
    onCancelBooking: (bookingId: number) => void;
    onMarkAsPaid: (bookingId: number) => void;
    isLoading?: boolean;
}

const BookingColumn: React.FC<BookingColumnProps> = ({
                                                         title,
                                                         bookings,
                                                         onAssignMechanic,
                                                         onCancelBooking,
                                                         onMarkAsPaid,
                                                         isLoading
                                                     }) => {
    return (
        <div className="dashboard-column"> {/* Ensure this class exists in your CSS */}
            <h2>{title} ({bookings.length})</h2>
            {isLoading && <p>Loading...</p>}
            {!isLoading && bookings.length === 0 ? (
                <p>No bookings in this category.</p>
            ) : (
                <div className="column-bookings-list"> {/* Ensure this class exists */}
                    {bookings.map(booking => (
                        <ManagerBookingCard
                            key={booking.id}
                            booking={booking}
                            onAssignMechanic={onAssignMechanic}
                            onCancelBooking={onCancelBooking}
                            onMarkAsPaid={onMarkAsPaid}
                        />
                    ))}
                </div>
            )}
        </div>
    );
};

export default BookingColumn;