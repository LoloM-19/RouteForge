using RouteForge.Api.Models;

namespace RouteForge.Api.Optimization;

public class RouteOptimizer
{
    public RouteOptimizationResult OptimizeRoute(
        DeliveryLocation warehouse,
        List<DeliveryLocation> deliveryStops)
    {
        return OptimizeRoute(
            warehouse,
            deliveryStops,
            DistanceCalculator.CalculateDistanceKm);
    }

    public RouteOptimizationResult OptimizeRoute(
        DeliveryLocation warehouse,
        List<DeliveryLocation> deliveryStops,
        Func<DeliveryLocation, DeliveryLocation, double> getDistanceKm)
    {
        // Preserve the supplied order to calculate the baseline distance.
        double originalDistanceKm = 0;
        var originalCurrentLocation = warehouse;

        foreach (var stop in deliveryStops)
        {
            originalDistanceKm += getDistanceKm(
                originalCurrentLocation,
                stop);

            originalCurrentLocation = stop;
        }

        // Include the return journey in the original distance.
        originalDistanceKm += getDistanceKm(
            originalCurrentLocation,
            warehouse);

        // Create a separate list so we don't modify the input list.
        var unvisitedStops = new List<DeliveryLocation>(deliveryStops);
        var optimizedRoute = new List<DeliveryLocation>();

        var currentLocation = warehouse;
        double totalDistanceKm = 0;

        // Repeatedly select the nearest unvisited delivery stop.
        while (unvisitedStops.Count > 0)
        {
            var nearestStop = unvisitedStops
                .Select(stop => new
                {
                    Location = stop,
                    Distance = getDistanceKm(currentLocation, stop)
                })
                .OrderBy(candidate => candidate.Distance)
                .First();

            optimizedRoute.Add(nearestStop.Location);
            totalDistanceKm += nearestStop.Distance;

            currentLocation = nearestStop.Location;
            unvisitedStops.Remove(nearestStop.Location);
        }

        // Include the return journey in the optimized distance.
        totalDistanceKm += getDistanceKm(
            currentLocation,
            warehouse);

        return new RouteOptimizationResult
        {
            OptimizedRoute = optimizedRoute,
            OriginalDistanceKm = originalDistanceKm,
            TotalDistanceKm = totalDistanceKm
        };
    }
}
