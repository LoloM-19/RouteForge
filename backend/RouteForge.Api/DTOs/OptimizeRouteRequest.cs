
namespace RouteForge.Api.DTOs;

public class OptimizeRouteRequest
{
    public int WarehouseId { get; set; }

    public List<int> DeliveryLocationIds { get; set; } = [];
}