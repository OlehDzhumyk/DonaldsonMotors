import React, { useCallback, useEffect, useMemo, useState } from 'react';
import { getSuppliers, createSupplier, updateSupplier, deleteSupplier } from '../api/supplierService';
import { getParts, createPart, updatePart, deletePart, updatePartStock } from '../api/partService';
import type { CreatePartPayload, Part, Supplier, SupplierPayload, UpdateStockPayload } from '../types/inventory';
import Modal from '../components/Modal';
import PartForm from '../components/PartForm/PartForm';
import SupplierForm from '../components/SupplierForm/SupplierForm';
import StockChangeForm from '../components/StockChangeForm';
import { formatMoney, getErrorMessage } from '../utils/format';
import './StockManagementPage.css';

const LOW_STOCK = 10;

type Tab = 'parts' | 'suppliers';

/** Which dialog is open, and for which record. */
type Dialog =
    | { kind: 'part'; part?: Part }
    | { kind: 'stock'; part: Part }
    | { kind: 'supplier'; supplier?: Supplier };

const StockManagementPage: React.FC = () => {
    const [tab, setTab] = useState<Tab>('parts');
    const [parts, setParts] = useState<Part[]>([]);
    const [suppliers, setSuppliers] = useState<Supplier[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [pageError, setPageError] = useState<string | null>(null);
    const [search, setSearch] = useState('');
    const [dialog, setDialog] = useState<Dialog | null>(null);
    const [dialogError, setDialogError] = useState<string | null>(null);

    const load = useCallback(async () => {
        try {
            const [loadedParts, loadedSuppliers] = await Promise.all([getParts(), getSuppliers()]);
            setParts(loadedParts.sort((a, b) => a.name.localeCompare(b.name)));
            setSuppliers(loadedSuppliers.sort((a, b) => a.name.localeCompare(b.name)));
        } catch (err) {
            setPageError(getErrorMessage(err, 'Could not load stock.'));
        } finally {
            setIsLoading(false);
        }
    }, []);

    useEffect(() => { void load(); }, [load]);

    const openDialog = (next: Dialog | null) => {
        setDialog(next);
        setDialogError(null);
    };

    /** Runs a save from a dialog, then closes it and reloads, or shows the error inside the dialog. */
    const saveFromDialog = async (action: () => Promise<unknown>, fallback: string) => {
        try {
            await action();
            openDialog(null);
            await load();
        } catch (err) {
            setDialogError(getErrorMessage(err, fallback));
        }
    };

    const removeItem = async (label: string, action: () => Promise<void>) => {
        if (!window.confirm(`Delete ${label}? This cannot be undone.`)) return;
        setPageError(null);
        try {
            await action();
            await load();
        } catch (err) {
            setPageError(getErrorMessage(err, `Could not delete ${label}.`));
        }
    };

    const filteredParts = useMemo(() => {
        const term = search.trim().toLowerCase();
        return term
            ? parts.filter(p => p.name.toLowerCase().includes(term) || p.supplierName.toLowerCase().includes(term))
            : parts;
    }, [parts, search]);

    const totalUnits = parts.reduce((sum, p) => sum + p.currentStockLevel, 0);
    const stockValue = parts.reduce((sum, p) => sum + p.currentStockLevel * p.costPrice, 0);
    const lowStockCount = parts.filter(p => p.currentStockLevel < LOW_STOCK).length;

    const savePart = (data: CreatePartPayload) => {
        const editing = dialog?.kind === 'part' ? dialog.part : undefined;
        return saveFromDialog(
            () => (editing ? updatePart(editing.id, data) : createPart(data)),
            'Could not save the part.');
    };

    const saveStock = (part: Part) => (data: UpdateStockPayload) =>
        saveFromDialog(() => updatePartStock(part.id, data), 'Could not update the stock level.');

    const saveSupplier = (data: SupplierPayload) => {
        const editing = dialog?.kind === 'supplier' ? dialog.supplier : undefined;
        return saveFromDialog(
            () => (editing ? updateSupplier(editing.id, data) : createSupplier(data)),
            'Could not save the supplier.');
    };

    if (isLoading) return <p className="loading">Loading stock…</p>;

    return (
        <div className="container page">
            <div className="page-header">
                <div>
                    <h1>Stock & suppliers</h1>
                    <p className="subtitle">Parts mechanics can use on jobs, and the suppliers they come from.</p>
                </div>
                <div className="page-header-actions">
                    {tab === 'parts'
                        ? <button className="btn btn-primary" onClick={() => { openDialog({ kind: 'part' }); }} disabled={suppliers.length === 0}>Add part</button>
                        : <button className="btn btn-primary" onClick={() => { openDialog({ kind: 'supplier' }); }}>Add supplier</button>}
                </div>
            </div>

            <div className="stats">
                <div className="card stat"><span className="stat-label">Parts</span><span className="stat-value">{parts.length}</span></div>
                <div className="card stat"><span className="stat-label">Units in stock</span><span className="stat-value">{totalUnits}</span></div>
                <div className="card stat"><span className="stat-label">Stock value at cost</span><span className="stat-value">{formatMoney(stockValue)}</span></div>
                <div className="card stat">
                    <span className="stat-label">Low stock (under {LOW_STOCK})</span>
                    <span className={`stat-value ${lowStockCount > 0 ? 'stat-warning' : ''}`}>{lowStockCount}</span>
                </div>
            </div>

            {pageError && <p className="alert alert-error" style={{ marginBottom: 16 }}>{pageError}</p>}

            <div className="card">
                <div className="card-header">
                    <div className="tabs">
                        <button className={tab === 'parts' ? 'active' : ''} onClick={() => { setTab('parts'); }}>Parts</button>
                        <button className={tab === 'suppliers' ? 'active' : ''} onClick={() => { setTab('suppliers'); }}>Suppliers</button>
                    </div>
                    {tab === 'parts' && (
                        <input className="table-search" type="search" placeholder="Search parts or suppliers"
                               value={search} onChange={(e) => { setSearch(e.target.value); }} />
                    )}
                </div>

                {tab === 'parts' && (
                    <div className="table-wrap">
                        <table className="table">
                            <thead>
                                <tr>
                                    <th>Part</th><th>Supplier</th>
                                    <th className="num">Cost</th><th className="num">Price</th><th className="num">In stock</th><th />
                                </tr>
                            </thead>
                            <tbody>
                                {filteredParts.map(part => (
                                    <tr key={part.id}>
                                        <td>
                                            <strong>{part.name}</strong>
                                            {part.barcode && <div className="muted mono">{part.barcode}</div>}
                                        </td>
                                        <td className="muted">{part.supplierName}</td>
                                        <td className="num muted">{formatMoney(part.costPrice)}</td>
                                        <td className="num">{formatMoney(part.price)}</td>
                                        <td className="num">
                                            {part.currentStockLevel < LOW_STOCK
                                                ? <span className="badge badge-pending">{part.currentStockLevel} low</span>
                                                : part.currentStockLevel}
                                        </td>
                                        <td className="actions">
                                            <button className="btn btn-sm btn-secondary" onClick={() => { openDialog({ kind: 'stock', part }); }}>Adjust stock</button>
                                            <button className="btn btn-sm btn-secondary" onClick={() => { openDialog({ kind: 'part', part }); }}>Edit</button>
                                            <button className="btn btn-sm btn-danger" onClick={() => void removeItem(part.name, () => deletePart(part.id))}>Delete</button>
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                        {filteredParts.length === 0 && <div className="card-body"><p className="empty-state">No parts found.</p></div>}
                    </div>
                )}

                {tab === 'suppliers' && (
                    <div className="table-wrap">
                        <table className="table">
                            <thead>
                                <tr><th>Supplier</th><th>Contact</th><th>Address</th><th className="num">Parts</th><th /></tr>
                            </thead>
                            <tbody>
                                {suppliers.map(supplier => (
                                    <tr key={supplier.id}>
                                        <td><strong>{supplier.name}</strong></td>
                                        <td>
                                            <div>{supplier.email ?? '—'}</div>
                                            <div className="muted small">{supplier.telephone ?? 'No phone'}</div>
                                        </td>
                                        <td className="muted">
                                            {[supplier.addressLine1, supplier.addressLine2, supplier.postcode].filter(Boolean).join(', ')}
                                        </td>
                                        <td className="num">{parts.filter(p => p.supplierId === supplier.id).length}</td>
                                        <td className="actions">
                                            <button className="btn btn-sm btn-secondary" onClick={() => { openDialog({ kind: 'supplier', supplier }); }}>Edit</button>
                                            <button className="btn btn-sm btn-danger" onClick={() => void removeItem(supplier.name, () => deleteSupplier(supplier.id))}>Delete</button>
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                        {suppliers.length === 0 && <div className="card-body"><p className="empty-state">No suppliers yet.</p></div>}
                    </div>
                )}
            </div>

            {dialog?.kind === 'part' && (
                <Modal title={dialog.part ? `Edit ${dialog.part.name}` : 'Add a part'} onClose={() => { openDialog(null); }}>
                    <PartForm part={dialog.part} suppliers={suppliers} onSubmit={savePart}
                              onCancel={() => { openDialog(null); }} error={dialogError} />
                </Modal>
            )}
            {dialog?.kind === 'stock' && (
                <Modal title="Adjust stock" onClose={() => { openDialog(null); }}>
                    <StockChangeForm part={dialog.part} onSubmit={saveStock(dialog.part)}
                                     onCancel={() => { openDialog(null); }} error={dialogError} />
                </Modal>
            )}
            {dialog?.kind === 'supplier' && (
                <Modal title={dialog.supplier ? `Edit ${dialog.supplier.name}` : 'Add a supplier'} onClose={() => { openDialog(null); }}>
                    <SupplierForm supplier={dialog.supplier} onSubmit={saveSupplier}
                                  onCancel={() => { openDialog(null); }} error={dialogError} />
                </Modal>
            )}
        </div>
    );
};

export default StockManagementPage;
