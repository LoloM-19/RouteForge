
import { useEffect, useState } from "react";
import AddressAutocomplete from "./AddressAutocomplete";
import type { AddressSuggestion } from "../services/addressService";
import {
    getWarehouses,
    createWarehouse,
    type Warehouse,
} from "../services/warehouseService";

interface WarehouseManagementProps {
    onWarehousesChanged?: () => void;
}

export default function WarehouseManagement({
    onWarehousesChanged,
}: WarehouseManagementProps) {
    const [warehouses, setWarehouses] = useState<Warehouse[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const [showForm, setShowForm] = useState(false);
    const [warehouseName, setWarehouseName] = useState("");
    const [addressQuery, setAddressQuery] = useState("");
    const [selectedAddress, setSelectedAddress] =
        useState<AddressSuggestion | null>(null);

    const [saving, setSaving] = useState(false);
    const [formError, setFormError] = useState<string | null>(null);
    const [successMessage, setSuccessMessage] = useState<string | null>(null);

    async function loadWarehouses() {
        try {
            setLoading(true);
            setError(null);

            const data = await getWarehouses();
            setWarehouses(data);
        } catch (err) {
            console.error(err);
            setError("Unable to load warehouses.");
        } finally {
            setLoading(false);
        }
    }

    useEffect(() => {
        void loadWarehouses();
    }, []);

    function resetForm() {
        setWarehouseName("");
        setAddressQuery("");
        setSelectedAddress(null);
        setFormError(null);
        setShowForm(false);
    }

    async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
        event.preventDefault();

        setFormError(null);
        setSuccessMessage(null);

        if (!warehouseName.trim()) {
            setFormError("Please enter a warehouse name.");
            return;
        }

        if (!selectedAddress) {
            setFormError("Please select an address from the suggestions.");
            return;
        }

        try {
            setSaving(true);

            const createdWarehouse = await createWarehouse({
                name: warehouseName.trim(),
                latitude: selectedAddress.latitude,
                longitude: selectedAddress.longitude,
            });

            await loadWarehouses();
            onWarehousesChanged?.();

            setSuccessMessage(
                `${createdWarehouse.name} was registered successfully.`
            );

            resetForm();
        } catch (err) {
            console.error(err);
            setFormError(
                err instanceof Error
                    ? err.message
                    : "Unable to register warehouse."
            );
        } finally {
            setSaving(false);
        }
    }

    return (
        <section className="warehouse-panel">
            <div className="panel-header">
                <div>
                    <h2>Registered warehouses</h2>
                    <p>Manage your distribution centres.</p>
                </div>

                <span className="demo-badge">
                    {warehouses.length} WAREHOUSES
                </span>
            </div>

            <div className="warehouse-toolbar">
                <button
                    type="button"
                    className="add-stop-button"
                    onClick={() => {
                        setShowForm(!showForm);
                        setFormError(null);
                        setSuccessMessage(null);
                    }}
                >
                    {showForm ? "Cancel" : "+ Add warehouse"}
                </button>
            </div>

            {successMessage && (
                <div className="route-result" role="status">
                    {successMessage}
                </div>
            )}

            {showForm && (
                <form className="warehouse-form" onSubmit={handleSubmit}>
                    <h3>Register a new warehouse</h3>

                    <div className="control-group">
                        <label className="address-label">
                            Warehouse name
                        </label>

                        <input
                            type="text"
                            value={warehouseName}
                            onChange={(event) =>
                                setWarehouseName(event.target.value)
                            }
                            placeholder="e.g. Cape Town Distribution Centre"
                            disabled={saving}
                            required
                        />
                    </div>

                    <div className="control-group">
                        <AddressAutocomplete
                            label="Warehouse address"
                            placeholder="Search for a warehouse address..."
                            value={addressQuery}
                            onChange={(value) => {
                                setAddressQuery(value);
                                setSelectedAddress(null);
                            }}
                            onSelect={(address) => {
                                setSelectedAddress(address);
                            }}
                            disabled={saving}
                        />
                    </div>

                    {selectedAddress && (
                        <div className="warehouse-coordinate-preview">
                            <strong>Selected location</strong>
                            <p>{selectedAddress.label}</p>
                            <p>
                                Latitude: {selectedAddress.latitude.toFixed(6)}
                            </p>
                            <p>
                                Longitude: {selectedAddress.longitude.toFixed(6)}
                            </p>
                        </div>
                    )}

                    {formError && (
                        <div className="route-error" role="alert">
                            {formError}
                        </div>
                    )}

                    <button
                        type="submit"
                        className="optimize-button"
                        disabled={saving || !selectedAddress}
                    >
                        {saving ? "Registering warehouse..." : "Register warehouse"}
                    </button>
                </form>
            )}

            {loading ? (
                <p>Loading warehouses...</p>
            ) : error ? (
                <div className="route-error" role="alert">
                    {error}
                    <button
                        type="button"
                        onClick={() => void loadWarehouses()}
                    >
                        Retry
                    </button>
                </div>
            ) : warehouses.length === 0 ? (
                <p>No warehouses have been registered yet.</p>
            ) : (
                <div className="warehouse-list">
                    {warehouses.map((warehouse) => (
                        <div className="warehouse-card" key={warehouse.id}>
                            <div>
                                <h3>{warehouse.name}</h3>
                                <p>
                                    Latitude: {warehouse.latitude.toFixed(4)}
                                </p>
                                <p>
                                    Longitude: {warehouse.longitude.toFixed(4)}
                                </p>
                            </div>

                            <span className="warehouse-status">
                                Warehouse
                            </span>
                        </div>
                    ))}
                </div>
            )}
        </section>
    );
}
