
const API_BASE_URL = "http://localhost:5124/api";

export interface RouteLocation {
  name: string;
  latitude: number;
  longitude: number;
}

export interface OptimizeRouteRequest {
  start: RouteLocation;
  end: RouteLocation;
  stops: RouteLocation[];
}

export interface RouteOptimizationResult {
  optimizedRoute: {
    id: number;
    name: string;
    latitude: number;
    longitude: number;
    type: number;
  }[];

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
    body: JSON.stringify({
      startLatitude: request.start.latitude,
      startLongitude: request.start.longitude,
      startName: request.start.name,

      endLatitude: request.end.latitude,
      endLongitude: request.end.longitude,
      endName: request.end.name,

      stops: request.stops.map((stop) => ({
        name: stop.name,
        latitude: stop.latitude,
        longitude: stop.longitude,
      })),
    }),
  });

  if (!response.ok) {
    const message = await response.text();

    throw new Error(
      message || `Route optimization failed: ${response.status}`
    );
  }

  return response.json() as Promise<RouteOptimizationResult>;
}
