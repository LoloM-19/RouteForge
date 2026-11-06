
import { useEffect, useState } from "react";
import {
  searchAddresses,
  type AddressSuggestion,
} from "../services/addressService";

interface AddressAutocompleteProps {
  label: string;
  placeholder: string;
  value: string;
  onChange: (value: string) => void;
  onSelect: (address: AddressSuggestion) => void;
  disabled?: boolean;
}

export default function AddressAutocomplete({
  label,
  placeholder,
  value,
  onChange,
  onSelect,
  disabled = false,
}: AddressAutocompleteProps) {
  const [suggestions, setSuggestions] = useState<AddressSuggestion[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [selectedLabel, setSelectedLabel] = useState<string | null>(null);

  useEffect(() => {
    const trimmedQuery = value.trim();

    if (trimmedQuery.length < 3 || trimmedQuery === selectedLabel) {
      setSuggestions([]);
      setLoading(false);
      setError(null);
      return;
    }

    const controller = new AbortController();

    const timeoutId = setTimeout(async () => {
      setLoading(true);
      setError(null);

      try {
        const results = await searchAddresses(
          trimmedQuery,
          controller.signal
        );

        setSuggestions(results);
      } catch (err) {
        if (err instanceof Error && err.name !== "AbortError") {
          setError("Address search is temporarily unavailable.");
          setSuggestions([]);
        }
      } finally {
        if (!controller.signal.aborted) {
          setLoading(false);
        }
      }
    }, 350);

    return () => {
      clearTimeout(timeoutId);
      controller.abort();
    };
  }, [value, selectedLabel]);

  function handleChange(newValue: string) {
    setSelectedLabel(null);
    onChange(newValue);
  }

  function handleSelect(address: AddressSuggestion) {
    setSelectedLabel(address.label);
    setSuggestions([]);
    setError(null);
    onChange(address.label);
    onSelect(address);
  }

  return (
    <div className="address-autocomplete">
      <label className="address-label">{label}</label>

      <input
        type="text"
        value={value}
        placeholder={placeholder}
        onChange={(event) => handleChange(event.target.value)}
        disabled={disabled}
        autoComplete="off"
        aria-label={label}
        aria-autocomplete="list"
      />

      {loading && (
        <p className="address-status">Searching addresses...</p>
      )}

      {error && (
        <p className="address-error">{error}</p>
      )}

      {suggestions.length > 0 && (
        <ul className="address-suggestions">
          {suggestions.map((address, index) => (
            <li key={`${address.latitude},${address.longitude},${index}`}>
              <button
                type="button"
                onClick={() => handleSelect(address)}
                disabled={disabled}
              >
                {address.label}
              </button>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
