
using RouteForge.Api.Models;

namespace RouteForge.Api.Optimization;

public static class DistanceCalculator
{
    private const double EarthRadiusKm = 6371.0;

    public static double CalculateDistanceKm(
        DeliveryLocation origin,
        DeliveryLocation destination)
    {
        double latitude1 = ToRadians(origin.Latitude);
        double latitude2 = ToRadians(destination.Latitude);

        double latitudeDifference =
            ToRadians(destination.Latitude - origin.Latitude);

        double longitudeDifference =
            ToRadians(destination.Longitude - origin.Longitude);

        double a =
            Math.Pow(Math.Sin(latitudeDifference / 2), 2) +
            Math.Cos(latitude1) *
            Math.Cos(latitude2) *
            Math.Pow(Math.Sin(longitudeDifference / 2), 2);

        // Protect against tiny floating-point rounding errors.
        a = Math.Clamp(a, 0.0, 1.0);

        double centralAngle =
            2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return EarthRadiusKm * centralAngle;
    }

    private static double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }
}