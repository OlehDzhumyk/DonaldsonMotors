import { describe, expect, it } from 'vitest';
import { render, screen } from '@testing-library/react';
import BookingCard from './BookingCard';
import type { Booking } from '../../types/booking';

const booking: Booking = {
    id: 7, slotStart: '2030-01-07T09:00:00Z', status: 'AwaitingPayment',
    serviceTypeId: 1, serviceTypeName: 'Brake Pad Replacement', serviceTypePrice: 80, serviceDurationHours: 2,
    vehicleRegistrationNumber: 'SG21ABC', vehicleMake: 'Ford', vehicleModel: 'Focus', vehicleYear: 2021,
    customerId: 3, customerFullName: 'Jamie Customer', customerPhoneNumber: '07700 900123', customerEmail: 'jamie@example.com',
    mechanicId: null, mechanicName: null,
};

describe('BookingCard', () => {
    it('shows the service, plate, status and price', () => {
        render(<BookingCard booking={booking} />);

        expect(screen.getByRole('heading', { name: 'Brake Pad Replacement' })).toBeInTheDocument();
        expect(screen.getByText('SG21ABC')).toBeInTheDocument();
        expect(screen.getByText('Awaiting payment')).toBeInTheDocument();
        expect(screen.getByText('£80.00')).toBeInTheDocument();
        expect(screen.getByText('Not assigned yet')).toBeInTheDocument();
    });

    it('only shows the customer to staff', () => {
        const { rerender } = render(<BookingCard booking={booking} />);
        expect(screen.queryByText('Jamie Customer')).not.toBeInTheDocument();

        rerender(<BookingCard booking={booking} showCustomer />);
        expect(screen.getByText('Jamie Customer')).toBeInTheDocument();
    });
});
