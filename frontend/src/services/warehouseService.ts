
const API_BASE_URL = "http://localhost:5124/api";

export interface Warehouse {
    id: number;
    name: string;
    latitude: number;
    longitude: number;
    type: number;
}

export interface WarehouseRequest {
    name: string;
    latitude: number;
    longitude: number;
}

export async function getWarehouses(): Promise<Warehouse[]> {
    const response = await fetch(`${API_BASE_URL}/warehouses`);

    if (!response.ok) {
        throw new Error(`Failed to fetch warehouses: ${response.status}`);
    }

    return response.json() as Promise<Warehouse[]>;
}

export async function createWarehouse(
    warehouse: WarehouseRequest
): Promise<Warehouse> {
    const response = await fetch(`${API_BASE_URL}/warehouses`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
        },
        body: JSON.stringify(warehouse),
    });

    if (!response.ok) {
        const message = await response.text();
        throw new Error(message || `Failed to create warehouse: ${response.status}`);
    }

    return response.json() as Promise<Warehouse>;
}

export async function updateWarehouse(
    id: number,
    warehouse: WarehouseRequest
): Promise<Warehouse> {
    const response = await fetch(`${API_BASE_URL}/warehouses/${id}`, {
        method: "PUT",
        headers: {
            "Content-Type": "application/json",
        },
        body: JSON.stringify(warehouse),
    });

    if (!response.ok) {
        const message = await response.text();
        throw new Error(message || `Failed to update warehouse: ${response.status}`);
    }

    return response.json() as Promise<Warehouse>;
}

export async function deleteWarehouse(id: number): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/warehouses/${id}`, {
        method: "DELETE",
    });

    if (!response.ok) {
        const message = await response.text();
        throw new Error(message || `Failed to delete warehouse: ${response.status}`);
    }
}
