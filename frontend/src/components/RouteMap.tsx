
import { useEffect } from "react";
import {
  MapContainer,
  TileLayer,
  Marker,
  Popup,
  Polyline,
  useMap,
} from "react-leaflet";
import L from "leaflet";
import "leaflet/dist/leaflet.css";

export interface MapLocation {
  name: string;
  latitude: number;
  longitude: number;
}

interface RouteMapProps {
  start: MapLocation | null;
  end: MapLocation | null;
  optimizedRoute: MapLocation[] | null;
  routeGeometry?: [number, number][] | null;
}

const defaultIcon = new L.Icon({
  iconUrl:
    "https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-icon.png",
  iconRetinaUrl:
    "https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-icon-2x.png",
  shadowUrl:
    "https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-shadow.png",
  iconSize: [25, 41],
  iconAnchor: [12, 41],
  popupAnchor: [1, -34],
});

const startIcon = new L.Icon({
  iconUrl:
    "https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-icon-2x.png",
  shadowUrl:
    "https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-shadow.png",
  iconSize: [25, 41],
  iconAnchor: [12, 41],
  popupAnchor: [1, -34],
});

const endIcon = new L.Icon({
  iconUrl:
    "https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-icon.png",
  shadowUrl:
    "https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-shadow.png",
  iconSize: [25, 41],
  iconAnchor: [12, 41],
  popupAnchor: [1, -34],
});

function MapView({
  positions,
}: {
  positions: [number, number][];
}) {
  const map = useMap();

  useEffect(() => {
    if (positions.length > 1) {
      map.fitBounds(positions, { padding: [35, 35] });
    } else if (positions.length === 1) {
      map.setView(positions[0], 12);
    }
  }, [map, positions]);

  return null;
}

export default function RouteMap({
  start,
  end,
  optimizedRoute,
  routeGeometry,
}: RouteMapProps) {
  const fallbackCenter: [number, number] = [-26.2041, 28.0473];

  const center: [number, number] = start
    ? [start.latitude, start.longitude]
    : fallbackCenter;

  const routePositions: [number, number][] =
    routeGeometry && routeGeometry.length > 1
      ? routeGeometry
      : start && optimizedRoute
        ? [
            [start.latitude, start.longitude],
            ...optimizedRoute.map(
              (stop): [number, number] => [
                stop.latitude,
                stop.longitude,
              ]
            ),
            ...(end
              ? [[end.latitude, end.longitude] as [number, number]]
              : []),
          ]
        : [];

  const displayLocations: {
    location: MapLocation;
    role: "start" | "stop" | "end";
    key: string;
  }[] = [];

  if (start) {
    displayLocations.push({
      location: start,
      role: "start",
      key: "start",
    });
  }

  if (optimizedRoute) {
    optimizedRoute.forEach((location, index) => {
      displayLocations.push({
        location,
        role: "stop",
        key: `stop-${index}-${location.name}`,
      });
    });
  }

  if (end && (start?.latitude !== end.latitude ||
      start?.longitude !== end.longitude)) {
    displayLocations.push({
      location: end,
      role: "end",
      key: "end",
    });
  }

  const positionsToDisplay = routePositions.length > 0
    ? routePositions
    : start
      ? [[start.latitude, start.longitude] as [number, number]]
      : [];

  return (
    <div style={{ height: "520px", width: "100%" }}>
      <MapContainer
        center={center}
        zoom={11}
        scrollWheelZoom
        style={{ height: "100%", width: "100%", borderRadius: "12px" }}
      >
        <TileLayer
          attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors | Routing by <a href="https://project-osrm.org/">OSRM</a>'
          url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
        />

        {displayLocations.map(({ location, role, key }) => (
          <Marker
            key={key}
            position={[location.latitude, location.longitude]}
            icon={
              role === "start"
                ? startIcon
                : role === "end"
                  ? endIcon
                  : defaultIcon
            }
          >
            <Popup>
              <strong>{location.name}</strong>
              <br />
              {role === "start"
                ? "Starting point"
                : role === "end"
                  ? "Finishing point"
                  : "Delivery stop"}
            </Popup>
          </Marker>
        ))}

        {optimizedRoute && routePositions.length > 1 && (
          <>
            <Polyline
              positions={routePositions}
              pathOptions={{
                color: "#2563eb",
                weight: 5,
                opacity: 0.9,
              }}
            />
            <MapView positions={positionsToDisplay} />
          </>
        )}
      </MapContainer>
    </div>
  );
}
