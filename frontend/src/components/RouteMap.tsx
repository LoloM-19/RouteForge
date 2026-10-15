import { useEffect, useState } from "react";
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
import type { DeliveryLocation } from "../services/locationService";

interface RouteMapProps {
  warehouse: DeliveryLocation | undefined;
  optimizedRoute: DeliveryLocation[] | null;
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

const warehouseIcon = new L.Icon({
  iconUrl:
    "https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-icon-2x.png",
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
  warehouse,
  optimizedRoute,
  routeGeometry,
}: RouteMapProps){
  const [locations, setLocations] = useState<DeliveryLocation[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let active = true;

    async function loadLocations() {
      try {
        const response = await fetch("http://localhost:5124/api/locations");

        if (!response.ok) {
          throw new Error(`Could not load locations (${response.status}).`);
        }

        const data = (await response.json()) as DeliveryLocation[];

        if (active) {
          setLocations(data);
        }
      } catch (err) {
        if (active) {
          setError(
            err instanceof Error
              ? err.message
              : "An unexpected error occurred."
          );
        }
      } finally {
        if (active) {
          setLoading(false);
        }
      }
    }

    void loadLocations();

    return () => {
      active = false;
    };
  }, []);

  const mapLocations = optimizedRoute
    ? [ ...(warehouse ? [warehouse] : []), ...optimizedRoute ]
    : locations;

  const routePositions: [number, number][] =
  routeGeometry && routeGeometry.length > 1
    ? routeGeometry
    : optimizedRoute
      ? [
          ...(warehouse
            ? [[warehouse.latitude, warehouse.longitude] as [number, number]]
            : []),
          ...optimizedRoute.map(
            (stop): [number, number] => [
              stop.latitude,
              stop.longitude,
            ]
          ),
          ...(warehouse
            ? [[warehouse.latitude, warehouse.longitude] as [number, number]]
            : []),
        ]
      : [];

  const fallbackCenter: [number, number] = [-26.2041, 28.0473];
  const center: [number, number] = warehouse
    ? [warehouse.latitude, warehouse.longitude]
    : fallbackCenter;

  if (loading) {
    return <div className="map-message">Loading delivery locations...</div>;
  }

  if (error) {
    return (
      <div className="map-message map-error">
        <strong>Could not load locations</strong>
        <p>{error}</p>
        <p>Check that the C# API is running on port 5124.</p>
      </div>
    );
  }

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


        {mapLocations.map((location) => {
          const isWarehouse = location.id === warehouse?.id;

          return (
            <Marker
              key={location.id}
              position={[location.latitude, location.longitude]}
              icon={isWarehouse ? warehouseIcon : defaultIcon}
            >
              <Popup>
                <strong>{location.name}</strong>
                <br />
                {isWarehouse ? "Warehouse" : "Delivery location"}
              </Popup>
            </Marker>
          );
        })}

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
            <MapView positions={routePositions} />
          </>
        )}
      </MapContainer>
    </div>
  );
}