// src/pages/ManagerDashboardPage.tsx
import React, { useState, useEffect, useCallback, useMemo } from 'react';
import {
    searchBookings,
    assignMechanicToBooking,
    adminCancelBooking, // <-- NEW
    markBookingAsPaid   // <-- NEW
} from '../api/bookingService';
import { getAvailableMechanicsForSlot } from '../api/scheduleService';
import type {
    Booking,
    AssignMechanicPayload,
    BookingSearchParameters,
} from '../types/booking';
import type {MechanicAvailability} from '../types/schedule';
import AssignMechanicModal from '../components/AssignMechanicModal/AssignMechanicModal';
import ManagerBookingCard from "../components/ManagerBookingCard/ManagerBookingCard";
import './ManagerDashboardPage.css';

// Updated Status constants based on your enum
const STATUS_COLUMN_NEEDS_ACTION: string[] = ["Pending"];
const STATUS_COLUMN_SCHEDULED_IN_PROGRESS: string[] = ["Assigned", "InProgress"];
const STATUS_COLUMN_AWAITING_PAYMENT: string[] = ["AwaitingPayment"];
const STATUS_COLUMN_HISTORY_COMPLETED_PAID: string[] = ["Paid"];
const STATUS_COLUMN_HISTORY_CANCELLED: string[] = ["Cancelled"];
const STATUS_COLUMN_HISTORY_ARCHIVED: string[] = ["Archived"];

const ALL_AVAILABLE_STATUSES_FOR_FILTER = [
    "Pending", "Assigned", "InProgress",
    "AwaitingPayment", "Paid", "Cancelled", "Archived"
];

type ManagerViewMode = 'active' | 'history';

const ManagerDashboardPage: React.FC = () => {
    const [allBookings, setAllBookings] = useState<Booking[]>([]);
    const [isLoading, setIsLoading] = useState<boolean>(true); // Combined loading for initial and search
    const [error, setError] = useState<string | null>(null);
    const [viewMode, setViewMode] = useState<ManagerViewMode>('active');

    // Filter States - these hold the current values of the input fields
    const [currentSearchVehicleReg, setCurrentSearchVehicleReg] = useState<string>('');
    const [currentSearchStatus, setCurrentSearchStatus] = useState<string>('');
    const [currentSearchCustomerId, setCurrentSearchCustomerId] = useState<string>('');
    const [currentSearchDateFrom, setCurrentSearchDateFrom] = useState<string>('');
    const [currentSearchDateTo, setCurrentSearchDateTo] = useState<string>('');

    // Modal & Action States
    const [selectedBookingForModal, setSelectedBookingForModal] = useState<Booking | null>(null);
    const [showAssignModal, setShowAssignModal] = useState<boolean>(false);
    const [availableMechanics, setAvailableMechanics] = useState<MechanicAvailability[]>([]);
    const [isLoadingMechanics, setIsLoadingMechanics] = useState<boolean>(false);
    const [selectedMechanicId, setSelectedMechanicId] = useState<string>('');
    const [assignmentError, setAssignmentError] = useState<string | null>(null);
    const [processingBookingId, setProcessingBookingId] = useState<number | null>(null);

    // Function to fetch/search bookings
    const executeFetchOrSearch = useCallback(async (params: BookingSearchParameters = {}) => {
        setIsLoading(true);
        setError(null);
        try {
            console.log("Searching with params:", params); // For debugging
            const data = await searchBookings(params);
            data.sort((a, b) =>
                viewMode === 'active'
                    ? new Date(a.slotStart).getTime() - new Date(b.slotStart).getTime() // Active: oldest first
                    : new Date(b.slotStart).getTime() - new Date(a.slotStart).getTime() // History: newest first
            );
            setAllBookings(data);
        } catch (err: any) {
            setError(err.response?.data?.message || err.message || 'Failed to fetch bookings.');
            setAllBookings([]);
        } finally {
            setIsLoading(false);
        }
    }, [viewMode]); // Re-sort if viewMode changes

    useEffect(() => {
        executeFetchOrSearch(); // Initial load with no filters
    }, [executeFetchOrSearch]); // Depends on executeFetchOrSearch which depends on viewMode

    const handleSearchClick = () => {
        const params: BookingSearchParameters = {};
        if (currentSearchVehicleReg.trim()) params.vehicleRegistrationNumber = currentSearchVehicleReg.trim();
        if (currentSearchStatus) params.status = currentSearchStatus;
        if (currentSearchCustomerId.trim()) {
            const custId = parseInt(currentSearchCustomerId.trim());
            if (!isNaN(custId) && custId > 0) params.customerId = custId;
            else if (currentSearchCustomerId.trim()) {
                setError("Customer ID must be a valid positive number for searching."); return;
            }
        }
        if (currentSearchDateFrom) params.dateFrom = currentSearchDateFrom;
        if (currentSearchDateTo) params.dateTo = currentSearchDateTo;

        executeFetchOrSearch(params);
    };

    const handleClearFilters = () => {
        setCurrentSearchVehicleReg('');
        setCurrentSearchStatus('');
        setCurrentSearchCustomerId('');
        setCurrentSearchDateFrom('');
        setCurrentSearchDateTo('');
        executeFetchOrSearch(); // Fetch all bookings (no filters)
    };

    // Memoized filtered lists for columns
    const needsActionBookings = useMemo(() => allBookings.filter(b => STATUS_COLUMN_NEEDS_ACTION.includes(b.status)), [allBookings]);
    const scheduledInProgressBookings = useMemo(() => allBookings.filter(b => STATUS_COLUMN_SCHEDULED_IN_PROGRESS.includes(b.status)), [allBookings]);
    const awaitingPaymentBookings = useMemo(() => allBookings.filter(b => STATUS_COLUMN_AWAITING_PAYMENT.includes(b.status)), [allBookings]);
    const historyCompletedBookings = useMemo(() => allBookings.filter(b => STATUS_COLUMN_HISTORY_COMPLETED_PAID.includes(b.status)), [allBookings]);
    const historyCancelledBookings = useMemo(() => allBookings.filter(b => STATUS_COLUMN_HISTORY_CANCELLED.includes(b.status)), [allBookings]);
    const historyArchivedBookings = useMemo(() => allBookings.filter(b => STATUS_COLUMN_HISTORY_ARCHIVED.includes(b.status)), [allBookings]);

    // --- Handler functions for card actions ---
    const handleOpenAssignModal = async (booking: Booking) => {

        setSelectedBookingForModal(booking);
        setShowAssignModal(true);
        setAssignmentError(null); setIsLoadingMechanics(true); setAvailableMechanics([]); setSelectedMechanicId('');
        try {
            const mechanics = await getAvailableMechanicsForSlot(booking.slotStart, booking.serviceTypeId);
            setAvailableMechanics(mechanics);
            if (mechanics.length > 0) setSelectedMechanicId(mechanics[0].mechanicId.toString());
            else setAssignmentError("No mechanics available for this slot/service type.");
        } catch (err: any) {
            setAssignmentError(err.response?.data?.message || err.message || "Failed to fetch available mechanics.");
        } finally { setIsLoadingMechanics(false); }
    };

    const handleConfirmAssignment = async () => {
        if (!selectedBookingForModal || !selectedMechanicId) { /* ... */ return; }
        setProcessingBookingId(selectedBookingForModal.id);
        setAssignmentError(null);
        const payload: AssignMechanicPayload = { bookingId: selectedBookingForModal.id, mechanicId: parseInt(selectedMechanicId) };
        try {
            await assignMechanicToBooking(payload);
            setShowAssignModal(false); setSelectedBookingForModal(null);
            await executeFetchOrSearch({ // Re-fetch with current filters to see update
                vehicleRegistrationNumber: currentSearchVehicleReg.trim() || undefined,
                status: currentSearchStatus || undefined,
                customerId: currentSearchCustomerId.trim() ? parseInt(currentSearchCustomerId.trim()) : undefined,
                dateFrom: currentSearchDateFrom || undefined,
                dateTo: currentSearchDateTo || undefined,
            });
            alert("Mechanic assigned successfully!");
        } catch (err: any) {
            setAssignmentError(err.response?.data?.message || err.message || "Failed to assign mechanic.");
        } finally { setProcessingBookingId(null); }
    };

    const handleManagerCancelAction = async (bookingId: number) => {
        const reason = prompt("Enter reason for cancellation (required):");
        if (reason === null) return; // User cancelled prompt
        if (!reason.trim()) {
            alert("Cancellation reason cannot be empty.");
            return;
        }
        setProcessingBookingId(bookingId);
        setError(null);
        try {
            await adminCancelBooking(bookingId, { reason });
            await executeFetchOrSearch({ /* current search params */ });
            alert("Booking cancelled successfully by manager.");
        } catch (err: any) {
            setError(err.response?.data?.message || err.message || `Failed to cancel booking ${bookingId}.`);
        } finally {
            setProcessingBookingId(null);
        }
    };

    const handleMarkAsPaidAction = async (bookingId: number) => {
        const paymentNotes = prompt("Enter payment notes (optional):");
        if (paymentNotes === null) return; // User cancelled prompt

        setProcessingBookingId(bookingId);
        setError(null);
        try {
            await markBookingAsPaid(bookingId, { paymentNotes: paymentNotes.trim() || undefined });
            await executeFetchOrSearch({ /* current search params */ });
            alert('Booking marked as paid successfully.');
        } catch (err: any) {
            setError(err.response?.data?.message || err.message || `Failed to mark booking ${bookingId} as paid.`);
        } finally {
            setProcessingBookingId(null);
        }
    };

    const renderBookingColumn = (title: string, bookingsForColumn: Booking[]) => (
        <div className="dashboard-column">
            <h2>{title} ({bookingsForColumn.length})</h2>
            {isLoading && bookingsForColumn.length === 0 && !allBookings.length && <p>Loading...</p> }
            {!isLoading && bookingsForColumn.length === 0 && <p>No bookings here.</p>}
            <div className="column-bookings-list">
                {bookingsForColumn.map(booking => (
                    <ManagerBookingCard
                        key={booking.id}
                        booking={booking}
                        onAssignMechanic={handleOpenAssignModal}
                        onCancelBooking={handleManagerCancelAction} // Updated handler
                        onMarkAsPaid={handleMarkAsPaidAction}       // Updated handler
                        isCardActionLoading={processingBookingId === booking.id}
                    />
                ))}
            </div>
        </div>
    );

    return (
        <div className="manager-dashboard-container">
            <h1>Manager Dashboard</h1>
            <div className="search-filter-section">
                <input type="text" placeholder="Vehicle Reg..." value={currentSearchVehicleReg} onChange={(e) => setCurrentSearchVehicleReg(e.target.value)} className="search-input"/>
                <input type="text" placeholder="Customer ID..." value={currentSearchCustomerId} onChange={(e) => setCurrentSearchCustomerId(e.target.value)} className="search-input"/>
                <input type="date" value={currentSearchDateFrom} onChange={(e) => setCurrentSearchDateFrom(e.target.value)} className="search-input" aria-label="Date From"/>
                <input type="date" value={currentSearchDateTo} onChange={(e) => setCurrentSearchDateTo(e.target.value)} className="search-input" aria-label="Date To"/>
                <select value={currentSearchStatus} onChange={(e) => setCurrentSearchStatus(e.target.value)} className="filter-select">
                    <option value="">All Statuses</option>
                    {ALL_AVAILABLE_STATUSES_FOR_FILTER.map(status => (<option key={status} value={status}>{status}</option>))}
                </select>
                <button onClick={handleSearchClick} className="search-button" disabled={isLoading}>
                    {isLoading ? 'Searching...' : 'Search'}
                </button>
                <button onClick={handleClearFilters} className="clear-button" disabled={isLoading}>
                    Clear Filters
                </button>
            </div>

            {error && <p className="error-message-slots" style={{textAlign: 'center'}}>{error}</p>}

            <div className="view-mode-toggle">
                {/* ... view mode buttons ... */}
                <button onClick={() => setViewMode('active')} className={viewMode === 'active' ? 'active' : ''} disabled={isLoading}>Active Bookings</button>
                <button onClick={() => setViewMode('history')} className={viewMode === 'history' ? 'active' : ''} disabled={isLoading}>Booking History</button>
            </div>

            {isLoading && <div className="loading-message">Loading...</div>}

            {!isLoading && viewMode === 'active' && (
                <div className="dashboard-columns">
                    {renderBookingColumn("Needs Action (Pending)", needsActionBookings)}
                    {renderBookingColumn("Scheduled / In Progress", scheduledInProgressBookings)}
                    {renderBookingColumn("Awaiting Payment", awaitingPaymentBookings)}
                </div>
            )}
            {!isLoading && viewMode === 'history' && (
                <div className="dashboard-columns">
                    {renderBookingColumn("Completed / Paid", historyCompletedBookings)}
                    {renderBookingColumn("Cancelled", historyCancelledBookings)}
                    {renderBookingColumn("Archived", historyArchivedBookings)}
                </div>
            )}

            {!isLoading && allBookings.length === 0 && !error && <p>No bookings match your current filters or criteria.</p>}

            <AssignMechanicModal
                isOpen={showAssignModal}
                onClose={() => { setShowAssignModal(false); setSelectedBookingForModal(null); }}
                booking={selectedBookingForModal}
                availableMechanics={availableMechanics}
                isLoadingMechanics={isLoadingMechanics}
                selectedMechanicId={selectedMechanicId}
                onSelectMechanicId={setSelectedMechanicId}
                onConfirmAssignment={handleConfirmAssignment}
                assignmentError={assignmentError}
                isAssigning={processingBookingId === selectedBookingForModal?.id && isLoadingMechanics}
            />
        </div>
    );
};
export default ManagerDashboardPage;