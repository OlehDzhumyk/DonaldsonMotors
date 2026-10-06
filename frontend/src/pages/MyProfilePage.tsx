import React, { useCallback, useEffect, useState } from 'react';
import { getMyProfile, addVehicle, deleteVehicle, updateVehicle } from '../api/userService';
import type { UserProfile, Vehicle, VehiclePayload } from '../types/user';
import VehicleForm from '../components/VehicleForm/VehicleForm';
import { getErrorMessage } from '../utils/format';

/** null = no form, 'new' = add form, a Vehicle = editing that vehicle */
type FormState = null | 'new' | Vehicle;

const MyProfilePage: React.FC = () => {
    const [profile, setProfile] = useState<UserProfile | null>(null);
    const [loadError, setLoadError] = useState<string | null>(null);
    const [form, setForm] = useState<FormState>(null);
    const [formError, setFormError] = useState<string | null>(null);
    const [listError, setListError] = useState<string | null>(null);

    const fetchProfile = useCallback(async () => {
        try {
            setProfile(await getMyProfile());
        } catch (err) {
            setLoadError(getErrorMessage(err, 'Could not load your profile.'));
        }
    }, []);

    useEffect(() => { void fetchProfile(); }, [fetchProfile]);

    const openForm = (state: FormState) => {
        setForm(state);
        setFormError(null);
        setListError(null);
    };

    const handleSave = async (data: VehiclePayload) => {
        try {
            if (form === 'new') {
                await addVehicle(data);
            } else if (form) {
                await updateVehicle(form.registrationNumber, data);
            }
            setForm(null);
            await fetchProfile();
        } catch (err) {
            setFormError(getErrorMessage(err, 'Could not save the vehicle.'));
        }
    };

    const handleDelete = async (vehicle: Vehicle) => {
        if (!window.confirm(`Remove ${vehicle.make} ${vehicle.model} (${vehicle.registrationNumber})?`)) return;
        setListError(null);
        try {
            await deleteVehicle(vehicle.registrationNumber);
            await fetchProfile();
        } catch (err) {
            setListError(getErrorMessage(err, 'Could not remove the vehicle.'));
        }
    };

    if (loadError) return <div className="container page"><p className="alert alert-error">{loadError}</p></div>;
    if (!profile) return <p className="loading">Loading your profile…</p>;

    return (
        <div className="container page">
            <div className="page-header">
                <div>
                    <h1>Profile & vehicles</h1>
                    <p className="subtitle">Keep your details and cars up to date so booking takes seconds.</p>
                </div>
            </div>

            <div className="profile-layout">
                <section className="card">
                    <div className="card-header"><h2>Your details</h2></div>
                    <dl className="meta card-body">
                        <dt>Name</dt><dd>{profile.fullName}</dd>
                        <dt>Email</dt><dd>{profile.email}</dd>
                        <dt>Address</dt><dd>{profile.address ?? <span className="muted">Not provided</span>}</dd>
                        <dt>Phone</dt><dd>{profile.phoneNumber ?? <span className="muted">Not provided</span>}</dd>
                    </dl>
                </section>

                <section className="stack">
                    {form !== null && (
                        <VehicleForm
                            key={form === 'new' ? 'new' : form.registrationNumber}
                            vehicle={form === 'new' ? undefined : form}
                            onSubmit={handleSave}
                            onCancel={() => { openForm(null); }}
                            error={formError}
                        />
                    )}

                    <div className="card">
                        <div className="card-header">
                            <h2>Vehicles <span className="count">{profile.vehicles.length}</span></h2>
                            {form === null && (
                                <button className="btn btn-sm btn-primary" onClick={() => { openForm('new'); }}>Add vehicle</button>
                            )}
                        </div>
                        {listError && <p className="alert alert-error" style={{ margin: 16 }}>{listError}</p>}
                        {profile.vehicles.length === 0 ? (
                            <div className="card-body"><p className="empty-state">Add a vehicle to start booking services.</p></div>
                        ) : (
                            <table className="table">
                                <thead>
                                    <tr><th>Registration</th><th>Vehicle</th><th className="num">Mileage</th><th /></tr>
                                </thead>
                                <tbody>
                                    {profile.vehicles.map(vehicle => (
                                        <tr key={vehicle.registrationNumber}>
                                            <td><span className="plate">{vehicle.registrationNumber}</span></td>
                                            <td>
                                                <strong>{vehicle.make} {vehicle.model}</strong>
                                                <div className="muted small">{vehicle.year}</div>
                                            </td>
                                            <td className="num">{vehicle.mileage.toLocaleString('en-GB')} mi</td>
                                            <td className="actions">
                                                <button className="btn btn-sm btn-secondary" onClick={() => { openForm(vehicle); }}>Edit</button>
                                                <button className="btn btn-sm btn-danger" onClick={() => void handleDelete(vehicle)}>Remove</button>
                                            </td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        )}
                    </div>
                </section>
            </div>
        </div>
    );
};

export default MyProfilePage;
