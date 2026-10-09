
using RouteForge.Api.Models;

namespace RouteForge.Api.Services;

public class DeliveryLocationService
{
    private readonly List<DeliveryLocation> _locations = new()
    {
        new DeliveryLocation
        {
            Id = 1,
            Name = "Johannesburg Warehouse",
            Latitude = -26.2041,
            Longitude = 28.0473
        },
        new DeliveryLocation
        {
            Id = 2,
            Name = "Sandton Delivery Hub",
            Latitude = -26.1076,
            Longitude = 28.0567
        }
    };

    private int _nextId = 3;

    public List<DeliveryLocation> GetAll()
    {
        return _locations;
    }

    public DeliveryLocation? GetById(int id)
    {
        return _locations.FirstOrDefault(location => location.Id == id);
    }

    public DeliveryLocation Create(DeliveryLocation location)
    {
        location.Id = _nextId++;
        _locations.Add(location);

        return location;
    }
}