
const API_BASE_URL = "http://localhost:5124/api";

export interface DeliveryLocation {
  id: number;
  name: string;
  latitude: number;
  longitude: number;
}

export async function getLocations(): Promise<DeliveryLocation[]> {
  const response = await fetch(`${API_BASE_URL}/locations`);

  if (!response.ok) {
    throw new Error(
      `Failed to fetch delivery locations: ${response.status} ${response.statusText}`
    );
  }

  return response.json() as Promise<DeliveryLocation[]>;
}
