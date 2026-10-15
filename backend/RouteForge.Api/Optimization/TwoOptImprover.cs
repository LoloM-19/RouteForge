
using RouteForge.Api.Models;

namespace RouteForge.Api.Optimization;

public class TwoOptImprover
{
    public List<DeliveryLocation> ImproveRoute(
        DeliveryLocation warehouse,
        List<DeliveryLocation> route)
    {
        return ImproveRoute(
            warehouse,
            route,
            DistanceCalculator.CalculateDistanceKm);
    }

    public List<DeliveryLocation> ImproveRoute(
        DeliveryLocation warehouse,
        List<DeliveryLocation> route,
        Func<DeliveryLocation, DeliveryLocation, double> getDistanceKm)
    {
        // Work on a copy so the caller's route remains unchanged.
        var improvedRoute = new List<DeliveryLocation>(route);

        if (improvedRoute.Count < 2)
        {
            return improvedRoute;
        }

        bool improvementFound;

        do
        {
            improvementFound = false;

            for (int i = 0; i < improvedRoute.Count - 1; i++)
            {
                for (int j = i + 1; j < improvedRoute.Count; j++)
                {
                    double currentDistance = CalculateRouteDistance(
                        warehouse,
                        improvedRoute,
                        getDistanceKm);

                    // Reverse this section to test an alternative route.
                    improvedRoute.Reverse(i, j - i + 1);

                    double swappedDistance = CalculateRouteDistance(
                        warehouse,
                        improvedRoute,
                        getDistanceKm);

                    if (swappedDistance < currentDistance - 1e-9)
                    {
                        improvementFound = true;
                    }
                    else
                    {
                        // Keep the original order if it wasn't better.
                        improvedRoute.Reverse(i, j - i + 1);
                    }
                }
            }
        }
        while (improvementFound);

        return improvedRoute;
    }

    private static double CalculateRouteDistance(
        DeliveryLocation warehouse,
        List<DeliveryLocation> route,
        Func<DeliveryLocation, DeliveryLocation, double> getDistanceKm)
    {
        double totalDistanceKm = 0;
        var currentLocation = warehouse;

        foreach (var stop in route)
        {
            totalDistanceKm += getDistanceKm(currentLocation, stop);
            currentLocation = stop;
        }

        // Include the return journey to the warehouse.
        totalDistanceKm += getDistanceKm(currentLocation, warehouse);

        return totalDistanceKm;
    }
}
