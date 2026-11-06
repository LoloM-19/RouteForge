
namespace RouteForge.Api.DTOs;

public class OptimizeRouteRequest
{
    public double StartLatitude { get; set; }

    public double StartLongitude { get; set; }

    public string StartName { get; set; } = string.Empty;

    public double EndLatitude { get; set; }

    public double EndLongitude { get; set; }

    public string EndName { get; set; } = string.Empty;

    public List<RouteStopRequest> Stops { get; set; } = [];
}

public class RouteStopRequest
{
    public string Name { get; set; } = string.Empty;

    public double Latitude { get; set; }

    public double Longitude { get; set; }
}
