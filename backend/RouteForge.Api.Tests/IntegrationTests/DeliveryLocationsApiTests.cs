using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace RouteForge.Api.Tests.IntegrationTests;

public class DeliveryLocationsApiTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public DeliveryLocationsApiTests(
        WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetLocations_ReturnsSuccessAndTwoLocations()
    {
        // Act
        var response = await _client.GetAsync("/api/locations");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var locations = await response.Content
            .ReadFromJsonAsync<List<DeliveryLocationResponse>>();

        Assert.NotNull(locations);
        Assert.Equal(2, locations.Count);
        Assert.Contains(
            locations,
            location => location.Name == "Johannesburg Warehouse");
    }

    private class DeliveryLocationResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}