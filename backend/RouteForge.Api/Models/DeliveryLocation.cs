
using RouteForge.Api.Enums;

namespace RouteForge.Api.Models;

public class DeliveryLocation
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public LocationType Type { get; set; } = LocationType.Delivery;
}
