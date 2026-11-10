import { useEffect, useState } from "react";
import RouteMap from "./components/RouteMap";
import AddressAutocomplete from "./components/AddressAutocomplete";
import type { AddressSuggestion } from "./services/addressService";
import {
  optimizeRoute,
  type RouteOptimizationResult,
  type RouteLocation,
} from "./services/routeService";
import { getWarehouses, type Warehouse } from "./services/warehouseService";
import "./App.css";
import WarehouseManagement from "./components/WarehouseManagement";
import DeliveryLocationManagement from "./components/DeliveryLocationManagement";

interface StopEntry {
  id: number;
  query: string;
  address: AddressSuggestion | null;
}

function App() {
  const [activePage, setActivePage] = useState<
    "dashboard" | "routes" | "locations" | "warehouses"
  >("routes");
  const [endQuery, setEndQuery] = useState("");
  const [endAddress, setEndAddress] = useState<AddressSuggestion | null>(null);

  const [returnToStart, setReturnToStart] = useState(true);

  const [stops, setStops] = useState<StopEntry[]>([
    { id: 1, query: "", address: null },
  ]);
  const [nextStopId, setNextStopId] = useState(2);

  const [result, setResult] = useState<RouteOptimizationResult | null>(null);
  const [optimizing, setOptimizing] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [warehouses, setWarehouses] = useState<Warehouse[]>([]);
  const [loadingWarehouses, setLoadingWarehouses] = useState(true);
  const [warehouseError, setWarehouseError] = useState<string | null>(null);

  const [selectedWarehouseId, setSelectedWarehouseId] = useState<number | null>(null);

  async function loadWarehouses() {
    try {
        setLoadingWarehouses(true);
        setWarehouseError(null);

        const data = await getWarehouses();

        setWarehouses(data);

        setSelectedWarehouseId(currentId => {
            const existingWarehouse = data.find(
                warehouse => warehouse.id === currentId
            );

            return existingWarehouse?.id ?? data[0]?.id ?? null;
        });
    } catch (error) {
        setWarehouseError("Unable to load warehouses.");
        console.error(error);
    } finally {
        setLoadingWarehouses(false);
    }
  }

  useEffect(() => {
      void loadWarehouses();
  }, []);

  const selectedWarehouse = warehouses.find(
    warehouse => warehouse.id === selectedWarehouseId
  );

  function clearResult() {
    setResult(null);
    setError(null);
  }

  function addStop() {
    if (stops.length >= 25) {
      setError("A maximum of 25 delivery stops is allowed.");
      return;
    }

    setStops((current) => [
      ...current,
      { id: nextStopId, query: "", address: null },
    ]);
    setNextStopId((current) => current + 1);
    clearResult();
  }

  function removeStop(id: number) {
    setStops((current) => current.filter((stop) => stop.id !== id));
    clearResult();
  }

  function updateStopQuery(id: number, query: string) {
    setStops((current) =>
      current.map((stop) =>
        stop.id === id
          ? { ...stop, query, address: null }
          : stop
      )
    );
    clearResult();
  }

  function selectStopAddress(id: number, address: AddressSuggestion) {
    setStops((current) =>
      current.map((stop) =>
        stop.id === id
          ? { ...stop, query: address.label, address }
          : stop
      )
    );
    clearResult();
  }

  async function handleOptimizeRoute() {
    if (!selectedWarehouse) {
    setError("Please select a starting warehouse.");
    return;
    }

    if (!returnToStart && !endAddress) {
      setError("Please select a valid destination address from the suggestions.");
      return;
    }

    if (stops.length === 0) {
      setError("Please add at least one delivery stop.");
      return;
    }

    if (stops.some((stop) => !stop.address)) {
      setError("Please select a valid address for every delivery stop.");
      return;
    }

    try {
      setOptimizing(true);
      setError(null);
      setResult(null);

      const start: RouteLocation = {
          name: selectedWarehouse.name,
          latitude: selectedWarehouse.latitude,
          longitude: selectedWarehouse.longitude,
      };

      const end: RouteLocation = returnToStart
        ? start
        : {
            name: endAddress!.label,
            latitude: endAddress!.latitude,
            longitude: endAddress!.longitude,
          };

      const deliveryStops: RouteLocation[] = stops.map((stop) => ({
        name: stop.address!.label,
        latitude: stop.address!.latitude,
        longitude: stop.address!.longitude,
      }));

      const optimizedResult = await optimizeRoute({
        start,
        end,
        stops: deliveryStops,
      });

      setResult(optimizedResult);
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : "Route optimization failed."
      );
    } finally {
      setOptimizing(false);
    }
  }

  const formatDistance = (distance: number) =>
    `${distance.toFixed(2)} km`;

  const selectedEnd = returnToStart ? selectedWarehouse : endAddress;

  const mapStart: RouteLocation | null = selectedWarehouse
    ? {
        name: selectedWarehouse.name,
        latitude: selectedWarehouse.latitude,
        longitude: selectedWarehouse.longitude,
    }
    : null;

  const mapEnd: RouteLocation | null = selectedEnd
    ? {
        name: "name" in selectedEnd
            ? selectedEnd.name
            : selectedEnd.label,
        latitude: selectedEnd.latitude,
        longitude: selectedEnd.longitude,
    }
    : null;

  const mapRoute: RouteLocation[] | null = result
    ? result.optimizedRoute.map((stop) => ({
        name: stop.name,
        latitude: stop.latitude,
        longitude: stop.longitude,
      }))
    : null;

  return (
    <div className="app">
      <aside className="sidebar">
        <div className="brand">
          <span className="brand-icon">R</span>
          <span>RouteForge</span>
        </div>

        <p className="nav-label">WORKSPACE</p>

        <nav>
          <button
            type="button"
            className={`nav-item ${activePage === "dashboard" ? "active" : ""}`}
            onClick={() => setActivePage("dashboard")}
          >
            <span>▦</span> Dashboard
          </button>

          <button
            type="button"
            className={`nav-item ${activePage === "routes" ? "active" : ""}`}
            onClick={() => setActivePage("routes")}
          >
            <span>⌁</span> Route Planning
          </button>

          <button
            type="button"
            className={`nav-item ${activePage === "locations" ? "active" : ""}`}
            onClick={() => setActivePage("locations")}
          >
            <span>⌖</span> Delivery Locations
          </button>

          <button
            type="button"
            className={`nav-item ${activePage === "warehouses" ? "active" : ""}`}
            onClick={() => setActivePage("warehouses")}
          >
            <span>▤</span> Warehouse Management
          </button>
        </nav>

        <div className="sidebar-footer">
          <span className="status-dot" />
          <span>RouteForge API</span>
          <span className="status-text">Connected</span>
        </div>
      </aside>

      <main className="main-content" id={activePage}>
        <header className="topbar">
          <div>
            <p className="eyebrow">
                {activePage === "warehouses"
                    ? "OPERATIONS / WAREHOUSES"
                    : activePage === "locations"
                        ? "OPERATIONS / LOCATIONS"
                        : activePage === "dashboard"
                            ? "OVERVIEW"
                            : "OPERATIONS / ROUTE PLANNING"}
            </p>

            <h1>
                {activePage === "warehouses"
                    ? "Warehouse Management"
                    : activePage === "locations"
                        ? "Delivery Locations"
                        : activePage === "dashboard"
                            ? "Dashboard"
                            : "Route Planning"}
            </h1>

            <p className="subtitle">
                {activePage === "warehouses"
                    ? "Manage your registered distribution centres."
                    : activePage === "locations"
                        ? "Manage your delivery destinations."
                        : activePage === "dashboard"
                            ? "Your logistics operations at a glance."
                            : "Plan efficient delivery routes using real-world addresses."}
            </p>
          </div>
          <div className="user-avatar">RF</div>
        </header>
        {activePage === "routes" && (
            <>
        <section className="stats-grid">
          <article className="stat-card">
            <p className="stat-label">Delivery stops</p>
            <h2>{result ? result.optimizedRoute.length : stops.length}</h2>
            <p className="stat-note">
              {result ? "Stops in optimized route" : "Stops added"}
            </p>
          </article>

          <article className="stat-card">
            <p className="stat-label">Optimized distance</p>
            <h2>
              {result
                ? formatDistance(result.totalDistanceKm)
                : "Not calculated"}
            </h2>
            <p className="stat-note">Total road distance</p>
          </article>

          <article className="stat-card">
            <p className="stat-label">Distance saved</p>
            <h2>
              {result
                ? formatDistance(result.distanceSavedKm)
                : "Not calculated"}
            </h2>
            <p className="stat-note">
              {result
                ? `${result.improvementPercentage.toFixed(1)}% improvement`
                : "Compared with original stop order"}
            </p>
          </article>
        </section>

        <section className="map-panel" id="routes">
          <div className="panel-header">
            <div>
              <h2>Delivery route</h2>
              <p>Plan your route between any supported addresses</p>
            </div>
            <span className="demo-badge">
              {result ? "OPTIMIZED ROUTE" : "ROUTE PLANNER"}
            </span>
          </div>

          <div className="route-controls" id="locations">
           <div className="control-group">
              <label className="address-label">Starting warehouse</label>

              {loadingWarehouses ? (
                  <p>Loading warehouses...</p>
              ) : warehouseError ? (
                  <p className="address-error">{warehouseError}</p>
              ) : warehouses.length === 0 ? (
                  <p>No warehouses have been registered.</p>
              ) : (
                  <select
                      value={selectedWarehouseId ?? ""}
                      onChange={(event) => {
                          setSelectedWarehouseId(Number(event.target.value));
                          clearResult();
                      }}
                      disabled={optimizing}
                  >
                      {warehouses.map((warehouse) => (
                          <option key={warehouse.id} value={warehouse.id}>
                              {warehouse.name}
                          </option>
                      ))}
                  </select>
                )}
            </div>

            <div className="control-group">
              <label className="stop-option">
                <input
                  type="checkbox"
                  checked={returnToStart}
                  onChange={(event) => {
                    setReturnToStart(event.target.checked);
                    clearResult();
                  }}
                  disabled={optimizing}
                />
                Return to starting address
              </label>
            </div>

            {!returnToStart && (
              <div className="control-group">
                <AddressAutocomplete
                  label="Final destination"
                  placeholder="Search for your final destination..."
                  value={endQuery}
                  onChange={(value) => {
                    setEndQuery(value);
                    setEndAddress(null);
                    clearResult();
                  }}
                  onSelect={(address) => {
                    setEndAddress(address);
                    clearResult();
                  }}
                  disabled={optimizing}
                />
              </div>
            )}

            <div className="control-group">
              <div className="panel-header">
                <div>
                  <h2>Delivery locations</h2>
                  <p>Addresses where packages must be delivered</p>
                </div>
                <span>{stops.length} / 25</span>
              </div>

              <div className="stop-list">
                {stops.map((stop, index) => (
                  <div className="stop-option" key={stop.id}>
                    <span>{index + 1}.</span>
                    <div className="stop-input">
                      <AddressAutocomplete
                        label={`Delivery stop ${index + 1}`}
                        placeholder="Search for a delivery address..."
                        value={stop.query}
                        onChange={(value) => updateStopQuery(stop.id, value)}
                        onSelect={(address) =>
                          selectStopAddress(stop.id, address)
                        }
                        disabled={optimizing}
                      />
                    </div>

                    <button
                      type="button"
                      className="remove-stop-button"
                      onClick={() => removeStop(stop.id)}
                      disabled={optimizing || stops.length === 1}
                      aria-label={`Remove delivery stop ${index + 1}`}
                    >
                      Remove
                    </button>
                  </div>
                ))}
              </div>

              <button
                type="button"
                className="add-stop-button"
                onClick={addStop}
                disabled={optimizing || stops.length >= 25}
              >
                + Add delivery stop
              </button>
            </div>

            <button
              className="optimize-button"
              onClick={() => void handleOptimizeRoute()}
              disabled={
                optimizing ||
                !selectedWarehouse ||
                stops.length === 0 ||
                stops.some((stop) => !stop.address) ||
                (!returnToStart && !endAddress)
              }
            >
              {optimizing ? "Optimizing route..." : "Optimize Route"}
            </button>

            {error && (
              <div className="route-error" role="alert">
                {error}
              </div>
            )}

            {result && (
              <div className="route-result" role="status">
                <strong>Route optimized successfully</strong>
                <p>
                  Original distance:{" "}
                  {formatDistance(result.originalDistanceKm)}
                </p>
                <p>
                  Nearest Neighbour:{" "}
                  {formatDistance(result.nearestNeighbourDistanceKm)}
                </p>
                <p>
                  Additional distance saved by 2-opt:{" "}
                  {formatDistance(result.twoOptDistanceSavedKm)}
                </p>
                <p>
                  Final route:{" "}
                  {[
                    selectedWarehouse?.name,
                    ...result.optimizedRoute.map((stop) => stop.name),
                    selectedEnd
                        ? ("name" in selectedEnd ? selectedEnd.name : selectedEnd.label)
                        : "",
                  ].filter(Boolean).join(" → ")}
                </p>
              </div>
            )}
          </div>

          <RouteMap
            start={mapStart}
            end={mapEnd}
            optimizedRoute={mapRoute}
            routeGeometry={result?.routeGeometry ?? null}
          />
        </section>
            </>
    )}

    {activePage === "warehouses" && (
    <WarehouseManagement
        onWarehousesChanged={loadWarehouses}
    />
    )}

        {activePage === "dashboard" && (
            <section className="map-panel">
                <div className="panel-header">
                    <div>
                        <h2>Dashboard</h2>
                        <p>Your logistics overview will appear here.</p>
                    </div>
                </div>
            </section>
        )}

        {activePage === "locations" && (
            <DeliveryLocationManagement
                onLocationsChanged={clearResult}
            />
        )}

        <footer className="page-footer">
          RouteForge · Intelligent logistics route optimization
        </footer>
      </main>
    </div>
  );
}

export default App;
