// src/pages/MyJobsPage.tsx
import React, { useState, useEffect, useCallback, useMemo } from 'react';
import {
    searchBookings,
    startBookingJob,
    finishBookingJob // <-- Import the new service function
} from '../api/bookingService';
import type {Booking, BookingSearchParameters, FinishJobPayload} from '../types/booking';
import MechanicBookingCard from '../components/MechanicBookingCard/MechanicBookingCard';
import FinishJobModal from '../components/FinishJobModal/FinishJobModal';
import './MyJobsPage.css';

// Status constants
const STATUS_ASSIGNED_TO_ME: string[] = ["Assigned"];
const STATUS_MY_IN_PROGRESS: string[] = ["InProgress"];

const MyJobsPage: React.FC = () => {
    // States for job lists, loading, errors
    const [allMyJobs, setAllMyJobs] = useState<Booking[]>([]);
    const [isLoading, setIsLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);
    const [processingJobId, setProcessingJobId] = useState<number | null>(null); // For card-specific loading

    // States for Finish Job Modal
    const [showFinishJobModal, setShowFinishJobModal] = useState<boolean>(false);
    const [jobToFinishDetails, setJobToFinishDetails] = useState<Booking | null>(null);

    // Callback to fetch jobs assigned to the mechanic
    const fetchMyJobs = useCallback(async () => {
        setIsLoading(true); // General loading for the list
        setError(null);
        try {
            // Backend should scope this search to the logged-in mechanic
            // and return jobs with statuses relevant to their active work.
            const params: BookingSearchParameters = {
                // Consider fetching only "Assigned" and "InProgress" statuses
                // status: "Assigned,InProgress" // If backend supports comma-separated list
            };
            const data = await searchBookings(params);
            data.sort((a, b) => new Date(a.slotStart).getTime() - new Date(b.slotStart).getTime()); // Sort by date
            setAllMyJobs(data);
        } catch (err: any) {
            const errorMessage = err.response?.data?.message || err.message || 'Failed to fetch your jobs.';
            setError(errorMessage);
            setAllMyJobs([]); // Clear jobs on error
        } finally {
            setIsLoading(false);
        }
    }, []);

    // Initial fetch of jobs on component mount
    useEffect(() => {
        fetchMyJobs();
    }, [fetchMyJobs]);

    // Memoized lists for columns
    const assignedJobs = useMemo(() =>
            allMyJobs.filter(b => STATUS_ASSIGNED_TO_ME.includes(b.status)),
        [allMyJobs]);

    const inProgressJobs = useMemo(() =>
            allMyJobs.filter(b => STATUS_MY_IN_PROGRESS.includes(b.status)),
        [allMyJobs]);

    // Handler to start a job
    const handleStartJobAction = async (bookingId: number) => {
        setProcessingJobId(bookingId);
        setError(null);
        try {
            await startBookingJob(bookingId);
            await fetchMyJobs(); // Refresh the job list
            alert('Job started successfully!');
        } catch (err: any) {
            const errorMessage = err.response?.data?.message || err.message || `Failed to start job ${bookingId}.`;
            setError(errorMessage); // Display error to the user
            alert(`Error: ${errorMessage}`);
        } finally {
            setProcessingJobId(null);
        }
    };

    // Handler to open the finish job modal
    const handleOpenFinishJobModal = (booking: Booking) => {
        setJobToFinishDetails(booking);
        setShowFinishJobModal(true);
        setError(null); // Clear page-level errors when opening modal
    };

    const handleConfirmFinishJob = async (bookingId: number, data: FinishJobPayload) => {
        setProcessingJobId(bookingId); // Indicate this specific job is being processed
        setError(null); // Clear previous page-level errors
        try {
            // Call the actual API service function
            await finishBookingJob(bookingId, data);

            alert('Job finished and details submitted successfully!');
            setShowFinishJobModal(false);
            setJobToFinishDetails(null);
            await fetchMyJobs(); // Refresh job list to reflect status change
        } catch (err: any) {
            // Error from finishBookingJob API call
            const errorMessage = err.response?.data?.message ||
                (err.response?.data?.errors ? Object.values(err.response.data.errors).flat().join(' ') : null) ||
                err.message ||
                'Failed to submit job completion.';
            // This error could be displayed within the modal or on the page.
            // For simplicity, we'll use an alert for now and also set the page error.
            alert(`Error submitting job details: ${errorMessage}`);
            setError(errorMessage); // Sets page-level error, modal might need its own error state.
            console.error("Error finishing job:", err.response || err);
        } finally {
            setProcessingJobId(null);
        }
    };

    // JSX for loading and error states
    if (isLoading && allMyJobs.length === 0) {
        return <div className="loading-message">Loading your jobs...</div>;
    }

    if (error && allMyJobs.length === 0) {
        return <div className="error-message-slots">{error}</div>;
    }

    return (
        <div className="mechanic-dashboard-container">
            <h1>My Jobs Dashboard</h1>
            {/* Display general page error if it occurred, e.g., during fetch or a failed action */}
            {error && <p className="error-message-slots" style={{textAlign: 'center', marginBottom: '15px'}}>{error}</p>}

            <div className="mechanic-jobs-columns">
                {/* Assigned Jobs Column */}
                <div className="job-column">
                    <h2>Assigned to Me ({assignedJobs.length})</h2>
                    {isLoading && assignedJobs.length === 0 && <p>Loading...</p>}
                    {!isLoading && assignedJobs.length === 0 && <p>No jobs currently assigned.</p>}
                    <div className="column-jobs-list">
                        {assignedJobs.map(job => (
                            <MechanicBookingCard
                                key={job.id}
                                booking={job}
                                onStartJob={handleStartJobAction}
                                // onFinishJob is not directly called from here, modal is opened instead
                                isProcessing={processingJobId === job.id}
                            />
                        ))}
                    </div>
                </div>

                {/* In Progress Jobs Column */}
                <div className="job-column">
                    <h2>In Progress ({inProgressJobs.length})</h2>
                    {isLoading && inProgressJobs.length === 0 && <p>Loading...</p>}
                    {!isLoading && inProgressJobs.length === 0 && <p>No jobs currently in progress.</p>}
                    <div className="column-jobs-list">
                        {inProgressJobs.map(job => (
                            <MechanicBookingCard
                                key={job.id}
                                booking={job}
                                onStartJob={handleStartJobAction} // Button will be disabled by logic in MechanicBookingCard
                                onFinishJob={() => handleOpenFinishJobModal(job)} // Opens the modal
                                isProcessing={processingJobId === job.id}
                            />
                        ))}
                    </div>
                </div>
            </div>

            {/* Finish Job Modal */}
            {jobToFinishDetails && ( // Ensure jobToFinishDetails is not null before rendering
                <FinishJobModal
                    isOpen={showFinishJobModal}
                    onClose={() => { setShowFinishJobModal(false); setJobToFinishDetails(null); }}
                    booking={jobToFinishDetails}
                    onSubmit={handleConfirmFinishJob} // Pass the handler
                    isSubmitting={processingJobId === jobToFinishDetails?.id} // Modal's submit button loading state
                />
            )}
        </div>
    );
};

export default MyJobsPage;