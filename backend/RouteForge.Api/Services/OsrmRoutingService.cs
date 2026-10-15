using System.Text.Json;
using RouteForge.Api.Models;

namespace RouteForge.Api.Services;

public sealed class OsrmRoutingService
{
    private readonly HttpClient _httpClient;

    public OsrmRoutingService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<double[,]> GetDistanceMatrixAsync(
        IReadOnlyList<DeliveryLocation> locations,
        CancellationToken cancellationToken = default)
    {
        if (locations.Count == 0)
        {
            return new double[0, 0];
        }

        var coordinates = string.Join(
            ";",
            locations.Select(location =>
                $"{location.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}," +
                $"{location.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}"));

        var url =
            $"table/v1/driving/{coordinates}?annotations=distance";

        using var response = await _httpClient.GetAsync(
            url,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(
            cancellationToken);

        using var document = await JsonDocument.ParseAsync(
            stream,
            cancellationToken: cancellationToken);

        var root = document.RootElement;

        var code = root.GetProperty("code").GetString();

        if (code != "Ok")
        {
            throw new InvalidOperationException(
                $"OSRM could not calculate driving distances: {code}");
        }

        var distances = root.GetProperty("distances");
        var matrix = new double[locations.Count, locations.Count];

        for (var i = 0; i < locations.Count; i++)
        {
            for (var j = 0; j < locations.Count; j++)
            {
                var metres = distances[i][j];

                if (metres.ValueKind == JsonValueKind.Null)
                {
                    throw new InvalidOperationException(
                        $"No driving route found from location {i} to location {j}.");
                }

                matrix[i, j] = metres.GetDouble() / 1000.0;
            }
        }

        return matrix;
    }

    public async Task<List<List<double>>> GetRouteGeometryAsync(
        IReadOnlyList<DeliveryLocation> locations,
        CancellationToken cancellationToken = default)
    {
        if (locations.Count < 2)
        {
            throw new ArgumentException(
                "At least two locations are required to calculate a route.",
                nameof(locations));
        }

        var coordinates = string.Join(
            ";",
            locations.Select(location =>
                $"{location.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}," +
                $"{location.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}"));

        var url =
            $"route/v1/driving/{coordinates}?overview=full&geometries=geojson&steps=false";

        using var response = await _httpClient.GetAsync(
            url,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(
            cancellationToken);

        using var document = await JsonDocument.ParseAsync(
            stream,
            cancellationToken: cancellationToken);

        var root = document.RootElement;

        var code = root.GetProperty("code").GetString();

        if (code != "Ok")
        {
            throw new InvalidOperationException(
                $"OSRM could not calculate route geometry: {code}");
        }

        var coordinatesElement = root
            .GetProperty("routes")[0]
            .GetProperty("geometry")
            .GetProperty("coordinates");

        var geometry = new List<List<double>>();

        foreach (var coordinate in coordinatesElement.EnumerateArray())
        {
            var longitude = coordinate[0].GetDouble();
            var latitude = coordinate[1].GetDouble();

            // Leaflet expects [latitude, longitude].
            geometry.Add([latitude, longitude]);
        }

        return geometry;
    }
}