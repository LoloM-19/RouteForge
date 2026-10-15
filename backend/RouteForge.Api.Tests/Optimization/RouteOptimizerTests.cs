
using RouteForge.Api.Models;
using RouteForge.Api.Optimization;

namespace RouteForge.Api.Tests.Optimization;

public class RouteOptimizerTests
{
    private readonly RouteOptimizer _optimizer = new();

    [Fact]
    public void OptimizeRoute_VisitsEveryDeliveryStopExactlyOnce()
    {
        var warehouse = new DeliveryLocation
        {
            Id = 1,
            Name = "Johannesburg Warehouse",
            Latitude = -26.2041,
            Longitude = 28.0473
        };

        var deliveryStops = new List<DeliveryLocation>
        {
            new()
            {
                Id = 2,
                Name = "Sandton Delivery Hub",
                Latitude = -26.1076,
                Longitude = 28.0567
            },
            new()
            {
                Id = 3,
                Name = "Rosebank Delivery Point",
                Latitude = -26.1467,
                Longitude = 28.0436
            },
            new()
            {
                Id = 4,
                Name = "Pretoria Distribution Centre",
                Latitude = -25.7479,
                Longitude = 28.2293
            }
        };

        var result = _optimizer.OptimizeRoute(
            warehouse, deliveryStops);

        Assert.Equal(3, result.OptimizedRoute.Count);
        Assert.Equal(
            3,
            result.OptimizedRoute.Select(stop => stop.Id).Distinct().Count());

        Assert.All(deliveryStops, stop =>
            Assert.Contains(
                result.OptimizedRoute,
                optimizedStop => optimizedStop.Id == stop.Id));

        Assert.True(result.OriginalDistanceKm > 0);

        Assert.Equal(
        result.OriginalDistanceKm - result.TotalDistanceKm,
        result.DistanceSavedKm,
        8);

    }

    [Fact]
    public void OptimizeRoute_IncludesReturnJourneyToWarehouse()
    {
        var warehouse = new DeliveryLocation
        {
            Id = 1,
            Name = "Warehouse",
            Latitude = -26.2041,
            Longitude = 28.0473
        };

        var stop = new DeliveryLocation
        {
            Id = 2,
            Name = "Delivery Stop",
            Latitude = -26.1076,
            Longitude = 28.0567
        };

        var result = _optimizer.OptimizeRoute(
            warehouse, new List<DeliveryLocation> { stop });

        var outboundDistance =
            DistanceCalculator.CalculateDistanceKm(warehouse, stop);

        var returnDistance =
            DistanceCalculator.CalculateDistanceKm(stop, warehouse);

        Assert.Equal(
            outboundDistance + returnDistance,
            result.TotalDistanceKm,
            8);
    }

    [Fact]
    public void OptimizeRoute_NoDeliveryStops_ReturnsToWarehouseWithZeroDistance()
    {
        var warehouse = new DeliveryLocation
        {
            Id = 1,
            Name = "Warehouse",
            Latitude = -26.2041,
            Longitude = 28.0473
        };

        var result = _optimizer.OptimizeRoute(
            warehouse, new List<DeliveryLocation>());

        Assert.Empty(result.OptimizedRoute);
        Assert.Equal(0, result.TotalDistanceKm, 8);
    }

    [Fact]
    public void OptimizeRoute_ImprovementPercentageIsCalculatedCorrectly()
    {
        // Arrange
        var result = new RouteOptimizationResult
        {
            OriginalDistanceKm = 100,
            TotalDistanceKm = 80
        };

        // Act
        var improvement = result.ImprovementPercentage;

        // Assert: reducing 100 km to 80 km is a 20% improvement.
        Assert.Equal(20, improvement, 8);
    }

   
    [Fact]
    public void OptimizeRoute_WorseRouteProducesNegativeImprovement()
    {
        // Arrange: the original route is 100 km,
        // but the resulting route is 120 km.
        var result = new RouteOptimizationResult
        {
            OriginalDistanceKm = 100,
            TotalDistanceKm = 120
        };

        // Act
        var improvement = result.ImprovementPercentage;

        // Assert: the route is 20% worse than the baseline.
        Assert.Equal(-20, improvement, 8);
        Assert.Equal(-20, result.DistanceSavedKm, 8);
    }

}