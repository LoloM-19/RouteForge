
using Microsoft.EntityFrameworkCore;
using RouteForge.Api.Models;

namespace RouteForge.Api.Data;

public class RouteForgeDbContext : DbContext
{
    public RouteForgeDbContext(
        DbContextOptions<RouteForgeDbContext> options)
        : base(options)
    {
    }

    public DbSet<DeliveryLocation> DeliveryLocations =>
        Set<DeliveryLocation>();
}