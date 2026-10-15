using RouteForge.Api.Models;

namespace RouteForge.Api.Optimization;

public class RouteOptimizationResult
{
    public List<DeliveryLocation> OptimizedRoute { get; set; } = [];
    
    public List<List<double>> RouteGeometry { get; set; } = [];

    // Distance of the final route after all improvements.
    public double TotalDistanceKm { get; set; }

    // Distance of the original route in the requested order.
    public double OriginalDistanceKm { get; set; }

    // Distance after the Nearest Neighbour algorithm,
    // before applying 2-opt.
    public double NearestNeighbourDistanceKm { get; set; }

    // Savings compared with the original requested order.
    public double DistanceSavedKm =>
        OriginalDistanceKm - TotalDistanceKm;

    public double ImprovementPercentage =>
        OriginalDistanceKm > 0
            ? (DistanceSavedKm / OriginalDistanceKm) * 100
            : 0;

    // Distance reduced by 2-opt compared with Nearest Neighbour.
    public double TwoOptDistanceSavedKm =>
        NearestNeighbourDistanceKm - TotalDistanceKm;

    public double TwoOptImprovementPercentage =>
        NearestNeighbourDistanceKm > 0
            ? (TwoOptDistanceSavedKm / NearestNeighbourDistanceKm) * 100
            : 0;
}
