// src/pages/MyProfilePage.tsx
import React, { useState, useEffect, useCallback } from 'react';
import { getMyProfile, addVehicle, deleteVehicle, updateVehicle } from '../api/userService';
import type {UserProfile, VehiclePayload, Vehicle} from '../types/user';
import VehicleCard from '../components/VehicleCard/VehicleCard';
import AddVehicleForm from '../components/AddVehicleForm/AddVehicleForm';
import EditVehicleForm from '../components/EditVehicleForm/EditVehicleForm';
import './MyProfilePage.css';

const MyProfilePage: React.FC = () => {
    const [profile, setProfile] = useState<UserProfile | null>(null);
    const [isLoading, setIsLoading] = useState<boolean>(true);
    const [formError, setFormError] = useState<string | null>(null);
    const [showAddVehicleForm, setShowAddVehicleForm] = useState<boolean>(false);
    const [isProcessingVehicle, setIsProcessingVehicle] = useState<boolean>(false);
    const [showEditVehicleForm, setShowEditVehicleForm] = useState<boolean>(false);
    const [vehicleToEdit, setVehicleToEdit] = useState<Vehicle | null>(null);

    const fetchProfile = useCallback(async () => {
        try {
            const data = await getMyProfile();
            setProfile(data);
            setFormError(null);
        } catch (err: any) {
            setFormError(err.response?.data?.message || err.message || 'Failed to fetch profile.');
        }
    }, []);

    useEffect(() => {
        const initialLoad = async () => {
            setIsLoading(true);
            await fetchProfile();
            setIsLoading(false);
        };
        initialLoad();
    }, [fetchProfile]);

    const handleAddVehicleClick = () => {
        setShowAddVehicleForm(true);
        setShowEditVehicleForm(false);
        setVehicleToEdit(null);
        setFormError(null);
    };

    const handleCancelAddVehicle = () => {
        setShowAddVehicleForm(false);
        setFormError(null);
    };

    // --- UPDATED handleSaveNewVehicle ---
    const handleSaveNewVehicle = async (vehicleData: VehiclePayload) => {
        setIsProcessingVehicle(true);
        setFormError(null);
        try {
            await addVehicle(vehicleData);
            setShowAddVehicleForm(false);
            await fetchProfile();
        } catch (err: any) {
            let message = 'Failed to add vehicle. Please try again.'; // Default error message
            if (err.response) {
                // Check for specific 409 Conflict error
                if (err.response.status === 409) {
                    message = err.response.data?.message || `Vehicle with registration number '${vehicleData.registrationNumber}' already exists.`;
                } else {
                    // For other API errors, use the message from the response if available
                    message = err.response.data?.message || err.response.data?.title || message;
                }
            } else {
                // For network errors or other issues
                message = err.message || message;
            }
            setFormError(message);
            console.error("Failed to add vehicle:", err);
        } finally {
            setIsProcessingVehicle(false);
        }
    };

    // ... (handleEditVehicleClick, handleCancelEditVehicle, handleSaveUpdatedVehicle, handleDeleteVehicle залишаються такими ж, як у попередньому кроці)
    const handleEditVehicleClick = (registrationNumber: string) => {
        const vehicle = profile?.vehicles.find(v => v.registrationNumber === registrationNumber);
        if (vehicle) {
            setVehicleToEdit(vehicle);
            setShowEditVehicleForm(true);
            setShowAddVehicleForm(false);
            setFormError(null);
        }
    };

    const handleCancelEditVehicle = () => {
        setShowEditVehicleForm(false);
        setVehicleToEdit(null);
        setFormError(null);
    };

    const handleSaveUpdatedVehicle = async (updatedVehicleData: VehiclePayload) => {
        if (!vehicleToEdit) return;

        setIsProcessingVehicle(true);
        setFormError(null);
        try {
            await updateVehicle(vehicleToEdit.registrationNumber, updatedVehicleData);
            setShowEditVehicleForm(false);
            setVehicleToEdit(null);
            await fetchProfile();
        } catch (err: any) {
            let message = 'Failed to update vehicle. Please try again.';
            if (err.response) {
                message = err.response.data?.message || err.response.data?.title || message;
            } else {
                message = err.message || message;
            }
            setFormError(message);
        } finally {
            setIsProcessingVehicle(false);
        }
    };

    const handleDeleteVehicle = async (registrationNumber: string) => {
        if (window.confirm(`Are you sure you want to delete vehicle ${registrationNumber}? This action cannot be undone.`)) {
            setIsProcessingVehicle(true);
            setFormError(null);
            try {
                await deleteVehicle(registrationNumber);
                await fetchProfile();
            } catch (err: any) {
                const message = err.response?.data?.message || err.message || `Failed to delete vehicle ${registrationNumber}.`;
                setFormError(message);
            } finally {
                setIsProcessingVehicle(false);
            }
        }
    };


    // ... (JSX рендеринг залишається таким же, як у попередньому кроці)
    // Переконайтесь, що блок {formError && ...} відображається під час активної форми
    if (isLoading) {
        return <div className="loading-message">Loading your profile...</div>;
    }

    if (formError && !profile && !showAddVehicleForm && !showEditVehicleForm) {
        return <div className="error-message">{formError}</div>;
    }

    if (!profile) {
        return <div className="loading-message">No profile data found.</div>;
    }

    return (
        <div className="profile-page-container">
            <h1>My Profile</h1>
            <div className="profile-details">
                <div className="detail-item"><strong>Full Name:</strong> <span>{profile.fullName}</span></div>
                <div className="detail-item"><strong>Email:</strong> <span>{profile.email}</span></div>
                <div className="detail-item"><strong>Address:</strong> <span>{profile.address || 'Not provided'}</span></div>
                <div className="detail-item"><strong>Phone Number:</strong> <span>{profile.phoneNumber || 'Not provided'}</span></div>
            </div>

            <div className="vehicles-section">
                <h2>My Vehicles</h2>

                {formError && (showAddVehicleForm || showEditVehicleForm) &&
                    <p className="error-message" style={{textAlign: 'center', marginBottom: '15px', backgroundColor: 'rgba(217, 48, 37, 0.1)', padding: '10px', borderRadius: '6px' }}>{formError}</p>}

                {!showAddVehicleForm && !showEditVehicleForm && (
                    <button onClick={handleAddVehicleClick} className="add-vehicle-btn" disabled={isProcessingVehicle}>
                        + Add New Vehicle
                    </button>
                )}

                {showAddVehicleForm && (
                    <AddVehicleForm
                        onSubmit={handleSaveNewVehicle}
                        onCancel={handleCancelAddVehicle}
                        isSubmitting={isProcessingVehicle}
                    />
                )}

                {showEditVehicleForm && vehicleToEdit && (
                    <EditVehicleForm
                        vehicleToEdit={vehicleToEdit}
                        onSubmit={handleSaveUpdatedVehicle}
                        onCancel={handleCancelEditVehicle}
                        isSubmitting={isProcessingVehicle}
                    />
                )}

                {profile.vehicles && profile.vehicles.length > 0 ? (
                    <div className="vehicles-list">
                        {profile.vehicles.map(vehicle => (
                            <VehicleCard
                                key={vehicle.registrationNumber}
                                vehicle={vehicle}
                                onEdit={handleEditVehicleClick}
                                onDelete={handleDeleteVehicle}
                            />
                        ))}
                    </div>
                ) : (
                    !showAddVehicleForm && !showEditVehicleForm && <p>You have no vehicles registered yet.</p>
                )}
            </div>
        </div>
    );
};

export default MyProfilePage;