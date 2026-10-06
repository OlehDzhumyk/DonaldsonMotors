// src/components/VehicleCard/VehicleCard.tsx
import React from 'react';
import type {Vehicle} from '../../types/user';
import './VehicleCard.css';

interface VehicleCardProps {
    vehicle: Vehicle;
    onEdit: (registrationNumber: string) => void; // Will be implemented later
    onDelete: (registrationNumber: string) => void; // Will be implemented later
}

const VehicleCard: React.FC<VehicleCardProps> = ({ vehicle, onEdit, onDelete }) => {
    return (
        <div className="vehicle-card">
            <h3>{vehicle.make} {vehicle.model} ({vehicle.year})</h3>
            <p><strong>Registration:</strong> {vehicle.registrationNumber}</p>
            <p><strong>Mileage:</strong> {vehicle.mileage.toLocaleString()} miles</p>
            <div className="vehicle-actions">
                <button
                    onClick={() => onEdit(vehicle.registrationNumber)}
                    className="edit-btn"
                >
                    Edit
                </button>
                <button
                    onClick={() => onDelete(vehicle.registrationNumber)}
                    className="delete-btn"
                >
                    Delete
                </button>
            </div>
        </div>
    );
};

export default VehicleCard;