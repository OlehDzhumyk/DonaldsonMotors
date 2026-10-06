
/**
 * Represents a Supplier as returned by the API.
 */
export interface Supplier {
    id: number;
    name: string;
    addressLine1: string;
    addressLine2: string | null;
    postcode: string;
    telephone: string | null; // API example shows it can be null, but create payload has 'telephone'
    email: string;
}

/**
 * Payload for creating a new Supplier.
 * Based on POST /api/Suppliers cURL example.
 */
export interface CreateSupplierPayload {
    name: string;
    addressLine1: string;
    addressLine2?: string | null; // Optional in creation
    postcode: string;
    telephone: string; // cURL example sends 'telephone' not 'telephoneNumber'
    email: string;
}

/**
 * Payload for updating an existing Supplier.
 * Can be partial, based on PUT /api/Suppliers/{id} cURL example.
 */
export interface UpdateSupplierPayload {
    name?: string;
    addressLine1?: string;
    addressLine2?: string | null;
    postcode?: string;
    telephone?: string;
    email?: string;
}

/**
 * Represents a Part (Stock Item) as returned by the API.
 */
export interface Part {
    id: number;
    name: string;
    price: number;
    costPrice: number; // Included from API response
    currentStockLevel: number;
    barcode: string | null;
    supplierId: number; // Assuming this is always present for a part linked to a supplier
    supplierName: string; // Comes with the part list
}

/**
 * Payload for creating a new Part.
 * Based on common fields and PartsController likelihood.
 * The API for creating a part was not in the last cURL,
 * so this is an educated guess based on Part properties.
 */
export interface CreatePartPayload {
    name: string;
    price: number;
    costPrice?: number;
    initialStockLevel: number; // Typically, you set initial stock on creation
    barcode?: string | null;
    supplierId: number; // A part must belong to a supplier
}

/**
 * Payload for updating an existing Part.
 * Based on PUT /api/Parts/{id} cURL example (partial update).
 */
export interface UpdatePartPayload {
    name?: string;
    price?: number;
    costPrice?: number;
    barcode?: string | null;
    // supplierId is typically not changed this way, currentStockLevel is via a dedicated endpoint.
}

/**
 * Payload for updating the stock level of a Part.
 * Based on PATCH /api/Parts/{id}/stock cURL example.
 */
export interface UpdateStockPayload {
    changeInQuantity: number;
    reason?: string; // API example included reason
}