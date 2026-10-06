import React, { useEffect, useMemo, useState } from 'react';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import { getAvailability } from '../api/scheduleService';
import { getMyProfile } from '../api/userService';
import { createBooking } from '../api/bookingService';
import { useServiceTypes } from '../hooks/useServiceTypes';
import type { Vehicle } from '../types/user';
import { formatDate, formatDateTime, formatHours, formatMoney, formatTime, getErrorMessage } from '../utils/format';
import './BookingPage.css';

const DAYS_SHOWN = 14;

const toIsoDate = (date: Date) => date.toISOString().slice(0, 10);
const addDays = (date: Date, days: number) => new Date(date.getTime() + days * 24 * 60 * 60 * 1000);

/** Groups UTC slot strings by local calendar day, keeping order. */
const groupByDay = (slots: string[]) => {
    const groups = new Map<string, string[]>();
    for (const slot of slots) {
        const key = new Date(slot).toDateString();
        groups.set(key, [...(groups.get(key) ?? []), slot]);
    }
    return [...groups.values()];
};

const BookingPage: React.FC = () => {
    const navigate = useNavigate();
    const [searchParams] = useSearchParams();
    const { serviceTypes, error: servicesError } = useServiceTypes();

    const [vehicles, setVehicles] = useState<Vehicle[] | null>(null);
    const [rangeStart, setRangeStart] = useState(() => new Date());
    const [slots, setSlots] = useState<string[] | null>(null);
    const [slotsError, setSlotsError] = useState<string | null>(null);

    const [serviceId, setServiceId] = useState<number | null>(() => Number(searchParams.get('service')) || null);
    const [vehicleReg, setVehicleReg] = useState<string | null>(null);
    const [slot, setSlot] = useState<string | null>(null);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [submitError, setSubmitError] = useState<string | null>(null);

    useEffect(() => {
        getMyProfile()
            .then(profile => {
                setVehicles(profile.vehicles);
                setVehicleReg(current => current ?? profile.vehicles[0]?.registrationNumber ?? null);
            })
            .catch((err: unknown) => { setSubmitError(getErrorMessage(err, 'Could not load your vehicles.')); });
    }, []);

    useEffect(() => {
        let cancelled = false;
        setSlots(null);
        setSlotsError(null);
        getAvailability(toIsoDate(rangeStart), toIsoDate(addDays(rangeStart, DAYS_SHOWN)))
            .then(result => { if (!cancelled) setSlots(result); })
            .catch((err: unknown) => { if (!cancelled) setSlotsError(getErrorMessage(err, 'Could not load free slots.')); });
        return () => { cancelled = true; };
    }, [rangeStart]);

    const days = useMemo(() => groupByDay(slots ?? []), [slots]);
    const service = serviceTypes.find(s => s.id === serviceId);
    const vehicle = vehicles?.find(v => v.registrationNumber === vehicleReg);
    const isFirstRange = toIsoDate(rangeStart) <= toIsoDate(new Date());

    const handleConfirm = async () => {
        if (!service || !vehicle || !slot) return;
        setIsSubmitting(true);
        setSubmitError(null);
        try {
            await createBooking({ serviceTypeId: service.id, vehicleRegistrationNumber: vehicle.registrationNumber, slotStart: slot });
            void navigate('/my-bookings', { state: { justBooked: `${service.name} on ${formatDateTime(slot)}` } });
        } catch (err) {
            setSubmitError(getErrorMessage(err, 'Could not create the booking.'));
            setIsSubmitting(false);
        }
    };

    return (
        <div className="container page">
            <div className="page-header">
                <div>
                    <h1>Book a service</h1>
                    <p className="subtitle">Choose a service, your car and a time. You'll get a confirmation email straight away.</p>
                </div>
            </div>

            <div className="booking-layout">
                <div className="stack">
                    <section className="card">
                        <div className="card-header"><h2><span className="step">1</span> Service</h2></div>
                        <div className="card-body option-grid">
                            {servicesError && <p className="alert alert-error">{servicesError}</p>}
                            {serviceTypes.map(s => (
                                <button key={s.id} type="button"
                                        className={`option ${s.id === serviceId ? 'selected' : ''}`}
                                        onClick={() => { setServiceId(s.id); }}>
                                    <span className="option-title">{s.name}</span>
                                    <span className="muted small">{s.description}</span>
                                    <span className="option-meta">{formatMoney(s.price)} · {formatHours(s.durationHours)}</span>
                                </button>
                            ))}
                        </div>
                    </section>

                    <section className="card">
                        <div className="card-header">
                            <h2><span className="step">2</span> Vehicle</h2>
                            <Link to="/profile" className="small">Manage vehicles</Link>
                        </div>
                        <div className="card-body option-grid">
                            {vehicles?.length === 0 && (
                                <p className="empty-state">You have no vehicles yet. <Link to="/profile">Add one</Link> to book.</p>
                            )}
                            {vehicles?.map(v => (
                                <button key={v.registrationNumber} type="button"
                                        className={`option option-row ${v.registrationNumber === vehicleReg ? 'selected' : ''}`}
                                        onClick={() => { setVehicleReg(v.registrationNumber); }}>
                                    <span className="plate">{v.registrationNumber}</span>
                                    <span>
                                        <span className="option-title">{v.make} {v.model}</span>
                                        <span className="muted small"> · {v.year}</span>
                                    </span>
                                </button>
                            ))}
                        </div>
                    </section>

                    <section className="card">
                        <div className="card-header">
                            <h2><span className="step">3</span> Date and time</h2>
                            <div className="row">
                                <button className="btn btn-sm btn-secondary" disabled={isFirstRange}
                                        onClick={() => { setRangeStart(addDays(rangeStart, -DAYS_SHOWN)); }}>← Earlier</button>
                                <button className="btn btn-sm btn-secondary"
                                        onClick={() => { setRangeStart(addDays(rangeStart, DAYS_SHOWN)); }}>Later →</button>
                            </div>
                        </div>
                        <div className="card-body">
                            {slotsError && <p className="alert alert-error">{slotsError}</p>}
                            {slots === null && !slotsError && <p className="loading">Loading free slots…</p>}
                            {slots?.length === 0 && <p className="empty-state">No free slots in these two weeks. Try later dates.</p>}
                            <div className="day-list">
                                {days.map(daySlots => (
                                    <div key={daySlots[0]} className="day">
                                        <span className="day-label">{formatDate(daySlots[0] ?? '')}</span>
                                        <div className="time-chips">
                                            {daySlots.map(s => (
                                                <button key={s} type="button"
                                                        className={`time-chip ${s === slot ? 'selected' : ''}`}
                                                        onClick={() => { setSlot(s); }}>
                                                    {formatTime(s)}
                                                </button>
                                            ))}
                                        </div>
                                    </div>
                                ))}
                            </div>
                        </div>
                    </section>
                </div>

                <aside className="card booking-summary">
                    <div className="card-header"><h2>Summary</h2></div>
                    <dl className="meta card-body">
                        <dt>Service</dt><dd>{service?.name ?? <span className="muted">Not chosen</span>}</dd>
                        <dt>Vehicle</dt><dd>{vehicle ? `${vehicle.make} ${vehicle.model}` : <span className="muted">Not chosen</span>}</dd>
                        <dt>Time</dt><dd>{slot ? formatDateTime(slot) : <span className="muted">Not chosen</span>}</dd>
                        <dt>Duration</dt><dd>{service ? formatHours(service.durationHours) : '—'}</dd>
                    </dl>
                    <div className="summary-total">
                        <span>Price</span>
                        <strong>{service ? formatMoney(service.price) : '—'}</strong>
                    </div>
                    <div className="card-body">
                        {submitError && <p className="alert alert-error" style={{ marginBottom: 12 }}>{submitError}</p>}
                        <button className="btn btn-primary btn-lg btn-block"
                                disabled={!service || !vehicle || !slot || isSubmitting}
                                onClick={() => void handleConfirm()}>
                            {isSubmitting ? 'Booking…' : 'Confirm booking'}
                        </button>
                        <p className="muted small" style={{ marginTop: 10 }}>Parts used during the job are added to the final invoice.</p>
                    </div>
                </aside>
            </div>
        </div>
    );
};

export default BookingPage;
