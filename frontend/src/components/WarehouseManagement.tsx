
import { useEffect, useState } from "react";
import AddressAutocomplete from "./AddressAutocomplete";
import type { AddressSuggestion } from "../services/addressService";
import {
    getWarehouses,
    createWarehouse,
    updateWarehouse,
    deleteWarehouse,
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

    // Registration form
    const [showForm, setShowForm] = useState(false);
    const [warehouseName, setWarehouseName] = useState("");
    const [addressQuery, setAddressQuery] = useState("");
    const [selectedAddress, setSelectedAddress] =
        useState<AddressSuggestion | null>(null);

    // Editing form
    const [editingWarehouseId, setEditingWarehouseId] =
        useState<number | null>(null);
    const [editWarehouseName, setEditWarehouseName] = useState("");
    const [editAddressQuery, setEditAddressQuery] = useState("");
    const [editSelectedAddress, setEditSelectedAddress] =
        useState<AddressSuggestion | null>(null);
    const [editAddressChanged, setEditAddressChanged] = useState(false);

    // Form and operation feedback
    const [saving, setSaving] = useState(false);
    const [deletingId, setDeletingId] = useState<number | null>(null);
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

    function startEditing(warehouse: Warehouse) {
        setShowForm(false);
        setEditingWarehouseId(warehouse.id);
        setEditWarehouseName(warehouse.name);
        setEditAddressQuery("");
        setEditSelectedAddress(null);
        setEditAddressChanged(false);
        setFormError(null);
        setSuccessMessage(null);
    }

    function cancelEditing() {
        setEditingWarehouseId(null);
        setEditWarehouseName("");
        setEditAddressQuery("");
        setEditSelectedAddress(null);
        setEditAddressChanged(false);
        setFormError(null);
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

    async function handleUpdate(
        event: React.FormEvent<HTMLFormElement>,
        warehouse: Warehouse
    ) {
        event.preventDefault();

        setFormError(null);
        setSuccessMessage(null);

        if (!editWarehouseName.trim()) {
            setFormError("Please enter a warehouse name.");
            return;
        }

        if (editAddressChanged && !editSelectedAddress) {
            setFormError(
                "Please select an address from the suggestions, or clear the address field to keep the existing location."
            );
            return;
        }

        try {
            setSaving(true);

            const updatedWarehouse = await updateWarehouse(
                warehouse.id,
                {
                    name: editWarehouseName.trim(),
                    latitude: editSelectedAddress?.latitude ?? warehouse.latitude,
                    longitude: editSelectedAddress?.longitude ?? warehouse.longitude,
                }
            );

            await loadWarehouses();
            onWarehousesChanged?.();

            setSuccessMessage(
                `${updatedWarehouse.name} was updated successfully.`
            );

            cancelEditing();
        } catch (err) {
            console.error(err);
            setFormError(
                err instanceof Error
                    ? err.message
                    : "Unable to update warehouse."
            );
        } finally {
            setSaving(false);
        }
    }

    async function handleDelete(warehouse: Warehouse) {
        const confirmed = window.confirm(
            `Are you sure you want to delete "${warehouse.name}"? This action cannot be undone.`
        );

        if (!confirmed) {
            return;
        }

        setSuccessMessage(null);
        setFormError(null);

        try {
            setDeletingId(warehouse.id);

            await deleteWarehouse(warehouse.id);

            if (editingWarehouseId === warehouse.id) {
                cancelEditing();
            }

            await loadWarehouses();
            onWarehousesChanged?.();

            setSuccessMessage(
                `${warehouse.name} was deleted successfully.`
            );
        } catch (err) {
            console.error(err);
            setError(
                err instanceof Error
                    ? err.message
                    : "Unable to delete warehouse."
            );
        } finally {
            setDeletingId(null);
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
                        cancelEditing();
                        setFormError(null);
                        setSuccessMessage(null);
                    }}
                    disabled={saving}
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

                            <div className="warehouse-actions">
                                <button
                                    type="button"
                                    className="add-stop-button"
                                    onClick={() => startEditing(warehouse)}
                                    disabled={
                                        saving ||
                                        deletingId !== null ||
                                        editingWarehouseId !== null
                                    }
                                >
                                    Edit
                                </button>

                                <button
                                    type="button"
                                    className="add-stop-button"
                                    onClick={() => void handleDelete(warehouse)}
                                    disabled={
                                        saving ||
                                        deletingId !== null ||
                                        editingWarehouseId !== null
                                    }
                                >
                                    {deletingId === warehouse.id
                                        ? "Deleting..."
                                        : "Delete"}
                                </button>
                            </div>

                            {editingWarehouseId === warehouse.id && (
                                <form
                                    className="warehouse-form"
                                    onSubmit={(event) =>
                                        void handleUpdate(event, warehouse)
                                    }
                                >
                                    <h3>Edit warehouse</h3>

                                    <div className="control-group">
                                        <label className="address-label">
                                            Warehouse name
                                        </label>

                                        <input
                                            type="text"
                                            value={editWarehouseName}
                                            onChange={(event) =>
                                                setEditWarehouseName(
                                                    event.target.value
                                                )
                                            }
                                            disabled={saving}
                                            required
                                        />
                                    </div>

                                    <div className="control-group">
                                        <AddressAutocomplete
                                            label="Change warehouse address (optional)"
                                            placeholder="Search for a new address..."
                                            value={editAddressQuery}
                                            onChange={(value) => {
                                                setEditAddressQuery(value);
                                                setEditAddressChanged(
                                                    value.trim().length > 0
                                                );
                                                setEditSelectedAddress(null);
                                            }}
                                            onSelect={(address) => {
                                                setEditSelectedAddress(address);
                                                setEditAddressChanged(true);
                                            }}
                                            disabled={saving}
                                        />
                                    </div>

                                    <p>
                                        Leave the address field empty to keep
                                        the existing coordinates.
                                    </p>

                                    {editSelectedAddress && (
                                        <div className="warehouse-coordinate-preview">
                                            <strong>New selected location</strong>
                                            <p>{editSelectedAddress.label}</p>
                                            <p>
                                                Latitude:{" "}
                                                {editSelectedAddress.latitude.toFixed(6)}
                                            </p>
                                            <p>
                                                Longitude:{" "}
                                                {editSelectedAddress.longitude.toFixed(6)}
                                            </p>
                                        </div>
                                    )}

                                    {formError && (
                                        <div className="route-error" role="alert">
                                            {formError}
                                        </div>
                                    )}

                                    <div className="warehouse-actions">
                                        <button
                                            type="submit"
                                            className="optimize-button"
                                            disabled={saving}
                                        >
                                            {saving ? "Saving changes..." : "Save changes"}
                                        </button>

                                        <button
                                            type="button"
                                            className="add-stop-button"
                                            onClick={cancelEditing}
                                            disabled={saving}
                                        >
                                            Cancel
                                        </button>
                                    </div>
                                </form>
                            )}
                        </div>
                    ))}
                </div>
            )}
        </section>
    );
}
