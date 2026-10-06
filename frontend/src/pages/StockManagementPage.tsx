// src/pages/StockManagementPage.tsx
import React, { useState, useEffect, useCallback } from 'react';

// API Service Imports
import {
    getAllSuppliers,
    createSupplier,
    updateSupplier,
    deleteSupplier
} from '../api/supplierService';
import {
    getAllParts,
    createPart,
    updatePart,
    deletePart,
    updatePartStock
} from '../api/partService';

// Type Imports
import type {
    Supplier,
    Part,
    CreateSupplierPayload,
    UpdateSupplierPayload,
    CreatePartPayload,
    UpdatePartPayload,
    UpdateStockPayload
} from '../types/inventory';

// Component Imports
import SupplierForm from '../components/SupplierForm/SupplierForm';
import PartForm from '../components/PartForm/PartForm';

// CSS Import
import './StockManagementPage.css';

const StockManagementPage: React.FC = () => {
    // --- State for Suppliers ---
    const [suppliers, setSuppliers] = useState<Supplier[]>([]);
    const [isLoadingSuppliers, setIsLoadingSuppliers] = useState<boolean>(true);
    const [suppliersListError, setSuppliersListError] = useState<string | null>(null); // Error for fetching list
    const [showSupplierForm, setShowSupplierForm] = useState<boolean>(false);
    const [isSubmittingSupplier, setIsSubmittingSupplier] = useState<boolean>(false);
    const [supplierFormError, setSupplierFormError] = useState<string | null>(null); // Error for form submission
    const [supplierToEdit, setSupplierToEdit] = useState<Supplier | null>(null);

    // --- State for Parts ---
    const [parts, setParts] = useState<Part[]>([]);
    const [isLoadingParts, setIsLoadingParts] = useState<boolean>(true);
    const [partsListError, setPartsListError] = useState<string | null>(null); // Error for fetching list
    const [showPartForm, setShowPartForm] = useState<boolean>(false);
    const [isSubmittingPart, setIsSubmittingPart] = useState<boolean>(false);
    const [partFormError, setPartFormError] = useState<string | null>(null); // Error for form submission
    const [partToEdit, setPartToEdit] = useState<Part | null>(null);

    // State for indicating which item's action is currently processing (e.g., delete, stock update)
    const [processingItemId, setProcessingItemId] = useState<number | null>(null);


    // --- Data Fetching Callbacks ---
    const fetchSuppliers = useCallback(async () => {
        setIsLoadingSuppliers(true); // Indicate loading for the supplier list
        try {
            const data = await getAllSuppliers();
            setSuppliers(data);
            setSuppliersListError(null);
        } catch (err: any) {
            setSuppliersListError(err.response?.data?.message || err.message || 'Failed to fetch suppliers.');
        } finally {
            setIsLoadingSuppliers(false);
        }
    }, []);

    const fetchParts = useCallback(async () => {
        setIsLoadingParts(true); // Indicate loading for the parts list
        try {
            const data = await getAllParts();
            setParts(data);
            setPartsListError(null);
        } catch (err: any) {
            setPartsListError(err.response?.data?.message || err.message || 'Failed to fetch parts.');
        } finally {
            setIsLoadingParts(false);
        }
    }, []);

    // Initial data load
    useEffect(() => {
        fetchSuppliers();
        fetchParts();
    }, [fetchSuppliers, fetchParts]);

    // --- Supplier Action Handlers ---
    const handleOpenAddSupplierForm = () => {
        setSupplierToEdit(null);
        setShowSupplierForm(true);
        setSupplierFormError(null);
        setShowPartForm(false); // Close part form if open
    };

    const handleOpenEditSupplierForm = (supplier: Supplier) => {
        setSupplierToEdit(supplier);
        setShowSupplierForm(true);
        setSupplierFormError(null);
        setShowPartForm(false); // Close part form if open
    };

    const handleCloseSupplierForm = () => {
        setShowSupplierForm(false);
        setSupplierToEdit(null);
        setSupplierFormError(null);
    };

    const handleSupplierFormSubmit = async (formData: CreateSupplierPayload | UpdateSupplierPayload) => {
        setIsSubmittingSupplier(true);
        setSupplierFormError(null);
        try {
            if (supplierToEdit) {
                await updateSupplier(supplierToEdit.id, formData as UpdateSupplierPayload);
                alert('Supplier updated successfully!');
            } else {
                await createSupplier(formData as CreateSupplierPayload);
                alert('Supplier added successfully!');
            }
            handleCloseSupplierForm();
            await fetchSuppliers(); // Refresh list
        } catch (err: any) {
            const message = err.response?.data?.message ||
                (err.response?.data?.errors ? Object.values(err.response.data.errors).flat().join(' ') : null) ||
                err.message ||
                (supplierToEdit ? 'Failed to update supplier.' : 'Failed to add supplier.');
            setSupplierFormError(message);
        } finally {
            setIsSubmittingSupplier(false);
        }
    };

    const handleDeleteSupplier = async (supplierId: number, supplierName: string) => {
        if (window.confirm(`Are you sure you want to delete supplier "${supplierName}" (ID: ${supplierId})? This action might affect associated parts.`)) {
            setProcessingItemId(supplierId); // Indicate this item is being processed
            setSuppliersListError(null); // Clear previous list errors
            try {
                await deleteSupplier(supplierId);
                alert(`Supplier "${supplierName}" deleted successfully!`);
                await fetchSuppliers(); // Refresh list
            } catch (err: any) {
                const message = err.response?.data?.message || err.message || `Failed to delete supplier ${supplierName}.`;
                setSuppliersListError(message); // Show error related to the list
                alert(`Error: ${message}`);
            } finally {
                setProcessingItemId(null);
            }
        }
    };

    // --- Part Action Handlers ---
    const handleOpenAddPartForm = () => {
        setPartToEdit(null);
        setShowPartForm(true);
        setPartFormError(null);
        setShowSupplierForm(false); // Close supplier form if open
    };

    const handleOpenEditPartForm = (part: Part) => {
        setPartToEdit(part);
        setShowPartForm(true);
        setPartFormError(null);
        setShowSupplierForm(false); // Close supplier form if open
    };

    const handleClosePartForm = () => {
        setShowPartForm(false);
        setPartToEdit(null);
        setPartFormError(null);
    };

    const handlePartFormSubmit = async (formData: CreatePartPayload | UpdatePartPayload) => {
        setIsSubmittingPart(true);
        setPartFormError(null);
        try {
            if (partToEdit) {
                const { initialStockLevel, ...updateData } = formData as CreatePartPayload; // initialStockLevel not part of UpdatePartPayload
                await updatePart(partToEdit.id, updateData as UpdatePartPayload);
                alert('Part updated successfully!');
            } else {
                await createPart(formData as CreatePartPayload);
                alert('Part added successfully!');
            }
            handleClosePartForm();
            await fetchParts(); // Refresh list
        } catch (err: any) {
            const message = err.response?.data?.message ||
                (err.response?.data?.errors ? Object.values(err.response.data.errors).flat().join(' ') : null) ||
                err.message ||
                (partToEdit ? 'Failed to update part.' : 'Failed to add part.');
            setPartFormError(message);
        } finally {
            setIsSubmittingPart(false);
        }
    };

    const handleDeletePart = async (partId: number, partName: string) => {
        if (window.confirm(`Are you sure you want to delete part "${partName}" (ID: ${partId})?`)) {
            setProcessingItemId(partId);
            setPartsListError(null);
            try {
                await deletePart(partId);
                alert(`Part "${partName}" deleted successfully!`);
                await fetchParts();
            } catch (err: any) {
                const message = err.response?.data?.message || err.message || `Failed to delete part ${partName}.`;
                setPartsListError(message);
                alert(`Error: ${message}`);
            } finally {
                setProcessingItemId(null);
            }
        }
    };

    const handleUpdateStock = async (part: Part) => {
        const changeStr = prompt(`Enter change in quantity for "${part.name}" (current: ${part.currentStockLevel}).\nUse positive for intake (e.g., 10), negative for dispatch (e.g., -5):`);
        if (changeStr === null) return;

        const changeInQuantity = parseInt(changeStr);
        if (isNaN(changeInQuantity)) {
            alert('Invalid quantity entered. Please enter a number.');
            return;
        }
        if (changeInQuantity === 0) {
            alert('No change in quantity entered.');
            return;
        }

        const reason = prompt(`Reason for stock change for "${part.name}" (e.g., "Stock intake", "Used for Job #123", "Stock correction"):`);
        if (reason === null) return; // User cancelled prompt for reason

        setProcessingItemId(part.id);
        setPartsListError(null);
        const payload: UpdateStockPayload = { changeInQuantity, reason: reason || undefined }; // reason is optional
        try {
            await updatePartStock(part.id, payload);
            alert(`Stock for "${part.name}" updated successfully!`);
            await fetchParts(); // Refresh list
        } catch (err: any) {
            const message = err.response?.data?.message || err.message || `Failed to update stock for ${part.name}.`;
            setPartsListError(message);
            alert(`Error: ${message}`);
        } finally {
            setProcessingItemId(null);
        }
    };

    // General disable flag for add buttons if any form is open or a list action is processing
    const isAnyFormOpen = showSupplierForm || showPartForm;
    const isAnyListActionProcessing = isSubmittingSupplier || isSubmittingPart || processingItemId !== null;

    return (
        <div className="stock-management-page">
            <h1>Stock & Suppliers Management</h1>

            {/* Suppliers Section */}
            <section className="page-section">
                <h2>Suppliers</h2>
                {!showSupplierForm && (
                    <div className="action-button-group">
                        <button
                            onClick={handleOpenAddSupplierForm}
                            className="add-new-btn"
                            disabled={isAnyFormOpen || isAnyListActionProcessing}
                        >
                            + Add New Supplier
                        </button>
                    </div>
                )}
                {showSupplierForm && (
                    <SupplierForm
                        initialData={supplierToEdit}
                        onSubmit={handleSupplierFormSubmit}
                        onCancel={handleCloseSupplierForm}
                        isSubmitting={isSubmittingSupplier}
                    />
                )}
                {showSupplierForm && supplierFormError &&
                    <p className="error-text form-error">{supplierFormError}</p>}
                {!showSupplierForm && suppliersListError &&
                    <p className="error-text">{suppliersListError}</p>}

                {isLoadingSuppliers && <p className="loading-text">Loading suppliers...</p>}
                {!isLoadingSuppliers && !suppliersListError && (
                    <ul className="item-list" style={{ marginTop: showSupplierForm ? '20px' : '0' }}>
                        {suppliers.length === 0 && !showSupplierForm && <p>No suppliers found. Click "Add New Supplier" to begin.</p>}
                        {suppliers.map(supplier => (
                            <li key={supplier.id}>
                                <div>
                                    <strong>{supplier.name}</strong>
                                    <br />
                                    <small>Email: {supplier.email} | Phone: {supplier.telephone || 'N/A'}</small>
                                    <br/>
                                    <small>Address: {supplier.addressLine1}, {supplier.postcode}</small>
                                </div>
                                <div className="item-actions">
                                    <button
                                        onClick={() => handleOpenEditSupplierForm(supplier)}
                                        className="edit-btn"
                                        disabled={isAnyFormOpen || processingItemId === supplier.id || isSubmittingSupplier}
                                    >
                                        Edit
                                    </button>
                                    <button
                                        onClick={() => handleDeleteSupplier(supplier.id, supplier.name)}
                                        className="delete-btn"
                                        disabled={isAnyFormOpen || processingItemId === supplier.id || isSubmittingSupplier}
                                    >
                                        {processingItemId === supplier.id && !isSubmittingSupplier ? 'Deleting...' : 'Delete'}
                                    </button>
                                </div>
                            </li>
                        ))}
                    </ul>
                )}
            </section>

            {/* Parts Section */}
            <section className="page-section">
                <h2>Parts / Stock</h2>
                {!showPartForm && (
                    <div className="action-button-group">
                        <button
                            onClick={handleOpenAddPartForm}
                            className="add-new-btn"
                            disabled={isAnyFormOpen || isAnyListActionProcessing}
                        >
                            + Add New Part
                        </button>
                    </div>
                )}
                {showPartForm && (
                    <PartForm
                        initialData={partToEdit}
                        onSubmit={handlePartFormSubmit}
                        onCancel={handleClosePartForm}
                        isSubmitting={isSubmittingPart}
                        suppliers={suppliers} // Pass the loaded suppliers
                    />
                )}
                {showPartForm && partFormError &&
                    <p className="error-text form-error">{partFormError}</p>}
                {!showPartForm && partsListError &&
                    <p className="error-text">{partsListError}</p>}

                {isLoadingParts && <p className="loading-text">Loading parts...</p>}
                {!isLoadingParts && !partsListError && (
                    <ul className="item-list" style={{ marginTop: showPartForm ? '20px' : '0' }}>
                        {parts.length === 0 && !showPartForm && <p>No parts found. Click "Add New Part" to begin.</p>}
                        {parts.map(part => (
                            <li key={part.id}>
                                <div>
                                    <strong>{part.name}</strong> (Price: £{part.price.toFixed(2)}, Stock: {part.currentStockLevel})
                                    <br />
                                    <small>Supplier: {part.supplierName || `ID: ${part.supplierId}`} | Barcode: {part.barcode || 'N/A'}</small>
                                </div>
                                <div className="item-actions">
                                    <button
                                        onClick={() => handleUpdateStock(part)}
                                        className="stock-btn"
                                        disabled={isAnyFormOpen || processingItemId === part.id || isSubmittingPart}
                                    >
                                        {processingItemId === part.id && !isSubmittingPart ? '...' : 'Update Stock'}
                                    </button>
                                    <button
                                        onClick={() => handleOpenEditPartForm(part)}
                                        className="edit-btn"
                                        disabled={isAnyFormOpen || processingItemId === part.id || isSubmittingPart}
                                    >
                                        Edit Info
                                    </button>
                                    <button
                                        onClick={() => handleDeletePart(part.id, part.name)}
                                        className="delete-btn"
                                        disabled={isAnyFormOpen || processingItemId === part.id || isSubmittingPart}
                                    >
                                        {processingItemId === part.id && !isSubmittingPart ? '...' : 'Delete'}
                                    </button>
                                </div>
                            </li>
                        ))}
                    </ul>
                )}
            </section>
        </div>
    );
};

export default StockManagementPage;