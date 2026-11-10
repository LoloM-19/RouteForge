
const API_BASE_URL = "http://localhost:5124/api";

export interface DeliveryLocation {
    id: number;
    name: string;
    latitude: number;
    longitude: number;
    type: number;
}

export interface DeliveryLocationRequest {
    name: string;
    latitude: number;
    longitude: number;
}

// READ: Retrieve all delivery locations
export async function getLocations(): Promise<DeliveryLocation[]> {
    const response = await fetch(`${API_BASE_URL}/locations`);

    if (!response.ok) {
        throw new Error(
            `Failed to fetch delivery locations: ${response.status} ${response.statusText}`
        );
    }

    return response.json() as Promise<DeliveryLocation[]>;
}

// CREATE: Register a new delivery location
export async function createLocation(
    location: DeliveryLocationRequest
): Promise<DeliveryLocation> {
    const response = await fetch(`${API_BASE_URL}/locations`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
        },
        body: JSON.stringify(location),
    });

    if (!response.ok) {
        const message = await response.text();
        throw new Error(message || `Failed to create location: ${response.status}`);
    }

    return response.json() as Promise<DeliveryLocation>;
}

// UPDATE: Modify an existing delivery location
export async function updateLocation(
    id: number,
    location: DeliveryLocationRequest
): Promise<DeliveryLocation> {
    const response = await fetch(`${API_BASE_URL}/locations/${id}`, {
        method: "PUT",
        headers: {
            "Content-Type": "application/json",
        },
        body: JSON.stringify(location),
    });

    if (!response.ok) {
        const message = await response.text();
        throw new Error(message || `Failed to update location: ${response.status}`);
    }

    return response.json() as Promise<DeliveryLocation>;
}

// DELETE: Remove an existing delivery location
export async function deleteLocation(id: number): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/locations/${id}`, {
        method: "DELETE",
    });

    if (!response.ok) {
        const message = await response.text();
        throw new Error(message || `Failed to delete location: ${response.status}`);
    }
}
