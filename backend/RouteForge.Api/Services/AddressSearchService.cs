
using System.Globalization;
using System.Text.Json;

using RouteForge.Api.DTOs;

namespace RouteForge.Api.Services;

public class AddressSearchService(HttpClient httpClient)
{
    public async Task<List<AddressSuggestion>> SearchAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        var encodedQuery = Uri.EscapeDataString(query);

        var url =
            $"https://photon.komoot.io/api/?q={encodedQuery}" +
            "&limit=5&lang=en&lat=-26.2041&lon=28.0473";

        using var response = await httpClient.GetAsync(
            url,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(
            cancellationToken);

        using var document = await JsonDocument.ParseAsync(
            stream,
            cancellationToken: cancellationToken);

        var suggestions = new List<AddressSuggestion>();

        foreach (var feature in document.RootElement
                     .GetProperty("features")
                     .EnumerateArray())
        {
            var properties = feature.GetProperty("properties");
            var coordinates = feature.GetProperty("geometry")
                .GetProperty("coordinates");

            var longitude = coordinates[0].GetDouble();
            var latitude = coordinates[1].GetDouble();

            var labelParts = new[]
            {
                GetProperty(properties, "name"),
                GetProperty(properties, "housenumber"),
                GetProperty(properties, "street"),
                GetProperty(properties, "city"),
                GetProperty(properties, "state"),
                GetProperty(properties, "postcode"),
                GetProperty(properties, "country")
            };

            var label = string.Join(
                ", ",
                labelParts
                    .Where(part => !string.IsNullOrWhiteSpace(part))
                    .Distinct());

            if (string.IsNullOrWhiteSpace(label))
            {
                continue;
            }

            suggestions.Add(new AddressSuggestion
            {
                Label = label,
                Latitude = latitude,
                Longitude = longitude
            });
        }

        return suggestions;
    }

    private static string? GetProperty(
        JsonElement properties,
        string propertyName)
    {
        return properties.TryGetProperty(propertyName, out var value)
               && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }
}
