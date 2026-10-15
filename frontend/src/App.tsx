import { useEffect, useState } from "react";
import RouteMap from "./components/RouteMap";
import {
  getLocations,
  type DeliveryLocation,
} from "./services/locationService";
import {
  optimizeRoute,
  type RouteOptimizationResult,
} from "./services/routeService";
import "./App.css";

function App() {
  const [locations, setLocations] = useState<DeliveryLocation[]>([]);
  const [warehouseId, setWarehouseId] = useState<number | "">("");
  const [selectedStopIds, setSelectedStopIds] = useState<number[]>([]);
  const [result, setResult] = useState<RouteOptimizationResult | null>(null);
  const [loadingLocations, setLoadingLocations] = useState(true);
  const [optimizing, setOptimizing] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    async function loadLocations() {
      try {
        const data = await getLocations();
        setLocations(data);

        const warehouse = data.find((location) =>
          location.name.toLowerCase().includes("warehouse")
        );

        if (warehouse) {
          setWarehouseId(warehouse.id);
        }
      } catch (err) {
        setError(
          err instanceof Error
            ? err.message
            : "Could not load delivery locations."
        );
      } finally {
        setLoadingLocations(false);
      }
    }

    void loadLocations();
  }, []);

  const warehouse = locations.find(
    (location) => location.id === warehouseId
  );

  const availableStops = locations.filter(
    (location) => location.id !== warehouseId
  );

  function toggleStop(id: number) {
    setSelectedStopIds((current) =>
      current.includes(id)
        ? current.filter((stopId) => stopId !== id)
        : [...current, id]
    );

    setResult(null);
  }

  async function handleOptimizeRoute() {
    if (warehouseId === "") {
      setError("Please select a warehouse.");
      return;
    }

    if (selectedStopIds.length === 0) {
      setError("Please select at least one delivery stop.");
      return;
    }

    try {
      setOptimizing(true);
      setError(null);

      const optimizedResult = await optimizeRoute({
        warehouseId,
        deliveryLocationIds: selectedStopIds,
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

  return (
    <div className="app">
      <aside className="sidebar">
        <div className="brand">
          <span className="brand-icon">R</span>
          <span>RouteForge</span>
        </div>

        <p className="nav-label">WORKSPACE</p>

        <nav>
          <a href="#dashboard" className="nav-item">
            <span>▦</span> Dashboard
          </a>
          <a href="#routes" className="nav-item active">
            <span>⌁</span> Route Planning
          </a>
          <a href="#locations" className="nav-item">
            <span>⌖</span> Delivery Locations
          </a>
        </nav>

        <div className="sidebar-footer">
          <span className="status-dot" />
          <span>RouteForge API</span>
          <span className="status-text">Connected</span>
        </div>
      </aside>

      <main className="main-content" id="dashboard">
        <header className="topbar">
          <div>
            <p className="eyebrow">OPERATIONS / ROUTE PLANNING</p>
            <h1>Route Planning</h1>
            <p className="subtitle">
              Visualize delivery stops and plan more efficient routes.
            </p>
          </div>
          <div className="user-avatar">RF</div>
        </header>

        <section className="stats-grid">
          <article className="stat-card">
            <p className="stat-label">Delivery stops</p>
            <h2>{result ? result.optimizedRoute.length : selectedStopIds.length}</h2>
            <p className="stat-note">
              {result ? "Stops in optimized route" : "Stops selected"}
            </p>
          </article>

          <article className="stat-card">
            <p className="stat-label">Optimized distance</p>
            <h2>
              {result ? formatDistance(result.totalDistanceKm) : "Not calculated"}
            </h2>
            <p className="stat-note">Including return to warehouse</p>
          </article>

          <article className="stat-card">
            <p className="stat-label">Distance saved</p>
            <h2>
              {result ? formatDistance(result.distanceSavedKm) : "Not calculated"}
            </h2>
            <p className="stat-note">
              {result
                ? `${result.improvementPercentage.toFixed(1)}% improvement`
                : "Compared with selected stop order"}
            </p>
          </article>
        </section>

        <section className="map-panel" id="routes">
          <div className="panel-header">
            <div>
              <h2>Delivery route</h2>
              <p>Johannesburg distribution area</p>
            </div>
            <span className="demo-badge">
              {result ? "OPTIMIZED ROUTE" : "LIVE LOCATIONS"}
            </span>
          </div>

          <div className="route-controls" id="locations">
            <div className="control-group">
              <label htmlFor="warehouse-select">Warehouse</label>
              <select
                id="warehouse-select"
                value={warehouseId}
                onChange={(event) => {
                  const value = event.target.value;
                  setWarehouseId(value === "" ? "" : Number(value));
                  setSelectedStopIds([]);
                  setResult(null);
                  setError(null);
                }}
                disabled={loadingLocations || optimizing}
              >
                <option value="">Select a warehouse</option>
                {locations.map((location) => (
                  <option key={location.id} value={location.id}>
                    {location.name}
                  </option>
                ))}
              </select>
            </div>

            <div className="control-group">
              <span className="control-label">Delivery stops</span>
              {loadingLocations ? (
                <p>Loading locations...</p>
              ) : availableStops.length === 0 ? (
                <p>Add delivery locations to your database to plan a route.</p>
              ) : (
                <div className="stop-list">
                  {availableStops.map((stop) => (
                    <label className="stop-option" key={stop.id}>
                      <input
                        type="checkbox"
                        checked={selectedStopIds.includes(stop.id)}
                        onChange={() => toggleStop(stop.id)}
                        disabled={optimizing}
                      />
                      <span>{stop.name}</span>
                    </label>
                  ))}
                </div>
              )}
            </div>

            <button
              className="optimize-button"
              onClick={() => void handleOptimizeRoute()}
              disabled={
                loadingLocations ||
                optimizing ||
                warehouseId === "" ||
                selectedStopIds.length === 0
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
                  Nearest Neighbour:{" "}
                  {formatDistance(result.nearestNeighbourDistanceKm)}
                </p>
                <p>
                  Additional distance saved by 2-opt:{" "}
                  {formatDistance(result.twoOptDistanceSavedKm)}
                </p>
                <p>
                  Final route: Warehouse →{" "}
                  {result.optimizedRoute.map((stop) => stop.name).join(" → ")}{" "}
                  → Warehouse
                </p>
              </div>
            )}
          </div>

          
        <RouteMap
          warehouse={warehouse}
          optimizedRoute={result?.optimizedRoute ?? null}
          routeGeometry={result?.routeGeometry ?? null}
        />
        </section>

        <footer className="page-footer">
          RouteForge · Intelligent logistics route optimization
        </footer>
      </main>
    </div>
  );
}

export default App;