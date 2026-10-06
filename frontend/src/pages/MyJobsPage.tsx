import React, { useCallback, useEffect, useState } from 'react';
import { searchBookings, startJob, finishJob } from '../api/bookingService';
import type { Booking, BookingStatus, FinishJobPayload } from '../types/booking';
import BookingCard from '../components/BookingCard/BookingCard';
import Modal from '../components/Modal';
import FinishJobForm from '../components/FinishJobForm';
import { getErrorMessage } from '../utils/format';

const COLUMNS: { title: string; hint: string; statuses: BookingStatus[] }[] = [
    { title: 'To start', hint: 'Assigned to you', statuses: ['Assigned'] },
    { title: 'In progress', hint: 'Finish to send the invoice', statuses: ['InProgress'] },
    { title: 'Finished', hint: 'Waiting for or received payment', statuses: ['AwaitingPayment', 'Paid'] },
];

const MyJobsPage: React.FC = () => {
    const [jobs, setJobs] = useState<Booking[] | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [busyId, setBusyId] = useState<number | null>(null);
    const [finishing, setFinishing] = useState<Booking | null>(null);
    const [finishError, setFinishError] = useState<string | null>(null);

    const load = useCallback(async () => {
        try {
            const result = await searchBookings();
            setJobs(result.sort((a, b) => a.slotStart.localeCompare(b.slotStart)));
        } catch (err) {
            setError(getErrorMessage(err, 'Could not load your jobs.'));
        }
    }, []);

    useEffect(() => { void load(); }, [load]);

    const handleStart = async (job: Booking) => {
        setBusyId(job.id);
        setError(null);
        try {
            await startJob(job.id);
            await load();
        } catch (err) {
            setError(getErrorMessage(err, 'Could not start the job.'));
        } finally {
            setBusyId(null);
        }
    };

    const handleFinish = async (data: FinishJobPayload) => {
        if (!finishing) return;
        try {
            await finishJob(finishing.id, data);
            setFinishing(null);
            await load();
        } catch (err) {
            setFinishError(getErrorMessage(err, 'Could not finish the job.'));
        }
    };

    return (
        <div className="container page">
            <div className="page-header">
                <div>
                    <h1>My jobs</h1>
                    <p className="subtitle">Start a job when the car is in, and finish it with the work done and parts used.</p>
                </div>
            </div>

            {error && <p className="alert alert-error" style={{ marginBottom: 16 }}>{error}</p>}
            {jobs === null && !error && <p className="loading">Loading your jobs…</p>}

            {jobs && (
                <div className="board">
                    {COLUMNS.map(column => {
                        const items = jobs.filter(j => column.statuses.includes(j.status));
                        return (
                            <section key={column.title} className="board-column">
                                <header>
                                    <h2>{column.title} <span className="count">{items.length}</span></h2>
                                    <p className="muted small">{column.hint}</p>
                                </header>
                                {items.length === 0 && <p className="empty-state">Nothing here.</p>}
                                {items.map(job => (
                                    <BookingCard key={job.id} booking={job} showCustomer>
                                        {job.status === 'Assigned' && (
                                            <button className="btn btn-sm btn-dark" disabled={busyId === job.id}
                                                    onClick={() => void handleStart(job)}>
                                                {busyId === job.id ? 'Starting…' : 'Start job'}
                                            </button>
                                        )}
                                        {job.status === 'InProgress' && (
                                            <button className="btn btn-sm btn-primary"
                                                    onClick={() => { setFinishing(job); setFinishError(null); }}>Finish job</button>
                                        )}
                                    </BookingCard>
                                ))}
                            </section>
                        );
                    })}
                </div>
            )}

            {finishing && (
                <Modal title={`Finish ${finishing.serviceTypeName}`} onClose={() => { setFinishing(null); }}>
                    <FinishJobForm booking={finishing} onSubmit={handleFinish} onCancel={() => { setFinishing(null); }} error={finishError} />
                </Modal>
            )}
        </div>
    );
};

export default MyJobsPage;
