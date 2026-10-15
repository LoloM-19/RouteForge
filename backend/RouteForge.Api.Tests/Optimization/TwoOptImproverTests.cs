
using RouteForge.Api.Models;
using RouteForge.Api.Optimization;

namespace RouteForge.Api.Tests.Optimization;

public class TwoOptImproverTests
{
    [Fact]
    public void ImproveRoute_ReducesDistanceForAnInefficientRoute()
    {
        // Arrange: four stops positioned to create a crossing route.
        var warehouse = new DeliveryLocation
        {
            Id = 1,
            Name = "Warehouse",
            Latitude = 0,
            Longitude = 0
        };

        var stopA = new DeliveryLocation
        {
            Id = 2,
            Name = "Stop A",
            Latitude = 0,
            Longitude = 2
        };

        var stopB = new DeliveryLocation
        {
            Id = 3,
            Name = "Stop B",
            Latitude = 2,
            Longitude = 0
        };

        var stopC = new DeliveryLocation
        {
            Id = 4,
            Name = "Stop C",
            Latitude = 2,
            Longitude = 2
        };

        var stopD = new DeliveryLocation
        {
            Id = 5,
            Name = "Stop D",
            Latitude = 0,
            Longitude = 1
        };

        // This order is deliberately inefficient.
        var originalRoute = new List<DeliveryLocation>
        {
            stopA,
            stopB,
            stopC,
            stopD
        };

        var improver = new TwoOptImprover();

        double originalDistance = CalculateRoundTripDistance(
            warehouse, originalRoute);

        // Act
        var improvedRoute = improver.ImproveRoute(
            warehouse, originalRoute);

        double improvedDistance = CalculateRoundTripDistance(
            warehouse, improvedRoute);

        // Assert
        Assert.True(
            improvedDistance < originalDistance,
            $"Expected an improvement, but distance changed from " +
            $"{originalDistance:F2} km to {improvedDistance:F2} km.");

        Assert.Equal(
            originalRoute.Select(location => location.Id).OrderBy(id => id),
            improvedRoute.Select(location => location.Id).OrderBy(id => id));
    }

    private static double CalculateRoundTripDistance(
        DeliveryLocation warehouse,
        List<DeliveryLocation> route)
    {
        double totalDistance = 0;
        var currentLocation = warehouse;

        foreach (var stop in route)
        {
            totalDistance += DistanceCalculator.CalculateDistanceKm(
                currentLocation, stop);

            currentLocation = stop;
        }

        totalDistance += DistanceCalculator.CalculateDistanceKm(
            currentLocation, warehouse);

        return totalDistance;
    }

   
    [Fact]
    public void ImproveRoute_WithNoStops_ReturnsEmptyRoute()
    {
        // Arrange
        var warehouse = new DeliveryLocation
        {
            Id = 1,
            Name = "Warehouse",
            Latitude = -26.2041,
            Longitude = 28.0473
        };

        var improver = new TwoOptImprover();

        // Act
        var improvedRoute = improver.ImproveRoute(
            warehouse, []);

        // Assert
        Assert.Empty(improvedRoute);
    }

    [Fact]
    public void ImproveRoute_DoesNotModifyOriginalRoute()
    {
        // Arrange
        var warehouse = new DeliveryLocation
        {
            Id = 1,
            Name = "Warehouse",
            Latitude = 0,
            Longitude = 0
        };

        var stopA = new DeliveryLocation
        {
            Id = 2,
            Name = "Stop A",
            Latitude = 0,
            Longitude = 2
        };

        var stopB = new DeliveryLocation
        {
            Id = 3,
            Name = "Stop B",
            Latitude = 2,
            Longitude = 0
        };

        var stopC = new DeliveryLocation
        {
            Id = 4,
            Name = "Stop C",
            Latitude = 2,
            Longitude = 2
        };

        var originalRoute = new List<DeliveryLocation>
        {
            stopA, stopB, stopC
        };

        var originalIds = originalRoute
            .Select(location => location.Id)
            .ToList();

        var improver = new TwoOptImprover();

        // Act
        improver.ImproveRoute(warehouse, originalRoute);

        // Assert
        Assert.Equal(
            originalIds,
            originalRoute.Select(location => location.Id).ToList());
    }
  
    [Fact]
    public void RouteOptimizationResult_CalculatesTwoOptSavingsCorrectly()
    {
        // Arrange: Nearest Neighbour gives 100 km.
        // 2-opt reduces the final route to 85 km.
        var result = new RouteOptimizationResult
        {
            OriginalDistanceKm = 120,
            NearestNeighbourDistanceKm = 100,
            TotalDistanceKm = 85
        };

        // Act and assert
        Assert.Equal(15, result.TwoOptDistanceSavedKm, 8);
        Assert.Equal(15, result.TwoOptImprovementPercentage, 8);

        // The overall improvement is measured against the original route.
        Assert.Equal(35, result.DistanceSavedKm, 8);
        Assert.Equal(
            (35.0 / 120.0) * 100,
            result.ImprovementPercentage,
            8);
    }

}
