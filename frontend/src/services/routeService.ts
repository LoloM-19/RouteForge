import type { DeliveryLocation } from "./locationService";

const API_BASE_URL = "http://localhost:5124/api";

export interface OptimizeRouteRequest {
  warehouseId: number;
  deliveryLocationIds: number[];
}


export interface RouteOptimizationResult {
  optimizedRoute: DeliveryLocation[];
  routeGeometry: [number, number][];
  totalDistanceKm: number;
  originalDistanceKm: number;
  nearestNeighbourDistanceKm: number;
  distanceSavedKm: number;
  improvementPercentage: number;
  twoOptDistanceSavedKm: number;
  twoOptImprovementPercentage: number;
}

export async function optimizeRoute(
  request: OptimizeRouteRequest
): Promise<RouteOptimizationResult> {
  const response = await fetch(`${API_BASE_URL}/routes/optimize`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(request),
  });

  if (!response.ok) {
    const message = await response.text();

    throw new Error(
      message || `Route optimization failed: ${response.status}`
    );
  }

  return response.json() as Promise<RouteOptimizationResult>;
}