import React from 'react';
import type {Booking} from '../../types/booking';
import type {MechanicAvailability} from '../../types/schedule';

interface AssignMechanicModalProps {
    isOpen: boolean;
    onClose: () => void;
    booking: Booking | null;
    availableMechanics: MechanicAvailability[];
    isLoadingMechanics: boolean;
    selectedMechanicId: string;
    onSelectMechanicId: (id: string) => void;
    onConfirmAssignment: () => Promise<void>;
    assignmentError: string | null;
    isAssigning: boolean; // To show loading state on confirm button
}

const AssignMechanicModal: React.FC<AssignMechanicModalProps> = ({
                                                                     isOpen,
                                                                     onClose,
                                                                     booking,
                                                                     availableMechanics,
                                                                     isLoadingMechanics,
                                                                     selectedMechanicId,
                                                                     onSelectMechanicId,
                                                                     onConfirmAssignment,
                                                                     assignmentError,
                                                                     isAssigning,
                                                                 }) => {
    if (!isOpen || !booking) {
        return null;
    }

    return (
        <>
            <div className="modal-overlay" onClick={onClose}></div>
            <div className="assign-mechanic-modal"> {/* Ensure this class exists in your CSS */}
                <div className="modal-header">
                    <h2>Assign Mechanic</h2>
                    <button onClick={onClose} className="close-modal-btn">&times;</button>
                </div>
                <p><strong>Booking ID:</strong> {booking.id} for {booking.serviceTypeName}</p>
                <p><strong>Time:</strong> {new Date(booking.slotStart).toLocaleString()}</p>
                <p><strong>Vehicle:</strong> {booking.vehicleRegistrationNumber}</p>

                {isLoadingMechanics && <p style={{textAlign: 'center'}}>Loading available mechanics...</p>}
                {assignmentError && <p className="error-message-slots" style={{textAlign: 'center'}}>{assignmentError}</p>}

                {!isLoadingMechanics && availableMechanics.length > 0 && (
                    <div className="form-group" style={{marginTop: '20px'}}>
                        <label htmlFor="mechanic-select" style={{fontWeight: 'bold'}}>Select Mechanic:</label>
                        <select
                            id="mechanic-select"
                            value={selectedMechanicId}
                            onChange={(e) => onSelectMechanicId(e.target.value)}
                            style={{width: '100%', padding: '10px', marginTop: '5px', borderRadius: '4px'}}
                            disabled={isAssigning}
                        >
                            {availableMechanics.map(mech => (
                                <option key={mech.mechanicId} value={mech.mechanicId.toString()}>
                                    {mech.fullName} (ID: {mech.mechanicId})
                                </option>
                            ))}
                        </select>
                    </div>
                )}
                {!isLoadingMechanics && availableMechanics.length === 0 && !assignmentError && (
                    <p style={{textAlign: 'center', marginTop: '20px'}}>No mechanics currently available for this service and time slot.</p>
                )}

                <div className="form-actions" style={{marginTop: '30px'}}>
                    <button onClick={onClose} className="cancel-btn" disabled={isAssigning}>Cancel</button>
                    <button
                        onClick={onConfirmAssignment}
                        className="assign-btn"
                        disabled={isAssigning || isLoadingMechanics || !selectedMechanicId || availableMechanics.length === 0}
                    >
                        {isAssigning
                            ? (booking.mechanicId ? 'Re-assigning...' : 'Assigning...')
                            : (booking.mechanicId ? 'Confirm Re-assignment' : 'Confirm Assignment')}
                    </button>
                </div>
            </div>
        </>
    );
};

export default AssignMechanicModal;