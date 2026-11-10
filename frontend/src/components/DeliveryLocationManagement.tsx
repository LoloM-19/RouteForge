
import { useEffect, useState } from "react";
import AddressAutocomplete from "./AddressAutocomplete";
import type { AddressSuggestion } from "../services/addressService";
import {
    getLocations,
    createLocation,
    updateLocation,
    deleteLocation,
    type DeliveryLocation,
} from "../services/locationService";

interface DeliveryLocationManagementProps {
    onLocationsChanged?: () => void;
}

export default function DeliveryLocationManagement({
    onLocationsChanged,
}: DeliveryLocationManagementProps) {
    const [locations, setLocations] = useState<DeliveryLocation[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    // Registration form
    const [showForm, setShowForm] = useState(false);
    const [locationName, setLocationName] = useState("");
    const [addressQuery, setAddressQuery] = useState("");
    const [selectedAddress, setSelectedAddress] =
        useState<AddressSuggestion | null>(null);

    // Editing form
    const [editingLocationId, setEditingLocationId] =
        useState<number | null>(null);
    const [editLocationName, setEditLocationName] = useState("");
    const [editAddressQuery, setEditAddressQuery] = useState("");
    const [editSelectedAddress, setEditSelectedAddress] =
        useState<AddressSuggestion | null>(null);
    const [editAddressChanged, setEditAddressChanged] = useState(false);

    // Feedback and operation state
    const [saving, setSaving] = useState(false);
    const [deletingId, setDeletingId] = useState<number | null>(null);
    const [formError, setFormError] = useState<string | null>(null);
    const [successMessage, setSuccessMessage] = useState<string | null>(null);
    const [searchQuery, setSearchQuery] = useState("");

    async function loadLocations() {
        try {
            setLoading(true);
            setError(null);

            const data = await getLocations();

            // Only display delivery locations, never warehouses.
            const deliveryLocations = data.filter(
                location => location.type === 1
            );

            setLocations(deliveryLocations);
        } catch (err) {
            console.error(err);
            setError("Unable to load delivery locations.");
        } finally {
            setLoading(false);
        }
    }

    useEffect(() => {
        void loadLocations();
    }, []);

    function resetForm() {
        setLocationName("");
        setAddressQuery("");
        setSelectedAddress(null);
        setFormError(null);
        setShowForm(false);
    }

    function startEditing(location: DeliveryLocation) {
        setShowForm(false);
        setEditingLocationId(location.id);
        setEditLocationName(location.name);
        setEditAddressQuery("");
        setEditSelectedAddress(null);
        setEditAddressChanged(false);
        setFormError(null);
        setSuccessMessage(null);
    }

    function cancelEditing() {
        setEditingLocationId(null);
        setEditLocationName("");
        setEditAddressQuery("");
        setEditSelectedAddress(null);
        setEditAddressChanged(false);
        setFormError(null);
    }

    async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
        event.preventDefault();

        setFormError(null);
        setSuccessMessage(null);

        if (!locationName.trim()) {
            setFormError("Please enter a delivery location name.");
            return;
        }

        if (!selectedAddress) {
            setFormError("Please select an address from the suggestions.");
            return;
        }

        try {
            setSaving(true);

            const createdLocation = await createLocation({
                name: locationName.trim(),
                latitude: selectedAddress.latitude,
                longitude: selectedAddress.longitude,
            });

            await loadLocations();
            onLocationsChanged?.();

            setSuccessMessage(
                `${createdLocation.name} was registered successfully.`
            );

            resetForm();
        } catch (err) {
            console.error(err);
            setFormError(
                err instanceof Error
                    ? err.message
                    : "Unable to register delivery location."
            );
        } finally {
            setSaving(false);
        }
    }

    async function handleUpdate(
        event: React.FormEvent<HTMLFormElement>,
        location: DeliveryLocation
    ) {
        event.preventDefault();

        setFormError(null);
        setSuccessMessage(null);

        if (!editLocationName.trim()) {
            setFormError("Please enter a delivery location name.");
            return;
        }

        if (editAddressChanged && !editSelectedAddress) {
            setFormError(
                "Please select an address from the suggestions."
            );
            return;
        }

        try {
            setSaving(true);

            const updatedLocation = await updateLocation(location.id, {
                name: editLocationName.trim(),
                latitude: editSelectedAddress?.latitude ?? location.latitude,
                longitude: editSelectedAddress?.longitude ?? location.longitude,
            });

            await loadLocations();
            onLocationsChanged?.();

            setSuccessMessage(
                `${updatedLocation.name} was updated successfully.`
            );

            cancelEditing();
        } catch (err) {
            console.error(err);
            setFormError(
                err instanceof Error
                    ? err.message
                    : "Unable to update delivery location."
            );
        } finally {
            setSaving(false);
        }
    }

    async function handleDelete(location: DeliveryLocation) {
        const confirmed = window.confirm(
            `Are you sure you want to delete "${location.name}"? This action cannot be undone.`
        );

        if (!confirmed) {
            return;
        }

        setSuccessMessage(null);
        setError(null);

        try {
            setDeletingId(location.id);

            await deleteLocation(location.id);

            if (editingLocationId === location.id) {
                cancelEditing();
            }

            await loadLocations();
            onLocationsChanged?.();

            setSuccessMessage(
                `${location.name} was deleted successfully.`
            );
        } catch (err) {
            console.error(err);
            setError(
                err instanceof Error
                    ? err.message
                    : "Unable to delete delivery location."
            );
        } finally {
            setDeletingId(null);
        }
    }

    const filteredLocations = locations.filter(location =>
        location.name.toLowerCase().includes(searchQuery.trim().toLowerCase())
    );

    return (
        <section className="warehouse-panel">
            <div className="panel-header">
                <div>
                    <h2>Delivery Locations</h2>
                    <p>Manage your delivery destinations.</p>
                </div>

                <span className="demo-badge">
                    {locations.length} LOCATIONS
                </span>
            </div>

            <div className="warehouse-toolbar">
                <input
                    type="search"
                    placeholder="Search delivery locations..."
                    value={searchQuery}
                    onChange={event => setSearchQuery(event.target.value)}
                    aria-label="Search delivery locations"
                />

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
                    {showForm ? "Cancel" : "+ Add delivery location"}
                </button>
            </div>

            {successMessage && (
                <div className="route-result" role="status">
                    {successMessage}
                </div>
            )}

            {showForm && (
                <form className="warehouse-form" onSubmit={handleSubmit}>
                    <h3>Register a delivery location</h3>

                    <div className="control-group">
                        <label className="address-label">
                            Delivery location name
                        </label>

                        <input
                            type="text"
                            value={locationName}
                            onChange={event => setLocationName(event.target.value)}
                            placeholder="e.g. Sandton Delivery Hub"
                            disabled={saving}
                            required
                        />
                    </div>

                    <div className="control-group">
                        <AddressAutocomplete
                            label="Delivery address"
                            placeholder="Search for a delivery address..."
                            value={addressQuery}
                            onChange={value => {
                                setAddressQuery(value);
                                setSelectedAddress(null);
                            }}
                            onSelect={setSelectedAddress}
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
                        {saving ? "Registering location..." : "Register delivery location"}
                    </button>
                </form>
            )}

            {loading ? (
                <p>Loading delivery locations...</p>
            ) : error ? (
                <div className="route-error" role="alert">
                    {error}
                    <button
                        type="button"
                        onClick={() => void loadLocations()}
                    >
                        Retry
                    </button>
                </div>
            ) : filteredLocations.length === 0 ? (
                <p>
                    {locations.length === 0
                        ? "No delivery locations have been registered yet."
                        : "No delivery locations match your search."}
                </p>
            ) : (
                <div className="warehouse-list">
                    {filteredLocations.map(location => (
                        <div className="warehouse-card" key={location.id}>
                            <div>
                                <h3>{location.name}</h3>
                                <p>
                                    Latitude: {location.latitude.toFixed(4)}
                                </p>
                                <p>
                                    Longitude: {location.longitude.toFixed(4)}
                                </p>
                            </div>

                            <span className="warehouse-status">
                                Delivery
                            </span>

                            <div className="warehouse-actions">
                                <button
                                    type="button"
                                    className="add-stop-button"
                                    onClick={() => startEditing(location)}
                                    disabled={
                                        saving ||
                                        deletingId !== null ||
                                        editingLocationId !== null
                                    }
                                >
                                    Edit
                                </button>

                                <button
                                    type="button"
                                    className="add-stop-button"
                                    onClick={() => void handleDelete(location)}
                                    disabled={
                                        saving ||
                                        deletingId !== null ||
                                        editingLocationId !== null
                                    }
                                >
                                    {deletingId === location.id
                                        ? "Deleting..."
                                        : "Delete"}
                                </button>
                            </div>

                            {editingLocationId === location.id && (
                                <form
                                    className="warehouse-form"
                                    onSubmit={event =>
                                        void handleUpdate(event, location)
                                    }
                                >
                                    <h3>Edit delivery location</h3>

                                    <div className="control-group">
                                        <label className="address-label">
                                            Delivery location name
                                        </label>

                                        <input
                                            type="text"
                                            value={editLocationName}
                                            onChange={event =>
                                                setEditLocationName(event.target.value)
                                            }
                                            disabled={saving}
                                            required
                                        />
                                    </div>

                                    <div className="control-group">
                                        <AddressAutocomplete
                                            label="Change delivery address (optional)"
                                            placeholder="Search for a new address..."
                                            value={editAddressQuery}
                                            onChange={value => {
                                                setEditAddressQuery(value);
                                                setEditAddressChanged(
                                                    value.trim().length > 0
                                                );
                                                setEditSelectedAddress(null);
                                            }}
                                            onSelect={address => {
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
                                                Latitude: {editSelectedAddress.latitude.toFixed(6)}
                                            </p>
                                            <p>
                                                Longitude: {editSelectedAddress.longitude.toFixed(6)}
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
