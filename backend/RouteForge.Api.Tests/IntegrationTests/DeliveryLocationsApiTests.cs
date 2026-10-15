using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RouteForge.Api.Data;
using RouteForge.Api.Models;
using RouteForge.Api.Optimization;
using RouteForge.Api.Services;

namespace RouteForge.Api.Tests.IntegrationTests;

public class DeliveryLocationsApiTests
: IClassFixture<DeliveryLocationsApiFactory>
{
private readonly HttpClient _client;


public DeliveryLocationsApiTests(DeliveryLocationsApiFactory factory)
{
    _client = factory.CreateClient();

    // Seed the test database using the application's service provider.
    using var scope = factory.Services.CreateScope();

    var context = scope.ServiceProvider
        .GetRequiredService<RouteForgeDbContext>();

    context.Database.EnsureCreated();

    context.DeliveryLocations.RemoveRange(
        context.DeliveryLocations);

    context.SaveChanges();

    context.DeliveryLocations.AddRange(
        new DeliveryLocation
        {
            Id = 1,
            Name = "Johannesburg Warehouse",
            Latitude = -26.2041,
            Longitude = 28.0473
        },
        new DeliveryLocation
        {
            Id = 2,
            Name = "Sandton Delivery Hub",
            Latitude = -26.1076,
            Longitude = 28.0567
        });

    context.SaveChanges();
} //Constructor ends here!


    [Fact]
    public async Task OptimizeRoute_Uses2OptToImproveRoute()
    {
        // Arrange: create a warehouse and four delivery stops.
        var warehouse = new
        {
            Name = "Test Warehouse",
            Latitude = 0.0,
            Longitude = 0.0
        };

        var warehouseResponse = await _client.PostAsJsonAsync(
            "/api/locations", warehouse);

        warehouseResponse.EnsureSuccessStatusCode();

        var createdWarehouse = await warehouseResponse.Content
            .ReadFromJsonAsync<DeliveryLocation>();

        Assert.NotNull(createdWarehouse);

        var stops = new[]
        {
            new { Name = "Stop A", Latitude = 0.0, Longitude = 2.0 },
            new { Name = "Stop B", Latitude = 2.0, Longitude = 0.0 },
            new { Name = "Stop C", Latitude = 2.0, Longitude = 2.0 },
            new { Name = "Stop D", Latitude = 0.0, Longitude = 1.0 }
        };

        var stopIds = new List<int>();

        foreach (var stop in stops)
        {
            var createResponse = await _client.PostAsJsonAsync(
                "/api/locations", stop);

            createResponse.EnsureSuccessStatusCode();

            var createdStop = await createResponse.Content
                .ReadFromJsonAsync<DeliveryLocation>();

            Assert.NotNull(createdStop);
            stopIds.Add(createdStop.Id);
        }

        // Act: request the stops in a deliberately inefficient order.
        var request = new
        {
            WarehouseId = createdWarehouse.Id,
            DeliveryLocationIds = stopIds
        };

        var response = await _client.PostAsJsonAsync(
            "/api/routes/optimize", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content
            .ReadFromJsonAsync<RouteOptimizationResult>();

        Assert.NotNull(result);
        Assert.Equal(stops.Length, result.OptimizedRoute.Count);
        Assert.True(result.TotalDistanceKm > 0);
        Assert.True(result.OriginalDistanceKm > 0);
            
        Assert.True(result.NearestNeighbourDistanceKm > 0);

        // 2-opt should not increase the route distance.
        Assert.True(
            result.TotalDistanceKm <= result.NearestNeighbourDistanceKm + 1e-8);

        // The reported 2-opt savings must match the two distances.
        Assert.Equal(
            result.NearestNeighbourDistanceKm - result.TotalDistanceKm,
            result.TwoOptDistanceSavedKm,
            8);

        // Verify the overall savings against the original route.
        Assert.Equal(
            result.OriginalDistanceKm - result.TotalDistanceKm,
            result.DistanceSavedKm,
            8);

        Assert.Equal(
            stopIds.OrderBy(id => id),
            result.OptimizedRoute
                .Select(location => location.Id)
                .OrderBy(id => id));

        Assert.True(
            result.TotalDistanceKm < result.OriginalDistanceKm,
            $"Expected 2-opt to improve the route, but the original " +
            $"distance was {result.OriginalDistanceKm:F2} km and the " +
            $"optimized distance was {result.TotalDistanceKm:F2} km.");
    }


[Fact]
public async Task OptimizeRoute_ValidRequest_ReturnsOptimizedRoute()
{
    // Arrange: create an additional delivery location.
    var newLocation = new
    {
        Name = "Pretoria Delivery Hub",
        Latitude = -25.7479,
        Longitude = 28.2293
    };

    var createResponse = await _client.PostAsJsonAsync(
        "/api/locations",
        newLocation);

    createResponse.EnsureSuccessStatusCode();

    var createdLocation = await createResponse.Content
        .ReadFromJsonAsync<DeliveryLocation>();

    Assert.NotNull(createdLocation);

    // Act: optimize the route from Johannesburg.
    var request = new
    {
        WarehouseId = 1,
        DeliveryLocationIds = new[] { 2, createdLocation.Id }
    };

    var response = await _client.PostAsJsonAsync(
        "/api/routes/optimize",
        request);

    // Assert: verify the optimization result.
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var result = await response.Content
        .ReadFromJsonAsync<RouteOptimizationResult>();

    Assert.NotNull(result);
    Assert.Equal(2, result.OptimizedRoute.Count);
    Assert.True(result.TotalDistanceKm > 0);
    Assert.True(result.OriginalDistanceKm > 0);

    // Every requested delivery stop must appear in the result.
    Assert.Equal(
        new[] { 2, createdLocation.Id }.OrderBy(id => id),
        result.OptimizedRoute
            .Select(location => location.Id)
            .OrderBy(id => id));

    // Savings must equal original distance minus final distance.
    Assert.Equal(
        result.OriginalDistanceKm - result.TotalDistanceKm,
        result.DistanceSavedKm,
        8);

    // Improvement percentage must match the distance reduction.
    var expectedImprovement = result.OriginalDistanceKm > 0
        ? (result.DistanceSavedKm / result.OriginalDistanceKm) * 100
        : 0;

    Assert.Equal(expectedImprovement, result.ImprovementPercentage, 8);

}

[Fact]
public async Task OptimizeRoute_WarehouseDoesNotExist_ReturnsNotFound()
{
    // Arrange
    var request = new
    {
        WarehouseId = 9999,
        DeliveryLocationIds = new[] { 2 }
    };

    // Act
    var response = await _client.PostAsJsonAsync(
        "/api/routes/optimize",
        request);

    // Assert
    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
}

[Fact]
public async Task OptimizeRoute_DeliveryLocationDoesNotExist_ReturnsNotFound()
{
    // Arrange: warehouse 1 exists, but location 9999 does not.
    var request = new
    {
        WarehouseId = 1,
        DeliveryLocationIds = new[] { 9999 }
    };

    // Act
    var response = await _client.PostAsJsonAsync(
        "/api/routes/optimize",
        request);

    // Assert
    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
}


[Fact]
public async Task OptimizeRoute_DuplicateDeliveryIds_ReturnsBadRequest()
{
    // Arrange: the same delivery location is listed twice.
    var request = new
    {
        WarehouseId = 1,
        DeliveryLocationIds = new[] { 2, 2 }
    };

    // Act
    var response = await _client.PostAsJsonAsync(
        "/api/routes/optimize",
        request);

    // Assert
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
}

[Fact]
public async Task OptimizeRoute_WarehouseAlsoListedAsDelivery_ReturnsBadRequest()
{
    // Arrange: warehouse ID 1 is also listed as a delivery stop.
    var request = new
    {
        WarehouseId = 1,
        DeliveryLocationIds = new[] { 1, 2 }
    };

    // Act
    var response = await _client.PostAsJsonAsync(
        "/api/routes/optimize",
        request);

    // Assert
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
}


[Fact]
public async Task GetLocations_ReturnsSuccessAndTwoLocations()
{
    var response = await _client.GetAsync("/api/locations");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var locations = await response.Content
        .ReadFromJsonAsync<List<DeliveryLocationResponse>>();

    Assert.NotNull(locations);
    Assert.Equal(2, locations.Count);

    Assert.Contains(
        locations,
        location => location.Name == "Johannesburg Warehouse");
}

[Fact]
public async Task CreateLocation_EmptyName_ReturnsBadRequest()
{
    var request = new
    {
        Name = "   ",
        Latitude = -26.2041,
        Longitude = 28.0473
    };

    var response = await _client.PostAsJsonAsync(
        "/api/locations", request);

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
}

[Fact]
public async Task CreateLocation_InvalidLatitude_ReturnsBadRequest()
{
    var request = new
    {
        Name = "Test Warehouse",
        Latitude = 100.0,
        Longitude = 28.0473
    };

    var response = await _client.PostAsJsonAsync(
        "/api/locations", request);

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
}

[Fact]
public async Task CreateLocation_InvalidLongitude_ReturnsBadRequest()
{
    var request = new
    {
        Name = "Test Warehouse",
        Latitude = -26.2041,
        Longitude = 200.0
    };

    var response = await _client.PostAsJsonAsync(
        "/api/locations", request);

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
}


[Fact]
public async Task UpdateLocation_ExistingId_ReturnsUpdatedLocation()
{
    var request = new
    {
        Name = "Updated Warehouse",
        Latitude = -25.7479,
        Longitude = 28.2293
    };

    var response = await _client.PutAsJsonAsync(
        "/api/locations/1", request);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var location = await response.Content
        .ReadFromJsonAsync<DeliveryLocationResponse>();

    Assert.NotNull(location);
    Assert.Equal("Updated Warehouse", location.Name);
}

[Fact]
public async Task UpdateLocation_NonexistentId_ReturnsNotFound()
{
    var request = new
    {
        Name = "Test Warehouse",
        Latitude = -26.2041,
        Longitude = 28.0473
    };

    var response = await _client.PutAsJsonAsync(
        "/api/locations/999", request);

    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
}

[Fact]
public async Task DeleteLocation_ExistingId_ReturnsNoContent()
{
    var response = await _client.DeleteAsync("/api/locations/1");

    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
}

[Fact]
public async Task DeleteLocation_NonexistentId_ReturnsNotFound()
{
    var response = await _client.DeleteAsync("/api/locations/999");

    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
}

private class DeliveryLocationResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

}

public class DeliveryLocationsApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<RouteForgeDbContext>>();
            services.RemoveAll<RouteForgeDbContext>();

            services.RemoveAll<
                Microsoft.EntityFrameworkCore.Infrastructure
                    .IDbContextOptionsConfiguration<RouteForgeDbContext>>();

            services.AddDbContext<RouteForgeDbContext>(options =>
                options.UseInMemoryDatabase("RouteForgeIntegrationTests"));

            // Replace the real OSRM HTTP client with a predictable fake.
            services.RemoveAll<OsrmRoutingService>();

            services.AddHttpClient<OsrmRoutingService>(client =>
            {
                client.BaseAddress = new Uri("http://localhost/");
            })
            .ConfigurePrimaryHttpMessageHandler(() =>
                new FakeOsrmHttpMessageHandler());
        });
    }
}

public class FakeOsrmHttpMessageHandler : HttpMessageHandler
{
    
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var path = request.RequestUri?.AbsolutePath ?? string.Empty;

        if (path.Contains("/table/v1/driving/"))
        {
            var coordinateSection = path
                .Split("/table/v1/driving/", StringSplitOptions.None)
                .Last();

            var coordinateText = coordinateSection
                .Split('?', StringSplitOptions.None)[0];

            var coordinates = coordinateText
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(pair =>
                {
                    var parts = pair.Split(',');

                    return (
                        Longitude: double.Parse(
                            parts[0],
                            System.Globalization.CultureInfo.InvariantCulture),
                        Latitude: double.Parse(
                            parts[1],
                            System.Globalization.CultureInfo.InvariantCulture)
                    );
                })
                .ToArray();

            var coordinateCount = coordinates.Length;
            var distances = new double?[coordinateCount][];

            for (var i = 0; i < coordinateCount; i++)
            {
                distances[i] = new double?[coordinateCount];

                for (var j = 0; j < coordinateCount; j++)
                {
                    var from = coordinates[i];
                    var to = coordinates[j];

                    const double earthRadiusKm = 6371.0;

                    var latitudeDifference =
                        (to.Latitude - from.Latitude) * Math.PI / 180.0;

                    var longitudeDifference =
                        (to.Longitude - from.Longitude) * Math.PI / 180.0;

                    var latitude1 = from.Latitude * Math.PI / 180.0;
                    var latitude2 = to.Latitude * Math.PI / 180.0;

                    var a =
                        Math.Pow(Math.Sin(latitudeDifference / 2), 2) +
                        Math.Cos(latitude1) *
                        Math.Cos(latitude2) *
                        Math.Pow(Math.Sin(longitudeDifference / 2), 2);

                    var centralAngle =
                        2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

                    distances[i][j] =
                        earthRadiusKm * centralAngle * 1000.0;
                }
            }

            var payload = new
            {
                code = "Ok",
                distances
            };

            return Task.FromResult(
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(payload)
                });
        }

        if (path.Contains("/route/v1/driving/"))
        {
            var routeCoordinateSection = path
                .Split("/route/v1/driving/", StringSplitOptions.None)
                .Last();

            var routeCoordinateText = routeCoordinateSection
                .Split('?', StringSplitOptions.None)[0];

            var routeCoordinates = routeCoordinateText
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(pair =>
                {
                    var parts = pair.Split(',');

                    return new[]
                    {
                        double.Parse(
                            parts[0],
                            System.Globalization.CultureInfo.InvariantCulture),
                        double.Parse(
                            parts[1],
                            System.Globalization.CultureInfo.InvariantCulture)
                    };
                })
                .ToArray();

            var routePayload = new
            {
                code = "Ok",
                routes = new[]
                {
                    new
                    {
                        geometry = new
                        {
                            coordinates = routeCoordinates
                        }
                    }
                }
            };

            return Task.FromResult(
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(routePayload)
                });
        }

        return Task.FromResult(
            new HttpResponseMessage(HttpStatusCode.NotFound));
    }

}


