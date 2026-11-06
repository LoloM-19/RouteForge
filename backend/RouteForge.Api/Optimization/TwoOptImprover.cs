
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
            warehouse,
            DistanceCalculator.CalculateDistanceKm);
    }

    public List<DeliveryLocation> ImproveRoute(
        DeliveryLocation warehouse,
        List<DeliveryLocation> route,
        Func<DeliveryLocation, DeliveryLocation, double> getDistanceKm)
    {
        return ImproveRoute(
            warehouse,
            route,
            warehouse,
            getDistanceKm);
    }

    public List<DeliveryLocation> ImproveRoute(
        DeliveryLocation start,
        List<DeliveryLocation> route,
        DeliveryLocation end,
        Func<DeliveryLocation, DeliveryLocation, double> getDistanceKm)
    {
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
                        start,
                        improvedRoute,
                        end,
                        getDistanceKm);

                    // Reverse a section to test a different stop order.
                    improvedRoute.Reverse(i, j - i + 1);

                    double swappedDistance = CalculateRouteDistance(
                        start,
                        improvedRoute,
                        end,
                        getDistanceKm);

                    if (swappedDistance < currentDistance - 1e-9)
                    {
                        improvementFound = true;
                    }
                    else
                    {
                        // Keep the original order if the change wasn't better.
                        improvedRoute.Reverse(i, j - i + 1);
                    }
                }
            }
        }
        while (improvementFound);

        return improvedRoute;
    }

    private static double CalculateRouteDistance(
        DeliveryLocation start,
        List<DeliveryLocation> route,
        DeliveryLocation end,
        Func<DeliveryLocation, DeliveryLocation, double> getDistanceKm)
    {
        double totalDistanceKm = 0;
        var currentLocation = start;

        foreach (var stop in route)
        {
            totalDistanceKm += getDistanceKm(currentLocation, stop);
            currentLocation = stop;
        }

        // Finish at the selected destination.
        totalDistanceKm += getDistanceKm(currentLocation, end);

        return totalDistanceKm;
    }
}
