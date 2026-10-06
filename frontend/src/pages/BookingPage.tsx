// src/pages/BookingPage.tsx
import React, { useState, useEffect } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { getAvailability } from '../api/scheduleService';
import { getMyProfile } from '../api/userService';
import { createBooking } from '../api/bookingService';
import { getAllServiceTypes } from '../api/serviceTypeService';
import type {UserProfile} from '../types/user';
import type {ServiceType} from '../types/serviceType';
import type {CreateBookingPayload} from '../types/booking';
import './BookingPage.css';

interface GroupedSlot {
    date: string;
    times: { originalUtc: string; displayTime: string }[];
}

const BookingPage: React.FC = () => {
    const navigate = useNavigate();
    const today = new Date().toISOString().split('T')[0];
    const oneWeekFromToday = new Date(Date.now() + 7 * 24 * 60 * 60 * 1000).toISOString().split('T')[0];

    // Date and Slot states
    const [startDate, setStartDate] = useState<string>(today);
    const [endDate, setEndDate] = useState<string>(oneWeekFromToday);
    const [groupedSlots, setGroupedSlots] = useState<GroupedSlot[]>([]);
    const [isLoadingSlots, setIsLoadingSlots] = useState<boolean>(false);
    const [slotsError, setSlotsError] = useState<string | null>(null);

    // User Profile and Vehicle states
    const [userProfile, setUserProfile] = useState<UserProfile | null>(null);
    const [isLoadingProfile, setIsLoadingProfile] = useState<boolean>(false);
    const [profileError, setProfileError] = useState<string | null>(null);

    // --- NEW: Service Types states ---
    const [serviceTypes, setServiceTypes] = useState<ServiceType[]>([]);
    const [isLoadingServiceTypes, setIsLoadingServiceTypes] = useState<boolean>(false);
    const [serviceTypesError, setServiceTypesError] = useState<string | null>(null);

    // Selections
    const [selectedSlot, setSelectedSlot] = useState<string | null>(null);
    const [selectedVehicleReg, setSelectedVehicleReg] = useState<string>('');
    const [selectedServiceTypeId, setSelectedServiceTypeId] = useState<string>(''); // Store as string for <select> value

    // Booking submission states
    const [isSubmittingBooking, setIsSubmittingBooking] = useState<boolean>(false);
    const [bookingSubmitError, setBookingSubmitError] = useState<string | null>(null);

    // Fetch user profile (for vehicles) and Service Types on component mount
    useEffect(() => {
        const loadInitialData = async () => {
            setIsLoadingProfile(true);
            setIsLoadingServiceTypes(true);
            try {
                const profileData = await getMyProfile();
                setUserProfile(profileData);
                if (profileData.vehicles && profileData.vehicles.length > 0) {
                    setSelectedVehicleReg(profileData.vehicles[0].registrationNumber);
                }
            } catch (err) {
                setProfileError('Failed to load your vehicles. Please try again.');
                console.error("Error fetching profile:", err);
            } finally {
                setIsLoadingProfile(false);
            }

            try {
                const types = await getAllServiceTypes();
                setServiceTypes(types);
            } catch (err) {
                setServiceTypesError('Failed to load service types. Please try again.');
                console.error("Error fetching service types:", err);
            } finally {
                setIsLoadingServiceTypes(false);
            }
        };
        loadInitialData();
    }, []);

    const groupAndFormatSlots = (slots: string[]): GroupedSlot[] => {
        // ... (groupAndFormatSlots function - no change)
        const grouped: { [key: string]: { originalUtc: string; displayTime: string }[] } = {};
        slots.forEach(utcSlot => {
            const localDate = new Date(utcSlot);
            const dateKey = localDate.toLocaleDateString(undefined, { year: 'numeric', month: 'long', day: 'numeric' });
            const timeKey = localDate.toLocaleTimeString(undefined, { hour: '2-digit', minute: '2-digit', hour12: true });
            if (!grouped[dateKey]) {
                grouped[dateKey] = [];
            }
            grouped[dateKey].push({ originalUtc: utcSlot, displayTime: timeKey });
        });
        Object.keys(grouped).forEach(dateKey => {
            grouped[dateKey].sort((a, b) => new Date(a.originalUtc).getTime() - new Date(b.originalUtc).getTime());
        });
        return Object.entries(grouped)
            .map(([date, times]) => ({ date, times }))
            .sort((a, b) => new Date(a.times[0].originalUtc).getTime() - new Date(b.times[0].originalUtc).getTime());
    };

    const handleFetchAvailability = async () => {
        // ... (handleFetchAvailability function - no change)
        if (!startDate || !endDate) {
            setSlotsError('Please select both start and end dates.');
            return;
        }
        if (new Date(startDate) > new Date(endDate)) {
            setSlotsError('Start date cannot be after end date.');
            return;
        }
        setIsLoadingSlots(true);
        setSlotsError(null);
        setSelectedSlot(null);
        setGroupedSlots([]);
        try {
            const rawSlots = await getAvailability(startDate, endDate);
            if (rawSlots.length === 0) {
                setSlotsError('No available slots found for the selected period.');
            } else {
                setGroupedSlots(groupAndFormatSlots(rawSlots));
            }
        } catch (err: any) {
            const message = err.response?.data || err.message || 'Failed to fetch availability.';
            setSlotsError(message);
        } finally {
            setIsLoadingSlots(false);
        }
    };

    const handleBookingSubmit = async () => {
        if (!selectedSlot || !selectedVehicleReg || !selectedServiceTypeId) {
            setBookingSubmitError('Please ensure you have selected a time slot, vehicle, and service type.');
            return;
        }

        // Create the payload matching the cURL request and updated CreateBookingPayload type
        const bookingPayload: CreateBookingPayload = {
            vehicleRegistrationNumber: selectedVehicleReg,
            serviceTypeId: parseInt(selectedServiceTypeId),
            slotStart: selectedSlot,
        };

        setIsSubmittingBooking(true);
        setBookingSubmitError(null);

        try {
            const newBooking = await createBooking(bookingPayload); // createBooking from bookingService
            console.log('Booking Created Successfully from Frontend:', newBooking);
            alert(`Booking successfully created! Your booking ID is ${newBooking.id}.`);
            navigate('/my-bookings');

        } catch (err: any) {
            let message = 'Failed to create booking. Please try again.';
            if (err.response) {
                if (err.response.status === 409) { // Example: SlotUnavailableException or similar conflict
                    message = err.response.data?.message || 'The selected slot is no longer available or conflicts with another booking.';
                } else if (err.response.data?.errors) { // ASP.NET Core validation errors
                    message = Object.values(err.response.data.errors).flat().join(' ');
                }
                else {
                    message = err.response.data?.message || err.response.data?.title || message;
                }
            } else {
                message = err.message || message;
            }
            setBookingSubmitError(message);
            console.error('Error creating booking from frontend:', err.response || err);
        } finally {
            setIsSubmittingBooking(false);
        }
    };

    return (
        <div className="booking-page-container">
            <h1>Book a Service Appointment</h1>

            {/* Date range picker section ... no change */}
            <div className="date-range-picker">
                <div className="form-group">
                    <label htmlFor="start-date">Start Date:</label>
                    <input type="date" id="start-date" value={startDate} min={today} onChange={(e) => setStartDate(e.target.value)} />
                </div>
                <div className="form-group">
                    <label htmlFor="end-date">End Date:</label>
                    <input type="date" id="end-date" value={endDate} min={startDate || today} onChange={(e) => setEndDate(e.target.value)} />
                </div>
                <button onClick={handleFetchAvailability} disabled={isLoadingSlots || isLoadingProfile || isLoadingServiceTypes} className="fetch-slots-btn">
                    {isLoadingSlots ? 'Fetching Slots...' : 'Find Slots'}
                </button>
            </div>

            {slotsError && <p className="error-message-slots">{slotsError}</p>}
            {profileError && <p className="error-message-slots">{profileError}</p>}
            {serviceTypesError && <p className="error-message-slots">{serviceTypesError}</p>}


            {/* Slots display section ... no change in structure */}
            {groupedSlots.length > 0 && !selectedSlot && (
                <div className="slots-selection-area">
                    <h2>Select an Available Slot:</h2>
                    {groupedSlots.map(group => (
                        <div key={group.date} className="slot-date-group">
                            <h3>{group.date}</h3>
                            <div className="time-slots-grid">
                                {group.times.map(timeSlot => (
                                    <button
                                        key={timeSlot.originalUtc}
                                        onClick={() => { setSelectedSlot(timeSlot.originalUtc); setBookingSubmitError(null); }}
                                        className={`time-slot-btn ${selectedSlot === timeSlot.originalUtc ? 'selected' : ''}`}
                                        disabled={isLoadingSlots}
                                    >
                                        {timeSlot.displayTime}
                                    </button>
                                ))}
                            </div>
                        </div>
                    ))}
                </div>
            )}


            {selectedSlot && (
                <div className="booking-details-section">
                    <h2>Confirm Your Booking</h2>
                    <p><strong>Selected Slot:</strong> {new Date(selectedSlot).toLocaleString()}</p>

                    {/* Vehicle Selection Dropdown ... no change in structure */}
                    <div className="form-group">
                        <label htmlFor="vehicle-select">Select Your Vehicle:</label>
                        {isLoadingProfile && <p>Loading your vehicles...</p>}
                        {!isLoadingProfile && userProfile && userProfile.vehicles.length > 0 ? (
                            <select
                                id="vehicle-select"
                                value={selectedVehicleReg}
                                onChange={(e) => setSelectedVehicleReg(e.target.value)}
                                disabled={isSubmittingBooking}
                            >
                                {userProfile.vehicles.map(v => (
                                    <option key={v.registrationNumber} value={v.registrationNumber}>
                                        {v.make} {v.model} ({v.registrationNumber})
                                    </option>
                                ))}
                            </select>
                        ) : (
                            !isLoadingProfile && <p>No vehicles found. <Link to="/profile">Add a vehicle to your profile first.</Link></p>
                        )}
                    </div>

                    {/* --- UPDATED: Service Type Selection Dropdown --- */}
                    <div className="form-group">
                        <label htmlFor="service-type-select">Select Service Type:</label>
                        {isLoadingServiceTypes && <p>Loading service types...</p>}
                        {!isLoadingServiceTypes && serviceTypes.length > 0 ? (
                            <select
                                id="service-type-select"
                                value={selectedServiceTypeId}
                                onChange={(e) => setSelectedServiceTypeId(e.target.value)}
                                disabled={isSubmittingBooking}
                            >
                                <option value="" disabled>-- Select a service --</option>
                                {serviceTypes.map(st => (
                                    <option key={st.id} value={st.id.toString()}>
                                        {st.name} (approx. {st.durationHours} hrs, £{st.price.toFixed(2)})
                                    </option>
                                ))}
                            </select>
                        ) : (
                            !isLoadingServiceTypes && <p>No service types available at the moment.</p>
                        )}
                    </div>

                    {bookingSubmitError && <p className="error-message-slots" style={{textAlign: 'center'}}>{bookingSubmitError}</p>}

                    <button
                        onClick={handleBookingSubmit}
                        className="submit-booking-btn"
                        disabled={!selectedVehicleReg || !selectedServiceTypeId || isSubmittingBooking || isLoadingProfile || isLoadingServiceTypes}
                    >
                        {isSubmittingBooking ? 'Submitting...' : 'Confirm Booking'}
                    </button>
                </div>
            )}
        </div>
    );
};

export default BookingPage;