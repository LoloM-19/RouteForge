
using RouteForge.Api.Models;
using RouteForge.Api.Optimization;

namespace RouteForge.Api.Tests.Optimization;

public class DistanceCalculatorTests
{
    [Fact]
    public void CalculateDistanceKm_SameLocation_ReturnsZero()
    {
        var location = new DeliveryLocation
        {
            Latitude = -26.2041,
            Longitude = 28.0473
        };

        var distance = DistanceCalculator.CalculateDistanceKm(
            location, location);

        Assert.Equal(0, distance, 10);
    }

    [Fact]
    public void CalculateDistanceKm_DifferentLocations_ReturnsPositiveDistance()
    {
        var johannesburg = new DeliveryLocation
        {
            Latitude = -26.2041,
            Longitude = 28.0473
        };

        var sandton = new DeliveryLocation
        {
            Latitude = -26.1076,
            Longitude = 28.0567
        };

        var distance = DistanceCalculator.CalculateDistanceKm(
            johannesburg, sandton);

        Assert.True(distance > 0);
        Assert.True(distance < 20);
    }

    [Fact]
    public void CalculateDistanceKm_ReversedLocations_ReturnsSameDistance()
    {
        var first = new DeliveryLocation
        {
            Latitude = -26.2041,
            Longitude = 28.0473
        };

        var second = new DeliveryLocation
        {
            Latitude = -25.7479,
            Longitude = 28.2293
        };

        var forwardDistance = DistanceCalculator.CalculateDistanceKm(
            first, second);

        var reverseDistance = DistanceCalculator.CalculateDistanceKm(
            second, first);

        Assert.Equal(forwardDistance, reverseDistance, 10);
    }
}