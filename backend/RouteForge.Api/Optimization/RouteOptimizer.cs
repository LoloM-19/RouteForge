
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
            warehouse,
            DistanceCalculator.CalculateDistanceKm);
    }

    public RouteOptimizationResult OptimizeRoute(
        DeliveryLocation warehouse,
        List<DeliveryLocation> deliveryStops,
        Func<DeliveryLocation, DeliveryLocation, double> getDistanceKm)
    {
        return OptimizeRoute(
            warehouse,
            deliveryStops,
            warehouse,
            getDistanceKm);
    }

    public RouteOptimizationResult OptimizeRoute(
        DeliveryLocation start,
        List<DeliveryLocation> deliveryStops,
        DeliveryLocation end,
        Func<DeliveryLocation, DeliveryLocation, double> getDistanceKm)
    {
        // Calculate the baseline distance using the supplied stop order.
        double originalDistanceKm = 0;
        var originalCurrentLocation = start;

        foreach (var stop in deliveryStops)
        {
            originalDistanceKm += getDistanceKm(
                originalCurrentLocation,
                stop);

            originalCurrentLocation = stop;
        }

        originalDistanceKm += getDistanceKm(
            originalCurrentLocation,
            end);

        // Create a separate list so we don't modify the input list.
        var unvisitedStops = new List<DeliveryLocation>(deliveryStops);
        var optimizedRoute = new List<DeliveryLocation>();

        var currentLocation = start;
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

        // Finish at the selected destination, not necessarily the start.
        totalDistanceKm += getDistanceKm(currentLocation, end);

        return new RouteOptimizationResult
        {
            OptimizedRoute = optimizedRoute,
            OriginalDistanceKm = originalDistanceKm,
            TotalDistanceKm = totalDistanceKm
        };
    }
}
