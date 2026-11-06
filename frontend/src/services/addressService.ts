
const API_BASE_URL = "http://localhost:5124/api";

export interface AddressSuggestion {
  label: string;
  latitude: number;
  longitude: number;
}

export async function searchAddresses(
  query: string,
  signal?: AbortSignal
): Promise<AddressSuggestion[]> {
  const response = await fetch(
    `${API_BASE_URL}/addresses/search?q=${encodeURIComponent(query)}`,
    { signal }
  );

  if (!response.ok) {
    throw new Error(
      `Address search failed: ${response.status} ${response.statusText}`
    );
  }

  return response.json() as Promise<AddressSuggestion[]>;
}
