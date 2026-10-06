export interface Supplier {
    id: number;
    name: string;
    addressLine1: string | null;
    addressLine2: string | null;
    postcode: string;
    telephone: string | null;
    email: string | null;
}

export type SupplierPayload = Omit<Supplier, 'id'>;

export interface Part {
    id: number;
    name: string;
    /** Price charged to the customer. */
    price: number;
    /** Price paid to the supplier. */
    costPrice: number;
    currentStockLevel: number;
    barcode: string | null;
    supplierId: number;
    supplierName: string;
}

export interface CreatePartPayload {
    name: string;
    price: number;
    costPrice: number;
    initialStockLevel: number;
    barcode: string | null;
    supplierId: number;
}

export type UpdatePartPayload = Omit<CreatePartPayload, 'initialStockLevel'>;

export interface UpdateStockPayload {
    /** Positive for a delivery, negative for a correction. */
    changeInQuantity: number;
    reason: string | null;
}
